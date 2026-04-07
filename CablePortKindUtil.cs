using System;

namespace FMF.HexLabelMod;

/// <summary>RJ / SFP / QSFP from UI text or item strings (same rules as held patch cables).</summary>
internal static class CablePortKindUtil
{
    /// <summary>Returns RJ45, SFP, or QSFP, or null.</summary>
    public static string ClassifyPortString(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        var u = s.ToUpperInvariant();
        if (u.Contains("QSFP", StringComparison.Ordinal))
            return "QSFP";
        if (u.Contains("SFP", StringComparison.Ordinal))
            return "SFP";
        if (u.Contains("RJ45", StringComparison.Ordinal) || u.Contains("RJ-45", StringComparison.Ordinal))
            return "RJ45";
        if (u == "RJ" || u.Contains("RJ ", StringComparison.Ordinal) || u.EndsWith("RJ", StringComparison.Ordinal))
            return "RJ45";

        return null;
    }

    /// <summary>HUD: user prefers "RJ" over "RJ45".</summary>
    public static string ToShortPortLabel(string portKind)
    {
        if (string.IsNullOrEmpty(portKind))
            return portKind;
        return portKind == "RJ45" ? "RJ" : portKind;
    }
}
