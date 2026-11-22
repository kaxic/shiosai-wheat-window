using System;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(HarvestAmbience))]
public sealed class HarvestGame : StudioGame
{
    static readonly Vector2[] Stations = {new Vector2(4.2f,2.2f),new Vector2(5.6f,-.4f),new Vector2(-6.2f,3.9f),new Vector2(1.1f,3.8f),new Vector2(-.1f,-3.8f)};
    static readonly HarvestMode[] Places = {HarvestMode.Kitchen,HarvestMode.Service,HarvestMode.Market,HarvestMode.Sleep,HarvestMode.Festival};
    static readonly string[] PlaceNames = {"厨房で調理","お客様に配膳","売店を開く","翌日を迎える","品評会に出品"};
    static readonly string[] OriginalCrops={"wheat","turnip","tomato","bean","pumpkin","corn","cabbage","carrot"};
    HarvestCatalog catalog;
    HarvestFarm farm;
    HarvestView view;
    HarvestHud hud;
    int epoch, destination = -1;
    float flashed = -100, checkpoint;
    HarvestRecord S => farm.S;
    public HarvestMode Mode => S.mode;
    public HarvestCrop[] Crops => catalog.crops;
    public HarvestRecipe[] Recipes => catalog.recipes;
    public string[] ToolNames => HarvestFarm.Tools;
    public int Season => farm.Season;
    public int Day => S.day;
    public int Year => farm.Year;
    public int SeasonDay => farm.SeasonDay;
    public int Weather => farm.Weather;
    public string SeasonName => farm.Almanac.name;
    public string WeatherName => HarvestFarm.Weathers[Weather];
    public int Coins => S.coins;
    public int Reputation => S.reputation;
    public int TotalServed => S.totalServed;
    public int SeasonServed => S.seasonServed;
    public int ServedToday => Array.FindAll(S.orders,o=>o.done).Length;
    public int MasteredCount => farm.MasteredCount;
    public int FestivalCount { get {int n=0;for(int i=0;i<4;i++)if(HasFestival(i))n++;return n;} }
    public int SelectedCrop => farm.Selected;
    public int Selection {get;private set;}
    public int MapVersion => epoch + farm.Version;
    public Vector2 Hero => S.hero;
    public Vector2 Facing => S.facing;
    public float Motion {get;private set;}
    public float Flash => Mathf.Clamp01(1-(Time.unscaledTime-flashed)*3);
    public int CookingRecipe => farm.CookingRecipe;
    public int CookStep => S.batch.step;
    public int PerfectSteps => S.batch.perfect;
    public float CookBeat => Mathf.PingPong(S.batch.phase,1);
    public float GoodBeatMin => farm.GoodMin;
    public float GoodBeatMax => farm.GoodMax;
    public int ShopCount => Crops.Length*2 + 3;
    public string LastLog => S.log;
    public string FestivalName => farm.Almanac.festival;
    public int FestivalRecipe => farm.FestivalRecipe;
    public int FestivalGrade => 1;
    public int FestivalQty => farm.FestivalQty;
    public int RequiredServings => farm.RequiredServings;
    public int EligibleFestivalDishes => farm.EligibleFestivalDishes;
    public bool CanSubmitFestival => farm.CanSubmit;
    public bool SeasonStamped => farm.SeasonStamped;
    public string FestivalNeed => $"上等以上の「{Recipes[FestivalRecipe].name}」{FestivalQty}皿と、{SeasonName}の配膳{RequiredServings}件";
    public string Objective => !S.started || (Day==1 && TotalServed==0) ? $"最初の注文は{Recipes[OrderRecipe(0)].name}。Kで調理してOで配膳しよう。" :
        !SeasonStamped ? FestivalName+"へ。"+FestivalNeed+"を用意しよう。" : MasteredCount<Recipes.Length ?
        $"今季の賞印を獲得。まだ作っていない料理は{Recipes.Length-MasteredCount}種類。" : "季節の注文を届けて、食堂の評判を育てよう。";
    public int FocusPlot { get { int nearest=Nearest(Hero,1.15f);return nearest>=0&&nearest<12?nearest:-1; } }
    public string Prompt
    {
        get
        {
            if(Mode!=HarvestMode.Farm)return "";
            int site=Nearest(Hero,1.15f);
            if(site<0)return "畑と設備に近づいて E　／　畑をクリックして世話";
            if(site>=12)return "E　"+PlaceNames[site-12];
            int i=PlotCrop(site);
            if(i<0)return $"E　{Crops[SelectedCrop].name}をまく（種 {SeedStock(SelectedCrop)}）";
            if(Ripe(site))return "E　"+Crops[i].name+"を収穫";
            if(Watered(site))return $"水やり済み　{Crops[i].name}・あと{S.plots[site].duration-S.plots[site].age}日";
            return "E　"+Crops[i].name+"に水をやる";
        }
    }
    public Vector2 PlotPosition(int i)=>new Vector2(-6.3f+i%4*1.9f,-2.8f+i/4*2.0f);
    public Vector2 StationPosition(int i)=>Stations[i];
    public int PlotCrop(int i)=>farm.PlotCrop(i);
    public int PlotArtKind(int i)
    {
        int crop=PlotCrop(i);if(crop<0)return -1;
        int art=Array.IndexOf(OriginalCrops,Crops[crop].id);return art>=0?art:Crops[crop].season*2;
    }
    public float Growth(int i)=>PlotCrop(i)<0 ? 0 : Mathf.Clamp01(S.plots[i].age/(float)Math.Max(1,S.plots[i].duration));
    public bool Ripe(int i)=>farm.Ripe(i);
    public bool Watered(int i)=>S.plots[i].watered;
    public int SeedStock(int i)=>farm.Seeds(i);
    public int Produce(int i)=>farm.Produce(i);
    public int FineProduce(int i)=>farm.Fine(i);
    public int DishStock(int i)=>farm.DishStock(i);
    public int DishGrade(int i)=>farm.Grade(i);
    public int CropIndex(string id)=>catalog.CropIndex(id);
    public bool Mastered(int i)=>farm.Mastered(i);
    public int Level(int i)=>S.upgrades[i];
    public int Cost(int i)=>farm.Cost(i);
    public bool CanUpgrade(int i)=>farm.CanUpgrade(i);
    public bool CanCook(int i)=>farm.CanCook(i);
    public int OrderRecipe(int i)=>farm.OrderRecipe(i);
    public int OrderPrice(int i)=>farm.OrderPrice(i);
    public string OrderName(int i)=>S.orders[i].guest;
    public bool OrderDone(int i)=>S.orders[i].done;
    public bool CanServe(int i)=>farm.CanServe(i);
    public bool HasFestival(int i)=>i>=0&&i<4&&(S.allStamps&(1<<i))!=0;
    public int CropSale(int i)=>farm.CropSale(i);
    public string RecipeNeeds(int index)
    {
        var b=new StringBuilder();var recipe=Recipes[index];
        for(int j=0;j<recipe.items.Length;j++)
        {
            int crop=catalog.CropIndex(recipe.items[j]);if(j>0)b.Append("　");
            b.Append(Crops[crop].name).Append(" ").Append(Produce(crop)).Append("/").Append(recipe.amounts[j]);
        }
        return b.ToString();
    }
    protected override void BeginGame()
    {
        catalog=HarvestCatalog.Load();farm=new HarvestFarm(catalog,Load<HarvestRecord>());epoch+=100000;
        hud=new HarvestHud();view=GetComponent<HarvestView>();if(!view)view=gameObject.AddComponent<HarvestView>();
        view.Render(this);
    }
    protected override void LateGame(){if(view && farm!=null){SetIntenseMusic(Playing && Mode==HarvestMode.Cooking);view.Render(this);}}
    protected override void SaveCheckpoint(){if(farm!=null)Save(S);checkpoint=4;}
    protected override void DrawGame()
    {
        var before=Mode;hud.Draw(this);
        if(before!=HarvestMode.Farm || Mode!=before || Event.current.type!=EventType.MouseDown || Event.current.button!=0)return;
        Vector2 pt=Event.current.mousePosition;
        if(pt.y<140 && (pt.x<225 || pt.x>W-251) || pt.y>H-112 || !Camera.main)return;
        Ray ray=Camera.main.ViewportPointToRay(new Vector3(pt.x/W,1-pt.y/H,0));
        int visual=view.PickSite(ray);
        if(visual>=0){destination=visual;Event.current.Use();return;}
        var ground=new Plane(Vector3.up,Vector3.zero);
        if(!ground.Raycast(ray,out float distance))return;
        var spot=ray.GetPoint(distance);int site=Nearest(new Vector2(spot.x,spot.z),.98f);
        if(site>=0){destination=site;Event.current.Use();}
    }
    int Nearest(Vector2 p,float range)
    {
        float best=range*range;int result=-1;
        for(int i=0;i<12+Stations.Length;i++)
        {
            Vector2 world=i<12?PlotPosition(i):StationPosition(i-12);float d=(p-world).sqrMagnitude;
            if(d>=best)continue;best=d;result=i;
        }
        return result;
    }
    public void Open(HarvestMode mode)
    {
        if(mode==HarvestMode.Kitchen && CookingRecipe>=0)mode=HarvestMode.Cooking;
        if(mode==HarvestMode.Cooking && CookingRecipe<0)mode=HarvestMode.Kitchen;
        S.mode=mode;Selection=mode==HarvestMode.Seeds?SelectedCrop:0;Motion=0;destination=-1;Tone(1);
    }
    public void Back(){Open(HarvestMode.Farm);SaveCheckpoint();}
    protected override bool HandleEscape()
    {
        if(Mode==HarvestMode.Farm)return false;
        Back();return true;
    }
    public void Highlight(int i){Selection=Mathf.Clamp(i,0,CountChoices()-1);}
    int CountChoices()=>Mode switch
    {
        HarvestMode.Seeds=>Crops.Length,HarvestMode.Cooking=>4,HarvestMode.Kitchen=>Recipes.Length,HarvestMode.Service=>3,
        HarvestMode.Market=>ShopCount,HarvestMode.Sleep=>2,_=>1
    };
    void Feedback(bool success,string failure=null)
    {
        if(success){flashed=Time.unscaledTime;Tone(3);Notice(S.log);SaveCheckpoint();}
        else Notice(string.IsNullOrEmpty(failure)?S.log:failure);
    }
    public void Interact(){if(Mode==HarvestMode.Farm)Operate(Nearest(S.hero,1.15f));}
    void Operate(int site)
    {
        if(site>=12){Open(Places[site-12]);return;}
        if(site<0){Notice("畑か設備の前へ移動しよう");return;}
        Feedback(farm.Tend(site));
    }
    public void PickSeed(int i){if(Mode==HarvestMode.Seeds)Feedback(farm.PickSeed(i));}
    public void StartCooking(int i)
    {
        if(Mode!=HarvestMode.Kitchen)return;
        if(farm.StartBatch(i)){Selection=0;Feedback(true);}else Notice("食材庫の必要数を確認しよう");
    }
    public void Technique(int i)
    {
        if(Mode!=HarvestMode.Cooking)return;
        int recipe=CookingRecipe;
        if(farm.Technique(i,CookBeat))
        {
            Selection=Mode==HarvestMode.Cooking?0:recipe;
            Feedback(true);
        }else{Tone(2);Notice(S.log);SaveCheckpoint();}
    }
    public void CancelCooking(){if(Mode==HarvestMode.Cooking && farm.CancelBatch()){Selection=0;Feedback(true);}}
    public void Serve(int i){if(Mode==HarvestMode.Service)Feedback(farm.Serve(i),"この料理を用意してから配膳しよう");}
    public void BuySeed(int i){if(Mode==HarvestMode.Market)Feedback(farm.BuySeed(i),"コインが足りません");}
    public void SellCrop(int i){if(Mode==HarvestMode.Market)Feedback(farm.SellCrop(i),"出荷できる食材がありません");}
    public void Upgrade(int i){if(Mode==HarvestMode.Market)Feedback(farm.Upgrade(i),"費用と現在の段階を確認しよう");}
    public void SubmitFestival(){if(Mode==HarvestMode.Festival)Feedback(farm.Submit(),"出品条件と賞印を確認しよう");}
    public void NextDay(){if(farm.Sleep()){Selection=0;Feedback(true);}}
    static Vector2 Arrow(bool down)
    {
        bool Key(KeyCode key)=>down?Input.GetKeyDown(key):Input.GetKey(key);
        return new Vector2((Key(KeyCode.RightArrow)||Key(KeyCode.D)?1:0)-(Key(KeyCode.LeftArrow)||Key(KeyCode.A)?1:0),
            (Key(KeyCode.UpArrow)||Key(KeyCode.W)?1:0)-(Key(KeyCode.DownArrow)||Key(KeyCode.S)?1:0));
    }
    protected override void TickGame()
    {
        float dt=Mathf.Min(.06f,Time.deltaTime);
        if(Mode==HarvestMode.Farm)
        {
            if(Input.GetKeyDown(KeyCode.P)){Open(HarvestMode.Seeds);return;}
            if(Input.GetKeyDown(KeyCode.K)){Open(HarvestMode.Kitchen);return;}
            if(Input.GetKeyDown(KeyCode.O)){Open(HarvestMode.Service);return;}
            if(Input.GetKeyDown(KeyCode.M)){Open(HarvestMode.Market);return;}
            if(Input.GetKeyDown(KeyCode.B)){Open(HarvestMode.Ledger);return;}
            if(Input.GetKeyDown(KeyCode.G)){Open(HarvestMode.Festival);return;}
            if(Input.GetKeyDown(KeyCode.N)){Open(HarvestMode.Sleep);return;}
            if(Input.GetKeyDown(KeyCode.E)){destination=-1;Interact();return;}
            var move=Vector2.ClampMagnitude(Arrow(false),1);
            if(move.sqrMagnitude>.01f)destination=-1;
            if(destination>=0)
            {
                var target=destination<12?PlotPosition(destination):StationPosition(destination-12);
                if(Vector2.Distance(S.hero,target)<.75f){int arrived=destination;destination=-1;Motion=0;Operate(arrived);return;}
                move=(target-S.hero).normalized;
            }
            Motion=move.magnitude;if(Motion>.01f)
            {
                S.facing=move;Vector2 point=S.hero+move*((Input.GetKey(KeyCode.LeftShift)?6.4f:4.5f)*dt);
                S.hero=new Vector2(Mathf.Clamp(point.x,-8.8f,8.8f),Mathf.Clamp(point.y,-5.15f,5.15f));
                checkpoint-=dt;if(checkpoint<=0)SaveCheckpoint();
            }
            return;
        }
        if(Input.GetKeyDown(KeyCode.Backspace)){Back();return;}
        if(Mode==HarvestMode.Cooking)
        {
            S.batch.phase=Mathf.Repeat(S.batch.phase+dt*.94f,2);
            if(Input.GetKeyDown(KeyCode.X)){CancelCooking();return;}
            for(int i=0;i<4;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i))||Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad1+i))){Technique(i);return;}
        }
        Vector2 dir=Arrow(true);int count=CountChoices();
        int columns=Mode==HarvestMode.Kitchen || Mode==HarvestMode.Seeds || Mode==HarvestMode.Market ? 4 : 1;
        int delta=(int)dir.x-(int)dir.y*columns;
        if(delta!=0 && count>1){Selection=(Selection+delta+count)%count;Tone(1);}
        if(!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))return;
        switch(Mode)
        {
            case HarvestMode.Seeds:PickSeed(Selection);break;
            case HarvestMode.Kitchen:StartCooking(Selection);break;
            case HarvestMode.Cooking:Technique(Selection);break;
            case HarvestMode.Service:Serve(Selection);break;
            case HarvestMode.Market:
                if(Selection<Crops.Length)BuySeed(Selection);
                else if(Selection<Crops.Length*2)SellCrop(Selection-Crops.Length);
                else Upgrade(Selection-Crops.Length*2);break;
            case HarvestMode.Sleep:if(Selection==1)NextDay();else Back();break;
            case HarvestMode.Ledger:Open(HarvestMode.Festival);break;
            case HarvestMode.Festival:SubmitFestival();break;
        }
    }
    public override void VerifyGame()
    {
        var checkedFarm=HarvestVerification.Run(catalog);
        if(!Reviewing)return;
        Save(checkedFarm);var restored=new HarvestFarm(catalog,Load<HarvestRecord>());
        if(restored.S.awards!=checkedFarm.awards || restored.S.coins!=checkedFarm.coins || restored.S.allStamps!=15)
            throw new InvalidOperationException("食堂のディスク再開に失敗しました");
    }
    public override int ShowcaseViews=>3;
    public override void PrepareShowcase()=>ShowcaseView(0);
    public override void ShowcaseView(int index)
    {
        var source=HarvestVerification.Showcase!=null?JsonUtility.FromJson<HarvestRecord>(HarvestVerification.Showcase):new HarvestRecord();
        farm=new HarvestFarm(catalog,source);epoch+=100000;destination=-1;Selection=0;
        S.hero=new Vector2(-1.4f,-2.9f);S.facing=Vector2.left;
        if(index==1){Open(HarvestMode.Kitchen);Selection=FestivalRecipe;}
        else if(index==2)Open(HarvestMode.Festival);
        else S.mode=HarvestMode.Farm;
        view.Render(this);
    }
}
