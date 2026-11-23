using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static HarvestMesh;

/// <summary>Renders the farm without owning its money, inventory or growth timers.</summary>
public sealed class HarvestView : MonoBehaviour
{
    sealed class Plot
    {
        public Transform root, custom;
        public MeshRenderer soil;
        public MeshFilter plant;
        public string crop;
        public int stage=-2;
        public bool damp;
        public Vector2 center;
        public float pickHeight;
    }
    readonly Dictionary<string,Mesh> collection=new Dictionary<string,Mesh>();
    readonly Dictionary<string,GameObject> customPlants=new Dictionary<string,GameObject>();
    readonly Plot[] beds=new Plot[12];
    readonly Transform[] limbs=new Transform[5], guests=new Transform[3], vapor=new Transform[6];
    readonly MeshRenderer[] smoke=new MeshRenderer[6];
    readonly MeshFilter[] meals=new MeshFilter[3];
    Transform garden,owner,focus,can,water;
    MeshFilter decoration, scenery, grounding;
    Transform wheel;
    int field=-1;
    Material paint,wet,wash,rain;
    MaterialPropertyBlock opacity;
    Camera lens;
    Light sun;
    GameObject showers;
    int version=int.MinValue,season=-1,weather=-1,day=-1;
    float time,stride,dusk,waterUntil;
    int wateredPlot=-1;
    Vector2 last;
    bool ready;

