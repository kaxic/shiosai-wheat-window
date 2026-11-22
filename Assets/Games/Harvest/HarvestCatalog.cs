using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class HarvestCrop
{
    public string id, name;
    public int season, days, seedPrice, sellPrice, yieldCount;
}

[Serializable]
public sealed class HarvestRecipe
{
    public string id, name, note;
    public int price;
    public string[] items;
    public int[] amounts, steps;
}

[Serializable]
public sealed class HarvestSeason
{
    public string name, festival, signature, starter;
    public string[] menu, specials, guests;
    public int servings;
}

[Serializable]
public sealed class HarvestCatalog
{
    public HarvestCrop[] crops;
    public HarvestRecipe[] recipes;
    public HarvestSeason[] seasons;
    [NonSerialized] Dictionary<string,int> cropIds, recipeIds;

    public static HarvestCatalog Load()
    {
        var file = Resources.Load<TextAsset>("Harvest/HarvestCatalog");
        if (!file) throw new InvalidOperationException("Harvest/HarvestCatalog.json が見つかりません。");
        var catalog = JsonUtility.FromJson<HarvestCatalog>(file.text);
        catalog.Validate(); return catalog;
    }
    public int CropIndex(string id) => id != null && cropIds.TryGetValue(id,out int i) ? i : -1;
    public int RecipeIndex(string id) => id != null && recipeIds.TryGetValue(id,out int i) ? i : -1;
    public void Validate()
    {
        cropIds = new Dictionary<string,int>(StringComparer.Ordinal); recipeIds = new Dictionary<string,int>(StringComparer.Ordinal);
        if (crops == null || crops.Length == 0 || recipes == null || recipes.Length == 0 || seasons == null || seasons.Length != 4)
            throw new InvalidOperationException("作物、料理、春から冬までの4季を登録してください。");
        for (int i=0;i<crops.Length;i++)
        {
            var c = crops[i];
            if (c == null || string.IsNullOrWhiteSpace(c.id) || string.IsNullOrWhiteSpace(c.name) || c.season < 0 || c.season > 3 ||
                c.days < 1 || c.yieldCount < 1 || c.seedPrice < 1 || c.sellPrice < 1) throw new InvalidOperationException($"作物の{i+1}番目の名前、日数または価格を確認してください。");
            if (cropIds.ContainsKey(c.id)) throw new InvalidOperationException("重複した作物IDがあります。「" + c.id + "」");
            cropIds.Add(c.id,i);
        }
        for (int i=0;i<recipes.Length;i++)
        {
            var r = recipes[i];
            if (r == null || string.IsNullOrWhiteSpace(r.id) || string.IsNullOrWhiteSpace(r.name) || r.items == null || r.amounts == null || r.items.Length == 0 ||
                r.items.Length != r.amounts.Length || r.steps == null || r.steps.Length == 0 || r.price < 1)
                throw new InvalidOperationException($"レシピの{i+1}番目の名前、材料、手順または価格を確認してください。");
            if (recipeIds.ContainsKey(r.id)) throw new InvalidOperationException("重複した料理IDがあります。「" + r.id + "」");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int j=0;j<r.items.Length;j++) if (CropIndex(r.items[j]) < 0 || r.amounts[j] < 1 || !seen.Add(r.items[j]))
                throw new InvalidOperationException("料理の食材: " + r.id);
            foreach (int step in r.steps) if (step < 0 || step > 3) throw new InvalidOperationException("料理の手順: " + r.id);
            recipeIds.Add(r.id,i);
        }
        foreach (var s in seasons)
        {
            if (s == null || string.IsNullOrWhiteSpace(s.name) || string.IsNullOrWhiteSpace(s.festival) || s.menu == null || s.menu.Length < 2 || s.specials == null || s.specials.Length == 0 || s.guests == null ||
                s.guests.Length < 3 || CropIndex(s.starter) < 0 || RecipeIndex(s.signature) < 0 || s.servings < 1)
                throw new InvalidOperationException("季節の名前、品評会、指定料理と来店客の設定を確認してください。");
            foreach (string id in s.menu) if (RecipeIndex(id) < 0) throw new InvalidOperationException("季節の料理: " + id);
            foreach (string id in s.specials) if (RecipeIndex(id) < 0) throw new InvalidOperationException("特別注文: " + id);
        }
    }
}
