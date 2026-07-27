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
    private static bool _hudHasTarget;
    private static Vector2 _scroll;
    private static readonly List<CableColorEntry> _entries = new();
    private static string _heldLine = "Held: —";
    private static string _hudLine = "—";
    private static string _hudDetail = "";

    private static Texture2D _texBg;
    private static Texture2D _texBorder;
    private static Texture2D _texWhite;

    private static readonly Color ColBg = new(10f / 255f, 12f / 255f, 16f / 255f, 1f);
    private static readonly Color ColBorder = new(30f / 255f, 36f / 255f, 46f / 255f, 1f);
    private static readonly Color ColTitle = new(80f / 255f, 220f / 255f, 210f / 255f, 1f);
    private static readonly Color ColMuted = new(154f / 255f, 164f / 255f, 178f / 255f, 1f);
    private static readonly Color ColPortTag = new(0f / 255f, 133f / 255f, 120f / 255f, 1f);

    public static void Initialize()
    {
        EnsureTextures();
    }

    public static void Shutdown() { }

    public static void SetHudEnabled(bool enabled)
    {
        _hudEnabled = enabled;
    }

    public static void UpdateHud()
    {
        if (!_hudEnabled) return;
        RefreshHudLine();
    }

    private static void EnsureTextures()
    {
        if (_texBg != null) return;
        _texBg = MakeTexture(ColBg);
        _texBorder = MakeTexture(ColBorder);
        _texWhite = MakeTexture(Color.white);
    }

    private static Texture2D MakeTexture(Color c)
    {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.SetPixel(0, 0, c);
        t.Apply();
        UnityEngine.Object.DontDestroyOnLoad(t);
        return t;
    }

    private static void RefreshHudLine()
    {
        try
        {
            if (HexTargetResolver.TryGetAimedColor(out var aimHex, out var aimDetailSuffix))
            {
                _hudLine = aimHex;
                _hudDetail = aimDetailSuffix ?? "";
                _hudHasTarget = true;
                UpdateHeldLine();
                return;
            }

            if (HeldCableKindResolver.TryGetHeldItemHex(out var heldHex, out var heldKind))
            {
                _hudLine = heldHex;
                _hudDetail = heldKind ?? "";
                _hudHasTarget = true;
                UpdateHeldLine();
                return;
            }

            _hudLine = "—";
            _hudDetail = "";
            _hudHasTarget = false;
            UpdateHeldLine();
        }
        catch (Exception ex)
        {
            _hudLine = "?";
            _hudDetail = ex.Message;
            _hudHasTarget = true;
        }
    }

    public static void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.f2Key.wasPressedThisFrame)
        {
            _visible = !_visible;
            if (_visible) RefreshList();
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
        if (_hudEnabled && _hudHasTarget)
            DrawHud();

        if (!_visible) return;

        if (Time.frameCount % 30 == 0) UpdateHeldLine();

        DrawColorList();
    }

    private static void DrawHud()
    {
        EnsureTextures();

        const float margin = 10f;
        const float width = 340f;
        const float swatchSize = 20f;

        var hasPortTag = TryExtractPortTag(_hudDetail, out _);
        var hexLineH = 26f;
        var tagLineH = hasPortTag ? 22f : 0f;
        var h = 12f + 16f + 6f + hexLineH + 4f + 14f + (hasPortTag ? 6f + tagLineH : 0f) + 10f;

        var x = Screen.width - width - margin;
        var y = margin;

        var bgRect = new Rect(x, y, width, h);
        GUI.DrawTexture(bgRect, _texBg);
        DrawBorder(bgRect, _texBorder);

        var pad = 10f;
        var textW = width - pad * 2;

        var titleStyle = new GUIStyle
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperRight,
            normal = { textColor = ColTitle }
        };

        var hexStyle = new GUIStyle
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.white }
        };

        var detailStyle = new GUIStyle
        {
            fontSize = 11,
            alignment = TextAnchor.LowerRight,
            normal = { textColor = ColMuted }
        };

        var portTagStyle = new GUIStyle
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = ColPortTag }
        };

        var line = string.IsNullOrEmpty(_hudLine) ? "—" : _hudLine;

        GUI.Label(new Rect(x + pad, y + 8, textW, 16), "Hexviewer", titleStyle);

        var hexY = y + 26f;
        GUI.Label(new Rect(x + pad, hexY, textW - swatchSize - 6f, hexLineH), line, hexStyle);

        if (HexColorUtil.TryHexToColor(_hudLine, out var swatchCol))
        {
            var swatchX = x + width - pad - swatchSize;
            var swatchY = hexY + (hexLineH - swatchSize) * 0.5f;
            var last = GUI.color;
            GUI.color = swatchCol;
            GUI.DrawTexture(new Rect(swatchX, swatchY, swatchSize, swatchSize), _texWhite);
            GUI.color = last;
            DrawBorder(new Rect(swatchX, swatchY, swatchSize, swatchSize), _texBorder);
        }

        var detailY = hexY + hexLineH + 2f;
        GUI.Label(new Rect(x + pad, detailY, textW, 14), _hudDetail, detailStyle);

        if (hasPortTag)
        {
            var tagY = detailY + 16f;
            GUI.Label(new Rect(x + pad, tagY, textW, tagLineH), ExtractPortTag(_hudDetail), portTagStyle);
        }
    }

    private static void DrawColorList()
    {
        const float w = 560f;
        const float h = 420f;
        var x = (Screen.width - w) * 0.5f;
        var y = (Screen.height - h) * 0.5f;

        GUI.Box(new Rect(x, y, w, h), "Hexviewer (F2)");
        GUILayout.BeginArea(new Rect(x + 10, y + 28, w - 20, h - 38));

        GUILayout.Label("Colors from scene (CableSpinner), Save.member_values, and save JSON files.", GUI.skin.box);

        var heldStyle = new GUIStyle
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        GUILayout.Label(_heldLine, heldStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(120))) RefreshList();
        if (GUILayout.Button("Close", GUILayout.Width(120))) _visible = false;
        GUILayout.EndHorizontal();

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(220));
        foreach (var e in _entries)
        {
            GUILayout.BeginHorizontal();

            var last = GUI.color;
            if (HexColorUtil.TryHexToColor(e.Hex, out var col))
                GUI.color = col;
            GUILayout.Box("", GUILayout.Width(22), GUILayout.Height(18));
            GUI.color = last;

            GUILayout.Label(e.Hex, GUILayout.Width(100));
            GUILayout.Label(e.Source, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private static void DrawBorder(Rect r, Texture2D tex)
    {
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, 1), tex);
        GUI.DrawTexture(new Rect(r.x, r.yMax - 1, r.width, 1), tex);
        GUI.DrawTexture(new Rect(r.x, r.y, 1, r.height), tex);
        GUI.DrawTexture(new Rect(r.xMax - 1, r.y, 1, r.height), tex);
    }

    private static bool TryExtractPortTag(string detail, out string port)
    {
        port = null;
        if (string.IsNullOrEmpty(detail)) return false;
        var idx = detail.IndexOf("·", StringComparison.Ordinal);
        if (idx < 0) return false;
        var after = detail.Substring(idx + 1).Trim();
        if (after.Equals("RJ", StringComparison.OrdinalIgnoreCase)
            || after.Equals("SFP", StringComparison.OrdinalIgnoreCase)
            || after.Equals("QSFP", StringComparison.OrdinalIgnoreCase))
        {
            port = after.ToUpperInvariant();
            return true;
        }
        return false;
    }

    private static string ExtractPortTag(string detail)
    {
        return TryExtractPortTag(detail, out var port) ? port : "";
    }
}
