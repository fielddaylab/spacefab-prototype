Shader "FieldDay/UI/Configurable"
{
    Properties
    {
		[Header(Textures)] [Space]
        [PerRendererData] [NoScaleOffset] _MainTex ("Sprite Texture", 2D) = "white" {}

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
            #pragma fragment DefaultUIFrag
            #pragma target 2.0

            #include "../CGIncludes/UI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local_fragment _ UNITY_UI_ALPHACLIP
			#pragma shader_feature_local_fragment _ FD_PREMULTIPLY_ALPHA
			#pragma multi_compile_local _ FD_COLORMOD_LERP
			#pragma multi_compile_local _ FD_COLORMOD_ADDITIVE
        ENDCG
        }
    }
}
