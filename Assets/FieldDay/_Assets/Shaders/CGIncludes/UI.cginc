// Portions from Unity built-in shader source, under MIT license.

#ifndef FD_UI_INCLUDED
#define FD_UI_INCLUDED

#include "UnityCG.cginc"
#include "./Common.cginc"
#include "UnityUI.cginc"

/// Configuration Defines

// UNITY_UI_CLIP_RECT       (Unity) Applies rect clipping
// UNITY_UI_ALPHACLIP       (Unity) Applies basic alpha clipping

/// Types

struct Attributes_UI
{
    float4 vertex   : POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    AttributesInstancing
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
    VaryingsStereo
};

/// Uniforms

// main texture
sampler2D _MainTex;
float4 _MainTex_ST;

// color/sample add
fixed4 _Color;
fixed4 _TextureSampleAdd;

// clipping
float4 _ClipRect;
half _UIMaskSoftnessX;
half _UIMaskSoftnessY;

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

inline float UIPerformRectClip(half4 mask)
{
    half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(mask.xy)) * mask.zw);
    return m.x * m.y;
}

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

/// Programs

Varyings_UI DefaultUIVert(Attributes_UI v)
{
    Varyings_UI output;
    InstancingInitialize(v);
    StereoInitialize(output);
    
    float4 vPosition = UnityObjectToClipPos(v.vertex);
    output.worldPosition = v.vertex;
    output.vertex = vPosition;
    
    output.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
#if UNITY_UI_CLIP_RECT
    output.mask = UIComputeRectMask(v.vertex);
#endif // UNITY_UI_CLIP_RECT
    
    output.color = v.color * _Color;
    return output;
}

fixed4 DefaultUIFrag(Varyings_UI f) : SV_Target
{
    f.color.a = Quantize8(f.color.a);
    half4 color = f.color * (tex2D(_MainTex, f.texcoord) + _TextureSampleAdd);
    
    UIRectClip(f.mask, color);
    UIAlphaClip(color);
    
    PremultiplyAlpha(color);
    return color;
}

#endif // FD_UI_INCLUDED