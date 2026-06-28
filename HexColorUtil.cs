using System;
using System.Globalization;
using UnityEngine;

namespace FMF.HexLabelMod;

internal static class HexColorUtil
{
    public static string ToHex(Color c)
    {
        var r = Mathf.Clamp(Mathf.RoundToInt(c.r * 255f), 0, 255);
        var g = Mathf.Clamp(Mathf.RoundToInt(c.g * 255f), 0, 255);
        var b = Mathf.Clamp(Mathf.RoundToInt(c.b * 255f), 0, 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static bool TryNormalizeHex(string raw, out string hex)
    {
        hex = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var s = raw.Trim();

        if (s.StartsWith("#", StringComparison.Ordinal))
        {
            var h = s.ToUpperInvariant();
            if (h.Length == 7)
            {
                hex = h;
                return true;
            }

            if (h.Length == 9)
            {
                hex = "#" + h.Substring(3, 6);
                return true;
            }
        }

        if (ColorUtility.TryParseHtmlString(s, out var colorFromHtml))
        {
            hex = ToHex(colorFromHtml);
            return true;
        }

        var parts = s.Split(',');
        if (parts.Length == 3
            && TryParseColorPart(parts[0], out var r)
            && TryParseColorPart(parts[1], out var g)
            && TryParseColorPart(parts[2], out var b))
        {
            hex = $"#{r:X2}{g:X2}{b:X2}";
            return true;
        }

        return false;
    }

    private static bool TryParseColorPart(string value, out int parsed)
    {
        parsed = 0;
        var token = value.Trim();
        if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
        {
            if (f <= 1f)
            {
                parsed = Mathf.Clamp(Mathf.RoundToInt(f * 255f), 0, 255);
                return true;
            }

            parsed = Mathf.Clamp(Mathf.RoundToInt(f), 0, 255);
            return true;
        }

        return false;
    }

    public static bool TryHexToColor(string hex, out Color color)
    {
        color = Color.white;
        if (string.IsNullOrWhiteSpace(hex))
            return false;
        var s = hex.Trim();
        if (!s.StartsWith("#", StringComparison.Ordinal))
            s = "#" + s;
        return ColorUtility.TryParseHtmlString(s, out color);
    }
}
