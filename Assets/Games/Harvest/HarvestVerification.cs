using System;
using UnityEngine;

public static class HarvestVerification
{
    public static string Showcase {get;private set;}
    public static HarvestRecord Run(HarvestCatalog catalog)
    {
        TestTerrace(catalog);
        TestKitchen(catalog);TestSeasonsAndRecovery(catalog);TestCatalogEdits(catalog);
        var farm=new HarvestFarm(catalog,new HarvestRecord());
        int score=-1,completedYear=0,visits=0;
        Showcase=null;
        for(int d=1;d<=35;d++)
        {
            Check(farm.S.day==d,"calendar advances once");
            int a=-1,b=-1;
            for(int c=0;c<catalog.crops.Length;c++)if(catalog.crops[c].season==farm.Season){if(a<0)a=c;else if(b<0)b=c;}
            Check(a>=0 && b>=0,"seasonal crops");

            void Snapshot()
            {
                if(farm.Season!=2 || !farm.SeasonStamped)return;
                int present=0,ripe=0,hasDay=0;
                for(int j=0;j<12;j++)if(farm.PlotCrop(j)>=0){present++;if(farm.Ripe(j))ripe++;else if(farm.S.plots[j].age>0)hasDay++;}
                int value=present*2+Math.Min(5,ripe)*6+Math.Min(4,hasDay)*4;
                if(value>score){score=value;Showcase=JsonUtility.ToJson(farm.S);}
            }
            Snapshot();
            for(int plot=0;plot<HarvestFarm.BedCount;plot++)
            {
                if(farm.Ripe(plot))Check(farm.Tend(plot),"harvest");
                if(farm.PlotCrop(plot)<0)
                {
                    // Rotate crops and sow some on later days to keep the field in different stages.
                    if(plot>=8 && plot<12 && (d+plot)%3!=0)continue;
                    int crop=plot%2==0?a:b;
                    if(plot>=12 && catalog.crops.Length>=16)crop=8+(plot-12+d)%8;
                    if(farm.Seeds(crop)==0 && farm.S.coins>=catalog.crops[crop].seedPrice+3)farm.BuySeed(crop);
                    if(farm.Seeds(crop)==0)continue;
                    Check(farm.PickSeed(crop) && farm.Tend(plot),"plant purchased or gifted seed");
                }
                if(!farm.Ripe(plot) && !farm.S.plots[plot].watered)Check(farm.Tend(plot),"irrigate");
            }
            int signature=farm.FestivalRecipe;
            while(!farm.SeasonStamped && farm.EligibleFestivalDishes<farm.FestivalQty && farm.CanCook(signature))Cook(farm,signature,perfect:false);
            for(int slot=0;slot<3;slot++)
            {
                int recipe=farm.OrderRecipe(slot);
                bool reserved=!farm.SeasonStamped && recipe==signature && farm.EligibleFestivalDishes<=farm.FestivalQty;
                if((reserved || farm.DishStock(recipe)==0) && farm.CanCook(recipe))Cook(farm,recipe,perfect:true);
                if(!farm.CanServe(slot) || (!farm.SeasonStamped && recipe==signature && farm.EligibleFestivalDishes<=farm.FestivalQty))continue;
                int wallet=farm.S.coins,price=farm.OrderPrice(slot),before=farm.DishStock(recipe);
                Check(farm.Serve(slot) && !farm.Serve(slot) && farm.S.coins==wallet+price && farm.DishStock(recipe)==before-1,"order paid once");
                visits++;
            }
            if(farm.CanSubmit)
            {
                int stash=farm.EligibleFestivalDishes;
                Check(farm.Submit() && !farm.Submit() && farm.EligibleFestivalDishes==stash-farm.FestivalQty,"one seasonal award");
                var saved=farm.S;farm=Restore(farm);
                Check(farm.SeasonStamped && farm.S.yearStamps==saved.yearStamps && farm.S.totalServed==saved.totalServed,"award and service resume");
            }
            for(int recipe=0;recipe<catalog.recipes.Length;recipe++)if(!farm.Mastered(recipe) && farm.CanCook(recipe))Cook(farm,recipe,perfect:false);
            for(int tool=0;tool<3;tool++)
            {
                if(!farm.CanUpgrade(tool) || farm.S.coins<farm.Cost(tool)+24)continue;
                int old=farm.S.coins,cost=farm.Cost(tool),level=farm.S.upgrades[tool];
                Check(farm.Upgrade(tool) && farm.S.coins==old-cost && farm.S.upgrades[tool]==level+1,"upgrade cost and benefit");
            }
            Snapshot();
            if(farm.SeasonDay==7)
            {
                Check(farm.SeasonStamped,"festival met before season end "+farm.Season);
                if(d==28)completedYear=farm.S.awards;
            }
            if(d==35)break;
            farm.S.mode=HarvestMode.Sleep; Check(farm.Sleep() && !farm.Sleep(),"explicit sleep required for advancing time");
        }
        var fieldCopy=Restore(farm);
        Check(fieldCopy.S.plots.Length>=24 && fieldCopy.S.plots[12]!=null,"terrace beds survive reload");
        Check(completedYear==4 && farm.Year==2 && farm.FestivalQty==2 && farm.RequiredServings==4 &&
            farm.S.allStamps==15 && farm.S.awards==5 && farm.S.yearStamps==1,"all first-year reviews and return festival");
        Check(farm.MasteredCount==catalog.recipes.Length && farm.S.totalServed==visits && farm.S.reputation>visits,"published menu and reputation");
        Check(Array.TrueForAll(farm.S.upgrades,level=>level==2) && !farm.Upgrade(0),"max upgrades");
        var final=Restore(farm);
        Check(final.MasteredCount==farm.MasteredCount && final.S.coins==farm.S.coins && final.S.awards==5,"postgame full ledger");
        Debug.Log($"Harvest verification passed: {farm.S.day} days, {farm.S.harvests} harvests, {visits} served orders, " +
            $"{farm.MasteredCount} recipes, four first-year seasonal stamps and one second-year review.");
        return final.S;
    }

