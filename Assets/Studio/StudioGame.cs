using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class StudioGame : MonoBehaviour
{
    public StudioSkin skin;
    public float W {get;private set;}
    public float H {get;private set;}
    public Color Ink=>skin.ink;
    public Color Paper=>skin.paper;
    public Color Accent=>skin.accent;
    public bool Playing=>screen=="";
    public bool Reviewing=>review!=null;
    public float EffectsVolume=>effectVolume;
    public string StorageDirectory {get;private set;}
    string screen="title",review,toast;
    float toastUntil;
    int slot;
    bool hasStarted;
    Font body,heading;
    GUIStyle style;
    Texture2D veil;
    AudioSource score,activity,fx;
    bool dramatic;
    float musicMix;
    float musicVolume, effectVolume;
    string optionsReturn="pause";
    AudioClip[] chimes;
    public static string Argument(string name)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        return null;
#else
        var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,name);
        return n<0||n+1>=args.Length?null:args[n+1];
#endif
    }
    protected abstract void BeginGame();
    protected abstract void TickGame();
    protected abstract void DrawGame();
    public abstract void VerifyGame();
    public virtual void PrepareShowcase(){}
    public virtual int ShowcaseViews=>1;
    public virtual void ShowcaseView(int index){PrepareShowcase();}
    protected virtual void LateGame(){}
    protected virtual void SaveCheckpoint(){}
    protected virtual void EndGame(){}
    protected virtual bool HandleEscape(){return false;}

    void Awake()
    {
        BrowserHome.Register(transform, QuitGame);
        Application.targetFrameRate=60;
        review=Argument("-studioReviewDir");
        slot=review!=null?1:Mathf.Clamp(PlayerPrefs.GetInt("slot-"+skin.key,1),1,3);
        StorageDirectory=review!=null ? Path.Combine(Path.GetFullPath(review),"test-save",skin.key)
            : Path.Combine(Application.persistentDataPath,skin.key,"Slot"+slot);
        body=BrowserTools.ReadableFont(new[]{"Meiryo","Yu Gothic","Arial"},23);
        heading=BrowserTools.ReadableFont(new[]{"Yu Mincho","MS Mincho","Meiryo"},42);
        veil=new Texture2D(128,1,TextureFormat.RGBA32,false);
        for(int i=0;i<128;i++)veil.SetPixel(i,0,new Color(.015f,.035f,.06f,.91f*(1-Mathf.SmoothStep(0,1,i/122f))));
        veil.wrapMode=TextureWrapMode.Clamp;veil.Apply();
        musicVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("score-volume",.47f));
        effectVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("effect-volume",.39f));
        score=gameObject.AddComponent<AudioSource>();score.playOnAwake=false;score.loop=true;score.volume=musicVolume;score.clip=skin.music;
        if(skin.activityMusic)
        {
            activity=gameObject.AddComponent<AudioSource>();activity.playOnAwake=false;activity.loop=true;activity.volume=0;activity.clip=skin.activityMusic;
        }
        fx=gameObject.AddComponent<AudioSource>();fx.playOnAwake=false;fx.volume=effectVolume;
        chimes=new AudioClip[5];
        for(int i=0;i<chimes.Length;i++)chimes[i]=Chime(i);
        if(score.clip && PlayerPrefs.GetInt("music",1)==1){score.Play();if(activity)activity.Play();}
        BeginGame();
        if(review!=null){Application.runInBackground=true;StartCoroutine(Review());}
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F12))StartCoroutine(Photo(Path.Combine(StorageDirectory,"Photos",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".png")));
        if(screen=="title"){if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space))ResumeGame();
            if(Input.GetKeyDown(KeyCode.F1))screen="titlehelp";return;}
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(screen=="options"){PlayerPrefs.Save();screen=optionsReturn;}
            else if(screen=="titlehelp")screen="title";
            else if(screen=="help")screen="pause";
            else if(screen=="pause")ResumeGame();
            else if(!HandleEscape()) {SaveCheckpoint();screen="pause";}
            return;
        }
        if(screen!="")return;
        if(Input.GetKeyDown(KeyCode.F1)){screen="help";return;}
        if(Input.GetKeyDown(KeyCode.F5)){SaveCheckpoint();Notice("進行を保存しました");return;}
        TickGame();
    }
    void LateUpdate()
    {
        if(activity)
        {
            musicMix=Mathf.MoveTowards(musicMix,dramatic&&Playing?1:0,Time.unscaledDeltaTime*.48f);
            score.volume=(1-musicMix)*musicVolume;activity.volume=musicMix*musicVolume;
        }
        LateGame();
    }
    public void SetIntenseMusic(bool intense){dramatic=intense;}
    public void ResumeGame(){hasStarted=true;screen="";}
    public void ShowControls(){screen="help";}
    public void ShowTitle(){SaveCheckpoint();screen="title";}
    public void Notice(string message){toast=message;toastUntil=Time.unscaledTime+3.1f;}
    public void DismissNotice(){toastUntil=0;}
    public void Tone(int index)
    {
        if(chimes==null || index<0 || index>=chimes.Length)return;
        var clip=skin.effects!=null && index<skin.effects.Length && skin.effects[index]?skin.effects[index]:chimes[index];
        fx.PlayOneShot(clip);
    }
    public T Load<T>() where T:class,new()
    {
        string path=Path.Combine(StorageDirectory,"progress.json");
        foreach(string file in new[]{path,path+".bak"})
        {
            if(!File.Exists(file))continue;
            try {var data=JsonUtility.FromJson<T>(File.ReadAllText(file));if(data!=null)return data;}
            catch(Exception e) when(e is IOException||e is ArgumentException||e is UnauthorizedAccessException){Debug.LogWarning(e.Message);}
        }
        return new T();
    }
    public void Save<T>(T data) where T:class
    {
        if(data==null)return;
        string path=Path.Combine(StorageDirectory,"progress.json");string tmp=path+".writing";
        try
        {
            Directory.CreateDirectory(StorageDirectory);File.WriteAllText(tmp,JsonUtility.ToJson(data,true));
            if(File.Exists(path))BrowserTools.ReplaceFile(tmp,path,path+".bak");else File.Move(tmp,path);
            BrowserTools.Saved();
        }
        catch(Exception e) when(e is IOException||e is UnauthorizedAccessException)
        {Notice("保存先を確認してください");Debug.LogWarning(e.Message);}
    }
    protected void QuitGame(){if(hasStarted)SaveCheckpoint();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#elif UNITY_WEBGL
        BrowserTools.ReturnToCover();
#else
        Application.Quit();
#endif
    }
    void OnApplicationQuit(){if(hasStarted)SaveCheckpoint();}
    void OnDestroy(){EndGame();Destroy(veil);Destroy(body);Destroy(heading);if(chimes!=null)foreach(var c in chimes)Destroy(c);}
    static AudioClip Chime(int index)
    {
        const int hz=22050;float f=new[]{172f,277f,392f,522f,698f}[index];float len=.13f+.055f*index;
        var a=new float[(int)(hz*len)];
        for(int i=0;i<a.Length;i++){float t=(float)i/hz,phase=t*f*Mathf.PI*2;
            a[i]=.42f*Mathf.Min(1,t*180)*Mathf.Min(1,(len-t)*42)*Mathf.Exp(-t*12)*(Mathf.Sin(phase)+.22f*Mathf.Sin(phase*2.02f));}
        var sample=AudioClip.Create("note",a.Length,1,hz,false);sample.SetData(a,0);return sample;
    }
    public void Box(Rect r,Color color){if(Event.current.type==EventType.Repaint)GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,color,0,0);}
    public void Art(Rect r,int index,int columns=2,int rows=2)
    {
        DrawPlate(skin.illustrations,r,index,columns,rows);
    }
    public void Plate(Rect r,int index,int columns=3,int rows=2)
    {
        DrawPlate(skin.specimenPlate,r,index,columns,rows);
    }
    static void DrawPlate(Texture2D source,Rect r,int index,int columns,int rows)
    {
        if(!source||Event.current.type!=EventType.Repaint||index<0||index>=columns*rows)return;
        GUI.DrawTextureWithTexCoords(r,source,new Rect((index%columns)/(float)columns,
            1-(index/columns+1)/(float)rows,1f/columns,1f/rows));
    }
    public void Label(Rect r,string value,int size,Color color,bool serif=false,TextAnchor align=TextAnchor.MiddleLeft)
    {
        if(Event.current.type!=EventType.Repaint||string.IsNullOrEmpty(value))return;
        if(style==null)style=new GUIStyle(GUI.skin.label){padding=new RectOffset(),wordWrap=true,clipping=TextClipping.Clip};
        style.font=serif?heading:body;style.fontSize=size;style.alignment=align;style.normal.textColor=color;GUI.Label(r,value,style);
    }
    public bool Button(Rect r,string caption,bool enabled=true)
    {
        bool hover=r.Contains(Event.current.mousePosition)&&enabled;
        Color color=enabled?(hover?Color.Lerp(Accent,Color.white,.18f):Accent):Color.Lerp(Ink,Paper,.24f);
        Box(r,color);Box(new Rect(r.x,r.yMax-2,r.width,2),new Color(0,0,0,.22f));
        Color txt=enabled?(color.grayscale>.46f?new Color(.06f,.095f,.095f):Color.white):Paper;
        Label(new Rect(r.x+3,r.y,r.width-6,r.height),caption,15,txt,false,TextAnchor.MiddleCenter);
        bool hit=enabled&&GUI.Button(r,GUIContent.none,GUIStyle.none);if(hit&&chimes!=null)fx.PlayOneShot(chimes[1],.55f);return hit;
    }
    void OnGUI()
    {
        if(!skin)return;
        var old=GUI.matrix;float scale=Mathf.Min(Screen.width/1080f,Screen.height/620f);
        W=Screen.width/scale;H=Screen.height/scale;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
        if(screen=="title")Title();
        else
        {
            if(screen=="")DrawGame();
            if(screen!="")Menu();
            if(Time.unscaledTime<toastUntil){float bw=Mathf.Min(530,Mathf.Max(190,toast.Length*15+36));
                var r=new Rect((W-bw)/2,H-70,bw,34);Box(r,new Color(.035f,.07f,.08f,.96f));Label(r,toast,15,Paper,false,TextAnchor.MiddleCenter);}
        }
        GUI.matrix=old;
    }
    void Title()
    {
        var full=new Rect(0,0,W,H);if(skin.cover)GUI.DrawTexture(full,skin.cover,ScaleMode.ScaleAndCrop);else Box(full,Ink);
        GUI.DrawTexture(full,veil);float x=W*.064f,y=H*.28f;
        Box(new Rect(x,y,68,3),Accent);
        string series = string.IsNullOrWhiteSpace(skin.subtitle) ? skin.english : skin.subtitle+"　／　"+skin.english;
        Label(new Rect(x,y+13,W*.84f,32),series,18,Accent);
        float titleWidth=W*.86f;
        int titleSize=Mathf.Clamp(Mathf.FloorToInt(titleWidth/Mathf.Max(1,skin.title.Length)),37,60);
        var titleBox=new Rect(x-3,y+54,titleWidth,97);
        Label(new Rect(titleBox.x+1.5f,titleBox.y+2,titleBox.width,titleBox.height),skin.title,titleSize,new Color(.05f,.08f,.10f,.84f),true);
        Label(titleBox,skin.title,titleSize,Color.white,true);
        Label(new Rect(x,y+160,495,67),skin.introduction,18,Paper);
        Label(new Rect(x,y+230,103,31),"旅の記録",15,Paper);
        for(int i=1;i<=3;i++)
        {
            var r=new Rect(x+88+(i-1)*45,y+231,38,29);
            Box(r,i==slot?Accent:new Color(.065f,.10f,.13f,.92f));
            Label(r,i.ToString("00"),14,i==slot?Ink:Paper,false,TextAnchor.MiddleCenter);
            if(i!=slot && GUI.Button(r,GUIContent.none,GUIStyle.none) && !Reviewing)
            {
                if(hasStarted)SaveCheckpoint();
                PlayerPrefs.SetInt("slot-"+skin.key,i);PlayerPrefs.Save();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);return;
            }
        }
        string launch=File.Exists(Path.Combine(StorageDirectory,"progress.json"))?"つづきから　›":"旅を始める　›";
        if(Button(new Rect(x,y+278,195,47),launch))ResumeGame();
        if(Button(new Rect(x+207,y+278,111,47),"操作案内"))screen="titlehelp";
        if(Button(new Rect(x,y+340,105,31),"設定")){optionsReturn="title";screen="options";}
        if(Button(new Rect(x+116,y+340,83,31),"終了"))QuitGame();
    }
    void ToggleMusic(){bool on=!score.isPlaying;PlayerPrefs.SetInt("music",on?1:0);PlayerPrefs.Save();
        if(on&&score.clip){score.Play();if(activity)activity.Play();}else{score.Pause();if(activity)activity.Pause();}}
    void Menu()
    {
        Box(new Rect(0,0,W,H),new Color(.015f,.03f,.045f,.74f));
        float w=580,h=425,x=(W-w)/2,y=(H-h)/2;
        Box(new Rect(x,y,w,h),Ink);Box(new Rect(x,y,5,h),Accent);
        Label(new Rect(x+27,y+19,490,25),skin.english,13,Accent);
        bool help=screen=="help"||screen=="titlehelp";
        Label(new Rect(x+25,y+51,500,52),help?"操作案内":screen=="options"?"設定":"休憩",33,Paper,true);
        if(screen=="options")
        {
            float newMusic=VolumeRow(x+30,y+115,"音楽",musicVolume);
            float newFx=VolumeRow(x+30,y+177,"効果音",effectVolume);
            if(Mathf.Abs(newMusic-musicVolume)>.001f){musicVolume=newMusic;score.volume=musicVolume;PlayerPrefs.SetFloat("score-volume",musicVolume);}
            if(Mathf.Abs(newFx-effectVolume)>.001f){effectVolume=newFx;fx.volume=effectVolume;PlayerPrefs.SetFloat("effect-volume",effectVolume);}
            if(Button(new Rect(x+30,y+254,250,43),score.isPlaying?"音楽を止める":"音楽を流す"))ToggleMusic();
            if(Button(new Rect(x+300,y+254,250,43),Screen.fullScreen?"ウィンドウ表示にする":"全画面表示にする"))
            {if(Screen.fullScreen)Screen.SetResolution(1280,720,FullScreenMode.Windowed);
                else Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,FullScreenMode.FullScreenWindow);}
            if(Button(new Rect(x+w-177,y+h-63,147,41),"戻る")){PlayerPrefs.Save();screen=optionsReturn;}
            return;
        }
        if(help)
        {
            Label(new Rect(x+30,y+110,w-60,219),skin.controls,17,Paper);
            Label(new Rect(x+30,y+326,w-60,32),"F5 保存　　F12 撮影　　Esc メニュー",14,Accent);
            if(Button(new Rect(x+w-172,y+h-56,144,36),screen=="titlehelp"?"タイトルへ":"ゲームに戻る"))
            {if(screen=="titlehelp")screen="title";else ResumeGame();}return;
        }
        if(Button(new Rect(x+30,y+120,w-60,48),"ゲームに戻る"))ResumeGame();
        if(Button(new Rect(x+30,y+182,250,43),"操作案内"))screen="help";
        if(Button(new Rect(x+300,y+182,250,43),"音量・画面の設定")){optionsReturn="pause";screen="options";}
        if(Button(new Rect(x+30,y+239,250,43),"進行を保存")){SaveCheckpoint();Notice("進行を保存しました");}
        if(Button(new Rect(x+300,y+239,250,43),"タイトルへ"))ShowTitle();
        if(Button(new Rect(x+30,y+313,w-60,42),"保存して終了"))QuitGame();
    }
    float VolumeRow(float x,float y,string caption,float value)
    {
        Label(new Rect(x,y,134,40),caption+"　"+Mathf.RoundToInt(value*100),17,Paper);
        if(Button(new Rect(x+148,y,40,40),"−"))value=Mathf.Clamp01(value-.1f);
        value=GUI.HorizontalSlider(new Rect(x+205,y+15,240,22),value,0,1);
        if(Button(new Rect(x+465,y,40,40),"＋"))value=Mathf.Clamp01(value+.1f);
        return value;
    }
    IEnumerator Photo(string path)
    {
        yield return new WaitForEndOfFrame();
        var image=ScreenCapture.CaptureScreenshotAsTexture();
        try
        {
            if(Reviewing)
            {
                float low=1,high=0;
                for(int x=1;x<16;x++)for(int y=1;y<12;y++)
                {
                    float v=image.GetPixel(x*image.width/16,y*image.height/12).grayscale;
                    low=Mathf.Min(low,v);high=Mathf.Max(high,v);
                }
                if(high-low<.08f)throw new InvalidDataException("描画のないフレーム "+path);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));BrowserTools.Picture(path,image.EncodeToPNG());
        }
        finally{Destroy(image);}
    }
    IEnumerator Review()
    {
        yield return new WaitForSecondsRealtime(1f);
        yield return Photo(Path.Combine(review,"Title.png"));
        ResumeGame();
        try {VerifyGame();}
        catch(Exception ex){Debug.LogException(ex);File.WriteAllText(Path.Combine(review,"failed.txt"),ex.ToString());Application.Quit(1);yield break;}
        for(int index=0;index<ShowcaseViews;index++)
        {
            ShowcaseView(index);
            yield return new WaitForSecondsRealtime(.45f);
            yield return Photo(Path.Combine(review,index==0?"World.png":"Detail"+index+".png"));
        }
        Debug.Log("STUDIO_RELEASE_CHECK_OK "+skin.key);Application.Quit(0);
    }
}
