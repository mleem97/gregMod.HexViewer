using System;
using System.Reflection;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;

namespace FMF.HexLabelMod;

/// <summary>
/// Port type on <see cref="CableSpinner"/> and Normal vs Colored on <see cref="Rack"/> via TMP + reflection.
/// </summary>
internal static class GameObjectKindResolver
{
    public static string GetSpinnerPortKind(CableSpinner sp)
    {
        if (sp == null)
            return null;

        try
        {
            var tl = sp.txtLength;
            if (tl != null && !string.IsNullOrEmpty(tl.text))
            {
                var k = CablePortKindUtil.ClassifyPortString(tl.text);
                if (k != null)
                    return k;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            foreach (var s in EnumerateTmpTexts(sp))
            {
                var k = CablePortKindUtil.ClassifyPortString(s);
                if (k != null)
                    return k;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            foreach (var s in EnumerateStringMembers(sp, depth: 0))
            {
                var k = CablePortKindUtil.ClassifyPortString(s);
                if (k != null)
                    return k;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>Returns "Normal", "Colored", or null if unknown.</summary>
    public static string GetRackVariantLabel(Rack rack)
    {
        if (rack == null)
            return null;

        try
        {
            var n = rack.gameObject != null ? rack.gameObject.name : null;
            if (!string.IsNullOrEmpty(n))
            {
                if (NameImpliesColored(n))
                    return "Colored";
                if (NameImpliesNormal(n))
                    return "Normal";
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            var t = rack.GetType();
            foreach (var m in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object val = null;
                try
                {
                    if (m is FieldInfo fi)
                        val = fi.GetValue(rack);
                    else if (m is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                        val = pi.GetValue(rack);
                    else
                        continue;
                }
                catch
                {
                    continue;
                }

                if (val == null)
                    continue;

                var name = m.Name;

                if (val is bool b)
                {
                    if (name.IndexOf("color", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.Equals("isColored", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("colored", StringComparison.OrdinalIgnoreCase))
                    {
                        return b ? "Colored" : "Normal";
                    }
                }

                if (val is string s)
                {
                    var v = ClassifyRackVariantString(s);
                    if (v != null)
                        return v;
                }

                if (val.GetType().IsEnum)
                {
                    var es = val.ToString();
                    var v = ClassifyRackVariantString(es);
                    if (v != null)
                        return v;
                }
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static bool NameImpliesColored(string n)
    {
        var u = n.ToUpperInvariant();
        return u.Contains("COLORED", StringComparison.Ordinal) || u.Contains("COLOR_RACK", StringComparison.Ordinal);
    }

    private static bool NameImpliesNormal(string n)
    {
        var u = n.ToUpperInvariant();
        return u.Contains("NORMAL", StringComparison.Ordinal) && u.Contains("RACK", StringComparison.Ordinal);
    }

    private static string ClassifyRackVariantString(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        var u = s.ToUpperInvariant();
        if (u.Contains("COLORED", StringComparison.Ordinal) && !u.Contains("NORMAL", StringComparison.Ordinal))
            return "Colored";
        if (u.Contains("NORMAL", StringComparison.Ordinal) && !u.Contains("COLORED", StringComparison.Ordinal))
            return "Normal";

        if (string.Equals(s, "Colored", StringComparison.OrdinalIgnoreCase))
            return "Colored";
        if (string.Equals(s, "Normal", StringComparison.OrdinalIgnoreCase))
            return "Normal";

        return null;
    }

    private static System.Collections.Generic.IEnumerable<string> EnumerateTmpTexts(Component root)
    {
        if (root == null)
            yield break;

        TextMeshProUGUI[] tmps;
        try
        {
            tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        }
        catch
        {
            yield break;
        }

        if (tmps == null)
            yield break;

        var n = tmps.Length;
        for (var i = 0; i < n; i++)
        {
            var tmp = tmps[i];
            if (tmp == null)
                continue;

            string text = null;
            try
            {
                text = tmp.text;
            }
            catch
            {
                continue;
            }

            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }

    private static System.Collections.Generic.IEnumerable<string> EnumerateStringMembers(object o, int depth)
    {
        if (o == null || depth > 2)
            yield break;

        var t = o.GetType();
        foreach (var m in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (m is FieldInfo fi && fi.FieldType == typeof(string))
            {
                string s = null;
                try
                {
                    s = fi.GetValue(o) as string;
                }
                catch
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(s))
                    yield return s;
            }
            else if (m is PropertyInfo pi && pi.PropertyType == typeof(string) && pi.GetIndexParameters().Length == 0)
            {
                string s = null;
                try
                {
                    s = pi.GetValue(o) as string;
                }
                catch
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(s))
                    yield return s;
            }
        }
    }
}
