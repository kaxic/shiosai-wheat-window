Shader "Harvest/Farmhouse Gouache" {
 Properties {
  [NoScaleOffset] _Grain("Field surface detail",2D)="gray"{}
  [NoScaleOffset] _Grass("Meadow clover",2D)="gray"{}
  [NoScaleOffset] _Soil("Garden loam",2D)="gray"{}
  [NoScaleOffset] _Wood("Chestnut",2D)="gray"{}
  [NoScaleOffset] _Roof("Clay roof",2D)="gray"{}
  [NoScaleOffset] _Plaster("Limewash",2D)="gray"{}
  [NoScaleOffset] _Stone("Courtyard stone",2D)="gray"{}
  _Damp("Watered soil",Range(0,1))=0
  _Sky("Season, weather and dusk",Vector)=(0,0,0,0)
 }
 SubShader {Tags {"RenderType"="Opaque"}
 Pass {Tags {"LightMode"="ForwardBase"}
  CGPROGRAM
  #pragma target 3.0
  #pragma vertex vert
  #pragma fragment frag
  #pragma multi_compile_fwdbase
  #include "UnityCG.cginc"
  #include "AutoLight.cginc"
  sampler2D _Grain; sampler2D _Grass,_Soil,_Wood,_Roof,_Plaster,_Stone;half _Damp;float4 _Sky;
  struct app {float4 vertex:POSITION;float3 normal:NORMAL;float4 uv:TEXCOORD0;half4 color:COLOR;};
  struct px {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float3 world:TEXCOORD1;half3 norm:TEXCOORD2;half3 color:COLOR;SHADOW_COORDS(3)};
  px vert(app v){px o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.uv=v.uv;o.norm=UnityObjectToWorldNormal(v.normal);o.color=v.color.rgb;TRANSFER_SHADOW(o);return o;}
  half3 ink(sampler2D page,float2 uv){half3 c=tex2D(page,uv).rgb;
   #ifndef UNITY_COLORSPACE_GAMMA
    c=LinearToGammaSpace(c);
   #endif
   return c;}
  half4 frag(px i):SV_Target{
   half3 c=i.color;float t=i.uv.z;float2 uv=i.uv.xy;half3 n=normalize(i.norm);
   half3 paper=0;
   if(t>.5&&t<1.5){
    half3 g=ink(_Grass,(uv+float2(.23*sin(uv.y*.63),.25*sin(uv.x*.48)))/5.8);
    paper=(g-half3(.490,.496,.127))*.32;
    if(_Sky.x>2.5)c=lerp(c,half3(.78,.82,.74),.83);
    else if(_Sky.x>1.5)c=lerp(c,half3(.76,.68,.41),.58);
    else if(_Sky.x>.5)c*=half3(.95,1.02,.86);
   }
   else if(t<2.5&&t>1.5){half3 s=ink(_Soil,uv/1.90);paper=(s-half3(.548,.290,.143))*.65;
      c=lerp(c,c*half3(.60,.66,.75),_Damp);
      paper+=_Damp*.06*smoothstep(.37,.59,s.r)*half3(.58,.83,1);
   }
   else if(t<3.5&&t>2.5)paper=(ink(_Wood,uv/1.62)-half3(.689,.345,.137))*.50;
   else if(t<4.5&&t>3.5)paper=(ink(_Roof,uv/1.81)-half3(.787,.454,.231))*.54;
   else if(t<5.5&&t>4.5)paper=(ink(_Plaster,uv/2.54)-half3(.952,.818,.598))*.47;
   else if(t<6.5&&t>5.5)paper=(ink(_Stone,uv/3.33)-half3(.896,.702,.474))*.61;
   else if(t<7.5&&t>6.5)paper=(ink(_Plaster,uv/.81)-half3(.952,.818,.598))*.12;
   else if(t<8.5&&t>7.5){
    paper=(ink(_Grass,uv/1.14)-half3(.490,.496,.127))*.26;
    if(_Sky.x>2.5)c=lerp(c,half3(.63,.72,.62),.45);
    else if(_Sky.x>1.5)c=lerp(c,half3(.84,.57,.29),.64);
   }
   else if(t>9.5){float r=sin(uv.y*5+sin(uv.x*2+_Time.y*.3)+_Time.y*.67);paper=half3(.11,.12,.10)*smoothstep(.88,1,r);}
   half3 relief=tex2D(_Grain,uv*.94).rgb-half3(.54,.50,.55);
   if(t>1.5&&t<2.5)paper+=relief.r*.16+relief.g*.14;
   else if(t>.5&&t<1.5)paper+=relief.g*half3(.058,.095,.045);
   else if(t>4.5&&t<6.5)paper+=relief.b*.12;
   else if(t>8.5&&t<9.5)paper+=relief.g*.06+saturate(dot(n,normalize(half3(-.45,.81,-.43))))*.022;
   float sun=saturate(dot(n,normalize(half3(-.48,.83,-.44))));
   float sky=n.y*.5+.5;
   float shadow=SHADOW_ATTENUATION(i);
   half3 shade=lerp(half3(.52,.59,.62),half3(.80,.83,.81),sky)+sun*lerp(.51,1,shadow)*half3(.31,.255,.19);
   shade*=lerp(half3(.84,.88,.91),half3(1,1,1),shadow);
   half3 lit=(c+paper)*shade;
   lit=lerp(lit,saturate((c+paper)*1.06),saturate(i.uv.w));
   float cloud=_Sky.y*(.63+.37*cos(i.world.x*.32+i.world.z*.22+_Time.y*.11));
   lit*=lerp(half3(1,1,1),half3(.85,.91,.98),cloud*.63);
   lit=lerp(lit,lit*half3(1.05,.83,.77)+half3(.008,0,.012),_Sky.z);
   #ifndef UNITY_COLORSPACE_GAMMA
    lit=GammaToLinearSpace(saturate(lit));
   #endif
   return half4(saturate(lit),1);
  }
  ENDCG
 }
 UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
 }
 Fallback "Diffuse"
}
