using System;
using UnityEngine;
using MelonLoader;

namespace greg.Mods.HexViewer;

internal static class HexViewerTargeting
{
    private static Camera _mainCamera = null!;
    private static GameObject _lastTarget = null!;
    private static HexTargetInfo _currentInfo;

    internal struct HexTargetInfo
    {
        public bool Valid;
        public string DisplayName;
        public string ObjectType;
        public string HexColor;
        public string CableType;
        public string CableMedium;
        public Vector3 Position;
    }

    internal static HexTargetInfo Current => _currentInfo;
    internal static bool HasTarget => _currentInfo.Valid;
    internal static bool TargetChanged { get; private set; }

    internal static void Update()
    {
        TargetChanged = false;

        try
        {
            if (_mainCamera == null || _mainCamera.Pointer == IntPtr.Zero)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null || _mainCamera.Pointer == IntPtr.Zero) return;
            }

            var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit, 10f))
            {
                var go = hit.collider?.gameObject;
                if (go != null && go.Pointer != IntPtr.Zero)
                {
                    if (_lastTarget != go)
                    {
                        _lastTarget = go;
                        TargetChanged = true;
                        ResolveTarget(go);
                    }
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[HexViewer] Targeting error: {ex.Message}");
        }

        if (_lastTarget != null)
        {
            _lastTarget = null!;
            TargetChanged = true;
            _currentInfo = new HexTargetInfo { Valid = false };
        }
    }

    private static void ResolveTarget(GameObject go)
    {
        var info = new HexTargetInfo { Valid = true, DisplayName = go.name, Position = go.transform.position };

        try
        {
            // 1. Rack
            var rack = go.GetComponent<Il2Cpp.Rack>();
            if (rack != null && rack.Pointer != IntPtr.Zero)
            {
                info.ObjectType = "Rack";
                info.HexColor = ExtractRackColor(rack);
                _currentInfo = info;
                return;
            }

            // 2. Server
            var server = go.GetComponent<Il2Cpp.Server>();
            if (server != null && server.Pointer != IntPtr.Zero)
            {
                info.ObjectType = "Server";
                info.HexColor = ExtractServerColor(server);
                _currentInfo = info;
                return;
            }

            // 3. Network Switch
            var sw = go.GetComponent<Il2Cpp.NetworkSwitch>();
            if (sw != null && sw.Pointer != IntPtr.Zero)
            {
                info.ObjectType = "Switch";
                info.CableType = ResolveSwitchPortType(sw);
                info.CableMedium = ResolveSwitchMedium(sw);
                _currentInfo = info;
                return;
            }

            // 4. CableSpinner (reel) — optional, wrapped because type may not exist in all builds
            try
            {
                var spinner = go.GetComponent<Il2Cpp.CableSpinner>();
                if (spinner != null && spinner.Pointer != IntPtr.Zero)
                {
                    info.ObjectType = "Cable Reel";
                    info.HexColor = ExtractSpinnerColor(spinner);
                    _currentInfo = info;
                    return;
                }
            }
            catch { /* type not available in this build */ }

            // 5. Generic fallback
            info.ObjectType = "Device";
            _currentInfo = info;
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[HexViewer] Resolve error: {ex.Message}");
            _currentInfo = info;
        }
    }

    // Color extraction stubs (replace with real game logic when reverse-engineered)

    private static string ExtractRackColor(Il2Cpp.Rack rack)
    {
        try
        {
            var mat = rack.GetComponentInChildren<MeshRenderer>()?.material;
            if (mat != null && mat.Pointer != IntPtr.Zero)
            {
                var c = mat.color;
                return $"#{(byte)(c.r * 255):X2}{(byte)(c.g * 255):X2}{(byte)(c.b * 255):X2}";
            }
        }
        catch { }
        return "#808080";
    }

    private static string ExtractServerColor(Il2Cpp.Server server)
    {
        try
        {
            var light = server.GetComponentInChildren<Light>();
            if (light != null && light.Pointer != IntPtr.Zero)
            {
                var c = light.color;
                return $"#{(byte)(c.r * 255):X2}{(byte)(c.g * 255):X2}{(byte)(c.b * 255):X2}";
            }
        }
        catch { }
        return "#4A90D9";
    }

    private static string ExtractSpinnerColor(Il2Cpp.CableSpinner spinner)
    {
        try
        {
            var rend = spinner.GetComponentInChildren<MeshRenderer>();
            if (rend != null && rend.Pointer != IntPtr.Zero)
            {
                var c = rend.material.color;
                return $"#{(byte)(c.r * 255):X2}{(byte)(c.g * 255):X2}{(byte)(c.b * 255):X2}";
            }
        }
        catch { }
        return "#CC4444";
    }

    private static string ResolveSwitchPortType(Il2Cpp.NetworkSwitch sw)
    {
        try
        {
            // Heuristic based on object name since 'ports' field is not exposed in dummy DLLs
            string name = sw.name?.ToLowerInvariant() ?? "";
            if (name.Contains("qsfp")) return "QSFP";
            if (name.Contains("sfp")) return "SFP";
            return "RJ45";
        }
        catch { }
        return "RJ45";
    }

    private static string ResolveSwitchMedium(Il2Cpp.NetworkSwitch sw)
    {
        try
        {
            string name = sw.name?.ToLowerInvariant() ?? "";
            if (name.Contains("fiber") || name.Contains("opt")) return "FIBER";
            return "COPPER";
        }
        catch { }
        return "COPPER";
    }
}
