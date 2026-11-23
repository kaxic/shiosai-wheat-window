using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BrowserRelease
{
    [MenuItem("Shiosai/Build browser game")]
    public static void Build()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            throw new InvalidOperationException("Web Build Support を導入してください。");
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled && File.Exists(s.path)).Select(s => s.path).ToArray();
        if (scenes.Length != 1) throw new InvalidOperationException("この作品のシーンをひとつ有効にしてください。");
        PlayerSettings.WebGL.template = "PROJECT:Shiosai";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.dataCaching = false;
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
        PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Release);
        EditorUserBuildSettings.SetPlatformSettings("WebGL", "CodeOptimization", "BuildTimes");
        const string output = "Builds/Browser";
        Directory.CreateDirectory(output);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = scenes, locationPathName = output, target = BuildTarget.WebGL, options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Web build failed");
        string html = File.ReadAllText(Path.Combine(output, "index.html"));
        foreach (var source in Directory.EnumerateFiles(output, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(output, source);
            if (Path.GetDirectoryName(relative) == "Build" && !html.Contains(Path.GetFileName(source))) continue;
            var destination = Path.Combine("docs", relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(source, destination, true);
        }
        foreach (var old in new[] { "Browser.loader.js", "Browser.data.unityweb", "Browser.framework.js.unityweb", "Browser.wasm.unityweb" })
        {
            string file = Path.Combine("docs", "Build", old);
            if (!html.Contains(old) && File.Exists(file)) File.Delete(file);
        }
        File.WriteAllText("docs/.nojekyll", "");
        Debug.Log("SHIOSAI_WEB_BUILD_OK " + scenes[0]);
    }
}
