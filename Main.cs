using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(FMF.HexLabelMod.HexLabelMelon), "FMF HexLabel Mod", "00.01.0010", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FMF.HexLabelMod;

public sealed class HexLabelMelon : MelonMod
{
    private const float LabelScanIntervalSeconds = 1.5f;
    private const float DeepScanIntervalSeconds = 10f;
    private const int SteamLogProbeLineLimit = 400;
    private const string AllowedSteamId64 = "76561198032682009";
    private const string LabelObjectName = "HexLabel_White";
    private const string RackLabelObjectName = "RackHexLabel_White";
    private static readonly Regex SteamIdRegex = new(
        @"(?<!\d)7656119\d{10}(?!\d)",
        RegexOptions.Compiled
    );
    private static volatile HexPositionConfig _config = HexPositionConfig.CreateDefault();

    private HarmonyLib.Harmony _harmony;
    private float _scanTimer;
    private float _deepScanTimer;
    private float _configReloadTimer;
    private float _steamPermissionRecheckTimer;
    private float _startupWaitTimer;
    private int _configReloadRunning;
    private CableSpinner[] _cachedSpinners = Array.Empty<CableSpinner>();
    private Rack[] _cachedRacks = Array.Empty<Rack>();
    private bool _isFullyInitialized;
    private bool _startupWaitMessageShown;
    private bool _liveReloadEnabled;
    private bool _liveReloadAllowed;

    public override void OnInitializeMelon()
    {
        _config = HexPositionConfig.CreateDefault();
        _cachedSpinners = Array.Empty<CableSpinner>();
        _cachedRacks = Array.Empty<Rack>();
        _isFullyInitialized = false;
        _liveReloadEnabled = false;
        _liveReloadAllowed = false;

        LoggerInstance.Msg("HexLabelMod waiting for Steam runtime initialization...");
        HexviewerFeature.Initialize();
    }

    public override void OnGUI()
    {
        HexviewerFeature.SetHudEnabled(_isFullyInitialized);
        HexviewerFeature.OnGui();
    }

    private void TryInitializeAfterSteamReady()
    {
        if (!IsSteamRuntimeReady())
        {
            if (!_startupWaitMessageShown)
            {
                LoggerInstance.Msg("HexLabelMod deferred: waiting for Steam runtime marker/SteamID in Latest.log.");
                _startupWaitMessageShown = true;
            }

            return;
        }

        _config = LoadOrCreateConfig();
        TryResolveLiveReloadPermission();
    RefreshObjectCaches();

        _harmony = new HarmonyLib.Harmony("de.mleem.hexlabelmod");
        _harmony.PatchAll(typeof(HexLabelMelon).Assembly);
        _isFullyInitialized = true;

        LoggerInstance.Msg($"HexLabelMod initialized. Config: {GetConfigPath()}");
        LoggerInstance.Msg("Hexviewer: HUD top-right; full list F2 (scene + save member_values / JSON).");

        if (_liveReloadAllowed)
        {
            LoggerInstance.Msg("Live reload available. Toggle with Ctrl+F1.");
        }
        else
        {
            LoggerInstance.Msg("POWERED BY FRIKADELLE MODDING FRAMEWORK - DONE CHECKING STEAMID FOR PERMISSION");
        }
    }

    public override void OnUpdate()
    {
        if (!_isFullyInitialized)
        {
            _startupWaitTimer += Time.deltaTime;
            if (_startupWaitTimer >= 1f)
            {
                _startupWaitTimer = 0f;
                TryInitializeAfterSteamReady();
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

        if (!_liveReloadAllowed)
        {
            _steamPermissionRecheckTimer += Time.deltaTime;
            if (_steamPermissionRecheckTimer >= 5f)
            {
                _steamPermissionRecheckTimer = 0f;
                TryResolveLiveReloadPermission();
            }
        }

        HandleLiveReloadToggleHotkey();

        if (_liveReloadAllowed && _liveReloadEnabled)
            _configReloadTimer += Time.deltaTime;

        if (_liveReloadAllowed && _liveReloadEnabled && _configReloadTimer >= 6f)
        {
            _configReloadTimer = 0f;
            _ = ReloadConfigAsync();
        }

        if (_scanTimer < LabelScanIntervalSeconds)
            return;

        _scanTimer = 0f;
        TryApplyToAllSpinners();
    }

    private void HandleLiveReloadToggleHotkey()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        var ctrlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        if (!ctrlPressed || !keyboard.f1Key.wasPressedThisFrame)
            return;

        if (!_liveReloadAllowed)
            TryResolveLiveReloadPermission();

        if (!_liveReloadAllowed)
        {
            LoggerInstance.Warning("Live reload is not allowed for this Steam account.");
            return;
        }

        _liveReloadEnabled = !_liveReloadEnabled;
        _configReloadTimer = 0f;

        LoggerInstance.Msg(_liveReloadEnabled
            ? "Live reload enabled (6s interval)."
            : "Live reload disabled.");
    }

    private async Task ReloadConfigAsync()
    {
        if (Interlocked.Exchange(ref _configReloadRunning, 1) == 1)
            return;

        try
        {
            var updated = await Task.Run(LoadOrCreateConfig);
            if (updated != null)
                _config = updated;
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Config live-reload failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _configReloadRunning, 0);
        }
    }

