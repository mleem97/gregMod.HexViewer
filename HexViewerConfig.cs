using MelonLoader;

namespace greg.Mods.HexViewer;

internal static class HexViewerConfig
{
    private const string Category = "HexViewer";

    internal static MelonPreferences_Entry<bool> Enabled = null!;
    internal static MelonPreferences_Entry<string> Mode = null!;
    internal static MelonPreferences_Entry<string> Anchor = null!;

    internal static void Initialize()
    {
        var pref = MelonPreferences.CreateCategory(Category);
        Enabled = pref.CreateEntry("Enabled", true, description: "Toggle HexViewer HUD on/off");
        Mode    = pref.CreateEntry("Mode", "Standard", description: "Standard = gameplay info only; Developer = includes registry names and coordinates");
        Anchor  = pref.CreateEntry("Anchor", "TopCenter", description: "HUD position: TopCenter, TopLeft, TopRight");
    }
}
