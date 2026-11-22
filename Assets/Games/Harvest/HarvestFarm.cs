using System;
using System.Collections.Generic;
using UnityEngine;

public enum HarvestMode { Farm, Seeds, Kitchen, Cooking, Service, Market, Ledger, Sleep, Festival }
[Serializable] public sealed class HarvestPlot
{
    public string cropId;
    public int age, duration, dryNights;
    public bool watered;
}
[Serializable] public sealed class HarvestStock
{
    public string id;
    public int seeds, normal, fine;
}
[Serializable] public sealed class HarvestStoredPlot
{
    public int originalIndex;
    public HarvestPlot plant;
}
[Serializable] public sealed class HarvestDish
{
    public string id;
    public int[] grades = new int[3];
    public int cooked;
}
[Serializable] public sealed class HarvestOrder
{
    public string recipeId, guest;
    public bool done;
}
[Serializable] public sealed class HarvestPortion
{
    public string id;
    public int normal, fine;
}
[Serializable] public sealed class HarvestBatch
{
    public string recipeId;
    public List<HarvestPortion> portions = new List<HarvestPortion>();
    public int step, perfect, mistakes;
    public int[] method;
    public float phase;
}
[Serializable] public sealed class HarvestRecord
{
    public int version = 1, day = 1, coins = 16, reputation, totalServed, seasonServed, allStamps, yearStamps, awards, harvests;
    public bool started;
    public string selectedCrop, log = "";
    public HarvestMode mode;
    public Vector2 hero = new Vector2(0, -1.2f), facing = Vector2.up;
    public int[] upgrades = new int[3];
    public HarvestPlot[] plots = new HarvestPlot[12];
    public List<HarvestStoredPlot> storedPlots = new List<HarvestStoredPlot>();
    public List<HarvestStock> stock = new List<HarvestStock>();
    public List<HarvestDish> dishes = new List<HarvestDish>();
    public HarvestOrder[] orders = new HarvestOrder[3];
    public HarvestBatch batch = new HarvestBatch();
}

public sealed class HarvestFarm
{
    public static readonly string[] Tools = {"じょうろ","銅の調理台","食堂の客席"};
    public static readonly string[] ToolNotes = {"周囲の区画も同時に水やり。段階ごとに範囲が1区画広がる。","工程を精密に仕上げやすくなり、黄金の範囲が広がる。","配膳の売上が一皿につき3コインずつ増える。"};
    public static readonly string[] Techniques = {"切る","煮る","焼く","和える"};
    public static readonly string[] Weathers = {"晴れ","雨","曇り"};
    public readonly HarvestCatalog C;
    public HarvestRecord S { get; }
    public int Version { get; private set; }
    public int Season => ((S.day - 1) / 7) % 4;
    public int SeasonDay => (S.day - 1) % 7 + 1;
    public int Year => (S.day - 1) / 28 + 1;
    public int Weather
    {
        get
        {
            if (S.day == 1) return 0;
            uint h = unchecked((uint)S.day * 2654435761u); h ^= h >> 16;
            return h % 7 < 2 ? 1 : h % 7 == 2 ? 2 : 0;
        }
    }
    public HarvestSeason Almanac => C.seasons[Season];
    public int FestivalRecipe => C.RecipeIndex(Almanac.signature);
    public int FestivalQty => Year == 1 ? 1 : 2;
    public int RequiredServings => Year == 1 ? Almanac.servings : 4;
    public bool SeasonStamped => (S.yearStamps & (1 << Season)) != 0;
    public int Selected => C.CropIndex(S.selectedCrop);
    public int CookingRecipe => C.RecipeIndex(S.batch.recipeId);
    public float GoodMin => .38f - .06f * S.upgrades[1];
    public float GoodMax => .62f + .06f * S.upgrades[1];
    public int LastGrade { get; private set; }