    internal static void EnsureLabel(CableSpinner spinner)
    {
        if (spinner == null)
            return;

        var config = _config;

        if (!GameObjectColorHex.TryGetSpinnerHex(spinner, out var hex))
            return;

        var sourceLabel = spinner.txtLength;
        if (sourceLabel == null)
            return;

        var parent = sourceLabel.transform.parent ?? spinner.transform;
        if (parent == null)
            return;

        TextMeshProUGUI label = null;
        var existing = parent.Find(LabelObjectName);
        if (existing != null)
            label = existing.GetComponent<TextMeshProUGUI>();

        if (label == null)
        {
            var clone = UnityEngine.Object.Instantiate(sourceLabel.gameObject, parent);
            clone.name = LabelObjectName;
            label = clone.GetComponent<TextMeshProUGUI>();
        }

        if (label == null)
            return;

        var targetFontMin = config.SpinnerFontMin;
        var targetFontMax = config.SpinnerFontMax;
        var targetFontSize = Mathf.Clamp(sourceLabel.fontSize * config.SpinnerFontScale, targetFontMin, targetFontMax);
        var targetAnchoredPosition = sourceLabel.rectTransform.anchoredPosition + new Vector2(config.SpinnerOffsetX, config.SpinnerOffsetY);

        var alreadyConfigured = string.Equals(label.text, hex, StringComparison.Ordinal)
            && Mathf.Approximately(label.fontSizeMin, targetFontMin)
            && Mathf.Approximately(label.fontSizeMax, targetFontMax)
            && Mathf.Approximately(label.fontSize, targetFontSize)
            && label.enableWordWrapping == false
            && label.enableAutoSizing
            && label.alignment == TextAlignmentOptions.Center
            && label.color == Color.white
            && Mathf.Approximately(label.alpha, 1f)
            && Vector2.Distance(label.rectTransform.anchoredPosition, targetAnchoredPosition) < 0.01f;

        if (alreadyConfigured)
            return;

        label.color = Color.white;
        label.alpha = 1f;
        label.text = hex;
        label.enableAutoSizing = true;
        label.fontSizeMin = targetFontMin;
        label.fontSizeMax = targetFontMax;
        label.fontSize = targetFontSize;
        label.enableWordWrapping = false;
        label.alignment = TextAlignmentOptions.Center;

        var rt = label.rectTransform;
        if (rt != null)
            rt.anchoredPosition = targetAnchoredPosition;
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
            LoggerInstance.Warning($"HexLabel scan failed: {ex.Message}");
        }
    }

    private void RefreshObjectCaches()
    {
        try
        {
            _cachedSpinners = UnityEngine.Object.FindObjectsOfType<CableSpinner>();
            _cachedRacks = UnityEngine.Object.FindObjectsOfType<Rack>();
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"HexLabel deep scan failed: {ex.Message}");
            _cachedSpinners = Array.Empty<CableSpinner>();
            _cachedRacks = Array.Empty<Rack>();
        }
    }

    internal static void EnsureRackLabel(Rack rack)
    {
        if (rack == null)
            return;

        var config = _config;

        if (!GameObjectColorHex.TryGetRackHex(rack, out var hex))
            hex = "#FFFFFF";

        var root = rack.transform;
        if (root == null)
            return;

        var existing = root.Find(RackLabelObjectName);
        TextMesh label;

        if (existing != null)
        {
            label = existing.GetComponent<TextMesh>();
        }
        else
        {
            var go = new GameObject(RackLabelObjectName);
            go.transform.SetParent(root, true);
            label = go.AddComponent<TextMesh>();
        }

        if (label == null)
            return;

        var targetScale = Vector3.one * config.RackScale;
        var hasWorldPosition = TryGetRackBackRightBottomPosition(rack, out var worldPos, config);
        var targetRotation = Quaternion.LookRotation(-rack.transform.forward, rack.transform.up);

        var alreadyConfigured = string.Equals(label.text, hex, StringComparison.Ordinal)
            && label.color == Color.white
            && label.anchor == TextAnchor.MiddleCenter
            && label.alignment == TextAlignment.Center
            && label.fontSize == config.RackFontSize
            && Mathf.Approximately(label.characterSize, config.RackCharacterSize)
            && (!hasWorldPosition || Vector3.Distance(label.transform.position, worldPos) < 0.001f)
            && Quaternion.Angle(label.transform.rotation, targetRotation) < 0.01f
            && Vector3.Distance(label.transform.localScale, targetScale) < 0.0001f;

        if (alreadyConfigured)
            return;

        label.text = hex;
        label.color = Color.white;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = config.RackFontSize;
        label.characterSize = config.RackCharacterSize;

        if (!hasWorldPosition)
            return;

        var t = label.transform;
        t.position = worldPos;
        t.rotation = targetRotation;
        t.localScale = targetScale;
    }

    private static bool TryGetRackBackRightBottomPosition(Rack rack, out Vector3 pos, HexPositionConfig config)
    {
        pos = default;

        try
        {
            var renderers = rack.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Count == 0)
                return false;

            var hasBounds = false;
            var bounds = new Bounds();

            for (var i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
                return false;

            var rightExtent = ProjectExtent(bounds.extents, rack.transform.right);
            var backExtent = ProjectExtent(bounds.extents, -rack.transform.forward);
            var downExtent = ProjectExtent(bounds.extents, -rack.transform.up);

            pos = bounds.center
                + rack.transform.right * (rightExtent + config.RackOffsetRight)
                + (-rack.transform.forward) * (backExtent + config.RackOffsetBack)
                + (-rack.transform.up) * (downExtent + config.RackOffsetDown);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static float ProjectExtent(Vector3 extents, Vector3 axis)
    {
        var a = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
        return extents.x * a.x + extents.y * a.y + extents.z * a.z;
    }

    private static HexPositionConfig LoadOrCreateConfig()
    {
        var path = GetConfigPath();
        var config = HexPositionConfig.CreateDefault();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, config.ToConfigText());
                return config;
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var key = ApplyConfigLine(config, line);
                if (!string.IsNullOrWhiteSpace(key))
                    seenKeys.Add(key);
            }

            if (seenKeys.Count < HexPositionConfig.ExpectedKeys.Length)
                File.WriteAllText(path, config.ToConfigText());

            return config;
        }
        catch
        {
            return config;
        }
    }

    private static string ApplyConfigLine(HexPositionConfig config, string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var trimmed = line.Trim();
        if (trimmed.StartsWith("#", StringComparison.Ordinal))
            return null;

        var splitIndex = trimmed.IndexOf('=');
        if (splitIndex <= 0 || splitIndex >= trimmed.Length - 1)
            return null;

        var key = trimmed.Substring(0, splitIndex).Trim();
        var value = trimmed.Substring(splitIndex + 1).Trim();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            return null;

        switch (key)
        {
            case "spinner_offset_x":
                config.SpinnerOffsetX = parsed;
                return key;
            case "spinner_offset_y":
                config.SpinnerOffsetY = parsed;
                return key;
            case "spinner_font_min":
                config.SpinnerFontMin = parsed;
                return key;
            case "spinner_font_max":
                config.SpinnerFontMax = parsed;
                return key;
            case "spinner_font_scale":
                config.SpinnerFontScale = parsed;
                return key;
            case "rack_offset_right":
                config.RackOffsetRight = parsed;
                return key;
            case "rack_offset_back":
                config.RackOffsetBack = parsed;
                return key;
            case "rack_offset_down":
                config.RackOffsetDown = parsed;
                return key;
            case "rack_font_size":
                config.RackFontSize = Mathf.RoundToInt(parsed);
                return key;
            case "rack_character_size":
                config.RackCharacterSize = parsed;
                return key;
            case "rack_scale":
                config.RackScale = parsed;
                return key;
            default:
                return null;
        }
    }

    private static string GetConfigPath()
    {
        return Path.Combine(MelonEnvironment.UserDataDirectory, "hexposition.cfg");
    }

    private static string TryGetSteamId64FromMelonLoader()
    {
        try
        {
            var logPath = Path.Combine(MelonEnvironment.GameRootDirectory, "MelonLoader", "Latest.log");
            if (!File.Exists(logPath))
                return null;

            using var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);

            var lineCount = 0;
            while (!reader.EndOfStream && lineCount++ < SteamLogProbeLineLimit)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var match = SteamIdRegex.Match(line);
                if (match.Success)
                    return match.Value;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSteamRuntimeReady()
    {
        try
        {
            var logPath = Path.Combine(MelonEnvironment.GameRootDirectory, "MelonLoader", "Latest.log");
            if (!File.Exists(logPath))
                return false;

            using var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);

            var lineCount = 0;
            while (!reader.EndOfStream && lineCount++ < SteamLogProbeLineLimit)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var hasSteamMarker = line.Contains("SteamInternal_SetMinidumpSteamID:", StringComparison.Ordinal)
                    || line.Contains("Caching Steam ID:", StringComparison.Ordinal);

                if (hasSteamMarker || SteamIdRegex.IsMatch(line))
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private void TryResolveLiveReloadPermission()
    {
        var detectedSteamId = TryGetSteamId64FromMelonLoader()?.Trim();
        if (string.IsNullOrWhiteSpace(detectedSteamId))
            return;

        if (string.Equals(detectedSteamId, AllowedSteamId64, StringComparison.Ordinal))
        {
            if (!_liveReloadAllowed)
                LoggerInstance.Msg("Live reload permission granted for this Steam account.");

            _liveReloadAllowed = true;
        }
    }
    private sealed class HexPositionConfig
    {
        public float SpinnerOffsetX;
        public float SpinnerOffsetY;
        public float SpinnerFontMin;
        public float SpinnerFontMax;
        public float SpinnerFontScale;

        public float RackOffsetRight;
        public float RackOffsetBack;
        public float RackOffsetDown;
        public int RackFontSize;
        public float RackCharacterSize;
        public float RackScale;

        public static readonly string[] ExpectedKeys =
        {
            "spinner_offset_x",
            "spinner_offset_y",
            "spinner_font_min",
            "spinner_font_max",
            "spinner_font_scale",
            "rack_offset_right",
            "rack_offset_back",
            "rack_offset_down",
            "rack_font_size",
            "rack_character_size",
            "rack_scale",
        };

        public static HexPositionConfig CreateDefault()
        {
            return new HexPositionConfig
            {
                SpinnerOffsetX = 0f,
                SpinnerOffsetY = -6f,
                SpinnerFontMin = 1.8f,
                SpinnerFontMax = 6.2f,
                SpinnerFontScale = 0.24f,

                RackOffsetRight = -0.03f,
                RackOffsetBack = 0.06f,
                RackOffsetDown = -0.02f,
                RackFontSize = 42,
                RackCharacterSize = 0.05f,
                RackScale = 1f,
            };
        }

        public string ToConfigText()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "# Hex Label Position Config",
                "# File: UserData/hexposition.cfg",
                "# Edit values, then restart game.",
                "",
                "# Spinner (UI text near cable spool)",
                $"spinner_offset_x={SpinnerOffsetX.ToString(CultureInfo.InvariantCulture)}",
                $"spinner_offset_y={SpinnerOffsetY.ToString(CultureInfo.InvariantCulture)}",
                $"spinner_font_min={SpinnerFontMin.ToString(CultureInfo.InvariantCulture)}",
                $"spinner_font_max={SpinnerFontMax.ToString(CultureInfo.InvariantCulture)}",
                $"spinner_font_scale={SpinnerFontScale.ToString(CultureInfo.InvariantCulture)}",
                "",
                "# Rack (world-space text at rack back-right-bottom)",
                $"rack_offset_right={RackOffsetRight.ToString(CultureInfo.InvariantCulture)}",
                $"rack_offset_back={RackOffsetBack.ToString(CultureInfo.InvariantCulture)}",
                $"rack_offset_down={RackOffsetDown.ToString(CultureInfo.InvariantCulture)}",
                $"rack_font_size={RackFontSize.ToString(CultureInfo.InvariantCulture)}",
                $"rack_character_size={RackCharacterSize.ToString(CultureInfo.InvariantCulture)}",
                $"rack_scale={RackScale.ToString(CultureInfo.InvariantCulture)}",
            });
        }
    }
}

[HarmonyPatch(typeof(CableSpinner), nameof(CableSpinner.Start))]
internal static class CableSpinnerStartPatch
{
    private static void Postfix(CableSpinner __instance)
    {
        HexLabelMelon.EnsureLabel(__instance);
    }
}
