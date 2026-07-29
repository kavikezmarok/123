namespace LetenkyMonitor;

public static class AppPaths
{
    public static string BaseDir => AppContext.BaseDirectory;
    public static string DataDir => Path.Combine(BaseDir, "data");
    public static string LogsDir => Path.Combine(BaseDir, "logs");
    public static string DbPath => Path.Combine(DataDir, "letenky.db");
    public static string SettingsPath => Path.Combine(DataDir, "settings.json");

    public static void Ensure()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(LogsDir);
    }
}
