// Portions from Unity built-in shader source, under MIT license.

#ifndef FD_SPRITES_INCLUDED
#define FD_SPRITES_INCLUDED

#include "./Common.cginc"

/// Types

struct Attributes_Sprite
{
    float4 vertex   : POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings_Sprite
{
    float4 vertex   : SV_POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

/// Instancing

// make sure to keep this structure aligned with UnitySprites

#ifdef UNITY_INSTANCING_ENABLED

    UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
        // SpriteRenderer.Color while Non-Batched/Instanced.
        UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
        // this could be smaller but that's how bit each entry is regardless of type
        UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
    UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

    #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
    #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

#endif // instancing

CBUFFER_START(UnityPerDrawSprite)
#ifndef UNITY_INSTANCING_ENABLED
    fixed4 _RendererColor;
    fixed2 _Flip;
#endif
    float _EnableExternalAlpha;
CBUFFER_END

/// Uniforms

fixed4 _Color;

sampler2D _MainTex;

#if ETC1_EXTERNAL_ALPHA
sampler2D _AlphaTex;
#endif // ETC1_EXTERNAL_ALPHA

/// Helpers

inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
{
    return float4(pos.xy * flip, pos.z, 1.0);
}

inline fixed4 SampleMainNoExternalAlpha(float2 uv)
{
    return tex2D(_MainTex, uv);
}

inline fixed4 SampleMainWithExternalAlpha(float2 uv)
{
    fixed4 color = tex2D(_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
    fixed4 alpha = tex2D(_AlphaTex, uv);
    color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
#endif

    return color;
}

#if ETC1_EXTERNAL_ALPHA
    #define SampleSpriteTexture    SampleMainWithExternalAlpha
#else
    #define SampleSpriteTexture    SampleMainNoExternalAlpha
#endif // ETC1_EXTERNAL_ALPHA

/// Programs

Varyings_Sprite DefaultSpriteVert(Attributes_Sprite v)
{
    Varyings_Sprite output;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(v);

    output.vertex = UnityObjectToClipPos(UnityFlipSprite(v.vertex, _Flip));
    output.texcoord = v.texcoord;
    output.color = v.color * _Color * _RendererColor;

    PixelSnapApply(output.vertex);

    return output;
}

fixed4 DefaultSpriteFrag(Varyings_Sprite v) : SV_Target
{
    fixed4 color = SampleSpriteTexture(v.texcoord) * v.color;
    PremultiplyAlpha(color);
    return color;
}

fixed4 DefaultSpriteFrag_NonPremultiplied(Varyings_Sprite v) : SV_Target
{
    return SampleSpriteTexture(v.texcoord) * v.color;
}

#endif // FD_SPRITES_INCLUDED