// Portions from Unity built-in shader source, under MIT license.

#ifndef FD_SPRITES_INCLUDED
#define FD_SPRITES_INCLUDED

#include "./Common.cginc"
#include "./Fog.cginc"
#include "./ColorMod.cginc"

/// Configuration Defines

// UNITY_INSTANCING_ENABLED     (Unity) Enables instanced rendering
// ETC1_EXTERNAL_ALPHA          (Unity) Reads sprite alpha from separate alpha texture
// FD_SPRITE_ALPHACLIP          Enables alpha clipping using the _AlphaCutoff uniform

/// Types

struct Attributes_Sprite
{
    float4 vertex   : POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    AttributesInstancing()
};

struct Varyings_Sprite
{
    float4 vertex   : SV_POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    VaryingsFog(1)
    VaryingsStereo()
    VaryingsInstancing()
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

#endif // UNITY_INSTANCING_ENABLED

// make sure to keep this structure aligned with UnitySprites

CBUFFER_START(UnityPerDrawSprite)
#ifndef UNITY_INSTANCING_ENABLED
    fixed4 _RendererColor;
    fixed2 _Flip;
#endif
    float _EnableExternalAlpha;
CBUFFER_END

/// Uniforms

fixed4 _Color;

#if FD_SPRITE_ALPHACLIP
half _AlphaCutoff;
#endif // FD_SPRITE_ALPHACLIP

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

#ifdef FD_SPRITE_ALPHACLIP
    #define SpriteAlphaClip(color) clip((color).a - _AlphaCutoff)
#else
    #define SpriteAlphaClip(color)
#endif // UNITY_UI_ALPHACLIP

/// Programs

Varyings_Sprite DefaultSpriteVert(Attributes_Sprite v)
{
    Varyings_Sprite output;
    InstancingInitialize(v);
    InstancingTransfer(v, output);
    StereoInitialize(output);

    output.vertex = UnityObjectToClipPos(UnityFlipSprite(v.vertex, _Flip));
    output.texcoord = v.texcoord;
    output.color = v.color * _Color * _RendererColor;
    
    FogTransfer(output, output.vertex);

    PixelSnapApply(output.vertex);

    return output;
}

fixed4 DefaultSpriteFrag(Varyings_Sprite v) : SV_Target
{
    InstancingInitialize(v);
    fixed4 color = SampleSpriteTexture(v.texcoord) * v.color;
    SpriteAlphaClip(color);
    LayerApplyLerpColor(color);
    LayerApplyAdditiveColor(color);
    FogApply(color, v);
    PremultiplyAlpha(color);
    return color;
}

#endif // FD_SPRITES_INCLUDED