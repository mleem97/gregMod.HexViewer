using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(GregModHexViewer.HexViewerMod), "gregMod.HexViewer", "1.0.0", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace GregModHexViewer;

public sealed class HexViewerMod : MelonMod
{
    private const float LabelScanIntervalSeconds = 1.5f;
    private const float DeepScanIntervalSeconds = 10f;

    private static volatile HexPositionConfig _config = HexPositionConfig.CreateDefault();
    private HarmonyLib.Harmony _harmony;
    private float _scanTimer;
    private float _deepScanTimer;
    private float _configReloadTimer;
    private int _configReloadRunning;
    private CableSpinner[] _cachedSpinners = Array.Empty<CableSpinner>();
    private Rack[] _cachedRacks = Array.Empty<Rack>();
    private bool _isFullyInitialized;

    public override void OnInitializeMelon()
    {
        _config = HexPositionConfig.CreateDefault();
        HexviewerFeature.Initialize();
        HexviewerFeature.SetHudEnabled(true);
        MelonLogger.Msg("[HexViewer] v1.0.0 — F2 list, Ctrl+F1 config reload.");
    }

    public override void OnGUI()
    {
        if (!_isFullyInitialized) return;
        HexviewerFeature.OnGui();
    }

    public override void OnUpdate()
    {
        if (!_isFullyInitialized)
        {
            _scanTimer += Time.deltaTime;
            if (_scanTimer >= 2f)
            {
                _scanTimer = 0f;
                TryInitialize();
            }
            return;
        }

        HexviewerFeature.Update();
        HexviewerFeature.UpdateHud();

        _scanTimer += Time.deltaTime;
        _deepScanTimer += Time.deltaTime;

        if (_cachedSpinners.Length == 0 || _cachedRacks.Length == 0 || _deepScanTimer >= DeepScanIntervalSeconds)
        {
            _deepScanTimer = 0f;
            RefreshObjectCaches();
        }

        HandleLiveReloadToggleHotkey();

        if (_configReloadTimer >= 6f)
        {
            _configReloadTimer = 0f;
            _ = ReloadConfigAsync();
        }

        if (_scanTimer < LabelScanIntervalSeconds) return;
        _scanTimer = 0f;
        TryApplyToAllSpinners();
    }

    public override void OnDeinitializeMelon()
    {
        HexviewerFeature.Shutdown();
    }

    private void TryInitialize()
    {
        _config = LoadOrCreateConfig();
        RefreshObjectCaches();
        _harmony = new HarmonyLib.Harmony("greg.mods.hexviewer");
        _harmony.PatchAll(typeof(HexViewerMod).Assembly);
        _isFullyInitialized = true;
        MelonLogger.Msg("[HexViewer] Initialized.");
    }

