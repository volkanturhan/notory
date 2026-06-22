using System.IO;

namespace notory.Services;

/// <summary>
/// Persists the scratchpad's text to a plain file under %APPDATA%\notory, so the
/// note survives restarts. Best-effort: a read/write failure simply yields/keeps
/// an empty note rather than throwing.
/// </summary>
public sealed class NoteStore
{
    private readonly string _filePath;

    public NoteStore()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "notory");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "note.txt");
    }

    /// <summary>Loads the saved note text, or empty if there is none.</summary>
    public string Load()
    {
        try
        {
            return File.Exists(_filePath) ? File.ReadAllText(_filePath) : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>Writes the note text to disk.</summary>
    public void Save(string text)
    {
        try
        {
            File.WriteAllText(_filePath, text);
        }
        catch
        {
            // Best-effort; losing a keystroke's worth of note is not worth crashing.
        }
    }
}
