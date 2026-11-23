using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StudioRelease
{
    [Serializable] class Definition {public string key="",title="",subtitle="",english="",product="",introduction="",controls="",accent="",ink="",paper="";}
    static Color Parse(string value){if(!ColorUtility.TryParseHtmlString(value,out var c))throw new Exception("色指定 "+value);return c;}
    public static void Build()
    {
        string key=StudioGame.Argument("-studioGame");
        if(string.IsNullOrEmpty(key))
        {
            var stage=UnityEngine.Object.FindFirstObjectByType<StudioGame>();
            if(!stage||!stage.skin)throw new Exception("ビルドする作品のシーンを開いてください。");key=stage.skin.key;
        }
        string root="Assets/Games/"+key,scenePath=root+"/"+key+".unity";
        var def=JsonUtility.FromJson<Definition>(File.ReadAllText(root+"/Stage.json"));
        var klass=TypeCache.GetTypesDerivedFrom<StudioGame>().FirstOrDefault(t=>!t.IsAbstract&&t.Name==key+"Game");
        if(klass==null)throw new Exception("Missing class "+key+"Game");
        {
            var skin=AssetDatabase.LoadAssetAtPath<StudioSkin>(root+"/Design.asset");
            if(!skin)
            {
                skin=ScriptableObject.CreateInstance<StudioSkin>();skin.key=key;skin.title=def.title;skin.subtitle=def.subtitle;skin.english=def.english;
                skin.product=def.product;skin.introduction=def.introduction;skin.controls=def.controls;
                skin.accent=Parse(def.accent);skin.ink=Parse(def.ink);skin.paper=Parse(def.paper);
                AssetDatabase.CreateAsset(skin,root+"/Design.asset");
            }
            if(!skin.cover)skin.cover=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/Art/Cover.png");
            if(!skin.music)skin.music=AssetDatabase.LoadAssetAtPath<AudioClip>(root+"/Art/Theme.ogg");
            if(!skin.activityMusic)skin.activityMusic=AssetDatabase.LoadAssetAtPath<AudioClip>(root+"/Art/Activity.ogg");
            if(!skin.illustrations)skin.illustrations=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/Art/Illustrations.png");
            if(!skin.specimenPlate)skin.specimenPlate=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/Art/Specimens.png");
            if(skin.effects==null||skin.effects.Length<5)Array.Resize(ref skin.effects,5);
            for(int i=0;i<5;i++)if(!skin.effects[i])
                skin.effects[i]=AssetDatabase.LoadAssetAtPath<AudioClip>(root+"/Art/Audio/Action"+i+".wav");
            if(!skin.cover||!skin.music)throw new Exception("表紙または楽曲が見つかりません "+key);
            EditorUtility.SetDirty(skin);
            if(!File.Exists(scenePath))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var game=(StudioGame)new GameObject(skin.title).AddComponent(klass);game.skin=skin;
                EditorSceneManager.SaveScene(scene,scenePath);
            }
            var playable=EditorBuildSettings.scenes.ToList();
            if(!playable.Any(s=>s.path==scenePath)){playable.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=playable.ToArray();}
            PlayerSettings.companyName="Shiosai Works";PlayerSettings.productName=skin.product;PlayerSettings.bundleVersion="1.0";
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true;PlayerSettings.forceSingleInstance=true;
            PlayerSettings.runInBackground=false;
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>(root+"/Art/Icon.png");
            if(icon)
            {
                var platform=UnityEditor.Build.NamedBuildTarget.Standalone;
                int sizes=PlayerSettings.GetIconSizes(platform,IconKind.Application).Length;
                if(sizes>0)PlayerSettings.SetIcons(platform,Enumerable.Repeat(icon,sizes).ToArray(),IconKind.Application);
            }
            AssetDatabase.SaveAssets();
            string output="Builds/"+key+"/"+key+".exe";Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scenePath},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("ビルド失敗 "+key);
            Debug.Log("STUDIO_BUILD_OK "+key);
        }
    }
    [MenuItem("潮彩作品集/現在の作品をWindows用にビルド")]
    public static void BuildActive(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())Build();}
}
