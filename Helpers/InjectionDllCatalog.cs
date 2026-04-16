namespace OptiscalerClient.Helpers;

public static class InjectionDllCatalog
{
    public static readonly string[] All =
    [
        "dxgi.dll", "winmm.dll", "d3d12.dll", "dbghelp.dll", "version.dll",
        "wininet.dll", "winhttp.dll", "OptiScaler.asi", "dlss-enabler.dll", "dinput8.dll"
    ];

    public static bool IsKnown(string name) =>
        Array.Exists(All, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
}
