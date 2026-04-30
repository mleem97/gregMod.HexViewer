using UnityEngine;
using UnityEngine.UIElements;
using gregCore.UI;

namespace greg.Mods.HexViewer;

internal static class HexViewerHud
{
    private static VisualElement _root = null!;
    private static Label _titleLabel = null!;
    private static Label _typeLabel = null!;
    private static Label _detailLabel = null!;
    private static Label _hexLabel = null!;
    private static VisualElement _colorSwatch = null!;
    private static bool _initialized;

    internal static bool IsVisible => _root != null && _root.style.display != DisplayStyle.None;

    internal static void Initialize()
    {
        if (_initialized) return;

        var builder = GregUIBuilder.CreateWidget("HexViewer_HUD", 20, 20)
            .SetSize(380, 140)
            .AddHeadline("Hardware Inspector");

        _root = builder.Build();

        // Dark semi-transparent overlay per spec (alpha ~0.85 for readability)
        _root.style.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.85f);
        _root.style.borderTopColor = new Color(0.3f, 0.6f, 0.9f);
        _root.style.borderBottomColor = new Color(0.3f, 0.6f, 0.9f);
        _root.style.borderLeftColor = new Color(0.3f, 0.6f, 0.9f);
        _root.style.borderRightColor = new Color(0.3f, 0.6f, 0.9f);
        _root.style.borderTopWidth = 2;
        _root.style.borderBottomWidth = 2;
        _root.style.borderLeftWidth = 2;
        _root.style.borderRightWidth = 2;

        UpdateAnchor();

        // Rebuild content area for full dynamic control
        var content = _root.Q<VisualElement>("Content");
        if (content != null)
        {
            content.Clear();

            _titleLabel = new Label("—")
            {
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = new Color(0.9f, 0.95f, 1f),
                    marginBottom = 4
                }
            };
            content.Add(_titleLabel);

            var divider = new VisualElement
            {
                style =
                {
                    height = 1,
                    backgroundColor = new Color(0.3f, 0.6f, 0.9f, 0.5f),
                    marginBottom = 6
                }
            };
            content.Add(divider);

            var detailRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            _typeLabel = new Label("—")
            {
                style =
                {
                    fontSize = 12,
                    color = new Color(0.75f, 0.8f, 0.85f),
                    flexGrow = 1
                }
            };
            detailRow.Add(_typeLabel);

            _hexLabel = new Label("—")
            {
                style =
                {
                    fontSize = 13,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.white,
                    marginRight = 6
                }
            };
            detailRow.Add(_hexLabel);

            _colorSwatch = new VisualElement
            {
                style =
                {
                    width = 24,
                    height = 24,
                    backgroundColor = Color.clear,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4
                }
            };
            detailRow.Add(_colorSwatch);

            content.Add(detailRow);

            _detailLabel = new Label("")
            {
                style =
                {
                    fontSize = 11,
                    color = new Color(0.6f, 0.65f, 0.7f),
                    marginTop = 4,
                    display = DisplayStyle.None
                }
            };
            content.Add(_detailLabel);
        }

        _initialized = true;
        Hide();
    }

    internal static void UpdateAnchor()
    {
        if (_root == null) return;

        var anchor = HexViewerConfig.Anchor.Value;
        switch (anchor)
        {
            case "TopLeft":
                _root.style.top = 20;
                _root.style.left = 20;
                _root.style.right = StyleKeyword.Auto;
                break;
            case "TopRight":
                _root.style.top = 20;
                _root.style.left = StyleKeyword.Auto;
                _root.style.right = 20;
                break;
            default: // TopCenter
                _root.style.top = 20;
                _root.style.left = (Screen.width - 380f) / 2f;
                _root.style.right = StyleKeyword.Auto;
                break;
        }
    }

    internal static void Show(HexViewerTargeting.HexTargetInfo info)
    {
        if (!HexViewerConfig.Enabled.Value || _root == null) return;

        _root.style.display = DisplayStyle.Flex;

        if (_titleLabel != null)
            _titleLabel.text = info.DisplayName;

        if (_typeLabel != null)
        {
            var mode = HexViewerConfig.Mode.Value;
            if (mode == "Developer")
            {
                _typeLabel.text = $"{info.ObjectType} | {info.Position:F1}";
            }
            else
            {
                _typeLabel.text = info.ObjectType;
            }
        }

        if (_hexLabel != null)
            _hexLabel.text = info.HexColor;

        if (_colorSwatch != null)
        {
            if (ColorUtility.TryParseHtmlString(info.HexColor, out var col))
                _colorSwatch.style.backgroundColor = col;
            else
                _colorSwatch.style.backgroundColor = Color.clear;
        }

        if (_detailLabel != null)
        {
            bool hasCableDetails = !string.IsNullOrEmpty(info.CableType) || !string.IsNullOrEmpty(info.CableMedium);
            if (hasCableDetails)
            {
                _detailLabel.style.display = DisplayStyle.Flex;
                _detailLabel.text = $"Typ: {info.CableType} | Medium: {info.CableMedium}";
            }
            else
            {
                _detailLabel.style.display = DisplayStyle.None;
            }
        }
    }

    internal static void Hide()
    {
        if (_root != null) _root.style.display = DisplayStyle.None;
    }

    internal static void Shutdown()
    {
        if (_root != null)
        {
            _root.RemoveFromHierarchy();
            _root = null!;
        }
        _initialized = false;
    }
}
