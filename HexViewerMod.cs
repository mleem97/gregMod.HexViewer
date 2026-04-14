using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using greg.Core;
using greg.Sdk.Services;

[assembly: MelonInfo(typeof(greg.Mods.HexViewer.HexViewerMod), "gregMod.HexViewer", "1.0.0.30-pre", "MLeeM97 (teamGreg)")]
[assembly: MelonGame("Waseku", "Data Center")]
[assembly: MelonAdditionalDependencies("gregCore")]

namespace greg.Mods.HexViewer;

/// <summary>
/// Standalone HexViewer mod — IL2CPP object inspector and telemetry overlay.
/// Extracted from gregCore embedded code to run as an independent MelonLoader mod.
/// Hotkeys: F1 = UI Tree, F2 = Hook Monitor, F3 = Inspector
/// </summary>
public class HexViewerMod : greg.Core.Plugins.gregModBase
{
    public override string[] RequiredDependencies => new[] { "gregCore" };
    public static HexViewerMod Instance;

    public override void OnInitializeMod()
    {
        Instance = this;
        HexViewerUI.Init();

        // Register with gregCore's ModRegistry for cross-mod discovery
        GregModRegistry.Register("HexViewer", "1.0.0.30-pre");

        MelonLogger.Msg("⬡ HexViewer v1.0.0 loaded (standalone mod).");
    }

    public override void OnUpdateMod()
    {
        HexViewerUI.OnUpdate();
    }
}


