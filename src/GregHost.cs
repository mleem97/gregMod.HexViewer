using System;

namespace GregModHexViewer;

// Erkennt zur Laufzeit, ob gregCore vorhanden ist (reiner Typname-Lookup).
// Methoden, die gregCore-Typen beruehren, duerfen NUR aufgerufen werden,
// wenn HasCore true ist (sonst JIT-TypeLoad bei fehlender DLL).
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
