using System.Text.Json;
using AnthropicCert.Domain;

namespace AnthropicCert.Infrastructure;

public sealed class ExamEntry
{
    public string Certification { get; set; } = "";
    public string Name { get; set; } = "";
    public List<string> Aliases { get; set; } = [];
    public string BankPath { get; set; } = "";
    public string ProfileId { get; set; } = "";
}
public sealed class ExamCatalogDocument
{
    public int SchemaVersion { get; set; }
    public List<ExamEntry> Entries { get; set; } = [];
}
public sealed record AvailableExam(ExamEntry Entry, QuestionBank? Bank, string? Error)
{
    public override string ToString() => $"{Entry.Name} ({Entry.Certification})" + (Bank == null ? " · pendiente" : "");
}
public static class ExamCatalog
{
    public static IReadOnlyList<AvailableExam> Load(string contentDirectory)
    {
        var root = Path.GetFullPath(contentDirectory) + Path.DirectorySeparatorChar;
        var catalog = JsonSerializer.Deserialize<ExamCatalogDocument>(File.ReadAllText(Path.Combine(root, "exams.json")), JsonFormat.Options)
            ?? throw new InvalidDataException("Catálogo vacío.");
        var entries = catalog.Entries;
        if (catalog.SchemaVersion != 1 || entries.Count == 0 || entries.Select(e => e.Certification).Distinct().Count() != entries.Count ||
            entries.Any(e => e.Certification is not ("CCAR-P" or "CCAR-F") || string.IsNullOrWhiteSpace(e.Name) || string.IsNullOrWhiteSpace(e.ProfileId)))
            throw new InvalidDataException("Identidades o versión del catálogo no válidas.");
        var identities = entries.SelectMany(e => e.Aliases.Prepend(e.Certification)).ToList();
        if (identities.Any(string.IsNullOrWhiteSpace) || identities.Distinct(StringComparer.OrdinalIgnoreCase).Count() != identities.Count)
            throw new InvalidDataException("Alias de certificación duplicados o vacíos.");
        // Validate all paths before loading any bank. Reject traversal and reparse points.
        var paths = entries.Select(e =>
        {
            var path = Path.GetFullPath(Path.Combine(root, e.BankPath));
            if (string.IsNullOrWhiteSpace(e.BankPath) || Path.IsPathRooted(e.BankPath) || !path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("La ruta de banco debe permanecer dentro de Content.");
            for (var part = new FileInfo(path) as FileSystemInfo; part != null && !part.FullName.Equals(root.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
                 part = part is FileInfo file ? file.Directory : ((DirectoryInfo)part).Parent)
                if (part.Exists && part.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    throw new InvalidDataException("La ruta de banco no puede contener enlaces de sistema de archivos.");
            return path;
        }).ToArray();
        return entries.Select((entry, i) =>
        {
            try
            {
                var bank = JsonFormat.LoadBank(paths[i]);
                if (bank.Certification != entry.Certification || bank.ProfileId != entry.ProfileId)
                    throw new InvalidDataException("El banco no corresponde a la identidad/perfil del catálogo.");
                return new AvailableExam(entry, bank, null);
            }
            catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException or ArgumentException or NullReferenceException)
            { return new AvailableExam(entry, null, ex.Message); }
        }).ToList();
    }
}
