using System;
using System.Collections;
using System.IO;
using UnityEngine;

// A print is one cleanly framed player image. The subclass draws it in place of its HUD.
public abstract class StudioPrint : MonoBehaviour
{
    protected StudioGame Game { get; private set; }
    protected abstract string Folder { get; }
    protected abstract string Prefix { get; }
    protected abstract string Caption { get; }
    protected abstract string CheckMarker { get; }
    protected abstract bool Eligible { get; }
    protected abstract void SetFrame(bool value);
    string DirectoryName => Path.Combine(Game.StorageDirectory, "Photos", Folder);
    bool busy;

    protected void Awake() { Game = GetComponent<StudioGame>(); }
    protected void Update()
    {
        if (!Game || !Game.Playing) return;
        if (Input.GetKeyDown(KeyCode.F8)) Take();
        if (Input.GetKeyDown(KeyCode.F9)) OpenAlbum();
    }
    public void Take()
    {
        if (!Game || !Game.Playing || !Eligible || busy) return;
        StartCoroutine(Develop());
    }

    IEnumerator Develop()
    {
        busy = true; Game.DismissNotice(); SetFrame(true);
        yield return null;
        yield return new WaitForEndOfFrame();
        Texture2D image = null;
        try
        {
            image = ScreenCapture.CaptureScreenshotAsTexture();
            Directory.CreateDirectory(DirectoryName);
            string name = Prefix + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".png";
            BrowserTools.Picture(Path.Combine(DirectoryName, name), image.EncodeToPNG());
            if (Game.Reviewing) Debug.Log(CheckMarker + " " + name);
            else Game.Notice(Caption + "を保存しました　 F9 でアルバムを開く");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        { Game.Notice(Caption + "の保存先を確認してください"); Debug.LogWarning(ex.Message); }
        finally { if (image) Destroy(image); busy = false; SetFrame(false); }
    }

    void OpenAlbum()
    {
        try
        {
            Directory.CreateDirectory(DirectoryName);
#if UNITY_WEBGL && !UNITY_EDITOR
            BrowserTools.Album(DirectoryName);
#else
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                FileName = DirectoryName, UseShellExecute = true
            });
#endif
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
            ex is System.ComponentModel.Win32Exception)
        { Game.Notice("アルバムを開けませんでした"); Debug.LogWarning(ex.Message); }
    }
    protected void OnDisable() { busy = false; SetFrame(false); }
}
