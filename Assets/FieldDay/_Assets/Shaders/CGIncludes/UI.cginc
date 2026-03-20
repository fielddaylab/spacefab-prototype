// Portions from Unity built-in shader source, under MIT license.

#ifndef FD_UI_INCLUDED
#define FD_UI_INCLUDED

#define IS_UI_SHADER    true

#include "./Common.cginc"
#include "UnityUI.cginc"
#include "./ColorMod.cginc"

/// Configuration Defines

// UNITY_UI_CLIP_RECT       (Unity) Applies rect clipping
// UNITY_UI_ALPHACLIP       (Unity) Applies basic alpha clipping
// UNITY_COLORSPACE_GAMMA   (Unity) Dictates output color space

/// Types

struct Attributes_UI
{
    float4 vertex   : POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    AttributesInstancing()
    AttributesUILerpColor(1)
    AttributesUIAdditiveColor(2)
};

struct Varyings_UI
{
    float4 vertex           : SV_POSITION;
    fixed4 color            : COLOR;
    float2 texcoord         : TEXCOORD0;
    float4 worldPosition    : TEXCOORD1;
#if UNITY_UI_CLIP_RECT
    half4  mask             : TEXCOORD2;
#endif // UNITY_UI_CLIP_RECT
    VaryingsStereo()
    VaryingsUILerpColor(3)
    VaryingsUIAdditiveColor(4)
};

/// Uniforms

// main texture
sampler2D _MainTex;

// color
fixed4 _Color;

// clipping
float4 _ClipRect;
half _UIMaskSoftnessX;
half _UIMaskSoftnessY;

// color space
int _UIVertexColorAlwaysGammaSpace;

/// Helpers

inline float2 UIComputePixelSize(float4 vertexPos)
{
    return vertexPos.w / (float2(1, 1) * abs(mul((float2x2) UNITY_MATRIX_P, _ScreenParams.xy)));
}

float4 UIComputeRectMask(float4 vertexPos, float2 pixelSize)
{
    float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
    return float4(vertexPos.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));
}

float4 UIComputeRectMask(float4 vertexPos)
{
    float2 pixelSize = UIComputePixelSize(vertexPos);
    float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
    return float4(vertexPos.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));
}

inline float UIPerformRectClip(float4 mask)
{
    half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(mask.xy)) * mask.zw);
    return m.x * m.y;
}

#if FD_SUPPORTS_HALF
inline float UIPerformRectClip(half4 mask)
{
    half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(mask.xy)) * mask.zw);
    return m.x * m.y;
}
#endif // FD_SUPPORTS_HALF

#ifdef UNITY_UI_CLIP_RECT
    #define UIRectClip(mask, color) (color).a *= UIPerformRectClip(mask)
#else
    #define UIRectClip(mask, color)
#endif // UNITY_UI_CLIP_RECT

#ifdef UNITY_UI_ALPHACLIP
    #define UIAlphaClip(color) clip((color).a - 0.001)
#else
    #define UIAlphaClip(color)
#endif // UNITY_UI_ALPHACLIP

#if !UNITY_COLORSPACE_GAMMA
    #define UICorrectColorSpace(color) if (_UIVertexColorAlwaysGammaSpace) (color).rgb = UIGammaToLinear((color).rgb)
#else
    #define UICorrectColorSpace(color)
#endif // UNITY_COLORSPACE_GAMMA

/// Programs

Varyings_UI DefaultUIVert(Attributes_UI v)
{
    Varyings_UI output;
    InstancingInitialize(v);
    StereoInitialize(output);
    
    float4 vPosition = UnityObjectToClipPos(v.vertex);
    output.worldPosition = v.vertex;
    output.vertex = vPosition;
    
    output.texcoord = v.texcoord.xy;
#if UNITY_UI_CLIP_RECT
    output.mask = UIComputeRectMask(v.vertex);
#endif // UNITY_UI_CLIP_RECT
    
    output.color = v.color * _Color;
    
    UICorrectColorSpace(output.color);
    
    UITransferLerpColor(v, output);
    UITransferAdditiveColor(v, output);
    
    return output;
}

fixed4 DefaultUIFrag(Varyings_UI f) : SV_Target
{
    f.color.a = Quantize8(f.color.a);
    half4 color = f.color * (tex2D(_MainTex, f.texcoord));
    
    UIRectClip(f.mask, color);
    UIAlphaClip(color);
    
    UIApplyLerpColor(color, f);
    UIApplyAdditiveColor(color, f);
    
    PremultiplyAlpha(color);
    return color;
}

#endif // FD_UI_INCLUDED