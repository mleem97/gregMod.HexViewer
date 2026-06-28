using Il2Cpp;
using UnityEngine;

namespace FMF.HexLabelMod;

/// <summary>Shared hex resolution for <see cref="CableSpinner"/> and <see cref="Rack"/> (world + HUD).</summary>
internal static class GameObjectColorHex
{
    public static bool TryGetSpinnerHex(CableSpinner spinner, out string hex)
    {
        hex = null;

        var raw = spinner.rgbColor;
        if (!string.IsNullOrWhiteSpace(raw) && HexColorUtil.TryNormalizeHex(raw, out hex))
            return true;

        var material = spinner.cableMaterial;
        if (material == null)
            return false;

        try
        {
            if (material.HasProperty("_BaseColor"))
            {
                hex = HexColorUtil.ToHex(material.GetColor("_BaseColor"));
                return true;
            }

            if (material.HasProperty("_Color"))
            {
                hex = HexColorUtil.ToHex(material.color);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    public static bool TryGetRackHex(Rack rack, out string hex)
    {
        hex = null;

        try
        {
            var renderers = rack.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Count == 0)
                return false;

            for (var i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;

                var mat = renderer.sharedMaterial;
                if (mat == null)
                    continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    hex = HexColorUtil.ToHex(mat.GetColor("_BaseColor"));
                    return true;
                }

                if (mat.HasProperty("_Color"))
                {
                    hex = HexColorUtil.ToHex(mat.color);
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
