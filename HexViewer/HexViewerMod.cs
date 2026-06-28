using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(GregModHexViewer.HexViewerMod), "gregMod.HexViewer", "1.0.5", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace GregModHexViewer;

public sealed class HexViewerMod : MelonMod
{
    private bool _initialized;

    public override void OnInitializeMelon()
    {
        HexviewerFeature.Initialize();
        HexviewerFeature.SetHudEnabled(true);
        MelonLogger.Msg("[HexViewer] v1.0.5 loaded.");
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
