using UnityEngine;
using static HarvestMesh;
using static HarvestMesh.Finish;

internal sealed class HarvestHomestead
{
    internal static readonly Vector2[] Tables={new Vector2(3.04f,-1.65f),new Vector2(5.76f,-3.21f),new Vector2(7.88f,.12f)};
    internal static readonly Vector3 Chimney=At(5.24f,3.91f,5.65f);
    readonly HarvestMesh m;
    const string Timber="916C51",Honey="C79B68",Cream="F1D9AF",Blue="639396",Sage="88A477",Iron="556368",Gold="D7AC69";
    public HarvestHomestead(HarvestMesh painter){m=painter;}
    void I(string c,Finish surface=Plain,float glow=0)=>m.Ink(c,surface,glow);
    void B(float x,float y,float z,float w,float h,float d,float turn=0)=>m.Block(x,y,z,w,h,d,turn);
    void Box(Vector3 p,Vector3 size,float tilt=0,float yaw=0,float roll=0)=>m.Box(p,size,Quaternion.Euler(tilt,yaw,roll));
    void D(float x,float y,float z,float r,float s,float h,int n=10)=>m.Drum(x,y,z,r,s,h,n);
    void O(float x,float y,float z,float w,float h,float d,int seed=0,int n=9)=>m.Blob(At(x,y,z),At(w,h,d),seed,n);
    void Pole(Vector3 a,Vector3 b,float r,float s,int n=8)=>m.Tube(a,b,r,s,n);
    public void Build()
    {
        Ground();Cottage();Market();Patio();Fair();WaterTap();Landscape();
    }
    void Ground()
    {
        I("B3C894",Grass);B(0,-.28f,0,95,.52f,85);
        I("B9CC95",Grass);B(0,-.043f,0,20.8f,.087f,12.4f);
        I("B4AF80",Grass);B(-3.48f,.004f,-.77f,7.89f,.015f,5.89f);
        I("D6C39B",Path);
        Trail(new[]{new Vector2(-.15f,-6.5f),new Vector2(.43f,-4.66f),new Vector2(.64f,-3.30f),new Vector2(.77f,-1.70f),
            new Vector2(.88f,-.13f),new Vector2(.57f,1.36f),new Vector2(.82f,2.49f)},1.12f);
        Trail(new[]{new Vector2(.74f,1.81f),new Vector2(-.83f,2.39f),new Vector2(-2.48f,2.44f),new Vector2(-4.07f,2.55f),
            new Vector2(-5.33f,2.72f),new Vector2(-6.36f,3.50f)},.84f);
        Trail(new[]{new Vector2(.73f,-1.41f),new Vector2(1.64f,-1.38f),new Vector2(2.31f,-.93f)},.84f);
        I("D8C5A0",Path);B(5.66f,.026f,-1.74f,7.24f,.080f,5.04f);
        I("C6AD85",Path);B(5.65f,.041f,-4.29f,7.35f,.096f,.15f);
        for(int j=0;j<8;j++){I(j%2==0?"D4B88E":"CAB188",Path);B(2.48f+j*.90f,.066f,-4.30f,.85f,.105f,.225f);}
        for(int j=0;j<4;j++){I("D8C8A3",Path);B(1.43f+.20f*j,.032f,1.17f+j*.333f,.60f,.046f,.22f,6*j);}
    }
    void Trail(Vector2[] point,float width)
    {
        Vector3 left=Vector3.zero,right=Vector3.zero;
        for(int j=0;j<point.Length;j++)
        {
            var delta=(point[Mathf.Min(j+1,point.Length-1)]-point[Mathf.Max(0,j-1)]).normalized;
            var center=At(point[j].x,.019f,point[j].y);
            var side=At(-delta.y,0,delta.x)*width*(.46f+.047f*Mathf.Cos(j));
            if(j>0)m.Quad(left,right,center-side,center+side,Vector3.up);
            left=center+side;right=center-side;
        }
    }
    void Cottage()
    {
        // A cutaway through the south wall and front roof keeps the kitchen and bedroom usable.
        float left=-.16f,right=8.14f,zBack=5.39f;
        I("AC9E85",Path);B(4,.119f,3.43f,8.46f,.23f,4.05f);
        I("D8B687",Wood);B(4,.248f,3.40f,8.41f,.05f,4.01f);
        for(int j=0;j<11;j++){I(j%2==0?"D7B284":"DDBA8E",Wood);B(.13f+j*.765f,.279f,3.43f,.751f,.029f,3.90f);}
        I("EBD9B9",Plaster);B(3.97f,1.49f,zBack,8.38f,2.55f,.25f);
        B(left,1.47f,4.35f,.24f,2.43f,2.1f);B(right,1.47f,4.35f,.24f,2.43f,2.1f);
        I(Timber,Wood);
        foreach(float x in new[]{left,2.39f,5.34f,right})B(x,1.48f,zBack-.142f,.145f,2.52f,.156f);
        B(3.99f,.45f,zBack-.145f,8.37f,.18f,.11f);B(3.99f,2.55f,zBack-.13f,8.52f,.17f,.17f);
        B(left,1.44f,3.30f,.16f,2.41f,.17f);B(right,1.44f,3.30f,.16f,2.41f,.17f);
        B(.0f,.31f,1.36f,.20f,.28f,.32f);B(8.04f,.31f,1.36f,.20f,.28f,.32f);
        I(Honey,Wood);B(4,.173f,1.30f,4.30f,.18f,.38f);B(4,.084f,1.00f,3.73f,.12f,.34f);
        // Full rear roof and a cropped front pitch reveal the working room from the playing camera.
        I("B87957",Roof);Box(At(4,3.03f,5.18f),At(8.88f,.149f,1.79f),-27);
        I("CB885F",Roof);Box(At(4,3.03f,6.74f),At(8.88f,.149f,1.79f),27);
        I("8C6750",Wood);B(4,2.64f,4.38f,9.0f,.14f,.19f);
        I("B97C59",Roof);Pole(At(-.47f,3.44f,5.96f),At(8.46f,3.44f,5.96f),.115f,.115f,10);
        foreach(float x in new[]{-.24f,8.22f})
        {
            I("E5CC99",Plaster);
            m.Tri(At(x,2.62f,4.41f),At(x,3.39f,5.96f),At(x,2.62f,7.5f),x<0?Vector3.left:Vector3.right);
        }
        I("B48D77",Path);B(Chimney.x,3.43f,Chimney.z,.50f,.93f,.56f);
        I("987764",Path);B(Chimney.x,3.90f,Chimney.z,.64f,.12f,.70f);
        I("574F4B");B(Chimney.x,3.966f,Chimney.z,.37f,.018f,.39f);
        Window(.98f,1.65f);Window(6.87f,1.64f);
        Bed();Kitchen();
        // Round farmhouse clock and suspended dried-herb bunches.
        I(Timber,Wood);Pole(At(2.90f,2.01f,5.17f),At(2.90f,2.01f,5.085f),.22f,.22f,14);
        I("F2E0B2",Plaster);Pole(At(2.90f,2.01f,5.075f),At(2.90f,2.01f,5.058f),.177f,.177f,14);
        I(Iron);Box(At(2.92f,2.059f,5.048f),At(.026f,.132f,.015f),0,0,-24);
        for(int j=0;j<3;j++){float x=3.27f+j*.29f;I(Timber,Wood);Pole(At(x,2.44f,5.10f),At(x,2.21f,5.1f),.012f,.012f,5);
            I(j==1?"A8AE84":"7C966F",Fruit);O(x,2.14f,5.08f,.101f,.163f,.070f,8+j);}
    }
    void Window(float x,float y)
    {
        I("9D7555",Wood);B(x,y,5.21f,1.21f,1.12f,.15f);
        I("8CAFB1");B(x,y,5.11f,.99f,.91f,.058f);
        I("F0DCB2",Wood);B(x,y,5.064f,.046f,.954f,.039f);B(x,y,5.064f,1.02f,.043f,.039f);
        I(Honey,Wood);B(x,y-.57f,5.13f,1.42f,.10f,.27f);
        foreach(float s in new[]{-1f,1f})
        {
            I("80A29A",Wood);Box(At(x+s*.78f,y,5.16f),At(.32f,1.03f,.072f),0,s*12);
            I("678D85",Wood);for(int j=0;j<4;j++)B(x+s*.78f,y-.35f+j*.23f,5.107f,.23f,.038f,.030f);
        }
        I("EECDB4",Cloth);B(x,y+.45f,5.035f,1.04f,.156f,.042f);
    }
    void Bed()
    {
        float x=.97f,z=3.99f;
        I(Timber,Wood);B(x,.513f,z,1.19f,.30f,1.95f);B(x,.79f,z+.93f,1.27f,.74f,.116f);
        I(Cream,Cloth);B(x,.711f,z,1.13f,.134f,1.83f);
        I("8CA9A1",Cloth);B(x,.801f,z-.29f,1.16f,.077f,1.15f);
        for(int j=0;j<4;j++){I(j%2==0?"C8CEC0":"759E9A",Cloth);B(x,.844f,z-.73f+j*.256f,1.15f,.011f,.060f);}
        I("E9D7B8",Cloth);O(x,.822f,z+.58f,.422f,.101f,.227f,88);
        I("BD946A",Wood);B(1.90f,.52f,4.57f,.45f,.49f,.42f);I("E4C58C",Cloth);
        D(1.90f,.82f,4.57f,.148f,.091f,.155f);I(Gold,Wood);D(1.90f,.762f,4.57f,.077f,.037f,.075f);
        I("A88677",Cloth);B(x,.311f,2.63f,1.33f,.029f,.45f);
    }
    void Kitchen()
    {
        // Bread oven, open fire mouth, hanging copper pans, crockery and fresh dough.
        I("AE8D76",Path);B(4.57f,.724f,4.42f,1.80f,.88f,1.10f);
        I("D8BF9D",Plaster);O(4.57f,1.13f,4.42f,.92f,.70f,.59f,3,13);
        I("88695C",Path);Pole(At(4.57f,1.075f,4.05f),At(4.57f,1.075f,3.833f),.390f,.386f,14);
        I("544741");Pole(At(4.57f,1.052f,3.835f),At(4.57f,1.052f,3.808f),.297f,.290f,14);
        I("EA9758",Fruit,.66f);O(4.57f,.957f,3.783f,.213f,.099f,.046f,5);
        I("E3BD83",Wood);B(4.57f,.778f,3.801f,1.47f,.105f,.37f);
        foreach(float x in new[]{2.8f,6.18f})
        {
            I(Timber,Wood);B(x,.701f,4.73f,1.14f,.82f,.67f);
            I(Honey,Wood);B(x,1.16f,4.70f,1.25f,.13f,.78f);
            I("C7AC81",Wood);B(x,.751f,4.375f,.94f,.16f,.048f);
            I(Iron);B(x,.755f,4.337f,.18f,.027f,.022f);
        }
        I("B88B64",Wood);B(4.2f,.74f,2.79f,1.94f,.89f,.83f);
        I("DFBC89",Wood);B(4.2f,1.23f,2.79f,2.04f,.116f,.93f);
        I("8AA4A0",Cloth);B(3.80f,1.298f,2.77f,.42f,.026f,.90f);
        B(3.80f,1.129f,2.292f,.42f,.317f,.031f);
        I("CEA16E",Wood);B(4.31f,1.309f,2.78f,.50f,.035f,.34f,11);
        Bread(4.29f,1.354f,2.78f,.13f);
        I("AD7152");D(4.86f,1.294f,2.90f,.151f,.188f,.173f,12);
        I("E6CBA2");D(4.86f,1.469f,2.90f,.152f,.152f,.012f,12);
        I("926A4B",Wood);Pole(At(4.88f,1.47f,2.91f),At(4.96f,1.72f,2.90f),.017f,.024f,6);
        I(Iron);B(5.88f,1.99f,5.06f,1.22f,.036f,.075f);
        for(int j=0;j<3;j++)
        {
            float x=5.46f+j*.39f;I(Iron);B(x,1.89f,5.01f,.041f,.19f,.042f);
            I(j==2?"778C89":"B47E62");Pole(At(x,1.69f,5.01f),At(x,1.69f,4.949f),.137f,.153f,10);
        }
        I(Honey,Wood);B(7.32f,1.38f,4.95f,1.43f,.090f,.29f);
        for(int j=0;j<5;j++)Jar(6.8f+j*.25f,1.43f,4.91f,j);
        I("F0DAAF",Plaster);D(2.7f,1.24f,4.73f,.23f,.29f,.14f,12);
        I("92A59A");D(2.7f,1.38f,4.73f,.225f,.224f,.009f,12);
    }
    void Bread(float x,float y,float z,float r)
    {
        I("DCA66A",Fruit);O(x,y,z,r*1.42f,r*.86f,r,22,10);I("F0CA88",Fruit);
        for(int j=0;j<3;j++)Box(At(x+(j-1)*r*.56f,y+r*.67f,z),At(r*.10f,r*.083f,r*.71f),0,-25);
    }
    void Jar(float x,float y,float z,int j)
    {
        I(j%3==0?"8EADA4":j%3==1?"CE9978":"E1C59C",Plaster);
        D(x,y,z,.066f,.097f,.068f);D(x,y+.068f,z,.097f,.087f,.14f);I(Timber,Wood);D(x,y+.208f,z,.092f,.093f,.041f);
    }
    void Market()
    {
        float x=-6.2f,z=4.25f;
        foreach(float dx in new[]{-.97f,.97f})foreach(float dz in new[]{-.46f,.46f})
        {I(Timber,Wood);B(x+dx,1.03f,z+dz,.091f,2.06f,.095f);}
        for(int j=0;j<8;j++)
        {
            I(j%2==0?"D88062":"F4DCAC",Cloth);
            Box(At(x-1.048f+j*.304f,2.01f,z),At(.305f,.056f,1.48f),-8);
            B(x-1.048f+j*.304f,1.817f,z-.72f,.304f,.20f,.04f);
        }
        I(Timber,Wood);B(x,.62f,z,2.14f,.82f,.93f);
        I(Honey,Wood);B(x,1.08f,z,2.25f,.104f,1.03f);
        for(int j=0;j<3;j++)
        {
            float xx=x+(j-1)*.65f;
            I("C79867",Wood);B(xx,1.18f,z,.60f,.16f,.55f);
            I("946C51",Wood);B(xx,1.264f,z,.51f,.019f,.45f);
            for(int k=0;k<5;k++)
            {
                I(j==0?"D7896C":j==1?"94AF6E":"E0BC6F",Fruit);
                O(xx+Mathf.Cos(k*2.4f)*.168f,1.298f,z+Mathf.Sin(k*2.4f)*.115f,.083f,.085f,.082f,17+j+k,7);
            }
        }
        I("78998D",Cloth);B(x,.734f,z-.485f,.77f,.43f,.035f);
        WheatBadge(At(x,.73f,z-.508f),.39f);
        I("AE8158",Wood);B(x+1.29f,.205f,z,.56f,.40f,.49f);B(x+1.28f,.57f,z+.055f,.49f,.31f,.41f,12);
    }
    void WheatBadge(Vector3 p,float size)
    {
        I("EED08F",Fruit);m.Beam(p+At(-.16f,-.31f,0)*size,p+At(.13f,.31f,0)*size,.024f,.015f);
        for(int i=0;i<4;i++)
        {
            var root=p+At(-.10f+i*.061f,-.16f+i*.108f,0)*size;
            foreach(float sign in new[]{-1f,1f})m.Beam(root,root+At(sign*.13f,.089f,0)*size,.050f*size,.020f);
        }
    }
    void Patio()
    {
        for(int j=0;j<Tables.Length;j++)
        {
            float x=Tables[j].x,z=Tables[j].y;
            I(Timber,Wood);D(x,.072f,z,.28f,.13f,.12f);D(x,.179f,z,.115f,.092f,.53f);
            I(Honey,Wood);D(x,.70f,z,.54f,.585f,.085f,15);
            I(j==1?"AD857C":"8EAAA2",Cloth);B(x-.02f,.792f,z,.251f,.018f,1.02f,8);
            I("EFDEC1",Plaster);D(x-.18f,.809f,z,.137f,.164f,.021f,12);
            D(x+.174f,.807f,z+.04f,.071f,.084f,.092f);
            I("A28465");D(x+.174f,.899f,z+.04f,.063f,.063f,.006f);
            I("84A99E",Plaster);D(x+.03f,.808f,z+.26f,.073f,.058f,.135f);
            I("699366",Fruit);Pole(At(x+.03f,.94f,z+.26f),At(x+.06f,1.096f,z+.26f),.011f,.009f,5);
            I("EDD39F",Fruit);O(x+.06f,1.10f,z+.26f,.059f,.039f,.057f,j,6);
            Chair(x,z+.74f,0);Chair(x-.72f,z-.02f,90);
        }
        float cx=5.60f,cz=-.15f;
        I("9F7D60",Wood);B(cx,.503f,cz,1.47f,.84f,.69f);
        I("D7B384",Wood);B(cx,.977f,cz,1.57f,.120f,.79f);
        I("8BA497",Cloth);B(cx+.21f,1.048f,cz,.38f,.025f,.78f);B(cx+.21f,.88f,cz-.414f,.38f,.34f,.030f);
        I("EFDEC1",Plaster);for(int j=0;j<3;j++)D(cx-.34f,1.042f+j*.044f,cz,.222f,.245f,.025f,13);
        I(Gold);D(cx+.38f,1.054f,cz,.132f,.058f,.097f,10);D(cx+.38f,1.154f,cz,.032f,.027f,.040f,7);
        // Menu board with a wheat emblem.
        I(Timber,Wood);Box(At(cx+.94f,.542f,cz-.1f),At(.53f,.82f,.066f),-8);
        I("516D67",Plaster);Box(At(cx+.94f,.546f,cz-.15f),At(.43f,.68f,.034f),-8);
        WheatBadge(At(cx+.94f,.64f,cz-.181f),.44f);I(Cream);
        for(int j=0;j<3;j++)B(cx+.94f,.49f-j*.068f,cz-.182f,.19f+.022f*(j%2),.015f,.012f);
    }
    void Chair(float x,float z,float yaw)
    {
        var q=Quaternion.Euler(0,yaw,0);Vector3 p(float a,float b,float c)=>At(x,0,z)+q*At(a,b,c);
        I(Timber,Wood);
        foreach(float a in new[]{-.16f,.16f})foreach(float c in new[]{-.14f,.14f})
            m.Box(p(a,.263f,c),At(.059f,.41f,.064f),q);
        I(Honey,Wood);m.Box(p(0,.472f,0),At(.43f,.075f,.39f),q);
        foreach(float a in new[]{-.166f,.166f})m.Box(p(a,.644f,.16f),At(.051f,.38f,.055f),q);
        m.Box(p(0,.805f,.17f),At(.43f,.110f,.050f),q);
    }
    void Fair()
    {
        float x=-.13f,z=-4.41f;
        foreach(float dx in new[]{-.38f,.38f}){I(Timber,Wood);B(x+dx,.34f,z,.075f,.63f,.31f);}
        I(Honey,Wood);B(x,.69f,z,1.12f,.11f,.59f);
        I("8899A8",Cloth);B(x,.753f,z,.48f,.019f,.58f);B(x,.605f,z-.316f,.48f,.29f,.025f);
        I("D4AB68");D(x,.768f,z,.105f,.15f,.121f,10);D(x,.889f,z,.147f,.076f,.19f,10);
        I("EBC983");D(x,1.08f,z,.082f,.079f,.043f,8);
        I(Timber,Wood);Pole(At(x+.62f,0,z+.15f),At(x+.62f,1.69f,z+.15f),.037f,.033f,7);
        I("C67E63",Cloth);m.Tri(At(x+.60f,1.66f,z+.15f),At(x+1.05f,1.51f,z+.14f),At(x+.60f,1.38f,z+.15f),Vector3.back);
    }
    void WaterTap()
    {
        float x=-8.30f,z=-2.50f;I("AC997E",Path);D(x,0,z,.51f,.48f,.12f);
        I("72948B",Plain);D(x,.12f,z,.10f,.098f,.72f);Pole(At(x,.81f,z),At(x+.25f,.81f,z),.065f,.061f,8);
        I(Timber,Wood);B(x-.14f,.88f,z,.41f,.048f,.06f,-12);
        I(Honey,Wood);D(x+.27f,.13f,z,.17f,.201f,.27f);I(Iron);D(x+.27f,.329f,z,.204f,.207f,.038f);
        I("89AFA5",Water);D(x+.27f,.399f,z,.163f,.164f,.008f);
    }
    void Landscape()
    {
        // Orchard rows, a narrow stream and low fences enclose the diorama without hiding plots.
        I("82AFA3",Water);B(-4.6f,-.007f,8.49f,31.7f,.065f,1.85f,-6);
        for(int j=0;j<8;j++)
        {
            float x=-15f+j*4.0f;I(j%2==0?"9DB493":"A7BC91",Grass);
            O(x,0,12.5f+Mathf.Sin(j*1.8f),3.96f,1.14f+.36f*Mathf.Sin(j),2.4f,j+84,11);
        }
        Fence(At(-9.2f,0,-4.60f),At(-9.2f,0,4.95f),6);
        Fence(At(-8.8f,0,-5.34f),At(-2.1f,0,-5.34f),4);
        Fence(At(2.01f,0,-5.34f),At(8.84f,0,-5.34f),4);
        Fence(At(9.52f,0,-4.85f),At(9.52f,0,4.27f),6);
        Fence(At(-8.9f,0,5.66f),At(-1.6f,0,5.66f),5);
        Tree(-9.52f,2.59f,1.13f,4);Tree(-8.07f,6.95f,1.12f,9);Tree(-3.77f,6.88f,1.06f,6);
        Tree(9.72f,5.27f,1.10f,13);Tree(10.29f,-3.69f,.87f,16);
        for(int j=0;j<14;j++)Tree(-15f+j*2.75f,10.15f+1.0f*Mathf.Sin(j*1.9f),.82f+.16f*Mathf.Sin(j),31+j);
        for(int j=0;j<22;j++)
        {
            float x=-9f+j*.85f;if(Mathf.Abs(x)<1.95f)continue;
            float z=-5.45f+.091f*Mathf.Sin(j*2);
            I(j%2==0?"7F9E70":"91A878",Foliage);O(x,.17f,z,.39f,.24f,.25f,j,7);
            for(int n=0;n<3;n++){I(n%2==0?"F2CF8C":"E8C8B3",Fruit);O(x+Mathf.Sin(n+j)*.19f,.347f,z+Mathf.Cos(n)*.096f,.053f,.029f,.048f,n+j,5);}
        }
    }
    void Fence(Vector3 a,Vector3 b,int n)
    {
        for(int j=0;j<=n;j++){Vector3 p=Vector3.Lerp(a,b,(float)j/n);I("B99265",Wood);B(p.x,.35f,p.z,.122f,.70f,.13f);I(Honey,Wood);D(p.x,.69f,p.z,.083f,.062f,.079f,4);}
        foreach(float y in new[]{.29f,.55f}){I("C7A676",Wood);m.Beam(a+Vector3.up*y,b+Vector3.up*y,.078f,.082f);}
    }
    void Tree(float x,float z,float s,int seed)
    {
        Vector3 p(float a,float b,float c)=>At(x+a*s,b*s,z+c*s);
        I("9C7B57",Wood);Pole(p(0,0,0),p(.045f,1.25f,.04f),.21f*s,.13f*s,8);
        Pole(p(0,.85f,0),p(-.37f,1.55f,.09f),.10f*s,.051f*s,7);
        for(int j=0;j<4;j++)
        {
            float a=(j+seed)*2.39f;
            I(j==0?"A3BC76":j==1?"8BA76D":"95B375",Foliage);
            m.Blob(p(Mathf.Cos(a)*.46f,1.72f+.17f*j,Mathf.Sin(a)*.44f),At(.76f,.62f,.72f)*s,seed+j,8);
        }
        I("ADC482",Foliage);m.Blob(p(.041f,2.36f,0),At(.71f,.56f,.66f)*s,seed+17,8);
    }
}
