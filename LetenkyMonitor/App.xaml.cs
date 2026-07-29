namespace LetenkyMonitor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppPaths.Ensure();

        var bundledBrowsers = Path.Combine(AppContext.BaseDirectory, "pw-browsers");
        Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", bundledBrowsers);

        Database.Initialize();
        base.OnStartup(e);
    }
}