    static void TestTerrace(HarvestCatalog catalog)
    {
        var f=new HarvestFarm(catalog,new HarvestRecord());
        int target=HarvestFarm.BedsPerField;
        Check(f.Tend(target) && f.Tend(target),"plant and water a terrace bed");
        string planted=f.S.plots[target].cropId;
        int firstAge=f.S.plots[0].age;
        f.S.field=0;f.S.mode=HarvestMode.Sleep;Check(f.Sleep(),"grow both fields");
        var restored=Restore(f);
        Check(restored.S.plots[target].cropId==planted && restored.S.plots[target].age==1 &&
              restored.S.plots[target].watered && restored.S.plots[0].age==firstAge,
              "terrace grows away from home and its channel irrigates on even mornings");
        var v1=JsonUtility.FromJson<HarvestRecord>(JsonUtility.ToJson(f.S));
        Array.Resize(ref v1.plots,HarvestFarm.BedsPerField);v1.field=0;
        var expanded=new HarvestFarm(catalog,v1);
        Check(expanded.S.plots.Length==HarvestFarm.BedCount && expanded.S.plots[0].age==firstAge
              && expanded.PlotCrop(target)<0,"old twelve-bed save keeps the first field");
    }
    static void TestKitchen(HarvestCatalog c)
    {
        var f=new HarvestFarm(c,new HarvestRecord());
        int wheat=c.CropIndex("wheat"),turnip=c.CropIndex("turnip"),mix=c.RecipeIndex("quiche");
        Check(f.Ripe(0) && f.Tend(0),"first ready crop");
        int harvested=f.S.harvests,stock=f.Produce(wheat),quality=f.Fine(wheat);
        f.Tend(0);
        Check(f.Produce(wheat)==stock && f.S.harvests==harvested && !f.Ripe(0),"no duplicate harvest");
        int vegetables=f.Produce(turnip);
        Check(!f.StartBatch(c.RecipeIndex("gratin")) && f.Produce(wheat)==stock,"reject insufficient ingredients atomically");
        Check(f.StartBatch(mix) && !f.StartBatch(mix) && f.Produce(wheat)==stock-1,"one reservation");
        int step=c.recipes[mix].steps[0];
        Check(!f.Technique((step+1)%4,.5f) && f.S.batch.step==0 && f.S.batch.mistakes==1,"wrong method preserves batch");
        Check(f.Technique(step,.5f) && f.S.batch.perfect==1,"optional precision");
        f.S.batch.phase=.4f;f=Restore(f);
        Check(f.CookingRecipe==mix && f.S.batch.step==1 && f.S.batch.perfect==1 && Mathf.Abs(f.S.batch.phase-.4f)<.001f,"resume timed cooking");
        Check(f.CancelBatch() && !f.CancelBatch() && f.Produce(wheat)==stock && f.Fine(wheat)==quality &&
            f.Produce(turnip)==vegetables,"cancel restores exact stock quality once");
        Cook(f,mix,true);
        Check(f.DishStock(mix)==1 && f.Grade(mix)==2,"precision dish ready");
        Check(!f.Technique(0,.5f) && f.DishStock(mix)==1,"no duplicate finish");
        int roll=c.RecipeIndex("roll");Cook(f,roll,false);
        Check(f.Grade(roll)==1 && f.OrderRecipe(0)==roll && f.Serve(0) && !f.Serve(0),"day-one playable service without precision timing");
        int balance=f.S.coins, before=f.Produce(wheat);
        if(before>0){int price=f.CropSale(wheat);Check(f.SellCrop(wheat) && f.S.coins==balance+price && f.Produce(wheat)==before-1,"raw sale");}
    }
    static void TestSeasonsAndRecovery(HarvestCatalog c)
    {
        // A penniless, empty garden can recover from the morning gift through the same commands as normal play.
        var f=new HarvestFarm(c,new HarvestRecord {started=true, coins=0,day=1});
        int starter=c.CropIndex(f.Almanac.starter);
        Check(f.Seeds(starter)==0 && !f.BuySeed(starter),"empty garden");
        f.S.mode=HarvestMode.Sleep;f.Sleep();
        Check(f.Seeds(starter)==1 && f.PickSeed(starter) && f.Tend(0),"morning seed saves the farm");
        if(!f.S.plots[0].watered)Check(f.Tend(0),"water free seed");
        f.S.mode=HarvestMode.Sleep;f.Sleep();
        Check(f.Ripe(0) && f.Tend(0) && f.SellCrop(starter) && f.S.coins>0,"recover spendable money");
        // Season changes preserve a growing crop's original planting time and stored produce.
        f=new HarvestFarm(c,new HarvestRecord {started=true, coins=12,day=7});
        int pumpkin=c.CropIndex("pumpkin");
        Check(f.BuySeed(pumpkin)&&f.PickSeed(pumpkin)&&f.Tend(0),"sow off-season");
        var p=f.S.plots[0];int span=p.duration;Check(span==c.crops[pumpkin].days+1,"off-season extra day");
        if(!p.watered)f.Tend(0);
        f.S.mode=HarvestMode.Sleep;f.Sleep();f=Restore(f);
        Check(f.Season==1 && f.S.plots[0].cropId=="pumpkin" && f.S.plots[0].duration==span && f.S.plots[0].age==1,"season transition crop retention");
        bool rain=false,dry=false;
        for(int i=0;i<14;i++)
        {
            if(f.Weather==1){rain=true;Check(f.S.plots[0].watered,"rain irrigates plot");}
            if(f.Weather!=1&&!f.S.plots[0].watered&&!f.Ripe(0))
            {
                dry=true;int age=f.S.plots[0].age;f.S.mode=HarvestMode.Sleep;f.Sleep();Check(f.S.plots[0].age==age,"dry plot waits safely");continue;
            }
            f.S.mode=HarvestMode.Sleep;f.Sleep();
        }
        Check(rain&&dry,"both weather and manual tending exercised");
    }
    static void Cook(HarvestFarm f,int recipe,bool perfect)
    {
        int before=f.DishStock(recipe);Check(f.StartBatch(recipe),"start "+f.C.recipes[recipe].name);
        foreach(int step in f.C.recipes[recipe].steps)Check(f.Technique(step,perfect?.5f:.02f),"recipe method");
        Check(f.DishStock(recipe)==before+1 && f.CookingRecipe<0,"serveable result");
        f.S.mode=HarvestMode.Farm;
    }
    static void TestCatalogEdits(HarvestCatalog original)
    {
        HarvestCatalog Clone(HarvestCatalog source)=>JsonUtility.FromJson<HarvestCatalog>(JsonUtility.ToJson(source));
        HarvestRecord Copy(HarvestRecord source)=>JsonUtility.FromJson<HarvestRecord>(JsonUtility.ToJson(source));
        var extended=Clone(original);
        Array.Resize(ref extended.crops,extended.crops.Length+1);
        extended.crops[extended.crops.Length-1]=new HarvestCrop {id="qa_mint",name="検証用ミント",season=0,days=2,seedPrice=2,sellPrice=2,yieldCount=2};
        Array.Resize(ref extended.recipes,extended.recipes.Length+1);
        extended.recipes[extended.recipes.Length-1]=new HarvestRecipe {id="qa_tea",name="検証用香草茶",note="香草を煮出す。",price=9,
            items=new[]{"qa_mint"},amounts=new[]{1},steps=new[]{0,1}};
        Array.Reverse(extended.crops);Array.Reverse(extended.recipes);extended.Validate();
        var f=new HarvestFarm(extended,new HarvestRecord());
        int wheat=extended.CropIndex("wheat"),turnip=extended.CropIndex("turnip"),mix=extended.RecipeIndex("quiche"),mint=extended.CropIndex("qa_mint");
        Check(f.PlotCrop(0)==wheat && f.OrderRecipe(0)==extended.RecipeIndex("roll") && f.Tend(0),"ID based opening after reorder");
        Check(f.BuySeed(mint) && f.PickSeed(mint) && f.Tend(2) && f.Tend(2),"grow designer crop");
        f.S.mode=HarvestMode.Sleep;f.Sleep();
        Check(f.S.plots[2].age==1,"part-grown designer crop");
        int stock=f.Produce(wheat),fine=f.Fine(wheat),vegetables=f.Produce(turnip);
        Check(f.StartBatch(mix) && f.Technique(extended.recipes[mix].steps[0],.5f),"valid old recipe in progress");
        // Historical guest slot that has already been paid, even if its recipe is subsequently removed.
        f.S.orders[2].recipeId="qa_tea";f.S.orders[2].done=true;f.S.mode=HarvestMode.Farm;
        var captured=Copy(f.S);
        var changed=Clone(original);changed.Validate();
        int q=changed.RecipeIndex("quiche");changed.recipes[q].steps=new[]{2};changed.Validate();
        var returned=new HarvestFarm(changed,Copy(captured));
        Check(returned.CookingRecipe<0 && returned.S.mode==HarvestMode.Farm && returned.Produce(changed.CropIndex("wheat"))==stock &&
            returned.Fine(changed.CropIndex("wheat"))==fine && returned.Produce(changed.CropIndex("turnip"))==vegetables,
            "shortened method refunds reserved quality");
        returned=Restore(returned);
        Check(returned.Produce(changed.CropIndex("wheat"))==stock && !returned.CancelBatch() && returned.S.orders[2].done &&
            !returned.Serve(2),"one refund and previously paid guest preserved");
        Check(returned.PlotCrop(2)<0 && returned.S.storedPlots.Count==1 && returned.S.storedPlots[0].plant.age==1,"preserve removed growing crop");
        Check(returned.PickSeed(changed.CropIndex("wheat")) && returned.Tend(2),"dormant crop frees original plot");
        var recovered=new HarvestFarm(extended,Copy(returned.S));int restoredBeds=0;
        foreach(var plot in recovered.S.plots)if(plot.cropId=="qa_mint" && plot.age==1)restoredBeds++;
        Check(restoredBeds==1 && recovered.S.storedPlots.Count==0 && recovered.PlotCrop(2)==wheat,"reintroduced ID restores once without replacing crop");
        var differentCost=Clone(extended);differentCost.Validate();differentCost.recipes[differentCost.RecipeIndex("quiche")].amounts[0]++;
        var revised=new HarvestFarm(differentCost,Copy(captured));
        Check(revised.CookingRecipe<0 && revised.Produce(wheat)==stock && revised.Fine(wheat)==fine,"changed material quantities refund old bill");
        var reordered=Clone(extended);Array.Reverse(reordered.crops);Array.Reverse(reordered.recipes);reordered.Validate();
        var recipe=reordered.recipes[reordered.RecipeIndex("quiche")];Array.Reverse(recipe.items);Array.Reverse(recipe.amounts);
        var resumed=new HarvestFarm(reordered,Copy(captured));
        Check(resumed.CookingRecipe==reordered.RecipeIndex("quiche") && resumed.S.batch.step==1 && resumed.S.selectedCrop=="qa_mint",
            "recipe, ingredient and crop reordering preserves in-flight work");
        captured.batch.method=null;
        var legacy=new HarvestFarm(extended,Copy(captured));
        Check(legacy.CookingRecipe==mix && legacy.S.batch.step==1 && legacy.S.batch.method.Length==extended.recipes[mix].steps.Length,
            "older in-progress save adopts compatible method");
    }
    static HarvestFarm Restore(HarvestFarm f)=>new HarvestFarm(f.C,JsonUtility.FromJson<HarvestRecord>(JsonUtility.ToJson(f.S)));
    static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException("Harvest farm check failed: "+label);}
}
