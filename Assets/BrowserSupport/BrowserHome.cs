using System;
using UnityEngine;
using UnityEngine.Scripting;

public sealed class BrowserHome : MonoBehaviour
{
    Action leave;

    public static void Register(Transform game, Action leaveGame)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var navigation = new GameObject("ShiosaiLibraryNavigation");
        navigation.transform.SetParent(game, false);
        navigation.AddComponent<BrowserHome>().leave = leaveGame;
#endif
    }

    // Called by the game page. Use the game's own save-and-exit path.
    [Preserve]
    public void GoHome() { leave?.Invoke(); }
}
