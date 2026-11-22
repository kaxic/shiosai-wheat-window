using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class BrowserTools
{
    public static Font ReadableFont(string[] systemFonts, int size)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        bool serif = systemFonts.Length > 0 && systemFonts[0].Contains("Mincho");
        return Object.Instantiate(Resources.Load<Font>(serif ? "WebType/ZenOldMincho-Regular" : "WebType/ZenKakuGothicNew-Regular"));
#else
        return Font.CreateDynamicFontFromOSFont(systemFonts, size);
#endif
    }

    public static void ReplaceFile(string temp, string destination, string backup)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (backup != null) File.Copy(destination, backup, true);
        File.Copy(temp, destination, true);
        File.Delete(temp);
#else
        File.Replace(temp, destination, backup);
#endif
    }

    public static void Saved()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShiosaiSync();
#endif
    }

    public static void Picture(string path, byte[] png)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, png);
#if UNITY_WEBGL && !UNITY_EDITOR
        ShiosaiSync();
        ShiosaiImage(path);
#endif
    }

    public static void Album(string path)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShiosaiAlbum(path);
#endif
    }

    public static void ReturnToCover()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShiosaiExit();
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void ShiosaiSync();
    [DllImport("__Internal")] static extern void ShiosaiImage(string path);
    [DllImport("__Internal")] static extern void ShiosaiAlbum(string path);
    [DllImport("__Internal")] static extern void ShiosaiExit();
#endif
}
