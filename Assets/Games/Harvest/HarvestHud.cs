using UnityEngine;

public sealed partial class HarvestHud
{
    static readonly Color Ink = new Color32(48, 55, 43, 255);
    static readonly Color Forest = new Color32(65, 87, 63, 255);
    static readonly Color Moss = new Color32(100, 113, 78, 255);
    static readonly Color Paper = new Color32(253, 245, 223, 255);
    static readonly Color Page = new Color32(237, 226, 197, 255);
    static readonly Color Edge = new Color32(200, 186, 151, 255);
    static readonly Color Honey = new Color32(237, 180, 101, 255);
    static readonly Color Red = new Color32(150, 74, 54, 255);
    static readonly Color Sage = new Color32(211, 222, 180, 255);
    static readonly string[] Seasons = { "春", "夏", "秋", "冬" };
    static readonly string[] Grades = { "家庭の味", "上等", "特選" };
    static readonly string[] Motions = { "切る", "煮る", "焼く", "和える" };
    static readonly HarvestMode[] FieldModes = { HarvestMode.Seeds, HarvestMode.Kitchen, HarvestMode.Service, HarvestMode.Market, HarvestMode.Ledger, HarvestMode.Festival, HarvestMode.Sleep };
    static readonly string[] FieldKeys = { "P", "K", "O", "M", "B", "G", "N" };
    static readonly string[] FieldLabels = { "種袋", "厨房", "配膳", "売店", "帳簿", "品評会", "就寝" };
    HarvestGame game;
    int ledgerPage;

    public void Draw(HarvestGame g)
    {
        game = g;
        switch (g.Mode)
        {
            case HarvestMode.Farm: Farm(); break;
            case HarvestMode.Seeds: Seeds(); break;
            case HarvestMode.Kitchen: Kitchen(); break;
            case HarvestMode.Cooking: Cooking(); break;
            case HarvestMode.Service: Service(); break;
            case HarvestMode.Market: Market(); break;
            case HarvestMode.Ledger: Ledger(); break;
            case HarvestMode.Sleep: Sleep(); break;
            case HarvestMode.Festival: Festival(); break;
        }
    }