    public void Render(HarvestGame g)
    {
        if(!garden)Setup(g);
        Frame();
        if(version!=g.MapVersion||day!=g.Day||season!=g.Season||weather!=g.Weather)
        {
            Refresh(g);version=g.MapVersion;
        }
        if(field==1)wheel.localRotation=Quaternion.AngleAxis(-time*24,Vector3.forward);
        time+=Mathf.Min(Time.unscaledDeltaTime,.06f);
        float goalDusk=g.Mode==HarvestMode.Sleep?.82f:g.Mode==HarvestMode.Service?.22f:.025f*(g.Day%4);
        dusk=Mathf.MoveTowards(dusk,goalDusk,Time.unscaledDeltaTime*.78f);
        var sky=new Vector4(season,weather==1?1:weather==2?.53f:0,dusk,0);
        paint.SetVector("_Sky",sky);wet.SetVector("_Sky",sky);
        AnimateOwner(g);
        for(int j=0;j<guests.Length;j++)
        {
            float tilt=.84f*Mathf.Sin(time*1.8f+j);
            guests[j].localRotation=Quaternion.Euler(0,180+tilt,0);
        }
        int current=g.FocusPlot;
        focus.gameObject.SetActive(current>=0&&current<beds.Length&&g.Mode==HarvestMode.Farm);
        if(focus.gameObject.activeSelf)focus.localPosition=At(beds[current].center.x,.21f,beds[current].center.y);
        for(int i=0;i<vapor.Length;i++)
        {
            float age=Mathf.Repeat(time*.25f+(float)i/vapor.Length,1);
            vapor[i].localPosition=HarvestHomestead.Chimney+At(age*.90f+.071f*Mathf.Sin(age*7+i),.15f+age*1.37f,.11f*age);
            vapor[i].localScale=Vector3.one*(.11f+age*.24f);
            opacity.Clear();opacity.SetFloat("_Opacity",Mathf.Sin(age*Mathf.PI)*.35f);smoke[i].SetPropertyBlock(opacity);
        }
    }
    static void Remove(UnityEngine.Object a){if(!a)return;if(Application.isPlaying)Destroy(a);else DestroyImmediate(a);}
    Transform Group(string label,Transform parent){var o=new GameObject(label);o.transform.SetParent(parent,false);return o.transform;}
    Mesh Cache(string label,Action<HarvestMesh> artist)
    {
        if(collection.TryGetValue(label,out var mesh))return mesh;
        var form=new HarvestMesh();artist(form);mesh=form.Build(label);collection.Add(label,mesh);return mesh;
    }
    MeshFilter Draw(string label,Transform parent,Mesh shape,Material material)
    {
        var node=Group(label,parent).gameObject;
        var filter=node.AddComponent<MeshFilter>();filter.sharedMesh=shape;
        var renderer=node.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.lightProbeUsage=LightProbeUsage.Off;
        renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
        if(material==wash||material==rain){renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;}
        return filter;
    }
    void Frame()
    {
        lens.orthographicSize=Mathf.Max(6.35f,11.55f/Mathf.Max(.73f,lens.aspect));
        Vector3 pivot=At(-.05f,.52f,.43f),offset=At(1.29f,15.1f,-18.6f);
        lens.transform.SetPositionAndRotation(pivot+offset,Quaternion.LookRotation(-offset));
    }
    void Setup(HarvestGame g)
    {
        garden=Group("Wheat Window garden",transform);opacity=new MaterialPropertyBlock();
        const string root="Harvest/Art/";
        paint=new Material(Resources.Load<Shader>(root+"FarmPaint")){name="Garden paints"};
        string[] fields={"_Grass","_Soil","_Wood","_Roof","_Plaster","_Stone","_Grain"};
        string[] images={"Clover","GardenLoam","Chestnut","ClayTile","LimeWash","Courtyard","FieldGrain"};
        for(int i=0;i<fields.Length;i++)paint.SetTexture(fields[i],Resources.Load<Texture2D>(root+images[i]));
        wet=new Material(paint){name="Watered loam"};wet.SetFloat("_Damp",1);
        wash=new Material(Resources.Load<Shader>(root+"FarmWash")){name="Soft shadows and smoke"};
        rain=new Material(Resources.Load<Shader>(root+"Showers")){name="Rain over the garden"};
        lens=Camera.main;if(!lens){var camera=Group("Courtyard camera",garden).gameObject;camera.tag="MainCamera";lens=camera.AddComponent<Camera>();}
        if(!FindAnyObjectByType<AudioListener>())lens.gameObject.AddComponent<AudioListener>();
        lens.orthographic=true;lens.nearClipPlane=.15f;lens.farClipPlane=125;lens.allowHDR=false;
        lens.clearFlags=CameraClearFlags.SolidColor;lens.backgroundColor=new Color32(179,207,198,255);
        sun=Group("Window light",garden).gameObject.AddComponent<Light>();sun.type=LightType.Directional;
        sun.transform.rotation=Quaternion.LookRotation(At(.48f,-.83f,.44f));sun.shadows=LightShadows.Soft;sun.shadowStrength=.71f;
        sun.shadowNormalBias=.22f;sun.color=new Color(1,.92f,.81f);sun.intensity=1.12f;
        scenery=Draw("Kitchen garden and cutaway cottage",garden,Cache("Homestead",m=>new HarvestHomestead(m).Build()),paint);
        grounding=Draw("Grounding",garden,Cache("Courtyard contact",StaticShadows),wash);
        wheel=Draw("Terrace water wheel",garden,Cache("Mill wheel",HarvestTerrace.Wheel),paint).transform;wheel.localPosition=HarvestTerrace.WheelHub;
        decoration=Draw("Seasonal orchard",garden,null,paint);
        for(int i=0;i<beds.Length;i++)
        {
            var plot=new Plot();beds[i]=plot;plot.center=g.PlotPosition(i);plot.root=Group("Kitchen bed "+(i+1),garden);
            plot.root.localPosition=At(plot.center.x,0,plot.center.y);
            var soil=Draw("Raised earth",plot.root,Cache("Furrowed soil",HarvestCrops.Bed),paint);
            plot.soil=soil.GetComponent<MeshRenderer>();plot.plant=Draw("Crop",plot.root,null,paint);
        }
        focus=Draw("Selected bed",garden,Cache("Plot corners",m=>{
            m.Ink("F8DE8C",Finish.Plain,.85f);
            foreach(float x in new[]{-.885f,.885f})foreach(float z in new[]{-.815f,.815f})
            {
                m.Block(x-Mathf.Sign(x)*.135f,0,z,.27f,.021f,.045f);
                m.Block(x,0,z-Mathf.Sign(z)*.13f,.045f,.021f,.26f);
            }
        }),paint).transform;
        Mesh baseShade=Cache("Soft feet",m=>Shadow(m,Vector3.zero,1,1));
        owner=Group("The cook",garden);
        var feet=Draw("Foot shadow",owner,baseShade,wash);feet.transform.localPosition=Vector3.up*.01f;feet.transform.localScale=At(.44f,1,.35f);
        var body=Group("Chef facing",owner);
        for(int p=0;p<limbs.Length;p++)
        {
            int part=p;limbs[p]=Draw("Chef "+p,body,Cache("Chef "+p,m=>HarvestLife.Person(m,-1,part)),paint).transform;
            limbs[p].localPosition=Joint(p);
        }
        can=Draw("Watering can",limbs[1],Cache("Watering can",HarvestLife.Can),paint).transform;can.localPosition=At(.024f,-.44f,.08f);
        water=Draw("Water drops",can,Cache("Pour",m=>{
            for(int j=0;j<11;j++){
                float a=j*.43f; m.Ink("C1E1DA",Finish.Plain,.66f);
                m.Blob(At(Mathf.Cos(j*2.4f)*.043f,-.018f-j*.039f,.32f+j*.028f+a*.001f),At(.016f,.030f,.015f),j,5);
            }
        }),paint).transform;
        for(int i=0;i<guests.Length;i++)
        {
            int guest=i;Vector2 table=HarvestHomestead.Tables[i];
            guests[i]=Group("Seasonal diner "+i,garden);guests[i].localPosition=At(table.x,.095f,table.y+.74f);
            Draw("Customer",guests[i],Cache("Seated visitor "+i,m=>{
                for(int part=0;part<5;part++){m.originOffset=Joint(part);HarvestLife.Person(m,guest,part);}
            }),paint);
            meals[i]=Draw("Today's plate",garden,null,paint);meals[i].transform.localPosition=At(table.x-.18f,.823f,table.y);
        }
        var smokeShape=Cache("Smoke",m=>{m.Ink("DDD9CACC");m.Blob(Vector3.zero,At(1,.74f,.81f),7,8);});
        for(int i=0;i<smoke.Length;i++)
        {var cloud=Draw("Chimney smoke",garden,smokeShape,wash);vapor[i]=cloud.transform;smoke[i]=cloud.GetComponent<MeshRenderer>();}
        var curtain=RainField();collection.Add("Garden shower",curtain);showers=Draw("Summer shower",garden,curtain,rain).gameObject;
        last=g.Hero;ready=true;
    }
    static Vector3 Joint(int part)
    {
        if(part==0)return Vector3.zero;
        float side=part==1||part==3?1:-1;
        return part<=2?At(side*.205f,.833f,0):At(side*.103f,.448f,0);
    }
    void Refresh(HarvestGame g)
    {
        if(field!=g.Field){
            field=g.Field;wheel.gameObject.SetActive(field==1);
            scenery.sharedMesh=field==0?Cache("Homestead",m=>new HarvestHomestead(m).Build()):Cache("Mill terrace",HarvestTerrace.Build);
            grounding.gameObject.SetActive(field==0);decoration.gameObject.SetActive(field==0);
            foreach(var customer in guests)customer.gameObject.SetActive(field==0);
            foreach(var meal in meals)meal.gameObject.SetActive(field==0);
            foreach(var cloud in vapor)cloud.gameObject.SetActive(field==0);
        }
        for(int i=0;i<beds.Length;i++)
        {
            var b=beds[i];int id=g.PlotCrop(i);
            string key=id>=0&&id<g.Crops.Length?g.Crops[id].id:"";
            float growth=g.Growth(i);int stage=key.Length==0?-1:g.Ripe(i)?3:growth>=.55f?2:growth>=.24f?1:0;
            bool moist=g.Watered(i);
            if(moist!=b.damp)
            {
                if(moist&&day==g.Day&&b.crop==key&&stage>=0&&g.FocusPlot==i)
                {wateredPlot=i;waterUntil=time+.58f;}
                b.soil.sharedMaterial=moist?wet:paint;b.damp=moist;
            }
            if(key==b.crop&&stage==b.stage)continue;
            if(b.custom){b.custom.gameObject.SetActive(false);Remove(b.custom.gameObject);b.custom=null;}
            b.stage=stage;b.crop=key;b.plant.sharedMesh=null;
            b.pickHeight=stage<1?.22f:stage==1?.41f:stage==2?.66f:.86f;
            if(stage<0)continue;
            // Optional stage prefabs let a hand-authored crop override the built-in illustrations.
            string resource="Harvest/Art/Crops/"+key+"/"+stage;
            if(!customPlants.TryGetValue(resource,out var prefab))
            {prefab=Resources.Load<GameObject>(resource);customPlants.Add(resource,prefab);}
            if(prefab){b.custom=Instantiate(prefab,b.root).transform;continue;}
            int type=Mathf.Clamp(g.PlotArtKind(i),0,15);
            b.plant.sharedMesh=Cache(key+" stage "+stage,m=>new HarvestCrops(m).Build(type,stage));
        }
        if(season!=g.Season)
        {
            season=g.Season;
            decoration.sharedMesh=Cache("Orchard "+season,m=>HarvestLife.Season(m,season));
        }
        weather=g.Weather;day=g.Day;showers.SetActive(weather==1);
        for(int j=0;j<meals.Length;j++)
        {
            int recipe=Mathf.Max(0,g.OrderRecipe(j));
            meals[j].sharedMesh=g.OrderDone(j)?Cache("Dish "+recipe,m=>HarvestLife.Meal(m,recipe)):null;
        }
    }
    void AnimateOwner(HarvestGame g)
    {
        var p=g.Hero;float y=FloorHeight(p);
        stride+=Mathf.Min(.20f,Vector2.Distance(p,last))*7.9f;last=p;
        owner.localPosition=At(p.x,y+.014f,p.y);
        Transform body=limbs[0].parent;
        if(g.Facing.sqrMagnitude>.001f)
            body.rotation=Quaternion.RotateTowards(body.rotation,Quaternion.LookRotation(At(g.Facing.x,0,g.Facing.y)),Mathf.Max(1,Time.unscaledDeltaTime*890));
        float walk=Mathf.Sin(stride)*g.Motion;body.localPosition=Vector3.up*(Mathf.Abs(walk)*.049f);
        for(int j=1;j<limbs.Length;j++)
        {
            int direction=j==1||j==4?1:-1;
            float act=j==1&&g.FocusPlot>=0?g.Flash:0;
            limbs[j].localRotation=Quaternion.Euler(walk*direction*(j<3?21:27)-act*49,0,0);
        }
        bool tending=g.FocusPlot>=0&&g.Mode==HarvestMode.Farm;
        can.gameObject.SetActive(tending);
        water.gameObject.SetActive(tending&&wateredPlot==g.FocusPlot&&time<waterUntil);
    }
    float FloorHeight(Vector2 p)
    {
        if(field==1)return .025f;
        if(p.x>-.19f&&p.x<8.23f&&p.y>1.40f&&p.y<5.45f)return .288f;
        if(p.x>2.02f&&p.x<9.32f&&p.y> -4.3f&&p.y<.81f)return .072f;
        foreach(var b in beds)if(Mathf.Abs(p.x-b.center.x)<.80f&&Mathf.Abs(p.y-b.center.y)<.73f)return .16f;
        return .023f;
    }
    public int PickSite(Ray ray)
    {
        if(!ready)return -1;
        int best=-1;float nearest=float.MaxValue;
        void bounds(int id,Vector3 center,Vector3 size)
        {
            if(new Bounds(center,size).IntersectRay(ray,out float at)&&at>=0&&at<nearest){best=id;nearest=at;}
        }
        for(int i=0;i<beds.Length;i++){var b=beds[i];bounds(i,At(b.center.x,b.pickHeight*.5f,b.center.y),At(1.70f,b.pickHeight,1.52f));}
        if(field==1)return best;
        bounds(12,At(4.20f,.70f,2.79f),At(2.04f,1.28f,.95f));bounds(12,At(4.57f,.93f,4.40f),At(1.80f,1.65f,1.29f));
        bounds(13,At(5.60f,.55f,-.15f),At(1.58f,1.05f,.85f));
        foreach(var table in HarvestHomestead.Tables)bounds(13,At(table.x,.60f,table.y+.21f),At(1.08f,1.17f,1.55f));
        bounds(14,At(-6.20f,1.05f,4.25f),At(2.37f,2.12f,1.49f));
        bounds(15,At(.97f,.68f,3.99f),At(1.27f,1.06f,2.04f));
        bounds(16,At(-.13f,.57f,-4.41f),At(1.15f,1.10f,.62f));
        return best;
    }
    static void Shadow(HarvestMesh m,Vector3 center,float w,float d)
    {
        string[] shades={"35463783","35463768","35463731","35463700"};
        float[] extent={0,.37f,.71f,1};
        for(int band=0;band<3;band++)for(int j=0;j<22;j++)
        {
            float a=j*Mathf.PI/11,b=(j+1)*Mathf.PI/11;
            Vector3 l=At(Mathf.Cos(a)*w,0,Mathf.Sin(a)*d),r=At(Mathf.Cos(b)*w,0,Mathf.Sin(b)*d);
            m.GradientTri(center+l*extent[band],center+l*extent[band+1],center+r*extent[band+1],shades[band],shades[band+1],shades[band+1]);
            m.GradientTri(center+l*extent[band],center+r*extent[band+1],center+r*extent[band],shades[band],shades[band+1],shades[band]);
        }
    }
    static void StaticShadows(HarvestMesh m)
    {
        Shadow(m,At(-6.34f,.043f,4.23f),1.51f,1.10f);Shadow(m,At(5.54f,.083f,-.14f),1.02f,.61f);
        Shadow(m,At(-.16f,.038f,-4.39f),.82f,.48f);Shadow(m,At(-8.32f,.019f,-2.50f),.72f,.56f);
        Shadow(m,At(4.20f,.309f,2.82f),1.35f,.75f);Shadow(m,At(.97f,.313f,3.99f),.88f,1.24f);
        foreach(var p in HarvestHomestead.Tables)
        {
            Shadow(m,At(p.x,.083f,p.y),.78f,.64f);Shadow(m,At(p.x-.73f,.083f,p.y),.36f,.34f);
            Shadow(m,At(p.x,.083f,p.y+.73f),.36f,.36f);
        }
        foreach(var p in new[]{At(-9.52f,.008f,2.59f),At(-8.07f,.007f,6.95f),At(-3.77f,.007f,6.88f),At(9.72f,.008f,5.27f),At(10.29f,.008f,-3.69f)})
            Shadow(m,p,1.40f,1.08f);
    }
    static Mesh RainField()
    {
        const int count=190;var v=new Vector3[count*4];var uv=new Vector2[v.Length];var phase=new Vector2[v.Length];var t=new int[count*6];
        var rng=new System.Random(194);
        for(int j=0;j<count;j++)
        {
            var p=At((float)rng.NextDouble()*27-13.5f,0,(float)rng.NextDouble()*18-8);
            var time=new Vector2((float)rng.NextDouble(),(float)rng.NextDouble());int k=j*4;
            for(int n=0;n<4;n++){v[k+n]=p;phase[k+n]=time;uv[k+n]=new Vector2(n==0||n==3?-1:1,n>=2?1:0);}
            int b=j*6;t[b]=k;t[b+1]=k+1;t[b+2]=k+2;t[b+3]=k;t[b+4]=k+2;t[b+5]=k+3;
        }
        var mesh=new Mesh{name="Garden rain streaks",vertices=v,uv=uv,uv2=phase,triangles=t};
        mesh.bounds=new Bounds(At(0,3,0),At(30,8,23));return mesh;
    }
    void OnDestroy()
    {
        foreach(var mesh in collection.Values)Remove(mesh);Remove(paint);Remove(wet);Remove(wash);Remove(rain);
    }
}
