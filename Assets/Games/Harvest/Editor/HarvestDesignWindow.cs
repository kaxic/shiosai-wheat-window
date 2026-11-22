using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// Keeping the JSON as the source of truth lets designers edit in either Unity or a text editor.
public sealed class HarvestDesignWindow : EditorWindow
{
    const string PathToCatalog = "Assets/Games/Harvest/Resources/Harvest/HarvestCatalog.json";
    sealed class Draft : ScriptableObject { public HarvestCatalog catalog; }
    static readonly string[] Tabs = { "作物", "レシピ", "季節・品評会" };
    static readonly string[] Keys = { "crops", "recipes", "seasons" };
    static readonly string[] Seasons = { "春", "夏", "秋", "冬" };
    Draft draft;
    SerializedObject serialized;
    Vector2 scroll;
    int tab, index;
    string issue, original;

    [MenuItem("潮彩作品集/麦窓食堂のデータ編集")]
    static void Open()
    {
        var win = GetWindow<HarvestDesignWindow>("麦窓食堂のデータ編集");
        win.minSize = new Vector2(440, 380);
    }

    void OnEnable() { Reload(); }
    void OnDisable() { if (draft) DestroyImmediate(draft); }
    void Reload()
    {
        if (!draft) { draft = CreateInstance<Draft>(); draft.hideFlags = HideFlags.HideAndDontSave; }
        try
        {
            original = File.ReadAllText(PathToCatalog);
            draft.catalog = JsonUtility.FromJson<HarvestCatalog>(original);
            draft.catalog.Validate();
            serialized = new SerializedObject(draft);
            issue = null;
        }
        catch (Exception ex) { issue = ex.Message; serialized = null; }
    }

    void OnGUI()
    {
        if (!string.IsNullOrEmpty(issue)) EditorGUILayout.HelpBox(issue, MessageType.Warning);
        if (serialized == null)
        {
            if (GUILayout.Button("ファイルを再読込")) Reload();
            return;
        }
        int chosen = GUILayout.Toolbar(tab, Tabs);
        if (chosen != tab) { tab = chosen; index = 0; scroll = Vector2.zero; }
        serialized.Update();
        var rows = serialized.FindProperty("catalog").FindPropertyRelative(Keys[tab]);
        index = Mathf.Clamp(index, 0, rows.arraySize - 1);
        using (new EditorGUILayout.HorizontalScope())
        {
            var labels = new string[rows.arraySize];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = rows.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
            index = EditorGUILayout.Popup(index, labels);
            if (tab < 2 && GUILayout.Button("複製して追加", GUILayout.Width(116)))
            {
                serialized.ApplyModifiedProperties();
                Undo.RecordObject(draft, "食堂の項目を複製");
                if (tab == 0)
                {
                    var copy = JsonUtility.FromJson<HarvestCrop>(JsonUtility.ToJson(draft.catalog.crops[index]));
                    index = draft.catalog.crops.Length;
                    copy.id += "_" + index; copy.name += "（新規）";
                    Array.Resize(ref draft.catalog.crops, index + 1);
                    draft.catalog.crops[index] = copy;
                }
                else
                {
                    var copy = JsonUtility.FromJson<HarvestRecipe>(JsonUtility.ToJson(draft.catalog.recipes[index]));
                    index = draft.catalog.recipes.Length;
                    copy.id += "_" + index; copy.name += "（新規）";
                    Array.Resize(ref draft.catalog.recipes, index + 1);
                    draft.catalog.recipes[index] = copy;
                }
                serialized.Update();
                rows = serialized.FindProperty("catalog").FindPropertyRelative(Keys[tab]);
            }
        }
        var item = rows.GetArrayElementAtIndex(index);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (tab < 2) Field(item, "id", "保存・参照用ID");
        Field(item, "name", "表示名");
        if (tab == 0)
        {
            var season = item.FindPropertyRelative("season");
            season.intValue = EditorGUILayout.Popup("旬の季節", season.intValue, Seasons);
            Field(item, "days", "成長に必要な日数"); Field(item, "yieldCount", "基本収穫量");
            Field(item, "seedPrice", "種の値段"); Field(item, "sellPrice", "出荷価格");
        }
        else if (tab == 1)
        {
            Field(item, "note", "料理の紹介"); Field(item, "price", "基本価格");
            Field(item, "items", "食材の作物ID"); Field(item, "amounts", "各食材の必要数");
            Field(item, "steps", "調理の手順");
            EditorGUILayout.HelpBox("手順番号は 0 切る、1 煮る、2 焼く、3 和える。食材IDと必要数は同じ順番・件数にします。独自の料理絵は Resources/Harvest/Dishes/ にIDと同名のPNGを置けます。", MessageType.Info);
        }
        else
        {
            Field(item, "festival", "品評会名"); Field(item, "signature", "指定料理のID");
            Field(item, "starter", "旬の支援作物ID"); Field(item, "menu", "季節の注文（料理ID）");
            Field(item, "specials", "特別注文（料理ID）"); Field(item, "guests", "来店客の名前");
            Field(item, "servings", "必要な配膳数");
        }
        EditorGUILayout.EndScrollView();
        serialized.ApplyModifiedProperties();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("記入内容を検証して保存", GUILayout.Height(32))) Save();
            if (GUILayout.Button("再読込", GUILayout.Width(75), GUILayout.Height(32)) &&
                EditorUtility.DisplayDialog("ファイルから再読込", "画面の値を保存済みのデータに戻します。", "再読込", "戻る")) Reload();
        }
    }

    static void Field(SerializedProperty item, string key, string title)
    { EditorGUILayout.PropertyField(item.FindPropertyRelative(key), new GUIContent(title), true); }

    void Save()
    {
        try
        {
            draft.catalog.Validate();
            if (original != File.ReadAllText(PathToCatalog))
                throw new IOException("開いている間にJSONが更新されました。再読込して変更を取り込んでください。");
            string next = JsonUtility.ToJson(draft.catalog, true) + Environment.NewLine;
            string tmp = Path.GetFullPath(PathToCatalog) + ".writing";
            File.WriteAllText(tmp, next, new UTF8Encoding(false));
            File.Replace(tmp, PathToCatalog, null);
            original = next; issue = null;
            AssetDatabase.ImportAsset(PathToCatalog);
            ShowNotification(new GUIContent("保存しました"));
        }
        catch (Exception ex) { issue = ex.Message; }
    }
}