    void Farm()
    {
        float x = 16, y = 15;
        Card(new Rect(x, y, 201, 74), Paper);
        Fill(new Rect(x, y, 5, 74), Forest);
        Label("麦 窓 食 堂", x + 13, y + 4, 121, 22, 13, Forest);
        Label($"{game.Year} 年目", x + 119, y + 4, 70, 22, 12, Moss, align: TextAnchor.MiddleRight);
        Label($"{game.SeasonName}　{game.SeasonDay} 日", x + 12, y + 28, 119, 35, 22, Ink, true);
        Label(game.WeatherName, x + 126, y + 29, 62, 33, 15, Moss, align: TextAnchor.MiddleRight);
        Rect service = new Rect(x, y + 82, 201, 41);
        Card(service, Hover(service) ? Sage : Paper);
        bool firstOrder = game.Day == 1 && game.TotalServed == 0;
        Label($"本日の配膳　{game.ServedToday} / 3", x + 11, service.y + 1, 155, firstOrder ? 20 : 36, 15, Ink);
        if (firstOrder) Label("最初に K で丸パンを焼く", x + 11, service.y + 20, 165, 18, 12, Forest);
        Badge(new Rect(service.xMax - 33, service.y + 9, 24, 22), "O");
        if (Hit(service)) game.Open(HarvestMode.Service);

        Rect wallet = new Rect(game.W - 244, y, 228, 54);
        Card(wallet, Paper);
        Label("売上金", wallet.x + 10, y + 3, 104, 18, 12, Moss);
        Label(game.Coins.ToString("N0") + " G", wallet.x + 10, y + 21, 108, 27, 18, Ink);
        Fill(new Rect(wallet.x + 122, y + 9, 1, 35), Edge);
        Label("店の評判", wallet.x + 135, y + 3, 81, 18, 12, Moss);
        Label(game.Reputation.ToString("N0"), wallet.x + 134, y + 21, 83, 27, 18, Ink);
        if (Hit(wallet)) game.Open(HarvestMode.Market);

        float wave = game.H - 52;
        float bw = 94;
        for (int i = 0; i < FieldModes.Length; i++)
            if (Shortcut(new Rect(x + i * (bw + 5), wave, bw, 36), FieldKeys[i], FieldLabels[i])) game.Open(FieldModes[i]);
        Label("WASD ／ 矢印で歩く", game.W - 241, wave, 221, 36, 15, Ink, align: TextAnchor.MiddleRight, shadow: true);
        string prompt = game.Prompt;
        if (game.Day == 1 && game.TotalServed == 0 && prompt.StartsWith("畑と設備"))
            prompt = "最初の注文は丸パン。K で調理、O で配膳。　／　畑をクリックして世話";
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            float pw = Mathf.Clamp(prompt.Length * 16 + 32, 182, game.W - 64);
            var r = new Rect((game.W - pw) / 2, wave - 47, pw, 36);
            bool actionable = prompt.StartsWith("E");
            Card(r, actionable && Hover(r) ? Forest : Ink);
            Label(prompt, r.x + 7, r.y, r.width - 14, r.height, 15, Paper, align: TextAnchor.MiddleCenter);
            if (actionable && Hit(r)) game.Interact();
        }
    }

    void Seeds()
    {
        Rect p = Book(770, 376, "THE SEED TIN", "種袋");
        Label("まく種を選び、空いた畑で E。水やりはもう一度 E。", p.x + 20, p.y + 75, p.width - 172, 32, 15, Moss);
        int first = PageStart(game.Selection);
        PageArrows(p.xMax-149, p.y+77, first, game.Crops.Length, game.Highlight);
        float w = (p.width - 61) / 4;
        for (int i = first; i < Mathf.Min(first+8, game.Crops.Length); i++)
        {
            var crop = game.Crops[i];
            int cell = i - first;
            var r = new Rect(p.x + 20 + cell % 4 * (w + 7), p.y + 120 + cell / 4 * 83, w, 75);
            bool selected = game.Selection == i;
            Row(r, selected);
            Label(crop.name, r.x + 8, r.y + 4, w - 16, 28, 18, Ink, true);
            Label($"{Season(crop.season)}の作物　{Days(i)} 日", r.x + 9, r.y + 32, w - 18, 20, 13, Moss);
            Label($"所持 {game.SeedStock(i)} 袋", r.x + 9, r.y + 51, w - 18, 20, 13, Forest);
            if (Hit(r)) game.PickSeed(i);
        }
        Label($"選んでいる種　{game.Crops[game.SelectedCrop].name}", p.x + 21, p.yMax - 57, p.width - 232, 35, 15, Ink);
        if (Button(new Rect(p.xMax - 183, p.yMax - 57, 163, 38), "売店で種を買う", true, false)) game.Open(HarvestMode.Market);
    }

    void Market()
    {
        Rect p = Book(804, 447, "VILLAGE MARKET", "種と道具の売店");
        int n = game.Crops.Length;
        int category = game.Selection < n ? 0 : game.Selection < 2*n ? 1 : 2;
        string[] names = { "種を買う", "収穫物を売る", "道具を強化" };
        float tabWidth = (p.width - 52) / 3;
        for (int i = 0; i < 3; i++)
            if (Tab(new Rect(p.x + 20 + (tabWidth + 6) * i, p.y + 78, tabWidth, 33), names[i], category == i))
                game.Highlight(i * n);
        category = game.Selection < n ? 0 : game.Selection < 2*n ? 1 : 2;
        Label($"所持金　{game.Coins:N0} G", p.x + 21, p.y + 122, p.width - 42, 30, 17, Forest);
        if (category < 2)
        {
            float cw = (p.width - 61) / 4;
            int offset = category * n;
            int first = PageStart(game.Selection-offset);
            PageArrows(p.xMax-149, p.y+122, first, n, i => game.Highlight(offset+i));
            for (int i = first; i < Mathf.Min(first+8, n); i++)
            {
                int index = category*n + i;
                var crop = game.Crops[i];
                int cell = i-first;
                Rect r = new Rect(p.x + 20 + cell % 4 * (cw + 7), p.y + 163 + cell / 4 * 94, cw, 85);
                Row(r, game.Selection == index);
                Label(crop.name, r.x + 9, r.y + 4, cw - 18, 27, 18, Ink, true);
                int price = category == 0 ? crop.seedPrice : game.CropSale(i);
                int qty = category == 0 ? game.SeedStock(i) : game.Produce(i);
                Label(category == 0 ? $"{Season(crop.season)}の種　所持 {qty}" : $"在庫 {qty} 個", r.x + 10, r.y + 33, cw - 20, 22, 14, Moss);
                Label($"{price} G ／ { (category == 0 ? "袋" : "個") }", r.x + 10, r.y + 55, cw - 20, 24, 15, Forest);
                if (Hit(r)) game.Highlight(index);
            }
        }
        else
        {
            string[] effects = { "一度に水をやれる範囲が広がる", "調理のちょうどよい時間が長くなる", "一皿を配膳したときの代金が増える" };
            float cw = (p.width - 54) / 3;
            for (int i = 0; i < game.ToolNames.Length; i++)
            {
                int index = n * 2 + i;
                Rect r = new Rect(p.x + 20 + i * (cw + 7), p.y + 165, cw, 147);
                Row(r, game.Selection == index);
                Label(game.ToolNames[i], r.x + 10, r.y + 7, r.width - 20, 32, 20, Ink, true);
                Label($"Lv. {game.Level(i)} / 2", r.x + 11, r.y + 43, cw - 22, 23, 14, Forest);
                Label(effects[i], r.x + 11, r.y + 70, cw - 22, 44, 14, Moss);
                Label(game.Cost(i) == 0 ? "最高段階" : $"強化費 {game.Cost(i)} G", r.x + 11, r.y + 116, cw - 22, 24, 15, Forest);
                if (Hit(r)) game.Highlight(index);
            }
        }
        int selected = Mathf.Clamp(game.Selection, 0, game.ShopCount - 1);
        int item = category == 0 ? selected : category == 1 ? selected - n : selected - n*2;
        Rect action = new Rect(p.xMax - 239, p.yMax - 58, 219, 39);
        string current;
        if (category == 0)
        {
            current = game.Crops[item].name + "の種";
            if (Button(action, "1 袋買う", game.Coins >= game.Crops[item].seedPrice)) game.BuySeed(item);
        }
        else if (category == 1)
        {
            current = game.Crops[item].name;
            if (Button(action, "1 個売る", game.Produce(item) > 0)) game.SellCrop(item);
        }
        else
        {
            current = game.ToolNames[item];
            if (Button(action, game.Cost(item) > 0 ? "強化する" : "強化済", game.CanUpgrade(item))) game.Upgrade(item);
        }
        Label($"選択中　{current}", p.x + 22, p.yMax - 57, p.width - 289, 36, 15, Ink);
    }

    void Ledger()
    {
        Rect p = Book(862, 503, "THE FARM LEDGER", "畑と食堂の帳簿");
        float x = p.x + 20, y = p.y + 83;
        Illustration(new Rect(x, y, 274, 157), game.Season);
        Fill(new Rect(x, y + 159, 274, 70), Forest);
        Label($"{game.Year} 年目の{game.SeasonName}　{game.SeasonDay} 日", x + 12, y + 163, 249, 34, 21, Paper, true);
        Label($"配膳累計 {game.TotalServed} 皿　評判 {game.Reputation}", x + 13, y + 199, 248, 26, 14, Paper);
        Label("育てた店の記録", x + 1, y + 247, 269, 31, 18, Ink, true);
        Label($"料理の習熟　{game.MasteredCount} / {game.Recipes.Length}\n四季の賞印　{game.FestivalCount} / 4", x + 2, y + 275, 270, 55, 16, Forest);
        float rx = x + 293, rw = p.xMax - rx - 20;
        Label("食材庫", rx, y, rw*.47f, 32, 20, Ink, true);
        Label("種 ／ 食材計 ／ うち上質", rx+rw*.39f, y, rw*.61f, 32, 13, Moss, align: TextAnchor.MiddleRight);
        int lastPage = PageStart(game.Crops.Length-1);
        if (lastPage > 0 && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.LeftArrow) { ledgerPage -= 8; Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.RightArrow) { ledgerPage += 8; Event.current.Use(); }
        }
        ledgerPage = Mathf.Clamp(ledgerPage, 0, lastPage);
        PageArrows(rx+rw-129, y+314, ledgerPage, game.Crops.Length, i => ledgerPage=i);
        for (int i=ledgerPage; i<Mathf.Min(ledgerPage+8,game.Crops.Length); i++)
        {
            float ry = y + 40 + (i-ledgerPage)*34;
            if (i%2==0) Fill(new Rect(rx,ry,rw,31),Page);
            Label(game.Crops[i].name,rx+8,ry,rw*.4f,31,15,Ink);
            Label($"{game.SeedStock(i)} 袋　 ／　 {game.Produce(i)} 個　 ／　 {game.FineProduce(i)} 個",rx+rw*.37f,ry,rw*.63f-8,31,14,Moss,align:TextAnchor.MiddleRight);
        }
        Rule(x, p.yMax - 85, p.width - 40);
        Label(game.Objective, x, p.yMax - 76, p.width - 222, 52, 15, Ink);
        if (Button(new Rect(p.xMax-173,p.yMax-59,153,39),"品評会の記録",true,false)) game.Open(HarvestMode.Festival);
    }

    void Festival()
    {
        Rect p=Book(847,477,"SEASONAL TABLE","季節の品評会");
        float x=p.x+20,y=p.y+85;
        int dish=game.FestivalRecipe;
        Illustration(new Rect(x,y,300,149),game.Season);
        Fill(new Rect(x,y+151,300,71),Forest);
        Label(game.FestivalName,x+11,y+155,278,30,20,Paper,true);
        Label($"{game.Year} 年目の{game.SeasonName}　　開催中",x+12,y+188,276,24,14,Paper);
        float rx=x+319,rw=p.xMax-rx-20;
        Dish(new Rect(rx,y,121,88),dish);
        Label(game.Recipes[dish].name,rx+132,y+2,rw-142,35,22,Ink,true);
        Label($"出品する品質　{Grade(game.FestivalGrade)} 以上",rx+133,y+43,rw-143,32,14,Moss);
        Rule(rx,y+99,rw);
        Label(game.FestivalNeed,rx+1,y+109,rw-2,67,15,Ink);
        Label($"用意できた料理 {game.EligibleFestivalDishes} / {game.FestivalQty} 皿",rx+1,y+181,rw-2,24,15,Forest);
        Label($"今季の配膳 {game.SeasonServed} / {game.RequiredServings} 件",rx+1,y+208,rw-2,24,15,Forest);
        Label("四季の出品印",x,y+244,p.width-40,30,18,Ink,true);
        float sw=(p.width-58)/4;
        for(int i=0;i<4;i++)
        {
            var r=new Rect(x+i*(sw+6),y+281,sw,43);
            Fill(r,game.HasFestival(i)?Sage:Page);
            Label(Seasons[i]+"　"+(game.HasFestival(i)?"出品済":"未出品"),r.x+4,r.y,r.width-8,r.height,15,game.HasFestival(i)?Forest:Moss,align:TextAnchor.MiddleCenter);
        }
        string entry = game.SeasonStamped ? "今季の品評会に出品しました。" : game.CanSubmitFestival
            ? "準備が整いました。料理棚から出品できます。" : "季節が替わる前に出品すると、賞印と賞金がもらえます。";
        Label(entry,x,p.yMax-56,p.width-253,38,14,Moss);
        if(Button(new Rect(p.xMax-216,p.yMax-58,196,39),game.SeasonStamped?"今季は出品済":"品評会に出品",game.CanSubmitFestival))game.SubmitFestival();
    }

    void Sleep()
    {
        Rect p=Book(598,310,"CLOSING TIME","店じまい");
        Label($"{game.SeasonName}　{game.SeasonDay} 日を終えますか？",p.x+21,p.y+87,p.width-42,43,24,Ink,true);
        Label($"本日の配膳　{game.ServedToday} / 3　　所持金　{game.Coins:N0} G",p.x+22,p.y+140,p.width-44,33,16,Moss);
        Label(game.SeasonDay==7?"明日から次の季節です。":"眠ると畑が育ち、翌日の注文に替わります。",p.x+22,p.y+180,p.width-44,37,16,Ink);
        float half=(p.width-49)/2;
        if(Button(new Rect(p.x+20,p.yMax-65,half,43),"まだ作業する",true,game.Selection==0))game.Back();
        if(Button(new Rect(p.x+29+half,p.yMax-65,half,43),"明日を迎える",true,game.Selection==1))game.NextDay();
    }

    string Season(int i) => i >= 0 && i < Seasons.Length ? Seasons[i] : "通年";
    int Days(int i) => game.Crops[i].days + (game.Crops[i].season == game.Season ? 0 : 1);
    static string Grade(int i) => Grades[Mathf.Clamp(i,0,Grades.Length-1)];

    Rect Book(float w,float h,string caption,string title)
    {
        Fill(new Rect(0,0,game.W,game.H),new Color(.11f,.15f,.12f,.58f));
        Rect p=new Rect((game.W-w)/2,(game.H-h)/2,w,h);Card(p,Paper);
        Fill(new Rect(p.x+10,p.y+13,4,44),Forest);
        Label(caption,p.x+24,p.y+10,w-170,20,12,Moss);
        Label(title,p.x+22,p.y+28,w-175,37,26,Ink,true);
        if(Button(new Rect(p.xMax-104,p.y+21,84,34),"閉じる",true,false))game.Back();
        Rule(p.x+16,p.y+69,w-32);
        return p;
    }

    static int PageStart(int index) => Mathf.Max(0,index) / 8 * 8;
    void PageArrows(float x,float y,int first,int count,System.Action<int> gotoIndex)
    {
        if(count<=8)return;
        if(Button(new Rect(x,y,62,29),"前の頁",first>0,false))gotoIndex(first-8);
        if(Button(new Rect(x+66,y,62,29),"次の頁",first+8<count,false))gotoIndex(first+8);
    }

    void Dish(Rect frame,int index)
    {
        Fill(frame,Page);
        var atlas=game.skin.specimenPlate;
        float ratio=atlas?atlas.width/(float)atlas.height:1;
        float w=Mathf.Min(frame.width,frame.height*ratio),h=w/ratio;
        HarvestDishArt.Draw(game,new Rect(frame.center.x-w/2,frame.center.y-h/2,w,h),index);
    }
    void Illustration(Rect frame,int index)
    {
        Fill(frame,Page);
        var texture=game.skin.illustrations;
        if(!texture)return;
        float ratio=texture.width/(float)texture.height;
        float w=Mathf.Max(frame.width,frame.height*ratio);
        float h=w/ratio;
        GUI.BeginGroup(frame);
        Rect inner=new Rect((frame.width-w)/2,(frame.height-h)/2,w,h);
        game.Art(inner,index);
        GUI.EndGroup();
    }

    bool Shortcut(Rect r,string key,string text)
    {
        Card(r,Hover(r)?Sage:Paper);Badge(new Rect(r.x+5,r.y+6,25,24),key);
        Label(text,r.x+32,r.y,r.width-36,r.height,15,Ink,align:TextAnchor.MiddleCenter);
        return Hit(r);
    }
    void Badge(Rect r,string text)
    {
        Fill(r,Forest);Label(text,r.x,r.y,r.width,r.height,13,Paper,align:TextAnchor.MiddleCenter);
    }
    void Row(Rect r,bool selected)
    {
        Fill(r,selected||Hover(r)?Sage:Page);
        if(selected)Fill(new Rect(r.x,r.y,3,r.height),Forest);
    }
    bool Tab(Rect r,string text,bool selected)=>Button(r,text,true,selected);
    bool Button(Rect r,string text,bool enabled,bool primary=true)
    {
        bool hover=enabled&&Hover(r);
        Fill(r,enabled?(primary?(hover?Ink:Forest):hover?Sage:Page):new Color32(222,218,197,255));
        Label(text,r.x+4,r.y,r.width-8,r.height,15,enabled&&primary?Paper:enabled?Ink:Moss,align:TextAnchor.MiddleCenter);
        return enabled&&Hit(r);
    }
    void Card(Rect r,Color color)
    {
        Fill(new Rect(r.x+2,r.y+3,r.width,r.height),new Color(.12f,.14f,.09f,.24f));Fill(r,color);
        Fill(new Rect(r.x,r.y,r.width,1),Edge);
    }
    static bool Hover(Rect r)=>r.Contains(Event.current.mousePosition);
    static bool Hit(Rect r)=>GUI.Button(r,GUIContent.none,GUIStyle.none);
    void Fill(Rect r,Color color)=>game.Box(r,color);
    void Rule(float x,float y,float w)=>Fill(new Rect(x,y,w,1),Edge);
    void Label(string text,float x,float y,float w,float h,int size,Color color,bool serif=false,TextAnchor align=TextAnchor.MiddleLeft,bool shadow=false)
    {
        if(Screen.width<800&&size>=12&&size<=15)size++;
        if(shadow)game.Label(new Rect(x+1,y+1,w,h),text,size,Paper,serif,align);
        game.Label(new Rect(x,y,w,h),text,size,color,serif,align);
    }
}
