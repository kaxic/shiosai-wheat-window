using UnityEngine;
using static HarvestMesh;
using static HarvestMesh.Finish;

internal static class HarvestTerrace
{
    public static readonly Vector3 WheelHub=At(4.35f,1.56f,1.72f);
    public static void Build(HarvestMesh m)
    {
        void block(float x,float y,float z,float w,float h,float d,string c,Finish f){m.Ink(c,f);m.Block(x,y,z,w,h,d);}
        void beam(Vector3 a,Vector3 b,float w,string c,Finish f){m.Ink(c,f);m.Beam(a,b,w,w);}
        block(0,-.41f,0,34,.81f,24,"AABD82",Grass);
        block(.75f,.019f,0,1.08f,.04f,12,"D1BB96",Path);
        block(3.75f,.026f,-2.85f,6.8f,.045f,1.0f,"DCC8A0",Path);
        // The irrigation channel runs under the mill wheel and along the nursery.
        block(4.40f,.035f,1.7f,1.46f,.06f,12,"7BAEAD",Water);
        for(int s=-1;s<=1;s+=2) for(int j=0;j<13;j++)
            block(4.4f+s*.83f,.15f,-4.3f+j*.89f,.26f,.33f,.86f,j%2==0?"B9AB91":"A49B86",Path);
        for(int i=0;i<18;i++){
            float z=-4.3f+i*.55f;block(4.3f+.25f*Mathf.Sin(i*1.7f),.072f,z,.47f,.01f,.025f,"BCCFC1",Water);}
        block(5.52f,.91f,3.53f,2.52f,1.8f,1.9f,"DFCAA5",Plaster);
        for(int i=0;i<11;i++)for(int side=-1;side<=1;side+=2){
            m.Ink(i%3==0?"889A95":"91A59A",Roof);m.Box(At(4.17f+i*.27f,2.20f,3.53f+side*.53f),
                At(.28f,.15f,1.34f),Quaternion.Euler(side*36,0,0));}
        for(int s=-1;s<=1;s+=2){block(5.49f+s*.90f,.92f,2.55f,.16f,1.83f,.14f,"8B7053",Wood);
            block(5.50f+s*.55f,1.22f,2.55f,.63f,.55f,.06f,"749B97",Water);}
        // Raised lavender banks, herb drying lines and a sheltered seedling frame.
        for(int row=0;row<3;row++)for(int i=0;i<14;i++){
            float x=-8.1f+i*.55f,z=3.18f+row*.57f;
            m.Ink("88A16E",Foliage);m.Blob(At(x,.12f,z),At(.27f,.16f,.22f),i);
            for(int n=0;n<3;n++){var tip=At(x+(n-1)*.067f,.34f+.045f*(i%4),z);
                beam(tip-Vector3.up*.22f,tip,.015f,"7B916C",Foliage);m.Ink(n%2==0?"9A85AB":"B8A4C2",Fruit);m.Blob(tip,At(.039f,.096f,.04f),i+n,6);}
        }
        for(int i=0;i<7;i++){float x=-7.7f+i*2.7f;
            block(x,.57f,5.48f,.17f,1.1f,.17f,"96805F",Wood);
            if(i<6){beam(At(x,.4f,5.48f),At(x+2.7f,.4f,5.48f),.073f,"AF956C",Wood);beam(At(x,.85f,5.48f),At(x+2.7f,.85f,5.48f),.065f,"BFAC85",Wood);}
        }
        for(int j=0;j<4;j++){float z=-3.5f+j*.88f;
            foreach(float x in new[]{6.37f,8.19f}){beam(At(x,0,z),At(x,1.47f,z),.075f,"9D9574",Wood);
                beam(At(x,1.47f,z),At(7.28f,2.14f,z),.076f,"A89F7B",Wood);}
            block(7.25f,.40f,z,1.65f,.14f,.59f,"B49975",Wood);
            for(int n=0;n<5;n++){m.Ink("B57961",Roof);m.Drum(6.59f+n*.32f,.48f,z,.105f,.14f,.19f);
                m.Ink("829C63",Foliage);m.Blob(At(6.59f+n*.32f,.75f,z),At(.14f,.17f,.15f),n,7);}
        }
        beam(At(7.28f,2.14f,-3.74f),At(7.28f,2.14f,-.57f),.080f,"B5A583",Wood);
    }
    public static void Wheel(HarvestMesh m)
    {
        for(int n=0;n<16;n++){
            float a=n*Mathf.PI/8,b=(n+1)*Mathf.PI/8;
            Vector3 p=At(Mathf.Cos(a),Mathf.Sin(a),0),q=At(Mathf.Cos(b),Mathf.Sin(b),0);
            foreach(float z in new[]{-.17f,.17f}){
                m.Ink("887152",Wood);m.Beam(p*1.14f+Vector3.forward*z,q*1.14f+Vector3.forward*z,.091f,.095f);
                if(n%2==0)m.Beam(Vector3.forward*z,p*1.11f+Vector3.forward*z,.074f,.074f);}
            m.Ink("B1966C",Wood);m.Beam(p*1.22f+Vector3.back*.24f,p*1.22f+Vector3.forward*.24f,.15f,.17f);
        }
        m.Ink("A59B7E");m.Tube(Vector3.back*.35f,Vector3.forward*.36f,.19f,.19f,12);
    }
}
