using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GregModHexViewer;

internal static class HexviewerFeature
{
    private static bool _visible;
    private static bool _hudEnabled;
    private static readonly List<CableColorEntry> _entries = new();
    private static string _heldLine = "Held: —";
    private static float _nextHudRefreshAt;
    private const float HudRefreshIntervalSeconds = 0.10f;

    // Readout toast throttle: only on color change, max 1 per 2 s.
    private static string _lastToastHex = "";
    private static float _lastToastAt;
    private const float ToastCooldownSeconds = 2f;

    // Swatch cache (hex -> 1x1 texture) for ShowRich covers, FIFO-capped.
    private static readonly Dictionary<string, Texture2D> _swatches = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Queue<string> _swatchOrder = new();
    private const int MaxSwatches = 24;

    private static Key _toggleKey = Key.F2;

    private static gregCore.UI.GregPanelBuilder _panel;

    public static string ToggleKeyLabel
    {
        get { try { return _toggleKey.ToString(); } catch { return "F2"; } }
    }

    public static bool IsVisible
    {
        get
        {
            try { if (_panel != null) return _panel.IsVisible; } catch { }
            return _visible;
        }
    }

    public static void ConfigureToggleKey(string raw)
    {
        try
        {
            if (Enum.TryParse<Key>(raw, true, out var k) && k != Key.None)
                _toggleKey = k;
            else
                MelonLogger.Warning($"[HexViewer] Unknown ToggleKey '{raw}', defaulting to F2.");
        }
        catch { }
    }

    public static void ToggleVisibility()
    {
        _visible = !_visible;
        if (_visible)
        {
            RefreshList();
            RebuildPanel();
            try { _panel?.Show(); } catch { }
        }
        else
        {
            try { _panel?.Hide(); } catch { }
        }
        try { ReportOpenState(); } catch { /* best-effort */ }
    }

    private static void ReportOpenState()
    {
        try { gregCore.UI.GregMenuRegistry.SetOpen("hexviewer", IsVisible); } catch { /* best-effort */ }
    }

    public static void Initialize() { }

    public static void Shutdown()
    {
        try { _panel?.Hide(); } catch { }
        _panel = null;
    }

    public static void SetHudEnabled(bool enabled)
    {
        _hudEnabled = enabled;
    }

    public static void UpdateHud()
    {
        if (!_hudEnabled) return;
        if (Time.unscaledTime < _nextHudRefreshAt) return;
        _nextHudRefreshAt = Time.unscaledTime + HudRefreshIntervalSeconds;

        string hex = null;
        string detail = "";
        try
        {
            if (HexTargetResolver.TryGetAimedColor(out var aimHex, out var aimDetailSuffix))
            {
                hex = aimHex;
                detail = aimDetailSuffix ?? "";
            }
            else if (HeldCableKindResolver.TryGetHeldItemHex(out var heldHex, out var heldKind))
            {
                hex = heldHex;
                detail = heldKind ?? "";
            }
        }
        catch { return; }

        if (string.IsNullOrEmpty(hex)) return;
        if (string.Equals(hex, _lastToastHex, StringComparison.OrdinalIgnoreCase)) return;
        if (Time.unscaledTime - _lastToastAt < ToastCooldownSeconds) return;
        _lastToastHex = hex;
        _lastToastAt = Time.unscaledTime;

        try
        {
            var cover = GetSwatch(hex);
            gregCore.UI.GregNotificationManager.ShowRich("HEXVIEWER", hex, detail, cover, null, 4f);
        }
        catch { }
    }

    private static Texture2D GetSwatch(string hex)
    {
        try
        {
            if (_swatches.TryGetValue(hex, out var cached) && cached != null) return cached;
            if (!HexColorUtil.TryHexToColor(hex, out var col)) return null;
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, col);
            t.Apply();
            UnityEngine.Object.DontDestroyOnLoad(t);
            _swatches[hex] = t;
            _swatchOrder.Enqueue(hex);
            while (_swatchOrder.Count > MaxSwatches)
            {
                var old = _swatchOrder.Dequeue();
                try
                {
                    if (_swatches.TryGetValue(old, out var ot) && ot != null)
                        UnityEngine.Object.Destroy(ot);
                    _swatches.Remove(old);
                }
                catch { }
            }
            return t;
        }
        catch { return null; }
    }

    public static void Update()
    {
        UpdateHud();

        var kb = Keyboard.current;
        if (kb == null) return;

        try
        {
            var ctrl = kb[_toggleKey];
            if (ctrl != null && ctrl.wasPressedThisFrame)
                ToggleVisibility();
        }
        catch { }
    }

    private static void RefreshList()
    {
        try
        {
            _entries.Clear();
            _entries.AddRange(CableColorCollector.CollectAll());
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[HexViewer] Refresh failed: {ex.GetBaseException().Message}");
            try { gregCore.UI.GregNotificationManager.Show("Hexviewer refresh failed.", gregCore.UI.GregNotificationManager.GregToastType.Error, 4f); } catch { }
            return;
        }
        UpdateHeldLine();
    }

    private static void RefreshFromButton()
    {
        RefreshList();
        RebuildPanel();
        try { gregCore.UI.GregNotificationManager.Show($"Hexviewer: {_entries.Count} colors listed.", 3f); } catch { }
    }

    private static void UpdateHeldLine()
    {
        var kind = HeldCableKindResolver.Resolve();
        HeldCableKindResolver.TryGetHeldCableHex(out var heldHex);

        if (string.IsNullOrEmpty(kind) && string.IsNullOrEmpty(heldHex))
            _heldLine = "Held: —";
        else if (!string.IsNullOrEmpty(kind) && !string.IsNullOrEmpty(heldHex))
            _heldLine = $"Held: {kind} — {heldHex}";
        else if (!string.IsNullOrEmpty(kind))
            _heldLine = $"Held: {kind}";
        else
            _heldLine = $"Held: {heldHex}";
    }

    private static void RebuildPanel()
    {
        try
        {
            if (_panel == null)
            {
                try { _panel = gregCore.UI.GregPanelBuilder.Create("Hexviewer").Build(); }
                catch { _panel = null; }
            }
            if (_panel == null) return;
            _panel.ClearContent();
            _panel.AddHeadline("Held");
            _panel.AddLabel(_heldLine);
            _panel.AddButton("Refresh", RefreshFromButton);
            _panel.AddSecondaryButton("Close", ToggleVisibility);
            _panel.AddSeparator();
            _panel.AddHeadline($"Scene colors ({_entries.Count})");
            foreach (var e in _entries)
            {
                _panel.AddLabel($"{e.Hex}  ·  {e.Source}");
            }
        }
        catch { }
    }
}