    private void HandleLiveReloadToggleHotkey()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if ((kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed) && kb.f1Key.wasPressedThisFrame)
        {
            _configReloadTimer = 0f;
            _ = ReloadConfigAsync();
            MelonLogger.Msg("[HexViewer] Config reloaded.");
        }
    }

    private async Task ReloadConfigAsync()
    {
        if (Interlocked.Exchange(ref _configReloadRunning, 1) == 1) return;
        try
        {
            var updated = await Task.Run(LoadOrCreateConfig);
            if (updated != null) _config = updated;
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[HexViewer] Config reload failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _configReloadRunning, 0);
        }
    }

    private void TryApplyToAllSpinners()
    {
        try
        {
            for (var i = 0; i < _cachedSpinners.Length; i++)
                EnsureLabel(_cachedSpinners[i]);
            for (var i = 0; i < _cachedRacks.Length; i++)
                EnsureRackLabel(_cachedRacks[i]);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[HexViewer] Label scan failed: {ex.Message}");
        }
    }

    private void RefreshObjectCaches()
    {
        try
        {
            _cachedSpinners = Object.FindObjectsOfType<CableSpinner>();
            _cachedRacks = Object.FindObjectsOfType<Rack>();
        }
        catch
        {
            _cachedSpinners = Array.Empty<CableSpinner>();
            _cachedRacks = Array.Empty<Rack>();
        }
    }

    internal static void EnsureLabel(CableSpinner spinner)
    {
        if (spinner == null) return;
        var config = _config;
        if (!GameObjectColorHex.TryGetSpinnerHex(spinner, out var hex)) return;

        var sourceLabel = spinner.txtLength;
        if (sourceLabel == null) return;

        var parent = sourceLabel.transform.parent ?? spinner.transform;
        if (parent == null) return;

        Il2CppTMPro.TextMeshProUGUI label = null;
        var existing = parent.Find("HexLabel_White");
        if (existing != null) label = existing.GetComponent<Il2CppTMPro.TextMeshProUGUI>();
        if (label == null)
        {
            var clone = Object.Instantiate(sourceLabel.gameObject, parent);
            clone.name = "HexLabel_White";
            label = clone.GetComponent<Il2CppTMPro.TextMeshProUGUI>();
        }
        if (label == null) return;

        var targetFontMin = config.SpinnerFontMin;
        var targetFontMax = config.SpinnerFontMax;
        var targetFontSize = Mathf.Clamp(sourceLabel.fontSize * config.SpinnerFontScale, targetFontMin, targetFontMax);
        var targetPos = sourceLabel.rectTransform.anchoredPosition + new Vector2(config.SpinnerOffsetX, config.SpinnerOffsetY);

        if (string.Equals(label.text, hex, StringComparison.Ordinal)
            && Mathf.Approximately(label.fontSize, targetFontSize)
            && Vector2.Distance(label.rectTransform.anchoredPosition, targetPos) < 0.01f)
            return;

        label.color = Color.white;
        label.alpha = 1f;
        label.text = hex;
        label.enableAutoSizing = true;
        label.fontSizeMin = targetFontMin;
        label.fontSizeMax = targetFontMax;
        label.fontSize = targetFontSize;
        label.enableWordWrapping = false;
        label.alignment = Il2CppTMPro.TextAlignmentOptions.Center;
        label.rectTransform.anchoredPosition = targetPos;
    }

    internal static void EnsureRackLabel(Rack rack)
    {
        if (rack == null) return;
        var config = _config;
        if (!GameObjectColorHex.TryGetRackHex(rack, out var hex)) hex = "#FFFFFF";

        var root = rack.transform;
        if (root == null) return;

        TextMesh label;
        var existing = root.Find("RackHexLabel_White");
        if (existing != null)
            label = existing.GetComponent<TextMesh>();
        else
        {
            var go = new GameObject("RackHexLabel_White");
            go.transform.SetParent(root, true);
            label = go.AddComponent<TextMesh>();
        }
        if (label == null) return;

        var targetScale = Vector3.one * config.RackScale;
        var hasPos = TryGetRackBackRightBottomPosition(rack, out var worldPos, config);
        var targetRot = Quaternion.LookRotation(-rack.transform.forward, rack.transform.up);

        if (string.Equals(label.text, hex, StringComparison.Ordinal)
            && label.fontSize == config.RackFontSize
            && Mathf.Approximately(label.characterSize, config.RackCharacterSize))
            return;

        label.text = hex;
        label.color = Color.white;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = config.RackFontSize;
        label.characterSize = config.RackCharacterSize;

        if (!hasPos) return;
        label.transform.position = worldPos;
        label.transform.rotation = targetRot;
        label.transform.localScale = targetScale;
    }

    private static bool TryGetRackBackRightBottomPosition(Rack rack, out Vector3 pos, HexPositionConfig config)
    {
        pos = default;
        try
        {
            var renderers = rack.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Count == 0) return false;
            var hasBounds = false;
            Bounds bounds = default;
            for (var i = 0; i < renderers.Count; i++)
            {
                var r = renderers[i];
                if (r == null) continue;
                if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                else bounds.Encapsulate(r.bounds);
            }
            if (!hasBounds) return false;

            pos = bounds.center
                + rack.transform.right * (ProjectExtent(bounds.extents, rack.transform.right) + config.RackOffsetRight)
                + (-rack.transform.forward) * (ProjectExtent(bounds.extents, -rack.transform.forward) + config.RackOffsetBack)
                + (-rack.transform.up) * (ProjectExtent(bounds.extents, -rack.transform.up) + config.RackOffsetDown);
            return true;
        }
        catch { return false; }
    }

    private static float ProjectExtent(Vector3 extents, Vector3 axis)
    {
        var a = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
        return extents.x * a.x + extents.y * a.y + extents.z * a.z;
    }

    private static HexPositionConfig LoadOrCreateConfig()
    {
        var path = Path.Combine(MelonEnvironment.UserDataDirectory, "hexposition.cfg");
        var config = HexPositionConfig.CreateDefault();
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(path)) { File.WriteAllText(path, config.ToConfigText()); return config; }
            foreach (var line in File.ReadAllLines(path))
                HexPositionConfig.ApplyLine(config, line);
            return config;
        }
        catch { return config; }
    }
}

[HarmonyPatch(typeof(CableSpinner), nameof(CableSpinner.Start))]
internal static class CableSpinnerStartPatch
{
    private static void Postfix(CableSpinner __instance)
    {
        HexViewerMod.EnsureLabel(__instance);
    }
}
