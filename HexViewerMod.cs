using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(greg.Mods.HexViewer.HexViewerMod), "gregMod.HexViewer", "2.0.0", "teamGreg")]
[assembly: MelonGame("Waseku", "Data Center")]
[assembly: MelonAdditionalDependencies("gregCore")]

namespace greg.Mods.HexViewer;

/// <summary>
/// Accessibility Layer for Data Center — Hardware Inspector HUD.
/// Displays unified object info, hex colors, and cable metadata via UI Toolkit.
/// </summary>
public class HexViewerMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        HexViewerConfig.Initialize();
        HexViewerHud.Initialize();
        MelonLogger.Msg("[HexViewer] v2.0.0 initialized — press F8 for config.");
    }

    public override void OnUpdate()
    {
        if (!HexViewerConfig.Enabled.Value) return;

        HexViewerTargeting.Update();

        if (HexViewerTargeting.HasTarget)
        {
            if (HexViewerTargeting.TargetChanged)
                HexViewerHud.Show(HexViewerTargeting.Current);
        }
        else
        {
            HexViewerHud.Hide();
        }
    }

    public override void OnDeinitializeMelon()
    {
        HexViewerHud.Shutdown();
    }
}
