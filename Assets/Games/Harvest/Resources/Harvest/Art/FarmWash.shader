Shader "Harvest/Soft Contact" {
 Properties {_Opacity("Opacity",Range(0,1))=1}
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Pass {Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 half _Opacity;
 struct app{float4 vertex:POSITION;half4 color:COLOR;};struct px{float4 pos:SV_POSITION;half4 color:COLOR;};
 px vert(app v){px o;o.pos=UnityObjectToClipPos(v.vertex);o.color=v.color;return o;}
 half4 frag(px i):SV_Target{half4 c=i.color;
  #ifndef UNITY_COLORSPACE_GAMMA
   c.rgb=GammaToLinearSpace(c.rgb);
  #endif
  c.a*=_Opacity;return c;}
 ENDCG
 }}}
