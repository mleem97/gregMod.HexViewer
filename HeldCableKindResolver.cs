using System;
using System.Reflection;
using Il2Cpp;
using UnityEngine;

namespace FMF.HexLabelMod;

/// <summary>
/// Resolves RJ / SFP / QSFP for the cable item the player is holding, using
/// reflection on <see cref="PlayerClass"/> and related types (names vary by build).
/// </summary>
internal static class HeldCableKindResolver
{
    public static string Resolve()
    {
        try
        {
            var pc = PlayerManager.instance?.playerClass;
            if (pc == null)
                return null;

            var t = pc.GetType();
            foreach (var member in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object val = null;
                try
                {
                    if (member is FieldInfo fi)
                        val = fi.GetValue(pc);
                    else if (member is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                        val = pi.GetValue(pc);
                    else
                        continue;
                }
                catch
                {
                    continue;
                }

                if (val == null)
                    continue;

                var kind = ClassifyObject(val);
                if (kind != null)
                    return kind;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>
    /// Rack, cable reel (spinner), or patch cable in hand — returns a display label and hex when possible.
    /// Labels include port (RJ/SFP/QSFP) for reels and Normal/Colored for racks when detected.
    /// </summary>
    public static bool TryGetHeldItemHex(out string hex, out string kindLabel)
    {
        hex = null;
        kindLabel = null;

        try
        {
            var pc = PlayerManager.instance?.playerClass;
            if (pc == null)
                return false;

            foreach (var m in pc.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object val = null;
                try
                {
                    if (m is FieldInfo fi)
                        val = fi.GetValue(pc);
                    else if (m is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                        val = pi.GetValue(pc);
                    else
                        continue;
                }
                catch
                {
                    continue;
                }

                if (val == null)
                    continue;

                if (TryGetHexFromHeldObject(val, out hex, out kindLabel))
                    return true;
            }
        }
        catch
        {
            // ignored
        }

        var kind = Resolve();
        if (TryGetHeldCableHex(out hex))
        {
            kindLabel = string.IsNullOrEmpty(kind)
                ? "Kabel"
                : $"Kabel · {CablePortKindUtil.ToShortPortLabel(kind)}";
            return true;
        }

        return false;
    }

    private static bool TryGetHexFromHeldObject(object val, out string hex, out string kindLabel)
    {
        hex = null;
        kindLabel = null;

        if (val is Rack rack && GameObjectColorHex.TryGetRackHex(rack, out hex))
        {
            var v = GameObjectKindResolver.GetRackVariantLabel(rack);
            kindLabel = v != null ? $"Rack · {v}" : "Rack";
            return true;
        }

        if (val is CableSpinner sp && GameObjectColorHex.TryGetSpinnerHex(sp, out hex))
        {
            var p = GameObjectKindResolver.GetSpinnerPortKind(sp);
            var shortPort = p != null ? CablePortKindUtil.ToShortPortLabel(p) : null;
            kindLabel = shortPort != null ? $"Kabelrolle · {shortPort}" : "Kabelrolle";
            return true;
        }

        if (val is GameObject go)
        {
            var r = go.GetComponentInParent<Rack>();
            if (r != null && GameObjectColorHex.TryGetRackHex(r, out hex))
            {
                var v = GameObjectKindResolver.GetRackVariantLabel(r);
                kindLabel = v != null ? $"Rack · {v}" : "Rack";
                return true;
            }

            var s = go.GetComponentInParent<CableSpinner>();
            if (s != null && GameObjectColorHex.TryGetSpinnerHex(s, out hex))
            {
                var p = GameObjectKindResolver.GetSpinnerPortKind(s);
                var shortPort = p != null ? CablePortKindUtil.ToShortPortLabel(p) : null;
                kindLabel = shortPort != null ? $"Kabelrolle · {shortPort}" : "Kabelrolle";
                return true;
            }
        }

        if (val is Component c)
        {
            var r = c.GetComponentInParent<Rack>();
            if (r != null && GameObjectColorHex.TryGetRackHex(r, out hex))
            {
                var v = GameObjectKindResolver.GetRackVariantLabel(r);
                kindLabel = v != null ? $"Rack · {v}" : "Rack";
                return true;
            }

            var s = c.GetComponentInParent<CableSpinner>();
            if (s != null && GameObjectColorHex.TryGetSpinnerHex(s, out hex))
            {
                var p = GameObjectKindResolver.GetSpinnerPortKind(s);
                var shortPort = p != null ? CablePortKindUtil.ToShortPortLabel(p) : null;
                kindLabel = shortPort != null ? $"Kabelrolle · {shortPort}" : "Kabelrolle";
                return true;
            }
        }

        return false;
    }

    public static bool TryGetHeldCableHex(out string hex)
    {
        hex = null;
        try
        {
            var pc = PlayerManager.instance?.playerClass;
            if (pc == null)
                return false;

            foreach (var m in pc.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object val = null;
                try
                {
                    if (m is FieldInfo fi)
                        val = fi.GetValue(pc);
                    else if (m is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                        val = pi.GetValue(pc);
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
                if (name.IndexOf("cable", StringComparison.OrdinalIgnoreCase) < 0
                    && name.IndexOf("held", StringComparison.OrdinalIgnoreCase) < 0
                    && name.IndexOf("item", StringComparison.OrdinalIgnoreCase) < 0
                    && name.IndexOf("inventory", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (TryHexFromObject(val, out hex))
                    return true;
            }

            return TryHexFromObject(pc, out hex);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryHexFromObject(object o, out string hex)
    {
        hex = null;
        if (o == null)
            return false;

        if (o is string str && HexColorUtil.TryNormalizeHex(str, out hex))
            return true;

        if (o is UnityEngine.Color c)
        {
            hex = HexColorUtil.ToHex(c);
            return true;
        }

        var t = o.GetType();
        foreach (var m in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (m.Name.IndexOf("rgb", StringComparison.OrdinalIgnoreCase) < 0
                && m.Name.IndexOf("color", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            object val = null;
            try
            {
                if (m is FieldInfo fi)
                    val = fi.GetValue(o);
                else if (m is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                    val = pi.GetValue(o);
                else
                    continue;
            }
            catch
            {
                continue;
            }

            if (val is string s2 && HexColorUtil.TryNormalizeHex(s2, out hex))
                return true;
        }

        return false;
    }

    private static string ClassifyObject(object o)
    {
        if (o is string s)
            return CablePortKindUtil.ClassifyPortString(s);

        var s2 = o.ToString();
        if (!string.IsNullOrEmpty(s2))
        {
            var k = CablePortKindUtil.ClassifyPortString(s2);
            if (k != null)
                return k;
        }

        var t = o.GetType();
        foreach (var member in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            object val = null;
            try
            {
                if (member is FieldInfo fi)
                    val = fi.GetValue(o);
                else if (member is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                    val = pi.GetValue(o);
                else
                    continue;
            }
            catch
            {
                continue;
            }

            if (val is string str)
            {
                var k = CablePortKindUtil.ClassifyPortString(str);
                if (k != null)
                    return k;
            }
        }

        return null;
    }
}
