using Il2Cpp;
using UnityEngine;

namespace FMF.HexLabelMod;

/// <summary>Raycast from the main camera for cable spinner / rack color under the crosshair.</summary>
internal static class HexTargetResolver
{
    private const float MaxRayDistance = 48f;

    /// <param name="aimDetailSuffix">Text after "Anvisiert ·", e.g. <c>Kabelrolle · RJ</c> or <c>Rack · Colored</c>.</param>
    public static bool TryGetAimedColor(out string hex, out string aimDetailSuffix)
    {
        hex = null;
        aimDetailSuffix = null;

        var cam = Camera.main;
        if (cam == null)
            return false;

        var ray = new Ray(cam.transform.position, cam.transform.forward);
        if (!Physics.Raycast(ray, out var hit, MaxRayDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            return false;

        var spinner = hit.collider.GetComponentInParent<CableSpinner>();
        if (spinner != null && GameObjectColorHex.TryGetSpinnerHex(spinner, out hex))
        {
            var p = GameObjectKindResolver.GetSpinnerPortKind(spinner);
            var shortPort = p != null ? CablePortKindUtil.ToShortPortLabel(p) : null;
            aimDetailSuffix = shortPort != null ? $"Kabelrolle · {shortPort}" : "Kabelrolle";
            return true;
        }

        var rack = hit.collider.GetComponentInParent<Rack>();
        if (rack != null && GameObjectColorHex.TryGetRackHex(rack, out hex))
        {
            var v = GameObjectKindResolver.GetRackVariantLabel(rack);
            aimDetailSuffix = v != null ? $"Rack · {v}" : "Rack";
            return true;
        }

        return false;
    }
}
