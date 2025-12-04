Shader "SpaceFab/Ink Dissolve (Screen)" {
	Properties {
		
		[PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
		_InkTex ("Ink Texture", 2D) = "white" {}
		_InkColor ("Ink Color", Color) = (1,1,1,1)

		_MaskTex0 ("Mask 1", 2D) = "white" {}
		_MaskRot0("Mask 1 Rotation", Range(0, 6.283)) = 0
		_MaskTex1 ("Mask 2", 2D) = "white" {}
		_MaskRot1("Mask 2 Rotation", Range(0, 6.283)) = 0
		
		_Cutoff ("Cutoff", Range(0, 1)) = 0.5
	}

	SubShader {
    
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always
		Blend One OneMinusSrcAlpha

		Pass {
			Name "Default"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				#pragma multi_compile_local _ INVERT_CUTOFF

				struct Attributes {
					float4 vertex : POSITION;
					fixed2 texcoord : TEXCOORD0;
					fixed4 color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct Varyings {
					fixed4 vertex : SV_POSITION;
					fixed2 texcoord : TEXCOORD0;
					fixed2 mask0uv : TEXCOORD1;
					fixed2 mask1uv : TEXCOORD2;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;

				sampler2D _InkTex;
				float4 _InkTex_ST;

				sampler2D _MaskTex0;
				float4 _MaskTex0_ST;

				sampler2D _MaskTex1;
				float4 _MaskTex1_ST;

				float _MaskRot0;
				float _MaskRot1;

				fixed4 _InkColor;
				float _Cutoff;

				fixed2 RotateVector(fixed2 vec, float radians) {
					float sin, cos;
					float x = vec.x;
					float y = vec.y;
					sincos(radians, sin, cos);
					return fixed2(
						(cos * x) - (sin * y),
						(sin * x) + (cos * y)
					);
				}

				Varyings vert(Attributes v)
				{
					Varyings o;
					UNITY_SETUP_INSTANCE_ID(v);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _InkTex);
					o.mask0uv = TRANSFORM_TEX(RotateVector(v.texcoord, _MaskRot0), _MaskTex0);
					o.mask1uv = TRANSFORM_TEX(RotateVector(v.texcoord, _MaskRot1), _MaskTex1);
					return o;
				}

				fixed4 frag(Varyings i): SV_Target
				{
					float mainA = tex2D(_MaskTex0, i.mask0uv).r;
					float secondaryA = tex2D(_MaskTex1, i.mask1uv).r;

					half4 col = tex2D(_InkTex, i.texcoord) * _InkColor;

					#if INVERT_CUTOFF
					col.a *= 1 - step(max(mainA, secondaryA) - 0.00001, 1 - _Cutoff);
					#else
					col.a *= step(max(mainA, secondaryA) - 0.00001, _Cutoff);
					#endif // INVERT_CUTOFF

					col.rgb *= col.a;
					return col;
				}
			ENDCG
		} 
	}
}