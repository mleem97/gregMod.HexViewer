using System;

namespace GregModHexViewer;

// Detects at runtime whether gregCore is present (type-name lookup only).
// Methods touching gregCore types must ONLY be called
// if HasCore is true (else JIT TypeLoad without DLL).
internal static class GregHost
{
    private const string ProbeType = "gregCore.UI.GregNotificationManager, gregCore";
    private static bool? _hasCore;

    public static bool HasCore
    {
        get
        {
            if (_hasCore == null)
            {
                try { _hasCore = Type.GetType(ProbeType) != null; }
                catch { _hasCore = false; }
            }
            return _hasCore.Value;
        }
    }
}
