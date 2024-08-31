Shader "UMA/Atlas/AtlasDiffuseShader_Subtract" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_AdditiveColor ("Additive Color", Color) = (0,0,0,0)
	// _MainTex contains the texture from the overlay
	_MainTex ("Base Texture", 2D) = "white" {}
	// _ExtraTex is the alpha mask from the overlay.
	_ExtraTex ("mask", 2D) = "white" {}
	// _BaseTex is the image from the previous layer. We use Graphics.Blit
	// to copy the result of the previous layer into this texture.
	_BaseTex ("Blendbase Texture", 2D) = "white" {}
}

SubShader 
{
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

	Pass 
	{
		Tags { "LightMode" = "Vertex" }
   		Fog { Mode Off }
		BlendOp Add, Add
		Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
		Lighting Off
		Cull Off
		ZWrite Off
		ZTest Always


		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		float4 _Color;
		float4 _AdditiveColor;
		sampler2D _MainTex;
		sampler2D _ExtraTex;
		sampler2D _BaseTex;

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
		};

		float4 _MainTex_ST;

		v2f vert (appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos (v.vertex);
			o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			return o;
		}

		float3 BlendMode_Subtract(float3 base, float3 blend)
		{
			return max(0, base - blend);
		}

		float4 frag (v2f i) : COLOR
		{
			float4 texcol = tex2D(_MainTex, i.uv); // get the color from the overlay
			float4 basecol = tex2D(_BaseTex, i.uv); // get the color from the previous pass
			texcol.rgb = BlendMode_Subtract(basecol.rgb, texcol.rgb); // subtract the overlay from the previous pass
			half4 mask = tex2D(_ExtraTex, i.uv);
			return float4(texcol.rgb, mask.a)*_Color+_AdditiveColor;
		}

		ENDCG
	}
}

//Fallback "Transparent/VertexLit"
} 