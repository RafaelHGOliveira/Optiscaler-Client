using System.Text.Json;
using System.Text.RegularExpressions;
using OptiscalerClient.Models;

namespace OptiscalerClient.Services;

public class GameQuirksService
{
    private readonly Lazy<List<GameQuirksProfile>> _profiles;
    private readonly object _userFileLock = new();

    public GameQuirksService()
    {
        _profiles = new Lazy<List<GameQuirksProfile>>(LoadProfiles);
    }

    private List<GameQuirksProfile> LoadProfiles()
    {
        // 1. Carregar bundled JSON
        var bundledPath = Path.Combine(AppContext.BaseDirectory, "assets", "configs", "game-quirks.json");

        GameQuirksBundle? bundled = null;
        if (!File.Exists(bundledPath))
        {
            System.Diagnostics.Debug.WriteLine($"[GameQuirksService] Bundled game-quirks.json not found at: {bundledPath}");
            return new List<GameQuirksProfile>();
        }

        try
        {
            var json = File.ReadAllText(bundledPath);
            bundled = JsonSerializer.Deserialize(json, OptimizerContext.Default.GameQuirksBundle);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GameQuirksService] Failed to parse bundled game-quirks.json: {ex.Message}");
            return new List<GameQuirksProfile>();
        }

        if (bundled is null)
        {
            System.Diagnostics.Debug.WriteLine("[GameQuirksService] Bundled game-quirks.json deserialized as null.");
            return new List<GameQuirksProfile>();
        }

        // Indexar por AppId para facilitar merge
        var profileMap = new Dictionary<string, GameQuirksProfile>(StringComparer.Ordinal);
        foreach (var profile in bundled.Profiles)
        {
            if (!string.IsNullOrEmpty(profile.AppId))
                profileMap[profile.AppId] = profile;
        }

        // Lista final começa com todos os bundled (incluindo os sem AppId)
        var finalProfiles = new List<GameQuirksProfile>(bundled.Profiles);

        // 2. Merge com arquivo do usuário
        var userConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OptiscalerClient");
        var userFilePath = Path.Combine(userConfigDir, "game-quirks.user.json");

        lock (_userFileLock)
        {
            if (File.Exists(userFilePath))
            {
                try
                {
                    var userJson = File.ReadAllText(userFilePath);
                    var userBundle = JsonSerializer.Deserialize(userJson, OptimizerContext.Default.GameQuirksBundle);

                    if (userBundle is not null)
                    {
                        foreach (var userProfile in userBundle.Profiles)
                        {
                            if (string.IsNullOrEmpty(userProfile.AppId))
                            {
                                System.Diagnostics.Debug.WriteLine("[GameQuirksService] Skipping user profile with null/empty AppId.");
                                continue;
                            }

                            if (profileMap.TryGetValue(userProfile.AppId, out var existing))
                            {
                                // Substituir o profile bundled pelo do usuário
                                var idx = finalProfiles.IndexOf(existing);
                                if (idx >= 0)
                                    finalProfiles[idx] = userProfile;
                                profileMap[userProfile.AppId] = userProfile;
                            }
                            else
                            {
                                // AppId novo: adicionar
                                finalProfiles.Add(userProfile);
                                profileMap[userProfile.AppId] = userProfile;
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"[GameQuirksService] Merged user overrides from: {userFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GameQuirksService] Failed to parse user game-quirks file: {ex.Message}");
                }
            }
        }

        System.Diagnostics.Debug.WriteLine($"[GameQuirksService] Loaded {finalProfiles.Count} profiles.");
        return finalProfiles;
    }

    public GameQuirksProfile? TryGetQuirks(Game game)
    {
        var profiles = _profiles.Value;

        // 1. Match exato por AppId
        if (!string.IsNullOrEmpty(game.AppId))
        {
            var byId = profiles.FirstOrDefault(p => p.AppId == game.AppId);
            if (byId is not null)
                return byId;
        }

        // 2. Fallback: Regex sobre o nome
        foreach (var profile in profiles)
        {
            if (string.IsNullOrEmpty(profile.NameRegex))
                continue;

            try
            {
                if (Regex.IsMatch(game.Name, profile.NameRegex, RegexOptions.IgnoreCase))
                    return profile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameQuirksService] Invalid NameRegex '{profile.NameRegex}': {ex.Message}");
            }
        }

        return null;
    }
}
