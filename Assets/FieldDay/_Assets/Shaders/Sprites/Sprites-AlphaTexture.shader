// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "FieldDay/Sprites/Alpha Texture"
{
    Properties
    {
        [PerRendererData] _MainTex ("Alpha Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [Toggle(FD_SAMPLE_A)] _SampleR ("Sample Alpha Channel", Float) = 1

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend Mode", Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestBlend("Destination Blend Mode", Int) = 10
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Int) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend [_SrcBlend] [_DestBlend]
        BlendOp [_BlendOp]

        Pass
        {
        CGPROGRAM
            #pragma vertex DefaultSpriteVert
            #pragma fragment SpriteFragAlpha
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile_local _ FD_SAMPLE_A

            #include "../CGIncludes/Sprites.cginc"

            fixed4 SpriteFragAlpha(Varyings_Sprite v) : SV_Target
            {
                float alpha = SampleSingle(_MainTex, v.texcoord);
                half4 color = v.color;
                color.a *= alpha;
    
                PremultiplyAlpha(color);
                return color;
            }
        ENDCG
        }
    }
}
