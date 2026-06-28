using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FMF.HexLabelMod;

internal static class HexviewerFeature
{
    private static bool _visible;
    private static bool _hudEnabled;
    private static Vector2 _scroll;
    private static readonly List<CableColorEntry> _entries = new();
    private static bool _colorblindMode;
    private static string _heldLine = "Held: —";
    private static string _hudLine = "—";
    private static string _hudDetail = "";

    public static void Initialize()
    {
    }

    public static void SetHudEnabled(bool enabled)
    {
        _hudEnabled = enabled;
    }

    public static void UpdateHud()
    {
        if (!_hudEnabled)
            return;

        RefreshHudLine();
    }

    private static void RefreshHudLine()
    {
        try
        {
            if (HexTargetResolver.TryGetAimedColor(out var aimHex, out var aimDetailSuffix))
            {
                _hudLine = aimHex;
                _hudDetail = $"Anvisiert · {aimDetailSuffix}";
                UpdateHeldLine();
                return;
            }

            if (HeldCableKindResolver.TryGetHeldItemHex(out var heldHex, out var heldKind))
            {
                _hudLine = heldHex;
                _hudDetail = $"In der Hand · {heldKind}";
                UpdateHeldLine();
                return;
            }

            _hudLine = "—";
            _hudDetail = "";
            UpdateHeldLine();
        }
        catch (Exception ex)
        {
            _hudLine = "?";
            _hudDetail = ex.Message;
        }
    }

    public static void Update()
    {
        var kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb.f2Key.wasPressedThisFrame)
        {
            _visible = !_visible;
            if (_visible)
                RefreshList();
        }
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
            MelonLogger.Msg($"Hexviewer: {ex.Message}");
        }

        UpdateHeldLine();
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

    public static void OnGui()
    {
        if (_hudEnabled)
            DrawHud();

        if (!_visible)
            return;

        if (Time.frameCount % 30 == 0)
            UpdateHeldLine();

        const float w = 560f;
        const float h = 420f;
        var x = (Screen.width - w) * 0.5f;
        var y = (Screen.height - h) * 0.5f;

        GUI.Box(new Rect(x, y, w, h), "Hexviewer (F2)");
        GUILayout.BeginArea(new Rect(x + 10, y + 28, w - 20, h - 38));

        GUILayout.Label("Colors from scene (CableSpinner), Save.member_values, and save JSON files.", GUI.skin.box);

        _colorblindMode = GUILayout.Toggle(_colorblindMode,
            "Colorblind: show RJ / SFP / QSFP + hex for held cable");

        var heldStyle = new GUIStyle
        {
            fontSize = _colorblindMode ? 22 : 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        GUILayout.Label(_heldLine, heldStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(120)))
            RefreshList();
        if (GUILayout.Button("Close", GUILayout.Width(120)))
            _visible = false;
        GUILayout.EndHorizontal();

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(220));
        foreach (var e in _entries)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(28), GUILayout.Height(22));
            var last = GUI.color;
            if (HexColorUtil.TryHexToColor(e.Hex, out var col))
                GUI.color = col;
            GUILayout.Box("", GUILayout.Width(28), GUILayout.Height(22));
            GUI.color = last;

            GUILayout.Label(e.Hex, GUILayout.Width(100));
            GUILayout.Label(e.Source, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private static void DrawHud()
    {
        const float margin = 10f;
        const float width = 320f;
        var hexLineH = _colorblindMode ? 32f : 26f;
        var h = 8f + 18f + hexLineH + 18f;

        var x = Screen.width - width - margin;
        var y = margin;

        GUI.Box(new Rect(x, y, width, h), "");

        var titleStyle = new GUIStyle
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperRight,
            normal = { textColor = new Color(0.95f, 0.98f, 1f, 1f) }
        };

        var hexStyle = new GUIStyle
        {
            fontSize = _colorblindMode ? 22 : 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = HexColorUtil.TryHexToColor(_hudLine, out var swatch) ? swatch : Color.white }
        };

        var detailStyle = new GUIStyle
        {
            fontSize = 11,
            alignment = TextAnchor.LowerRight,
            normal = { textColor = new Color(0.85f, 0.9f, 0.95f, 1f) }
        };

        var line = string.IsNullOrEmpty(_hudLine) ? "—" : _hudLine;
        GUI.Label(new Rect(x + 8, y + 6, width - 16, 18), "Hexviewer", titleStyle);
        GUI.Label(new Rect(x + 8, y + 26, width - 16, hexLineH), line, hexStyle);
        GUI.Label(new Rect(x + 8, y + h - 20, width - 16, 16), _hudDetail, detailStyle);
    }
}
