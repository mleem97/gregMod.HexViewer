using System;
using System.Globalization;
using UnityEngine;

namespace GregModHexViewer;

internal sealed class HexPositionConfig
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

    public static HexPositionConfig CreateDefault() => new()
    {
        SpinnerOffsetX = 0f, SpinnerOffsetY = -6f,
        SpinnerFontMin = 1.8f, SpinnerFontMax = 6.2f, SpinnerFontScale = 0.24f,
        RackOffsetRight = -0.03f, RackOffsetBack = 0.06f, RackOffsetDown = -0.02f,
        RackFontSize = 42, RackCharacterSize = 0.05f, RackScale = 1f,
    };

    public string ToConfigText() => string.Join(Environment.NewLine, new[]
    {
        "# Hex Label Position Config",
        $"spinner_offset_x={SpinnerOffsetX.ToString(CultureInfo.InvariantCulture)}",
        $"spinner_offset_y={SpinnerOffsetY.ToString(CultureInfo.InvariantCulture)}",
        $"spinner_font_min={SpinnerFontMin.ToString(CultureInfo.InvariantCulture)}",
        $"spinner_font_max={SpinnerFontMax.ToString(CultureInfo.InvariantCulture)}",
        $"spinner_font_scale={SpinnerFontScale.ToString(CultureInfo.InvariantCulture)}",
        $"rack_offset_right={RackOffsetRight.ToString(CultureInfo.InvariantCulture)}",
        $"rack_offset_back={RackOffsetBack.ToString(CultureInfo.InvariantCulture)}",
        $"rack_offset_down={RackOffsetDown.ToString(CultureInfo.InvariantCulture)}",
        $"rack_font_size={RackFontSize}",
        $"rack_character_size={RackCharacterSize.ToString(CultureInfo.InvariantCulture)}",
        $"rack_scale={RackScale.ToString(CultureInfo.InvariantCulture)}",
    });

    public static void ApplyLine(HexPositionConfig config, string line)
    {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) return;
        var idx = line.IndexOf('=');
        if (idx <= 0) return;
        var key = line.Substring(0, idx).Trim();
        var val = line.Substring(idx + 1).Trim();
        if (!float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return;
        switch (key)
        {
            case "spinner_offset_x": config.SpinnerOffsetX = f; break;
            case "spinner_offset_y": config.SpinnerOffsetY = f; break;
            case "spinner_font_min": config.SpinnerFontMin = f; break;
            case "spinner_font_max": config.SpinnerFontMax = f; break;
            case "spinner_font_scale": config.SpinnerFontScale = f; break;
            case "rack_offset_right": config.RackOffsetRight = f; break;
            case "rack_offset_back": config.RackOffsetBack = f; break;
            case "rack_offset_down": config.RackOffsetDown = f; break;
            case "rack_font_size": config.RackFontSize = Mathf.RoundToInt(f); break;
            case "rack_character_size": config.RackCharacterSize = f; break;
            case "rack_scale": config.RackScale = f; break;
        }
    }
}
