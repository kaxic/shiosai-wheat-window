Shader "Harvest/Garden Showers" {
 SubShader {Tags {"Queue"="Transparent+5" "RenderType"="Transparent"} Pass {Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;float2 timing:TEXCOORD1;};struct px{float4 pos:SV_POSITION;half alpha:TEXCOORD0;};
 px vert(app v){
  px o;float age=frac(_Time.y*(.86+.48*v.timing.y)+v.timing.x);
  v.vertex.y+=6*(1-age)+v.uv.y*.17;v.vertex.x+=v.uv.x*.011+v.uv.y*.041-age*.85;
  o.pos=UnityObjectToClipPos(v.vertex);o.alpha=.31*saturate(age*8)*saturate((1-age)*14);return o;
 }
 half4 frag(px i):SV_Target{return half4(.84,.91,.94,i.alpha);}
 ENDCG
 }}}
