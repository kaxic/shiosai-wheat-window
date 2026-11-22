using System.Collections.Generic;
using UnityEngine;

public static class HarvestDishArt
{
    static readonly Dictionary<string, Texture2D> Overrides = new Dictionary<string, Texture2D>();
    static Texture2D housePlate;
    static readonly string[] OriginalIds = {
        "roll", "white_soup", "quiche", "tomato_sauce", "pea_soup", "ratatouille",
        "roast_pumpkin", "corn_soup", "gratin", "cabbage_steak", "carrot_soup", "pot_au_feu",
        "pizza", "harvest_pie", "carrot_cake", "galette"
    };

    public static void Draw(HarvestGame game, Rect rect, int index)
    {
        if (Event.current.type != EventType.Repaint || index < 0 || index >= game.Recipes.Length) return;
        string id = game.Recipes[index].id;
        if (!Overrides.TryGetValue(id, out var texture))
        {
            texture = Resources.Load<Texture2D>("Harvest/Dishes/" + id);
            Overrides.Add(id, texture);
        }
        if (texture)
        {
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            return;
        }
        int cell = System.Array.IndexOf(OriginalIds, id);
        if (cell >= 0) game.Plate(rect, cell, 4, 4);
        else
        {
            game.Box(rect, game.Paper);
            if (!housePlate) housePlate = Resources.Load<Texture2D>("Harvest/MenuPlate");
            if (housePlate) GUI.DrawTexture(rect, housePlate, ScaleMode.ScaleToFit);
        }
    }
}
