using System.Text.Json;
using System.Text.Json.Serialization;
using AnthropicCert.Application;
using AnthropicCert.Domain;

namespace AnthropicCert.Infrastructure;
public static class JsonFormat
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true, Converters = { new JsonStringEnumConverter() }
    };
    public static QuestionBank LoadBank(string path)
    {
        try
        {
            var bank = JsonSerializer.Deserialize<QuestionBank>(File.ReadAllText(path), Options)
                ?? throw new InvalidDataException("Banco vacío.");
            bank.Validate(); return bank;
        }
        catch (Exception ex) when (ex is NullReferenceException or ArgumentNullException)
        { throw new InvalidDataException("Banco incompleto: contiene campos nulos obligatorios.", ex); }
    }
}
// One atomic JSON document per attempt: no external database or runtime dependency.
public sealed class FileAttemptRepository : IAttemptRepository, IDisposable
{
    readonly string directory;
    readonly string attemptsDirectory;
    readonly FileStream sessionLock;
    public FileAttemptRepository(string directory)
    {
        this.directory = Path.GetFullPath(directory); Directory.CreateDirectory(this.directory);
        try { sessionLock = new FileStream(Path.Combine(this.directory, "session.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException ex) { throw new IOException("Otra instancia está usando esta carpeta de progreso, o no se puede escribir en ella.", ex); }
        var versionFile = Path.Combine(this.directory, "schema-version.txt");
        try
        {
            var version = File.Exists(versionFile) ? File.ReadAllText(versionFile).Trim() :
                Directory.GetFiles(this.directory, "*.attempt.json").Length > 0 ? "1" : "2";
            if (version is not ("1" or "2"))
                throw new InvalidDataException("Versión de datos no compatible. Conserva Data y usa la versión de aplicación que la creó.");
            attemptsDirectory = Path.Combine(this.directory, "attempts-v2");
            if (version == "1") MigrateV1(versionFile);
            else if (!File.Exists(versionFile)) AtomicWrite(versionFile, "2");
            Directory.CreateDirectory(attemptsDirectory);
        }
        catch { sessionLock.Dispose(); throw; }
    }
    public List<Attempt> Load()
    {
        var result = new List<Attempt>();
        foreach (var file in Directory.GetFiles(attemptsDirectory, "*.attempt.json"))
        {
            try
            {
                var raw = File.ReadAllText(file);
                using var doc = JsonDocument.Parse(raw);
                if (new[] { "schemaVersion", "certification", "bankId", "profileId", "profileVersion", "blueprintVersion", "domains" }
                    .Any(key => !doc.RootElement.TryGetProperty(key, out _)))
                    throw new InvalidDataException("Faltan metadatos explícitos del intento v2.");
                if (!doc.RootElement.TryGetProperty("schemaVersion", out var schema) || schema.GetInt32() != 2 ||
                    !doc.RootElement.TryGetProperty("certification", out var cert) || cert.GetString() is not ("CCAR-P" or "CCAR-F"))
                    throw new InvalidDataException("Identidad o versión de intento no compatible.");
                var a = JsonSerializer.Deserialize<Attempt>(raw, JsonFormat.Options)
                    ?? throw new InvalidDataException("Intento vacío.");
                ValidateAttempt(a);
                if (Path.GetFileName(file) != $"{a.Id}.attempt.json") throw new InvalidDataException("ID y archivo no coinciden.");
                result.Add(a);
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException or NullReferenceException or InvalidOperationException or FormatException)
            { throw new InvalidDataException($"No se puede leer {Path.GetFileName(file)}. Se conserva sin modificar; hay una copia .bak del guardado anterior si existía.", ex); }
        }
        return result;
    }
    public void Save(Attempt a)
    {
        ValidateAttempt(a);
        var path = Path.Combine(attemptsDirectory, $"{a.Id}.attempt.json");
        AtomicWrite(path, JsonSerializer.Serialize(a, JsonFormat.Options), true);
    }
    static void ValidateAttempt(Attempt a)
    {
        if (a.SchemaVersion != 2 || a.Certification is not ("CCAR-P" or "CCAR-F") || a.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(a.BankId) || string.IsNullOrWhiteSpace(a.BankVersion) || string.IsNullOrWhiteSpace(a.ProfileId) ||
            string.IsNullOrWhiteSpace(a.ProfileVersion) || string.IsNullOrWhiteSpace(a.BlueprintVersion) ||
            !Enum.IsDefined(a.State) || !Enum.IsDefined(a.Mode) || a.Questions.Count == 0 || a.Domains.Count == 0 ||
            a.Position < 0 || a.Position >= a.Questions.Count || a.Questions.Select(q => q.Id).Distinct().Count() != a.Questions.Count ||
            a.Questions.Any(q => !a.Domains.Any(d => d.Id == q.DomainId) || !a.Responses.ContainsKey(q.Id) || a.Answer(q).Selected.Any(id => !q.Options.Any(o => o.Id == id))))
            throw new InvalidDataException("Intento incompleto o identidad incompatible.");
    }
    void MigrateV1(string versionFile)
    {
        // Stage every converted record before publishing a single atomic version marker.
        // Original v1 files remain untouched, including after a crash; repeat staging is idempotent.
        var converted = new List<Attempt>();
        foreach (var file in Directory.GetFiles(directory, "*.attempt.json"))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(file));
            var root = doc.RootElement;
            if (root.TryGetProperty("schemaVersion", out _) || !root.TryGetProperty("profile", out var profile) ||
                profile.GetString() != "CCAR-P · provisional v1" ||
                !root.TryGetProperty("bankVersion", out var bank) || bank.GetString() != "1.0.0")
                throw new InvalidDataException("No se reconoce la procedencia del intento v1. Datos originales conservados.");
            var a = JsonSerializer.Deserialize<Attempt>(root.GetRawText(), JsonFormat.Options)
                ?? throw new InvalidDataException("Intento v1 vacío.");
            a.Certification = "CCAR-P"; a.BankId = "ccarp-local";
            a.ProfileId = "ccarp-legacy"; a.ProfileVersion = "1";
            a.BlueprintVersion = "provisional-2026-10-21";
            a.Domains = a.Questions.Select(q => q.DomainId).Distinct().Select(id => new DomainInfo(id, id, 0)).ToList();
            ValidateAttempt(a);
            if (Path.GetFileName(file) != $"{a.Id}.attempt.json") throw new InvalidDataException("ID v1 no coincide con el archivo.");
            converted.Add(a);
        }
        var backup = Path.Combine(directory, "migration-v1-backup");
        var sources = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
            .Where(f => !Path.GetRelativePath(directory, f).StartsWith("migration-v1-backup" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                        !Path.GetRelativePath(directory, f).StartsWith("attempts-v2" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                        f != Path.Combine(directory, "session.lock")).ToList();
        foreach (var source in sources)
        {
            var dest = Path.Combine(backup, Path.GetRelativePath(directory, source));
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            if (!File.Exists(dest)) File.Copy(source, dest);
            if (!File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(dest)))
                throw new IOException("La copia de migración no coincide. Originales conservados.");
        }
        Directory.CreateDirectory(attemptsDirectory);
        foreach (var a in converted) AtomicWrite(Path.Combine(attemptsDirectory, $"{a.Id}.attempt.json"), JsonSerializer.Serialize(a, JsonFormat.Options));
        AtomicWrite(versionFile, "2");
    }
    static void AtomicWrite(string path, string value, bool keepBackup = false)
    {
        var temp = path + ".tmp";
        try
        {
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            { var bytes = System.Text.Encoding.UTF8.GetBytes(value); stream.Write(bytes); stream.Flush(true); }
            if (File.Exists(path)) File.Replace(temp, path, keepBackup ? path + ".bak" : null);
            else File.Move(temp, path);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    public void Dispose() => sessionLock.Dispose();
}