    public HarvestFarm(HarvestCatalog catalog, HarvestRecord saved)
    {
        C = catalog; S = saved ?? new HarvestRecord();
        S.day = Mathf.Max(1,S.day); S.coins = Mathf.Max(0,S.coins);
        if (S.upgrades == null || S.upgrades.Length != 3) Array.Resize(ref S.upgrades,3);
        for (int i=0;i<3;i++) S.upgrades[i] = Mathf.Clamp(S.upgrades[i],0,2);
        if (S.plots == null || S.plots.Length < 12) Array.Resize(ref S.plots,12);
        for (int i=0;i<S.plots.Length;i++) S.plots[i] ??= new HarvestPlot();
        S.stock ??= new List<HarvestStock>(); S.dishes ??= new List<HarvestDish>();
        foreach (var c in C.crops) Stock(c.id);
        foreach (var r in C.recipes)
        {
            var d = Dish(r.id);
            if (d.grades == null || d.grades.Length != 3) Array.Resize(ref d.grades,3);
        }
        S.batch ??= new HarvestBatch(); S.batch.portions ??= new List<HarvestPortion>();
        var firstDish=C.recipes[C.RecipeIndex(C.seasons[0].menu[0])];
        string firstIngredient=firstDish.items[0];
        if (Selected < 0) S.selectedCrop = S.started ? Almanac.starter : firstIngredient;
        if (!S.started)
        {
            S.started = true;
            Stock(firstIngredient).seeds += 3; Stock(Almanac.starter).seeds += 3;
            for(int j=0;j<firstDish.items.Length;j++) Stock(firstDish.items[j]).normal+=firstDish.amounts[j]*2;
            Stock(Almanac.starter).normal += 1;
            int initialDays=C.crops[C.CropIndex(firstIngredient)].days;
            S.plots[0] = new HarvestPlot {cropId = firstIngredient, age = initialDays, duration = initialDays};
            S.plots[1] = new HarvestPlot {cropId = Almanac.starter, duration = C.crops[C.CropIndex(Almanac.starter)].days};
            S.log = $"左手の畑に収穫を待つ作物。厨房では最初の「{firstDish.name}」を作れます。";
        }
        ReconcilePlots();
        if (S.orders == null || S.orders.Length != 3 || Array.Exists(S.orders,o => o == null || C.RecipeIndex(o.recipeId) < 0))
        {
            var previous=S.orders;Guests();
            for(int i=0;previous!=null && i<Math.Min(previous.Length,3);i++)
            {
                var old=previous[i];if(old==null)continue;
                if(C.RecipeIndex(old.recipeId)>=0) S.orders[i]=old;
                else S.orders[i].done=old.done;
            }
        }
        CheckBatch();
        if (S.mode == HarvestMode.Cooking && CookingRecipe < 0) S.mode = HarvestMode.Kitchen;
        if (!Enum.IsDefined(typeof(HarvestMode),S.mode)) S.mode = HarvestMode.Farm;
    }
    // A removed crop keeps its complete growth record under its stable ID while the plot remains usable.
    // Reintroducing the ID restores it to its original plot, or the next plot that becomes available.
    void ReconcilePlots()
    {
        S.storedPlots ??= new List<HarvestStoredPlot>();
        for(int i=0;i<12;i++)
        {
            var p=S.plots[i];
            if(string.IsNullOrEmpty(p.cropId) || C.CropIndex(p.cropId)>=0)continue;
            S.storedPlots.Add(new HarvestStoredPlot {originalIndex=i, plant=p});
            S.plots[i]=new HarvestPlot();S.log="定義から外れた作物を、生育状態のまま預かりました。";
        }
        for(int i=0;i<S.storedPlots.Count;)
        {
            var item=S.storedPlots[i];
            if(item?.plant==null || string.IsNullOrEmpty(item.plant.cropId)){S.storedPlots.RemoveAt(i);continue;}
            if(C.CropIndex(item.plant.cropId)<0){i++;continue;}
            int target=item.originalIndex;
            if(target<0 || target>=12 || !string.IsNullOrEmpty(S.plots[target].cropId))
            {
                target=-1;
                for(int j=0;j<12;j++)if(string.IsNullOrEmpty(S.plots[j].cropId)){target=j;break;}
            }
            if(target<0){i++;continue;}
            S.plots[target]=item.plant;S.storedPlots.RemoveAt(i);
        }
    }
    HarvestStock Stock(string id)
    {
        foreach (var stock in S.stock) if (stock != null && stock.id == id) return stock;
        var item = new HarvestStock {id=id}; S.stock.Add(item); return item;
    }
    HarvestDish Dish(string id)
    {
        foreach (var dish in S.dishes) if (dish != null && dish.id == id) return dish;
        var item = new HarvestDish {id=id}; S.dishes.Add(item); return item;
    }
    public int Seeds(int i) => Stock(C.crops[i].id).seeds;
    public int Fine(int i) => Stock(C.crops[i].id).fine;
    public int Produce(int i) { var stock=Stock(C.crops[i].id); return stock.normal + stock.fine; }
    public int DishStock(int i) { var a=Dish(C.recipes[i].id).grades; return a[0]+a[1]+a[2]; }
    public int Grade(int i)
    {
        var a=Dish(C.recipes[i].id).grades;
        for (int g=2;g>=0;g--) if (a[g]>0) return g;
        return -1;
    }
    public bool Mastered(int i) => Dish(C.recipes[i].id).cooked>0;
    public int MasteredCount { get { int n=0; for(int i=0;i<C.recipes.Length;i++) if(Mastered(i)) n++; return n; } }
    public int PlotCrop(int i) => C.CropIndex(S.plots[i].cropId);
    public bool Ripe(int i) => PlotCrop(i)>=0 && S.plots[i].age>=S.plots[i].duration;
    public int Duration(int crop) => C.crops[crop].days + (C.crops[crop].season==Season ? 0 : 1);
    public bool PickSeed(int i)
    {
        if (i<0 || i>=C.crops.Length) return false;
        S.selectedCrop=C.crops[i].id; S.log = C.crops[i].name+"の種を選んだ。空き区画でE。";
        S.mode=HarvestMode.Farm; Version++; return true;
    }
    public bool Tend(int i)
    {
        if (S.mode!=HarvestMode.Farm || i<0 || i>=12) return false;
        var p=S.plots[i]; int crop=PlotCrop(i);
        if (crop < 0)
        {
            var stock=Stock(S.selectedCrop);
            if (stock.seeds<=0) {S.log="種がありません。売店で購入するか、翌朝の無料の種を使おう。";return false;}
            stock.seeds--; S.plots[i]=new HarvestPlot {cropId=stock.id, duration=Duration(Selected), watered=Weather==1};
            S.log=C.crops[Selected].name+"の種をまいた。"+(Weather==1?"雨が畑を潤している。":"もう一度Eで水やり。");
        }
        else if (Ripe(i))
        {
            var c=C.crops[crop]; var stock=Stock(c.id); bool fine=p.dryNights==0 && c.season==Season;
            if (fine) stock.fine+=c.yieldCount; else stock.normal+=c.yieldCount;
            S.harvests++; S.plots[i]=new HarvestPlot();
            S.log=$"{c.name}を{c.yieldCount}個収穫。"+(fine?"旬の上質な食材です。":"厨房で使えます。");
            if(S.storedPlots.Count>0)ReconcilePlots();
        }
        else
        {
            if (p.watered) {S.log="今日の水やりは済んでいます。就寝すると成長します。";return false;}
            int count=0;
            for(int j=0;j<12;j++)
            {
                var bed=S.plots[j];
                if (PlotCrop(j)<0 || Ripe(j) || bed.watered || Math.Abs(i%4-j%4)+Math.Abs(i/4-j/4)>S.upgrades[0]) continue;
                bed.watered=true; count++;
            }
            S.log=$"{count}区画に水をやった。";
        }
        Version++;return true;
    }
    public bool BuySeed(int i)
    {
        if (i<0 || i>=C.crops.Length || S.coins<C.crops[i].seedPrice) return false;
        var c=C.crops[i];S.coins-=c.seedPrice;Stock(c.id).seeds++;S.log=c.name+"の種を1袋購入。";Version++;return true;
    }
    public int CropSale(int i) => C.crops[i].sellPrice + (Stock(C.crops[i].id).normal==0 && Fine(i)>0 ? 1 : 0);
    public bool SellCrop(int i)
    {
        if (i<0 || i>=C.crops.Length || Produce(i)<=0) return false;
        var item=Stock(C.crops[i].id);int price=CropSale(i);
        if(item.normal>0)item.normal--;else item.fine--;
        S.coins+=price;S.log=$"{C.crops[i].name}を出荷。売上 {price}コイン。";Version++;return true;
    }
    public int Cost(int i) => i<0 || i>=3 || S.upgrades[i]>=2 ? 0 : (S.upgrades[i]==0?22:46)+i*4;
    public bool CanUpgrade(int i) => Cost(i)>0 && S.coins>=Cost(i);
    public bool Upgrade(int i)
    {
        if(!CanUpgrade(i))return false;
        S.coins-=Cost(i);S.upgrades[i]++;S.log=Tools[i]+$"を{S.upgrades[i]}段階に強化。";Version++;return true;
    }
    public bool CanCook(int index)
    {
        if(index<0 || index>=C.recipes.Length || CookingRecipe>=0) return false;
        var r=C.recipes[index];
        for(int j=0;j<r.items.Length;j++)if(Produce(C.CropIndex(r.items[j]))<r.amounts[j])return false;
        return true;
    }
    public bool StartBatch(int index)
    {
        if (!CanCook(index)) return false;
        var r=C.recipes[index];var batch=new HarvestBatch { recipeId=r.id, method=(int[])r.steps.Clone() };
        for(int j=0;j<r.items.Length;j++)
        {
            var item=Stock(r.items[j]);int fine=Math.Min(item.fine,r.amounts[j]), normal=r.amounts[j]-fine;
            batch.portions.Add(new HarvestPortion {id=item.id,normal=normal,fine=fine});item.fine-=fine;item.normal-=normal;
        }
        S.batch=batch;S.mode=HarvestMode.Cooking;S.log=r.name+"の調理を始めた。";Version++;return true;
    }
    public bool CancelBatch()
    {
        if(string.IsNullOrEmpty(S.batch.recipeId))return false;
        foreach(var reserved in S.batch.portions)
        {
            if(reserved==null || string.IsNullOrEmpty(reserved.id))continue;
            var item=Stock(reserved.id);item.normal+=Math.Max(0,reserved.normal);item.fine+=Math.Max(0,reserved.fine);
        }
        S.batch=new HarvestBatch();S.mode=HarvestMode.Kitchen;S.log="調理を取り消し、預けた食材を品質ごとに戻しました。";Version++;return true;
    }
    bool CheckBatch()
    {
        var b=S.batch;
        if(string.IsNullOrEmpty(b.recipeId))return false;
        bool valid=CookingRecipe>=0;
        if(valid)
        {
            var r=C.recipes[CookingRecipe];
            valid=b.step>=0 && b.step<r.steps.Length && b.portions.Count==r.items.Length;
            if(valid && b.method!=null && b.method.Length>0)
            {
                valid=b.method.Length==r.steps.Length;
                for(int j=0;valid && j<b.method.Length;j++)valid=b.method[j]==r.steps[j];
            }
            for(int j=0;valid && j<r.items.Length;j++)
            {
                var item=b.portions.Find(p=>p!=null && p.id==r.items[j]);
                valid=item!=null && item.normal>=0 && item.fine>=0 && (long)item.normal+item.fine==r.amounts[j];
            }
            // Older saves did not store the method. Keep a valid step with matching reservations.
            if(valid)
            {
                if(b.method==null || b.method.Length==0)b.method=(int[])r.steps.Clone();
                return true;
            }
        }
        var previous=S.mode;CancelBatch();
        if(previous!=HarvestMode.Cooking)S.mode=previous;
        S.log="料理の定義が変わったため、預かっていた食材を元の品質で食材庫に戻しました。";
        return false;
    }
    // The beat is optional. A correctly followed method always completes the recipe.
    public bool Technique(int technique, float beat)
    {
        int index=CookingRecipe;
        if(S.mode!=HarvestMode.Cooking || !CheckBatch() || index<0 || technique<0 || technique>=Techniques.Length)return false;
        var b=S.batch;var r=C.recipes[index];
        if(technique!=r.steps[b.step]) {b.mistakes++;S.log="次の工程は「"+Techniques[r.steps[b.step]]+"」。食材はそのままです。";Version++;return false;}
        if(beat>=GoodMin && beat<=GoodMax)b.perfect++;
        b.step++;b.phase=0;Version++;
        if(b.step<r.steps.Length){S.log="次は「"+Techniques[r.steps[b.step]]+"」。";return true;}
        int fine=0; foreach(var item in b.portions)fine+=item.fine;
        int grade=(b.mistakes==0 || fine>0)?1:0;
        if(b.mistakes==0 && (b.perfect*3>=2*r.steps.Length || (fine>0 && b.perfect>0)))grade=2;
        var meal=Dish(r.id);meal.grades[grade]++;meal.cooked++;LastGrade=grade;
        S.log=r.name+"が完成。"+new[]{"家庭の味","上等の仕上がり","特選の仕上がり"}[grade]+"！";
        S.batch=new HarvestBatch();S.mode=HarvestMode.Kitchen;return true;
    }
    void Guests()
    {
        var a=Almanac; int n=SeasonDay-1;
        // The first guest always requests one of this season's two single-crop dishes.
        string[] ids={a.menu[n%2],a.menu[(n+1)%a.menu.Length],a.specials[n%a.specials.Length]};
        S.orders=new HarvestOrder[3];
        for(int i=0;i<3;i++)S.orders[i]=new HarvestOrder {recipeId=ids[i],guest=a.guests[(n+i)%a.guests.Length]};
    }
    public int OrderRecipe(int i)=>C.RecipeIndex(S.orders[i].recipeId);
    public int OrderPrice(int i)
    {
        int recipe=OrderRecipe(i);int grade=Math.Max(0,Grade(recipe));
        return C.recipes[recipe].price + grade*3 + 3*S.upgrades[2] + (i==2?2:0);
    }
    public bool CanServe(int i) => i>=0 && i<3 && !S.orders[i].done && DishStock(OrderRecipe(i))>0;
    public bool Serve(int i)
    {
        if(!CanServe(i))return false;
        int recipe=OrderRecipe(i),grade=Grade(recipe),price=OrderPrice(i);
        Dish(C.recipes[recipe].id).grades[grade]--;S.orders[i].done=true;S.coins+=price;
        S.reputation+=grade==2?2:1;S.totalServed++;S.seasonServed++;
        S.log=$"{S.orders[i].guest}に配膳。売上 {price}コイン、評判 +{(grade==2?2:1)}。";
        Version++;return true;
    }
    public int EligibleFestivalDishes {get{var g=Dish(Almanac.signature).grades;return g[1]+g[2];}}
    public bool CanSubmit => !SeasonStamped && S.seasonServed>=RequiredServings && EligibleFestivalDishes>=FestivalQty;
    public bool Submit()
    {
        if(!CanSubmit)return false;
        var g=Dish(Almanac.signature).grades;int plain=Math.Min(FestivalQty,g[1]);
        g[1]-=plain;g[2]-=FestivalQty-plain;S.yearStamps|=1<<Season;S.allStamps|=1<<Season;S.awards++;
        int prize=25+Season*5+(Year>1?15:0);S.coins+=prize;S.reputation+=5;
        S.log=$"{Almanac.festival}の賞印を獲得。賞金 {prize}コイン。"+(S.allStamps==15?"四季の賞印が揃いました！":"");
        Version++;return true;
    }
    public bool Sleep()
    {
        if(S.mode!=HarvestMode.Sleep || S.day==int.MaxValue)return false;
        int oldSeason=Season,oldYear=Year;
        for(int i=0;i<12;i++)
        {
            var p=S.plots[i];
            if(PlotCrop(i)>=0 && !Ripe(i)){if(p.watered)p.age++;else p.dryNights++;}
        }
        S.day++;
        for(int i=0;i<12;i++)S.plots[i].watered=Weather==1 && PlotCrop(i)>=0;
        if(Season!=oldSeason)S.seasonServed=0;
        if(Year!=oldYear)S.yearStamps=0;
        Stock(Almanac.starter).seeds++;
        if(Season!=oldSeason)
        {
            foreach(var crop in C.crops)if(crop.season==Season)Stock(crop.id).seeds+=2;
            S.selectedCrop=Almanac.starter;
        }
        Guests();S.mode=HarvestMode.Farm;S.hero=new Vector2(0,-1.2f);S.facing=Vector2.up;
        S.log=$"{Year}年目・{Almanac.name}{SeasonDay}日。{Weathers[Weather]}。朝の種が届きました。";
        Version++;return true;
    }
}
