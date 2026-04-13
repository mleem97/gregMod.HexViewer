using System;
using System.Collections.Generic;
using UnityEngine;
using Il2CppTMPro;
using greg.Core.UI;
using greg.Core.UI.Components;
using greg.Sdk.Services;
using greg.Sdk;

namespace greg.Mods.HexViewer;

/// <summary>
/// A refined HexViewer telemetry overlay. 
/// Refactored to use the gregCore SDK services as a reference implementation.
/// Extracted from gregCore to run as standalone mod.
/// </summary>
public static class HexViewerUI
{
    private const string ModId = "HexViewer";
    
    private static GregPanel _inspectorPanel;
    private static GregPanel _hookMonitorPanel;
    private static GregPanel _uiTreePanel;

    private static List<string> _hookLogs = new();
    private const int MaxHookLogs = 15;

    public static void Init()
    {
        _uiTreePanel = GregUIBuilder.Panel("hexviewer.uitree").Title("⬡ UI TREE").Position(GregUIAnchor.TopLeft).Size(280, 450).Build();
        _hookMonitorPanel = GregUIBuilder.Panel("hexviewer.hookmonitor").Title("⬡ HOOK LOG").Position(GregUIAnchor.BottomRight).Size(320, 250).Build();
        _inspectorPanel = GregUIBuilder.Panel("hexviewer.inspector").Title("⬡ INSPECTOR").Position(GregUIAnchor.BottomLeft).Size(280, 200).Build();

        // Register for world interactions using SDK dispatcher
        gregEventDispatcher.On(gregNativeEventHooks.WorldInteractionHovered, OnHookDetected, ModId);
        
        MelonLoader.MelonLogger.Msg("[HexViewer] Subsystem initialized.");
    }

    public static void Shutdown()
    {
        gregEventDispatcher.UnregisterAll(ModId);
    }

    public static void OnUpdate()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null) return;

        // Toggle UI Panels
        if (UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame) { 
            _uiTreePanel?.Toggle(); 
            if (_uiTreePanel != null && _uiTreePanel.IsVisible) RefreshUITree(); 
        }
        if (UnityEngine.InputSystem.Keyboard.current.f2Key.wasPressedThisFrame) _hookMonitorPanel?.Toggle();
        if (UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame) _inspectorPanel?.Toggle();
        
        // JADE Logic (Now using SDK)
        UpdateJadeOverlay();
        
        if (_hookMonitorPanel?.IsVisible == true) UpdateHookMonitorUI();
    }

    private static void UpdateJadeOverlay()
    {
        // Ensure service is ready
        GregHudService.Initialize();

        // 1. Get Targeting info from SDK
        var target = GregTargetingService.GetTargetInfo(10.0f);
        
        if (target.TargetType == GregTargetType.None)
        {
            GregHudService.HideJadeBox();
            return;
        }

        // 2. Get Metadata from SDK
        var metadata = GregComponentMetadataService.GetMetadata(target);
        string title = target.TargetType.ToString().Replace("_", " ");
        string subHeader = "TELEMETRY SCAN";

        // 3. Update HUD via SDK
        GregHudService.UpdateJadeBox(title, subHeader, metadata);

        // EXTRA: Inspector logic (Still here as it is mod-specific)
        if (_inspectorPanel?.IsVisible == true && target.Entity != null)
        {
            var components = target.Entity.GetComponents<Component>();
            var compLogs = new List<string>();
            compLogs.Add($"INSPECTING: {target.Entity.name}");
            foreach (var comp in components) if (comp != null) compLogs.Add($"• {comp.GetType().Name}");
            UpdateStandardPanel("INSPECTOR", compLogs, _inspectorPanel);
        }
    }

    private static void RefreshUITree()
    {
        var logs = new List<string>();
        logs.Add("SCANNING ACTIVE CANVASES...");
        
        var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (!c.gameObject.activeInHierarchy) continue;
            logs.Add($"■ {c.name} (Sorting: {c.sortingOrder})");
            for (int i = 0; i < c.transform.childCount; i++)
            {
                var child = c.transform.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                    logs.Add($"  ∟ {child.name}");
            }
        }
        UpdateStandardPanel("UI TREE", logs, _uiTreePanel);
    }

    private static void OnHookDetected(object payload)
    {
        try {
            string hookName = gregPayload.Get(payload, "HookName", "unknown");
            _hookLogs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {hookName}");
            if (_hookLogs.Count > MaxHookLogs) _hookLogs.RemoveAt(MaxHookLogs);
        } catch {}
    }

    private static void UpdateHookMonitorUI()
    {
        UpdateStandardPanel("HOOK LOG", _hookLogs, _hookMonitorPanel);
    }

    private static void UpdateStandardPanel(string title, List<string> details, GregPanel panel)
    {
        if (panel == null) return;
        var panelGO = panel.PanelRoot;
        if (panelGO == null) return;

        var titleText = panelGO.transform.Find("Header/Title")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null) titleText.text = $"⬡ {title}";

        var content = panelGO.transform.Find("Content");
        if (content != null)
        {
            for (int i = content.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(content.GetChild(i).gameObject);

            foreach (var detail in details)
            {
                var detailGO = new GameObject("Detail");
                detailGO.transform.SetParent(content, false);
                var text = detailGO.AddComponent<TextMeshProUGUI>();
                text.text = $"> {detail}";
                text.fontSize = 11;
                text.fontStyle = FontStyles.Bold;
                text.color = greg.Core.UI.GregUITheme.OnSurface;
            }
        }
    }
}

