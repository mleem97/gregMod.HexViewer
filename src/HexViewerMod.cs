using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(GregModHexViewer.HexViewerMod), "gregMod.HexViewer", "1.0.7", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace GregModHexViewer;

public sealed class HexViewerMod : MelonMod
{
    private bool _initialized;

    public override void OnInitializeMelon()
    {
        try
        {
            var cat = MelonPreferences.CreateCategory("HexViewer");
            var keyEntry = cat.CreateEntry("ToggleKey", "F2", "ToggleKey",
                "Hotkey to open/close the HexViewer panel.");
            HexviewerFeature.ConfigureToggleKey(keyEntry.Value);
        }
        catch { }
        HexviewerFeature.Initialize();
        HexviewerFeature.SetHudEnabled(true);
        MelonLogger.Msg($"[HexViewer] v1.0.7 loaded. {HexviewerFeature.ToggleKeyLabel} = HexViewer panel.");
        if (GregHost.HasCore)
        {
            try { RegisterCoreExtras(); } catch { }
        }
    }

    // Mod contract + key HUD + opener for F1 hub. Call only with gregCore
    // (own method for JIT split without gregCore DLL).
    private void RegisterCoreExtras()
    {
        try
        {
            gregCore.Core.Mods.GregModRegistry.Register(
                "gregMod.HexViewer", "HexViewer", "1.0.7",
                new string[] { "hexviewer" });
            gregCore.UI.GregHudRegistry.Register("hexviewer",
                HexviewerFeature.ToggleKeyLabel, "Hex");
            gregCore.UI.GregMenuBinding.BindToggle("hexviewer",
                HexviewerFeature.ToggleVisibility, () => HexviewerFeature.IsVisible);
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning("[HexViewer] Hub registration failed: " + ex.GetBaseException().Message);
        }
    }

    public override void OnGUI()
    {
        if (!_initialized) return;
        HexviewerFeature.OnGui();
    }

    public override void OnUpdate()
    {
        if (!_initialized)
        {
            TryInitialize();
            return;
        }

        HexviewerFeature.Update();
        HexviewerFeature.UpdateHud();
    }

    public override void OnDeinitializeMelon()
    {
        HexviewerFeature.Shutdown();
    }

    private void TryInitialize()
    {
        try
        {
            var networkMap = NetworkMap.instance;
            if (networkMap == null) return;

            _initialized = true;
            MelonLogger.Msg("[HexViewer] Initialized.");
        }
        catch
        {
        }
    }
}
