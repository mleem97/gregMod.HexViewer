using Il2Cpp;
using MelonLoader;
using System;

[assembly: MelonInfo(typeof(GregModHexViewer.HexViewerMod), "gregMod.HexViewer", "2.0.0", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace GregModHexViewer;

// gregMod.HexViewer 1.0.8+: hard dependency on gregCore (UI fully central).
// No standalone fallback, no GregHost probe — fail fast without the DLL.
public sealed class HexViewerMod : MelonMod
{
    private const string CoreProbeType = "gregCore.UI.GregNotificationManager, gregCore";
    private bool _initialized;
    private bool _disabled;

    public override void OnInitializeMelon()
    {
        bool hasCore = false;
        try { hasCore = Type.GetType(CoreProbeType) != null; } catch { }
        if (!hasCore)
        {
            LoggerInstance.Error("[HexViewer] gregCore not found — hard dependency, staying disabled. Put gregCore.dll in Mods/.");
            _disabled = true;
            return;
        }

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
        MelonLogger.Msg($"[HexViewer] v1.0.9 loaded (gregCore UI). {HexviewerFeature.ToggleKeyLabel} = HexViewer panel.");
        try { RegisterCoreExtras(); } catch { }
    }

    private void RegisterCoreExtras()
    {
        try
        {
            gregCore.Core.Mods.GregModRegistry.Register(
                "gregMod.HexViewer", "HexViewer", "2.0.0",
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

    public override void OnUpdate()
    {
        if (_disabled) return;
        if (!_initialized)
        {
            TryInitialize();
            return;
        }

        HexviewerFeature.Update();
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
