using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(GregModHexViewer.HexViewerMod), "gregMod.HexViewer", "1.0.6", "mleem97")]
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
        MelonLogger.Msg($"[HexViewer] v1.0.6 loaded. {HexviewerFeature.ToggleKeyLabel} = HexViewer panel.");
        if (GregHost.HasCore)
        {
            try { RegisterCoreExtras(); } catch { }
        }
    }

    // Mod-Vertrag + Tasten-HUD + Oeffner fuers F1-Hub. Nur mit gregCore
    // aufrufen (eigene Methode wegen JIT-Trennung ohne gregCore-DLL).
    private void RegisterCoreExtras()
    {
        try
        {
            gregCore.Core.Mods.GregModRegistry.Register(
                "gregMod.HexViewer", "HexViewer", "1.0.6",
                new string[] { "hexviewer" });
            gregCore.UI.GregHudRegistry.Register("hexviewer",
                HexviewerFeature.ToggleKeyLabel, "Hex");
            gregCore.UI.GregMenuRegistry.RegisterOpener("hexviewer",
                () => HexviewerFeature.ToggleVisibility());
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning("[HexViewer] Hub-Registrierung fehlgeschlagen: " + ex.GetBaseException().Message);
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
