Shader "FieldDay/Sprites/Intensity  Texture"
{
    Properties
    {
        [PerRendererData] _MainTex ("Intensity Texture", 2D) = "white" {}
        [Toggle(FD_SAMPLE_A)] _SampleAlpha ("Sample Alpha Channel", Float) = 1
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)

        [Header(Blending)] [Space]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend Mode", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DestBlend("Destination Blend Mode", Int) = 10
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Int) = 0
		[Toggle(FD_PREMULTIPLY_ALPHA)] _PremultiplyAlpha("Premultiply Alpha", Float) = 1

		[Header(Culling and Clipping)] [Space]
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Int) = 2

		[Header(Depth)] [Space]
		[Toggle] _ZWriteMode("ZWrite", Int) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTestMode("ZTest Mode", Int) = 4

        [Header(Effects)] [Space]
        [Toggle(FD_ENABLE_FOG)] _EnableFog("Enable Fog", Int) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull [_CullMode]
        Lighting Off
        ZWrite [_ZWriteMode]
		ZTest [_ZTestMode]
        Blend [_SrcBlend] [_DestBlend]
        BlendOp [_BlendOp]

        Pass
        {
        CGPROGRAM
            #pragma vertex DefaultSpriteVert
            #pragma fragment SpriteFragAlpha
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile_local _ PIXELSNAP_ON
			#pragma multi_compile_local _ FD_PREMULTIPLY_ALPHA
            #pragma multi_compile_local _ FD_SAMPLE_A
            #pragma multi_compile_local _ FD_ENABLE_FOG

            #include "../CGIncludes/Sprites.cginc"
			#include "../CGIncludes/Layers.cginc"

            fixed4 SpriteFragAlpha(Varyings_Sprite v) : SV_Target
            {
				half4 color = LayerIntensityTexture(_MainTex, v.texcoord, v.color);
                FogApply(color, v);
				PremultiplyAlpha(color);
                return color;
            }
        ENDCG
        }
    }
}
