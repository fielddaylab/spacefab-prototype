Shader "FieldDay/UI/Palette Texture"
{
    Properties
    {
		[Header(Textures)] [Space]
        [PerRendererData] [NoScaleOffset] _MainTex ("Intensity Texture", 2D) = "white" {}
        [KeywordEnum(R,G,B,A)] FD_SAMPLE ("Sample Channel", Int) = 0
		[NoScaleOffset] _PaletteTex ("Palette Texture", 2D) = "white" {}

		[Header(Intensity Texture)] [Space]
		[Toggle(FD_PALETTE_ATTENUATE_ALPHA)] _AttenuateAlpha("Attenuate Alpha Channel", Int) = 2
		_PaletteColorThreshold("Color Threshold", Range(0.001, 1)) = 1
		_PaletteAlphaThreshold("Alpha Threshold", Range(0.001, 1)) = 1

		[Header(Colors)] [Space]
        _Color ("Tint", Color) = (1,1,1,1)

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

		[Header(Blending)] [Space] 
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend Mode", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DestBlend("Destination Blend Mode", Int) = 10
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Int) = 0
		[Toggle(FD_PREMULTIPLY_ALPHA)] _PremultiplyAlpha("Premultiply Alpha", Float) = 1

		[Header(Culling)] [Space]
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Int) = 0

		[Header(ColorMod)] [Space]
		[Toggle(FD_COLORMOD_LERP)] _ApplyLerpColor("Apply Lerp Color", Float) = 0
		[Toggle(FD_COLORMOD_ADDITIVE)] _ApplyAdditiveColor("Apply Additive Color", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull [_CullMode]
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] [_DestBlend]
        BlendOp [_BlendOp]
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex DefaultUIVert
            #pragma fragment CustomFrag
            #pragma target 2.0

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local_fragment _ UNITY_UI_ALPHACLIP
            #pragma shader_feature_local_fragment FD_SAMPLE_R FD_SAMPLE_G FD_SAMPLE_B FD_SAMPLE_A
			#pragma shader_feature_local_fragment _ FD_PREMULTIPLY_ALPHA
			#pragma shader_feature_local_fragment _ FD_PALETTE_ATTENUATE_ALPHA
			#pragma multi_compile_local _ FD_COLORMOD_LERP
			#pragma multi_compile_local _ FD_COLORMOD_ADDITIVE

            #include "../CGIncludes/UI.cginc"
			#include "../CGIncludes/Palette.cginc"

            fixed4 CustomFrag(Varyings_UI IN) : SV_Target
            {
				half4 color = LayerPaletteTexture(_MainTex, IN.texcoord, _PaletteTex) * IN.color;

                UIRectClip(IN.mask, color);
                UIAlphaClip(color);

				UIApplyLerpColor(color, IN);
			    UIApplyAdditiveColor(color, IN);
    
                PremultiplyAlpha(color);
                return color;
            }
        ENDCG
        }
    }
}
