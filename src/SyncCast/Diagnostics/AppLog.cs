namespace SyncCast.Diagnostics;

/// <summary>Best-effort local logs for Windows audio troubleshooting.</summary>
internal static class AppLog
{
    private static readonly object Gate = new();

    public static string LogPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SyncCast", "logs", "synccast.log");

    public static void Write(string message, Exception? exception = null)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath,
                    $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}" +
                    (exception is null ? "" : exception + Environment.NewLine));
            }
        }
        catch { /* logging must never interrupt sound playback */ }
    }
}
