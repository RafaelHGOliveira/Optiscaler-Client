namespace OptiscalerClient.Models
{
    public class GameQuirksProfile
    {
        public string? AppId { get; set; }
        public string? NameRegex { get; set; }
        public string? InjectionFileName { get; set; }
        public string? InstallSubPath { get; set; }
        public List<string> RequiredPrereqs { get; set; } = new();
        public List<string> RecommendedComponents { get; set; } = new();
        public string? RendererHint { get; set; }
        public Dictionary<string, Dictionary<string, string>> IniOverrides { get; set; } = new();
        public string? WikiUrl { get; set; }
        public string? Notes { get; set; }
        public bool Official { get; set; }
        public List<string> SafeToRemoveOnUninstall { get; set; } = new();
    }

    public class GameQuirksBundle
    {
        public int Version { get; set; }
        public List<GameQuirksProfile> Profiles { get; set; } = new();
    }
}
