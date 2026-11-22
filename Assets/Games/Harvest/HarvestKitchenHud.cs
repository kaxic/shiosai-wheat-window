using UnityEngine;

public sealed partial class HarvestHud
{
    void Kitchen()
    {
        Rect book = Book(988, 561, "THE RECIPE BOOK", "厨房の献立帳");
        float left = book.x + 20, top = book.y + 110;
        float gridWidth = 578;
        int count = game.Recipes.Length;
        bool compact = Screen.width < 800;
        int pageSize = compact ? 8 : 16;
        int first = Mathf.Clamp(game.Selection, 0, count-1) / pageSize * pageSize;
        Label($"献立 {count} 品　／　習熟 {game.MasteredCount} 品", left, book.y + 74, gridWidth, 29, 15, Moss);
        float cardWidth = (gridWidth - 18) / 4, cardHeight = compact ? 182 : 88;
        for (int i = first; i < Mathf.Min(first + pageSize, count); i++)
        {
            int cell = i - first;
            Rect r = new Rect(left + cell % 4 * (cardWidth + 6), top + cell / 4 * (cardHeight + 6), cardWidth, cardHeight);
            bool selected = game.Selection == i;
            Row(r, selected);
            float imageHeight = compact ? 114 : 57;
            Dish(new Rect(r.x + 5, r.y + 4, r.width - 10, imageHeight), i);
            if (selected) Fill(new Rect(r.x, r.y, 3, r.height), Forest);
            if (game.Mastered(i)) Fill(new Rect(r.xMax - 12, r.y + 5, 7, 7), Honey);
            string name = game.Recipes[i].name;
            int size = compact ? 16 : Mathf.Clamp(Mathf.FloorToInt((r.width-10) / Mathf.Max(1,name.Length)),11,14);
            Label(name, r.x + 4, r.y + imageHeight+4, r.width - 8, r.height-imageHeight-8, size, Ink, align: TextAnchor.MiddleCenter);
            if (Hit(r)) game.Highlight(i);
        }
        float cx = left + gridWidth + 17, cw = book.xMax - 20 - cx;
        Fill(new Rect(cx-8,book.y+84,1,book.height-105),Edge);
        int selectedRecipe = Mathf.Clamp(game.Selection, 0, count - 1);
        var recipe = game.Recipes[selectedRecipe];
        Dish(new Rect(cx, book.y + 84, cw, 116), selectedRecipe);
        Label(recipe.name, cx + 1, book.y + 205, cw - 2, 36, 24, Ink, true);
        Label(recipe.note, cx + 2, book.y + 245, cw - 4, 44, 14, Moss);
        Rule(cx, book.y + 294, cw);
        Label("必要な食材", cx + 1, book.y + 298, cw - 3, 26, 15, Forest);
        Label(game.RecipeNeeds(selectedRecipe), cx + 2, book.y + 326, cw - 4, 69, 14, Ink);
        Rule(cx, book.y + 400, cw);
        Label("工程　" + Steps(recipe.steps), cx + 1, book.y + 406, cw - 3, 44, 14, Moss);
        Label($"基本価格 {recipe.price} G　／　料理棚 {game.DishStock(selectedRecipe)} 皿", cx + 1, book.y + 455, cw - 2, 28, 14, Ink);
        bool can = game.CanCook(selectedRecipe);
        if (Button(new Rect(cx, book.yMax - 58, cw, 39), can ? "この料理を作る" : "食材が足りません", can))
            game.StartCooking(selectedRecipe);
        if (count > pageSize)
        {
            if (Button(new Rect(left, book.yMax - 52, 100, 33), "前の頁", first > 0, false)) game.Highlight(first-pageSize);
            if (Button(new Rect(left+108, book.yMax - 52, 100, 33), "次の頁", first+pageSize < count, false)) game.Highlight(first+pageSize);
            Label($"{first/pageSize+1} / {(count+pageSize-1)/pageSize} 頁",left+221,book.yMax-52,113,33,14,Moss);
        }
        else Label("矢印で献立を選ぶ　／　Enter で調理",left,book.yMax-54,gridWidth,36,14,Moss);
    }

    static string Steps(int[] steps)
    {
        if (steps == null || steps.Length == 0) return "";
        var labels = new string[steps.Length];
        for (int i = 0; i < labels.Length; i++) labels[i] = Motions[Mathf.Clamp(steps[i], 0, Motions.Length-1)];
        return string.Join(" → ", labels);
    }

