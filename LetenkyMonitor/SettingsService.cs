namespace LetenkyMonitor;

public static class SettingsService
{
    public static AppSettings Load()
    {
        if (!File.Exists(AppPaths.SettingsPath))
        {
            var defaults = new AppSettings();
            Save(defaults);
            return defaults;
        }

        try
        {
            return JsonSerializer.Deserialize<AppSettings>(
                File.ReadAllText(AppPaths.SettingsPath)
            ) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        File.WriteAllText(
            AppPaths.SettingsPath,
            JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true })
        );
    }
}
