using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Il2Cpp;
using UnityEngine;

namespace FMF.HexLabelMod;

internal readonly struct CableColorEntry
{
    public CableColorEntry(string hex, string source)
    {
        Hex = hex;
        Source = source;
    }

    public string Hex { get; }
    public string Source { get; }
}

internal static class CableColorCollector
{
    public static List<CableColorEntry> CollectAll()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in CollectFromSpinners())
            map[e.Hex] = e.Source;
        foreach (var e in CollectFromSaveReflection())
            map[e.Hex] = e.Source;
        foreach (var e in CollectFromSaveJsonFiles())
            map[e.Hex] = e.Source;

        return map
            .Select(kv => new CableColorEntry(kv.Key, kv.Value))
            .OrderBy(x => x.Hex, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<CableColorEntry> CollectFromSpinners()
    {
        CableSpinner[] spinners;
        try
        {
            spinners = UnityEngine.Object.FindObjectsOfType<CableSpinner>();
        }
        catch
        {
            yield break;
        }

        foreach (var sp in spinners)
        {
            if (sp == null)
                continue;

            string hex = null;
            var raw = sp.rgbColor;
            if (!string.IsNullOrWhiteSpace(raw) && HexColorUtil.TryNormalizeHex(raw, out var h))
                hex = h;
            else
            {
                var mat = sp.cableMaterial;
                if (mat != null)
                {
                    try
                    {
                        if (mat.HasProperty("_BaseColor"))
                            hex = HexColorUtil.ToHex(mat.GetColor("_BaseColor"));
                        else if (mat.HasProperty("_Color"))
                            hex = HexColorUtil.ToHex(mat.color);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            if (!string.IsNullOrEmpty(hex))
                yield return new CableColorEntry(hex, "scene (CableSpinner)");
        }
    }

    private static IEnumerable<CableColorEntry> CollectFromSaveReflection()
    {
        var asm = typeof(CableSpinner).Assembly;
        foreach (var typeName in new[] { "Il2Cpp.Save", "Il2Cpp.SaveData", "Il2Cpp.GameSave" })
        {
            var t = asm.GetType(typeName);
            if (t == null)
                continue;

            object instance = null;
            try
            {
                var inst = t.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (inst != null)
                    instance = inst.GetValue(null);

                if (instance == null)
                {
                    var cur = t.GetProperty("current", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (cur != null)
                        instance = cur.GetValue(null);
                }

                if (instance == null)
                {
                    var fld = t.GetField("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fld != null)
                        instance = fld.GetValue(null);
                }
            }
            catch
            {
                continue;
            }

            if (instance == null)
                continue;

            foreach (var fieldName in new[] { "member_values", "memberValues" })
            {
                FieldInfo mf = null;
                try
                {
                    mf = instance.GetType().GetField(fieldName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
                catch
                {
                    // ignored
                }

                if (mf == null)
                    continue;

                object mv = null;
                try
                {
                    mv = mf.GetValue(instance);
                }
                catch
                {
                    continue;
                }

                if (mv == null)
                    continue;

                foreach (var s in WalkForColorStrings(mv, 0))
                {
                    if (HexColorUtil.TryNormalizeHex(s, out var hex))
                        yield return new CableColorEntry(hex, $"Save.{typeName}.{fieldName}");
                }
            }

            foreach (var hex in ExtractColorsFromMemberValues(instance))
                yield return new CableColorEntry(hex, $"Save.{typeName} (member_values)");
        }
    }

    private static IEnumerable<string> ExtractColorsFromMemberValues(object root)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in root.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            object val = null;
            try
            {
                if (m is FieldInfo fi)
                    val = fi.GetValue(root);
                else if (m is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                    val = pi.GetValue(root);
            }
            catch
            {
                continue;
            }

            if (val == null)
                continue;

            var name = m.Name;
            if (name.IndexOf("member", StringComparison.OrdinalIgnoreCase) < 0
                && name.IndexOf("values", StringComparison.OrdinalIgnoreCase) < 0
                && name.IndexOf("cable", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            foreach (var s in WalkForColorStrings(val, 0))
            {
                if (HexColorUtil.TryNormalizeHex(s, out var hex) && seen.Add(hex))
                    yield return hex;
            }
        }

        foreach (var s in WalkForColorStrings(root, 0))
        {
            if (s.IndexOf("cable", StringComparison.OrdinalIgnoreCase) < 0
                && s.IndexOf("color", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            if (HexColorUtil.TryNormalizeHex(s, out var hex) && seen.Add(hex))
                yield return hex;
        }
    }

    private static IEnumerable<string> WalkForColorStrings(object o, int depth)
    {
        if (o == null || depth > 8)
            yield break;

        if (o is string str)
        {
            if (LooksLikeColorString(str))
                yield return str;
            yield break;
        }

        if (o is IEnumerable en && o is not string)
        {
            foreach (var item in en)
            {
                foreach (var s in WalkForColorStrings(item, depth + 1))
                    yield return s;
            }

            yield break;
        }

        var t = o.GetType();
        if (t.IsPrimitive || t.IsEnum)
            yield break;

        foreach (var m in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            object val = null;
            try
            {
                if (m is FieldInfo fi)
                    val = fi.GetValue(o);
                else if (m is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                    val = pi.GetValue(o);
            }
            catch
            {
                continue;
            }

            if (val == null)
                continue;

            var name = m.Name;
            if (name.IndexOf("cableColor", StringComparison.OrdinalIgnoreCase) >= 0
                || (name.IndexOf("color", StringComparison.OrdinalIgnoreCase) >= 0 && name.IndexOf("cable", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                if (val is string cs && HexColorUtil.TryNormalizeHex(cs, out _))
                    yield return cs;
            }

            foreach (var s in WalkForColorStrings(val, depth + 1))
                yield return s;
        }
    }

    private static bool LooksLikeColorString(string s)
    {
        if (string.IsNullOrWhiteSpace(s) || s.Length > 200)
            return false;
        return s.IndexOf('#') >= 0
               || (s.IndexOf(',') >= 0 && char.IsDigit(s[0]));
    }

    private static IEnumerable<CableColorEntry> CollectFromSaveJsonFiles()
    {
        string root;
        try
        {
            root = Application.persistentDataPath;
        }
        catch
        {
            yield break;
        }

        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            yield break;

        string[] files;
        try
        {
            files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f).ToLowerInvariant();
                    return ext is ".json" or ".txt" or ".save" or ".dat";
                })
                .Take(64)
                .ToArray();
        }
        catch
        {
            yield break;
        }

        foreach (var path in files)
        {
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch
            {
                continue;
            }

            if (text.IndexOf("cable", StringComparison.OrdinalIgnoreCase) < 0
                && text.IndexOf("member_values", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            foreach (var hex in ExtractHexFromJson(text))
                yield return new CableColorEntry(hex, $"file:{Path.GetFileName(path)}");
        }
    }

    private static IEnumerable<string> ExtractHexFromJson(string json)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch
        {
            yield break;
        }

        using (doc)
        {
            foreach (var hex in WalkJsonElement(doc.RootElement))
                yield return hex;
        }
    }

    private static IEnumerable<string> WalkJsonElement(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var p in el.EnumerateObject())
                {
                    var key = p.Name;
                    if (key.IndexOf("cableColor", StringComparison.OrdinalIgnoreCase) >= 0
                        || (key.IndexOf("color", StringComparison.OrdinalIgnoreCase) >= 0
                            && key.IndexOf("cable", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        if (p.Value.ValueKind == JsonValueKind.String)
                        {
                            var s = p.Value.GetString();
                            if (!string.IsNullOrEmpty(s) && HexColorUtil.TryNormalizeHex(s, out var hex))
                                yield return hex;
                        }
                    }

                    foreach (var h in WalkJsonElement(p.Value))
                        yield return h;
                }

                break;
            case JsonValueKind.Array:
                foreach (var item in el.EnumerateArray())
                {
                    foreach (var h in WalkJsonElement(item))
                        yield return h;
                }

                break;
            case JsonValueKind.String:
            {
                var s = el.GetString();
                if (!string.IsNullOrEmpty(s) && s.Length < 200 && HexColorUtil.TryNormalizeHex(s, out var hex))
                    yield return hex;
                break;
            }
        }
    }
}
