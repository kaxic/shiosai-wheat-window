using UnityEngine;
using static HarvestMesh;
using static HarvestMesh.Finish;

internal static class HarvestLife
{
    public static void Person(HarvestMesh m,int guest,int part)
    {
        bool owner=guest<0;string skin=guest==1?"BF8F76":guest==2?"DBA58B":"EBC2A0";
        string hair=guest==0?"ABA7A0":guest==1?"625348":guest==2?"87604D":"795A48";
        string top=owner?"ECE0BC":guest==0?"AB91A3":guest==1?"7E9B9F":"C88970";
        if(part==1 || part==2)
        {
            float sign=part==1?1:-1;
            m.Ink(top,Cloth);m.Tube(Vector3.zero,At(sign*.047f,-.176f,.028f),.093f,.074f,8);
            m.Ink(skin);var wrist=owner?At(sign*.051f,-.30f,.06f):At(sign*.019f,-.22f,.14f);
            m.Tube(At(sign*.047f,-.158f,.027f),wrist,.061f,.052f,7);
            m.Blob(wrist,At(.069f,.075f,.063f),part,7);return;
        }
        if(part>=3)
        {
            float sign=part==3?1:-1;
            m.Ink(owner?"5D7778":"66737B",Cloth);
            if(owner)
            {
                m.Tube(Vector3.zero,At(sign*.014f,-.278f,.004f),.102f,.075f,7);
                m.Ink("8C6E53",Wood);m.Blob(At(sign*.013f,-.357f,.043f),At(.092f,.100f,.139f),6,8);
            }
            else
            {
                m.Tube(Vector3.zero,At(sign*.017f,-.03f,.18f),.092f,.077f,7);
                m.Tube(At(sign*.017f,-.03f,.18f),At(sign*.018f,-.271f,.18f),.077f,.055f,7);
                m.Ink("90735E",Wood);m.Blob(At(sign*.018f,-.30f,.211f),At(.072f,.068f,.10f),9,7);
            }
            return;
        }
        m.Ink(top,Cloth);m.Blob(At(0,.67f,0),At(.237f,.274f,.163f),3,9);
        m.Ink(owner?"839C85":guest==0?"958898":guest==1?"7E8A8E":"BB8E78",Cloth);m.Drum(0,.402f,0,.199f,.163f,.190f,9);
        m.Ink(skin);m.Drum(0,.86f,0,.098f,.095f,.114f,9);m.Blob(At(0,1.05f,.016f),At(.197f,.208f,.179f),21,9);
        m.Ink(hair,Cloth);m.Blob(At(0,1.125f,-.040f),At(.202f,.166f,.162f),61,10);
        m.Ink("685549");foreach(float x in new[]{-.075f,.075f})m.Blob(At(x,1.068f,.175f),At(.017f,.022f,.010f),5,6);
        m.Ink("D6A084");m.Blob(At(0,1.023f,.191f),At(.032f,.023f,.020f),9,6);
        if(owner)
        {
            m.Ink("789D88",Cloth);m.Block(0,.65f,.145f,.302f,.40f,.035f);
            m.Ink("A4B79A",Cloth);m.Block(0,.557f,.173f,.187f,.105f,.028f);
            foreach(float x in new[]{-.111f,.111f}){m.Ink("80977D",Cloth);m.Beam(At(x,.80f,.137f),At(x*.89f,.896f,.020f),.047f,.039f);}
            m.Ink("B88664",Wood);m.Block(0,.533f,-.137f,.29f,.13f,.077f);
            m.Ink("E5CCA4",Cloth);m.Drum(0,1.19f,0,.202f,.194f,.081f,12);
            m.Ink("EDE0C5",Cloth);m.Blob(At(-.021f,1.308f,-.020f),At(.247f,.112f,.201f),29,10);
            m.Ink("CB876B",Cloth);m.Drum(0,.872f,0,.112f,.134f,.057f,9);
            m.Beam(At(.051f,.876f,.091f),At(.109f,.760f,.166f),.06f,.036f);
        }
        else if(guest==0)
        {
            m.Ink("B5AEA3",Cloth);m.Blob(At(-.013f,1.18f,-.157f),At(.126f,.115f,.095f),23,9);
            m.Ink("E3D1BD",Cloth);m.Block(0,.771f,.133f,.259f,.136f,.031f);
        }
        else if(guest==1)
        {
            m.Ink("BEA679",Cloth);m.Blob(At(0,1.21f,0),At(.223f,.095f,.196f),14,10);
            m.Blob(At(0,1.162f,.137f),At(.173f,.030f,.149f),3,9);
            m.Ink("A0B3A7",Cloth);m.Drum(0,.854f,0,.139f,.13f,.07f,8);
        }
        else
        {
            m.Ink("9D7567",Cloth);m.Blob(At(.023f,1.244f,-.010f),At(.223f,.088f,.181f),32,9);
            m.Ink("E4CB9E",Cloth);m.Beam(At(-.085f,.875f,.09f),At(.17f,.539f,.159f),.036f,.024f);
        }
    }
    public static void Can(HarvestMesh m)
    {
        m.Ink("7CA59D");m.Drum(0,-.125f,0,.121f,.145f,.207f,10);
        m.Ink("A4BEB0");m.Drum(0,.081f,0,.148f,.141f,.023f,10);
        m.Ink("738E88");m.Tube(At(0,-.038f,.095f),At(0,.043f,.286f),.057f,.029f,7);
        m.Tube(At(0,.043f,.286f),At(0,.080f,.307f),.056f,.069f,8);
        m.Ink("9EB4A5");m.Beam(At(0,.081f,-.099f),At(0,.165f,-.088f),.024f,.025f);
        m.Beam(At(0,.165f,-.088f),At(0,.167f,.064f),.024f,.025f);m.Beam(At(0,.167f,.064f),At(0,.081f,.087f),.024f,.025f);
    }
    public static void Meal(HarvestMesh m,int style)
    {
        m.Ink("F1DEC3",Plaster);m.Drum(0,0,0,.161f,.193f,.031f,13);
        if(style%3==0)
        {
            m.Ink("D6A36F",Fruit);m.Blob(At(0,.073f,0),At(.132f,.092f,.116f),8,10);
            m.Ink("EFCC8A");for(int j=0;j<2;j++)m.Box(At((j-.5f)*.071f,.151f,.002f),At(.012f,.009f,.071f),Quaternion.Euler(0,-31,0));
        }
        else if(style%3==1)
        {
            m.Ink("8AA6A1",Plaster);m.Drum(0,.023f,0,.093f,.146f,.087f,12);
            m.Ink("D9A062",Fruit);m.Drum(0,.111f,0,.121f,.121f,.008f,12);
            m.Ink("9CB67C",Fruit);m.Leaf(At(0,.126f,0),.068f,.022f,38,.03f);
        }
        else
        {
            for(int k=0;k<7;k++){float a=k*2.39f;m.Ink(k%3==0?"C57B5D":"A3B67C",Fruit);
                m.Blob(At(Mathf.Cos(a)*.062f,.064f,Mathf.Sin(a)*.071f),At(.048f,.043f,.047f),k,7);}
        }
    }
    public static void Season(HarvestMesh m,int season)
    {
        var trunks=new[]{At(-9.52f,1.13f,2.59f),At(-8.07f,1.12f,6.95f),At(-3.77f,1.06f,6.88f),At(9.72f,1.10f,5.27f),At(10.29f,.87f,-3.69f)};
        int[] treeSeeds={4,9,6,13,16};
        for(int j=0;j<trunks.Length;j++)
        {
            var p=trunks[j];float s=p.y;p.y=0;
            if(season<=1)
            {
                for(int n=0;n<14;n++)
                {
                    int branch=n%5;
                    float azimuth=(branch+treeSeeds[j])*2.39f,a=n*2.399f+j*.53f;
                    var crown=branch==4?At(.041f,2.36f,0):At(Mathf.Cos(azimuth)*.46f,1.72f+.17f*branch,Mathf.Sin(azimuth)*.44f);
                    var normal=At(Mathf.Cos(a),.23f+.40f*Mathf.Sin(n*.84f),-.28f-.72f*Mathf.Abs(Mathf.Sin(a))).normalized;
                    var q=p+(crown+Vector3.Scale(normal,At(.67f,.54f,.63f)))*s;
                    if(season==0)
                    {
                        m.Ink(n%2==0?"D79391":"E2B1A6",Fruit);
                        m.Blob(q,At(.156f,.088f,.147f)*s,n+j,7);
                        m.Ink("F3D8CA",Fruit);
                        m.Blob(q+At(.057f,.059f,-.053f)*s,At(.115f,.060f,.105f)*s,n+7,7);
                    }
                    else
                    {
                        m.Ink(n%2==0?"C77859":"E9B272",Fruit);
                        m.Blob(q,At(.116f,.127f,.115f)*s,n+j,8);
                    }
                }
            }
            else if(season==3)
            {
                for(int n=0;n<4;n++){float a=(j+3+n)*2.4f;
                    m.Ink("DBE2D3",Cloth);m.Blob(p+At(Mathf.Cos(a)*.43f,2.42f+n*.063f,Mathf.Sin(a)*.38f)*s,At(.48f,.104f,.37f)*s,n+17,9);}
            }
        }
        if(season==2)
        {
            for(int j=0;j<36;j++){float x=-8.9f+17.8f*Mathf.Repeat(j*.618f,1),z=2.54f+.30f*Mathf.Sin(j*.91f);
                m.Ink(j%2==0?"BD8B58":"CBA363",Fruit);m.Leaf(At(x,.036f,z),.095f,.027f,j*57,.046f);}
        }
        if(season==3)
        {
            m.Ink("DBE1D7",Cloth);m.Box(At(4,3.062f,5.19f),At(8.71f,.131f,1.72f),Quaternion.Euler(-27,0,0));
            m.Box(At(4,3.064f,6.76f),At(8.70f,.129f,1.68f),Quaternion.Euler(27,0,0));
        }
        else
        {
            string[] blooms=season==0?new[]{"EABBAF","F3D7AA","EFE1C4"}:season==1?new[]{"A6A2C5","BCB5CF","D8BCD5"}:new[]{"DC9E65","D2AD68","D28662"};
            for(int i=0;i<32;i++)
            {
                float t=i/31f;
                float x=i%2==0?-8.61f+.31f*Mathf.Sin(i*1.72f):2.23f+6.56f*t;
                float z=i%2==0?-4.27f+5.39f*t:-4.86f+.18f*Mathf.Cos(i*2.71f);
                float h=.14f+.14f*Mathf.Repeat(i*.618f,1);
                var basePoint=At(x,.029f,z);var tip=basePoint+At(.035f*Mathf.Sin(i),h,0);
                m.Ink("6C8D68",Foliage);m.Tube(basePoint,tip,.017f,.013f,5);
                m.Leaf(basePoint+Vector3.up*.073f,.13f,.034f,i*67,.063f);
                m.Ink(blooms[i%3],Fruit);
                for(int k=0;k<4;k++){float a=i+k*1.57f;m.Blob(tip+At(Mathf.Cos(a)*.042f,0,Mathf.Sin(a)*.042f),At(.049f,.029f,.046f),k+i,5);}
                m.Ink("DDB464",Fruit);m.Blob(tip+Vector3.up*.017f,At(.032f,.030f,.031f),i,6);
            }
        }
    }
}