    void Cooking()
    {
        int id = game.CookingRecipe;
        if (id < 0 || id >= game.Recipes.Length) return;
        var recipe = game.Recipes[id];
        int stage = Mathf.Clamp(game.CookStep, 0, recipe.steps.Length-1);
        int technique = recipe.steps[stage];
        Rect book = Book(814, 453, "AT THE STOVE", "今日の仕込み");
        float x = book.x + 20, y = book.y + 84;
        Dish(new Rect(x, y, 243, 150), id);
        Label(recipe.name, x + 1, y + 156, 241, 37, 22, Ink, true);
        Label($"精密な工程　{game.PerfectSteps} 回", x + 2, y + 200, 239, 27, 15, Forest);
        Label("途中で閉じても材料と進み具合は残ります。厨房を開いて続きを作れます。", x + 1, y + 244, 240, 85, 14, Moss);
        float sx = x + 263, sw = book.xMax - 20 - sx;
        Fill(new Rect(sx,y,sw,84), Forest);
        Label($"工程 {stage+1} / {recipe.steps.Length}",sx+13,y+5,sw-26,24,14,Paper);
        Label(Motions[technique],sx+12,y+32,sw-24,43,27,Paper,true);
        Label(Steps(recipe.steps),sx+1,y+94,sw-2,31,15,Ink);
        Label("針が金色の帯に入ったら、今の工程を押す",sx+1,y+136,sw-2,28,15,Forest);
        float bx = sx + 3, by = y + 175, width = sw - 6;
        Fill(new Rect(bx,by,width,15),Edge);
        Fill(new Rect(bx+width*game.GoodBeatMin,by,width*(game.GoodBeatMax-game.GoodBeatMin),15),Honey);
        float cursor = bx + Mathf.Clamp01(game.CookBeat) * width;
        Fill(new Rect(cursor-2,by-7,4,29),Ink);
        Fill(new Rect(cursor-5,by-7,10,4),Ink);
        float buttonWidth = (sw - 18) / 4;
        for (int i=0; i<Motions.Length; i++)
        {
            Rect r = new Rect(sx+i*(buttonWidth+6), y+216, buttonWidth, 43);
            if (Button(r, $"{i+1}　{Motions[i]}", true, game.Selection==i)) game.Technique(i);
        }
        Label("数字 1〜4 または矢印＋Enter で調理",sx+1,y+270,sw-2,34,14,Moss);
        Rule(sx,book.yMax-70,sw);
        if(Button(new Rect(sx,book.yMax-56,sw*.48f,38),"材料を戻して中止",true,false))game.CancelCooking();
        if(Button(new Rect(sx+sw*.5f,book.yMax-56,sw*.50f,38),"いったん中断",true,false))game.Back();
    }

    void Service()
    {
        Rect book = Book(864, 430, "THE LUNCH COUNTER", "今日の注文票");
        float left = book.x + 20;
        Label($"{game.SeasonName} {game.SeasonDay} 日　／　配膳 {game.ServedToday} / 3", left, book.y + 74, book.width - 40, 29, 15, Moss);
        float cardWidth=(book.width-56)/3;
        for(int i=0;i<3;i++)
        {
            int dish=game.OrderRecipe(i);
            Rect r=new Rect(left+i*(cardWidth+8),book.y+117,cardWidth,237);
            bool done=game.OrderDone(i);
            Row(r,game.Selection==i);
            Label(game.OrderName(i),r.x+9,r.y+3,r.width-18,24,14,Forest);
            Dish(new Rect(r.x+8,r.y+31,r.width-16,91),dish);
            Label(game.Recipes[dish].name,r.x+9,r.y+128,r.width-18,32,19,Ink,true);
            Label(done?"配膳しました":$"注文の代金　{game.OrderPrice(i)} G",r.x+10,r.y+164,r.width-20,26,15,done?Forest:Ink);
            int stock=game.DishStock(dish);
            Label(stock>0?$"料理棚 {stock} 皿　最良 {Grade(game.DishGrade(dish))}":"料理棚　0 皿",r.x+10,r.y+195,r.width-20,28,14,Moss);
            if(Hit(r)) game.Highlight(i);
        }
        int order=Mathf.Clamp(game.Selection,0,2);
        Rect action=new Rect(book.xMax-219,book.yMax-57,199,38);
        if(game.OrderDone(order))
            Button(action,"配膳済",false);
        else if(game.CanServe(order))
        {
            if(Button(action,"このお客様に配膳",true))game.Serve(order);
        }
        else if(Button(action,"厨房でこの料理を見る",true,false))
        {
            int requested=game.OrderRecipe(order);
            game.Open(HarvestMode.Kitchen);
            if(game.Mode==HarvestMode.Kitchen)game.Highlight(requested);
        }
        Label($"選んだ注文　{game.OrderName(order)}",left,book.yMax-56,book.width-258,35,15,Ink);
    }
}
