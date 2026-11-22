using UnityEngine;
using static HarvestMesh;
using static HarvestMesh.Finish;

internal sealed class HarvestCrops
{
    readonly HarvestMesh m;
    int stage;
    float scale;
    const string LeafA="578551",LeafB="78A15A",LeafC="94B873";
    public HarvestCrops(HarvestMesh shape){m=shape;}
    public static void Bed(HarvestMesh m)
    {
        m.Ink("9A704D",Loam);m.Block(0,.065f,0,1.63f,.12f,1.49f);
        for(int j=0;j<3;j++)
        {
            float z=(j-1)*.44f;m.Ink(j==1?"B68D65":"AA815C",Loam);
            m.Blob(At(0,.113f,z),At(.78f,.072f,.182f),15+j,13);
        }
        m.Ink("9C7954",Wood);
        foreach(float x in new[]{-.825f,.825f})m.Block(x,.105f,0,.054f,.12f,1.53f);
        foreach(float z in new[]{-.76f,.76f})m.Block(0,.105f,z,1.70f,.12f,.046f);
        m.Ink("D4B788",Wood);
        foreach(float x in new[]{-.825f,.825f})
        foreach(float z in new[]{-.76f,.76f})m.Drum(x,.02f,z,.052f,.049f,.17f,6);
        m.Ink("9F7D54",Wood);m.Block(-.66f,.262f,.634f,.040f,.285f,.044f);
        m.Ink("E5CEA4",Wood);m.Box(At(-.66f,.389f,.632f),At(.223f,.136f,.047f),Quaternion.Euler(0,-9,1));
        m.Ink("8D9A74");m.Block(-.66f,.384f,.605f,.112f,.018f,.010f);
    }
    public void Build(int crop,int age)
    {
        stage=age;scale=age==0?.34f:age==1?.63f:age==2?.86f:1;
        bool tall=crop==0||crop==3||crop==5;
        int nx=tall?3:2,nz=crop==4?1:2;
        for(int z=0;z<nz;z++)for(int x=0;x<nx;x++)
        {
            float xx=(x-(nx-1)*.5f)*(tall?.46f:.66f),zz=(z-(nz-1)*.5f)*(tall?.48f:.58f);
            var p=At(xx,.163f,zz);int index=z*nx+x;
            if(stage==0){Rosette(p,.225f,.071f,3,index,LeafB,LeafC);continue;}
            switch(crop)
            {
                case 0: Grain(p,index);break;
                case 1: Turnip(p,index);break;
                case 2: Tomato(p,index);break;
                case 3: Beans(p,index);break;
                case 4: Pumpkin(p,index);break;
                case 5: Corn(p,index);break;
                case 6: Cabbage(p,index);break;
                case 7: Carrot(p,index);break;
            }
        }
    }
    void Rosette(Vector3 p,float length,float width,int number,int seed,string a,string b,float lift=.72f)
    {
        for(int i=0;i<number;i++)
        {
            m.Ink(i%2==0?a:b,Fruit);
            m.Leaf(p,length*(.9f+.1f*Mathf.Sin(i+seed)),width,seed*48+i*360f/number,lift);
        }
    }
    void Stem(Vector3 a,Vector3 b,float r,string c)
    {m.Ink(c,Fruit);m.Tube(a,b,r,r*.80f,5);}
    void FruitBlob(Vector3 p,Vector3 bounds,string color,int n,int sides=8)
    {m.Ink(color,Fruit);m.Blob(p,bounds,n,sides);}
    void Flower(Vector3 p,float radius,string color,int seed)
    {
        m.Ink(color,Fruit);
        for(int i=0;i<5;i++){float a=i*1.257f+seed;var d=At(Mathf.Cos(a),0,Mathf.Sin(a));
            var side=Vector3.Cross(Vector3.up,d);m.Quad(p,p+(d*.46f+side*.44f)*radius,p+d*radius+Vector3.up*.016f,p+(d*.46f-side*.44f)*radius,Vector3.up);}
        m.Ink("ECC270",Fruit);m.Drum(p.x,p.y+.009f,p.z,.02f,.018f,.011f,5);
    }
    void Grain(Vector3 p,int k)
    {
        string stem=stage==3?"D7AD59":LeafB,grain=stage==3?"ECC872":"A7B776";
        for(int i=0;i<3;i++)
        {
            float a=i*2.4f+k;Vector3 root=p+At(Mathf.Cos(a)*.071f,0,Mathf.Sin(a)*.068f);
            Vector3 head=root+At(.045f*Mathf.Sin(k+i),(.51f+.092f*Mathf.Sin(a))*scale,.021f);
            Stem(root,head,.018f*scale,stem);m.Ink(stem,Fruit);m.Leaf(root+Vector3.up*.13f,.33f*scale,.027f*scale,k*43+i*78,1.0f);
            if(stage<2)continue;
            for(int g=0;g<4;g++)
            {
                var mid=head+At(.015f*g,.038f*g,0)*scale;
                foreach(float s in new[]{-1f,1f})
                {
                    var tip=mid+At(s*.045f,.051f+s*.005f,0)*scale;
                    m.Ink(grain,Fruit);m.Tube(mid,tip,.027f*scale,.007f*scale,5);
                    Stem(tip,tip+At(s*.026f,.063f,.003f)*scale,.006f,stem);
                }
            }
        }
    }
    void Turnip(Vector3 p,int k)
    {
        if(stage>=2)
        {
            float s=stage==3?1:.61f;
            FruitBlob(p+Vector3.up*.061f,At(.145f,.129f,.141f)*s,"F6E6C8",k);
            FruitBlob(p+Vector3.up*.129f*s,At(.118f,.054f,.108f)*s,k%2==0?"D9B4BC":"E2CFBC",k+5);
        }
        Rosette(p+Vector3.up*.108f*scale,.30f*scale,.079f*scale,7,k,LeafA,LeafB,1.12f);
    }
    void Tomato(Vector3 p,int k)
    {
        var top=p+At(.025f,.69f*scale,0);Stem(p,top,.028f,LeafA);
        if(stage>=2){m.Ink("B59B71",Wood);m.Tube(p+At(.073f,0,.060f),p+At(.083f,.73f,.055f),.021f,.017f,6);}
        for(int n=0;n<4;n++)
        {
            float a=n*2.40f+k;var root=Vector3.Lerp(p,top,.28f+.14f*n);
            var end=root+At(Mathf.Cos(a)*.17f,.060f,Mathf.Sin(a)*.17f)*scale;
            Stem(root,end,.016f,LeafA);Rosette(end,.16f*scale,.045f,3,k*7+n,LeafA,LeafB,.53f);
            if(stage>1&&n<3)
            {
                var fruit=end-Vector3.up*.075f*scale;
                FruitBlob(fruit,Vector3.one*(stage==3?.092f:.055f),stage==3?(n%2==0?"D96548":"DC7950"):"91AB70",k+n);
                m.Ink(LeafA,Fruit);m.Star(fruit+Vector3.up*.069f*scale,.058f*scale,5);
            }
            else if(stage==2)Flower(end,.047f,"F2CF7A",k);
        }
    }
    void Beans(Vector3 p,int k)
    {
        var top=p+At(.063f,.594f*scale,0);Stem(p,top,.023f,LeafA);
        for(int i=0;i<4;i++)
        {
            var leaf=Vector3.Lerp(p,top,.30f+i*.17f);float a=i*2.4f+k;
            var end=leaf+At(Mathf.Sin(a)*.123f,.034f,Mathf.Cos(a)*.123f)*scale;
            Stem(leaf,end,.013f,LeafB);Rosette(end,.143f*scale,.063f*scale,3,i+k,LeafB,LeafA,.76f);
            if(stage==3 && i<3)
            {
                m.Ink("A9BE6B",Fruit);var bend=end+At(.026f,-.09f,.008f);
                m.Tube(end,bend,.027f,.037f,6);m.Tube(bend,bend+At(-.027f,-.10f,.01f),.037f,.009f,6);
            }
            else if(stage==2)Flower(end,.043f,"EBDBC2",i);
        }
    }
    void Pumpkin(Vector3 p,int k)
    {
        m.Ink(LeafA,Fruit);m.Tube(p+At(-.25f,.01f,.11f),p+At(.24f,.048f,-.07f),.021f,.015f,5);
        for(int i=0;i<4;i++)
        {
            var root=p+At(Mathf.Cos(k+i)*.16f,.038f,Mathf.Sin(i*2.4f)*.16f);
            m.Ink(i%2==0?"66935C":"78A466",Fruit);m.Leaf(root,.31f*scale,.153f*scale,i*93+k*31,.40f);
        }
        if(stage<2)return;
        float r=stage==3?.211f:.129f;var c=p+At(.01f,r*.68f,-.038f);
        for(int l=0;l<8;l++)
        {
            float a=l*Mathf.PI/4;
            FruitBlob(c+At(Mathf.Cos(a)*r*.48f,0,Mathf.Sin(a)*r*.48f),
                At(r*.70f,r*.88f,r*.70f),stage==3?(l%2==0?"EBA35A":"DC914B"):"8EAC74",k+l,7);
        }
        m.Ink("7B8250",Wood);m.Tube(c+Vector3.up*r*.73f,c+At(.046f,r*1.18f,0),r*.18f,r*.12f,6);
    }
    void Corn(Vector3 p,int k)
    {
        var top=p+At(.025f,.84f*scale,0);Stem(p,top,.031f*scale,"739359");
        for(int l=0;l<5;l++)
        {
            var q=Vector3.Lerp(p,top,.21f+l*.14f);
            m.Ink(l%2==0?"79A15A":"92AF66",Fruit);m.Leaf(q,(.31f-l*.015f)*scale,.053f*scale,k*65+l*145,.72f);
        }
        if(stage>=2)
        {
            var ear=p+At(.050f,.45f*scale,.025f);
            FruitBlob(ear,At(.076f,.151f,.065f)*scale,stage==3?"EBC160":"ABC277",k);
            m.Ink("7FA462",Fruit);m.Leaf(ear-Vector3.up*.107f,.181f,.064f,16+k,1.1f);
            m.Ink("C7AC74",Fruit);for(int i=0;i<4;i++){float a=i*1.57f;
                m.Tube(top,top+At(Mathf.Sin(a)*.104f,.118f,Mathf.Cos(a)*.104f)*scale,.012f,.004f,5);}
        }
    }
    void Cabbage(Vector3 p,int k)
    {
        Rosette(p+Vector3.up*.021f,.285f*scale,.131f*scale,7,k,"64926D","82AC80",.53f);
        if(stage==1)return;
        float r=stage==3?.17f:.11f;var c=p+Vector3.up*.10f;
        FruitBlob(c,At(r,r*.88f,r),"A8C798",k,9);
        for(int j=0;j<5;j++){float a=j*1.257f+k;
            FruitBlob(c+At(Mathf.Cos(a)*r*.58f,-.003f,Mathf.Sin(a)*r*.58f),At(r*.57f,r*.73f,r*.57f),"92B88B",k+j,7);}
    }
    void Carrot(Vector3 p,int k)
    {
        float r=stage==3?.116f:.072f;
        if(stage>=2)
        {
            FruitBlob(p+Vector3.up*.038f,At(r,.106f*scale,r),"E49A5B",k,8);
            m.Ink("F0AC6D",Fruit);m.Drum(p.x,p.y+.085f*scale,p.z,r*.66f,r*.55f,.024f,7);
        }
        for(int i=0;i<5;i++)
        {
            float a=i*1.256f+k;var basePoint=p+Vector3.up*.08f*scale;
            var stem=basePoint+At(Mathf.Cos(a)*.129f,.28f,Mathf.Sin(a)*.129f)*scale;Stem(basePoint,stem,.010f,LeafA);
            for(int j=1;j<4;j++){m.Ink(j%2==0?LeafB:LeafA,Fruit);
                m.Leaf(Vector3.Lerp(basePoint,stem,j*.24f),(.105f-j*.01f)*scale,.022f*scale,a*Mathf.Rad2Deg+55*(j%2==0?1:-1),.90f);}
        }
    }
}
