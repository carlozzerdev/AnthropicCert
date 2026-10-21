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
        var bank = JsonSerializer.Deserialize<QuestionBank>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException("Banco vacío.");
        bank.Validate(); return bank;
    }
}
// One atomic JSON document per attempt: no external database or runtime dependency.
public sealed class FileAttemptRepository : IAttemptRepository, IDisposable
{
    readonly string directory;
    readonly FileStream sessionLock;
    public FileAttemptRepository(string directory)
    {
        this.directory = Path.GetFullPath(directory); Directory.CreateDirectory(this.directory);
        try { sessionLock = new FileStream(Path.Combine(this.directory, "session.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException ex) { throw new IOException("Otra instancia está usando esta carpeta de progreso, o no se puede escribir en ella.", ex); }
        var versionFile = Path.Combine(this.directory, "schema-version.txt");
        try
        {
            if (File.Exists(versionFile) && File.ReadAllText(versionFile).Trim() != "1")
                throw new InvalidDataException("Versión de datos no compatible. Conserva Data y usa la versión de aplicación que la creó.");
            if (!File.Exists(versionFile)) File.WriteAllText(versionFile, "1");
        }
        catch { sessionLock.Dispose(); throw; }
    }
    public List<Attempt> Load()
    {
        var result = new List<Attempt>();
        foreach (var file in Directory.GetFiles(directory, "*.attempt.json"))
        {
            try
            {
                var a = JsonSerializer.Deserialize<Attempt>(File.ReadAllText(file), JsonFormat.Options)
                    ?? throw new InvalidDataException("Intento vacío.");
                if (a.Questions.Count == 0 || a.Questions.Any(q => !a.Responses.ContainsKey(q.Id)))
                    throw new InvalidDataException("Intento incompleto.");
                result.Add(a);
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException)
            { throw new InvalidDataException($"No se puede leer {Path.GetFileName(file)}. Se conserva sin modificar; hay una copia .bak del guardado anterior si existía.", ex); }
        }
        return result;
    }
    public void Save(Attempt a)
    {
        var path = Path.Combine(directory, $"{a.Id}.attempt.json"); var temp = path + ".tmp";
        try
        {
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            { JsonSerializer.Serialize(stream, a, JsonFormat.Options); stream.Flush(true); }
            if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
            else File.Move(temp, path);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    public void Dispose() => sessionLock.Dispose();
}
