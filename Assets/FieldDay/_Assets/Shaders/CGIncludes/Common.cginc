#ifndef FD_COMMON_INCLUDED
#define FD_COMMON_INCLUDED

#include "UnityCG.cginc"
#include "./DXCompat.cginc"

/// Keywords

/// Configuration Defines

// FD_PREMULTIPLY_ALPHA     Premultiplies color by alpha
// PIXELSNAP_ON             (Unity) Applies pixelsnap
// FD_SAMPLE_R              Single-channel samples will read the Red channel
// FD_SAMPLE_G              Single-channel samples will read the Green channel
// FD_SAMPLE_B              Single-channel samples will read the Blue channel
// FD_SAMPLE_A              Single-channel samples will read the Alpha channel

/// Instancing

#define     AttributesInstancing()    UNITY_VERTEX_INPUT_INSTANCE_ID
#define     VaryingsInstancing()      UNITY_VERTEX_INPUT_INSTANCE_ID
#define     InstancingInitialize(input)     UNITY_SETUP_INSTANCE_ID(input)
#define     InstancingTransfer(input, output)   UNITY_TRANSFER_INSTANCE_ID(input, output)

/// Stereo

#define     VaryingsStereo()                UNITY_VERTEX_OUTPUT_STEREO
#define     StereoInitialize(output)        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output)

/// Quantization

#define QUANTIZE_PRECISION_8            half(0xff)
#define INV_QUANTIZE_PRECISION_8        half(1.0 / QUANTIZE_PRECISION_8)

inline float Quantize8(float value)
{
    return round(value * QUANTIZE_PRECISION_8) * INV_QUANTIZE_PRECISION_8;
}

#if FD_SUPPORTS_HALF

inline half Quantize8(half value)
{
    return round(value * QUANTIZE_PRECISION_8) * INV_QUANTIZE_PRECISION_8;
}

#endif // FD_SUPPORTS_HALF

/// Math

inline float2x2 MatrixCreateRotation2d(float radians)
{
    float s, c;
    sincos(radians, s, c);
    return float2x2(
        c, -s, s, c
    );
}

inline float2 Rotate2d(float2 base, float radians)
{
    return mul(MatrixCreateRotation2d(radians), base);
}

/// Color Space

/// Fragment Operations

#if FD_PREMULTIPLY_ALPHA
    #define PremultiplyAlpha(color) ((color).rgb *= (color).a)
#else
    #define PremultiplyAlpha(color)
#endif // FD_PREMULTIPLY_ALPHA

#if PIXELSNAP_ON
    #define PixelSnapApply(position) (position) = UnityPixelSnap((position))
#else
    #define PixelSnapApply(position)
#endif // PIXELSNAP_ON

#define ColorMakeOpaque(color)  (color).a = 1

/// Texture Coordinates

#define TexCoordOffsetScaleByTexture(uv, texture)   TRANSFORM_TEX((uv), (texture))
#define TexCoordRotate2d(uv, radians)               Rotate2d((uv), (radians))

/// Samplers

#define SampleR(texture, uv)  ((tex2D((texture), (uv))).r)
#define SampleG(texture, uv)  ((tex2D((texture), (uv))).g)
#define SampleB(texture, uv)  ((tex2D((texture), (uv))).b)
#define SampleA(texture, uv)  ((tex2D((texture), (uv))).a)

#if FD_SAMPLE_R
    #define SampleSingle(texture, uv) SampleR(texture, uv)
#elif FD_SAMPLE_G
    #define SampleSingle(texture, uv) SampleG(texture, uv)
#elif FD_SAMPLE_B
    #define SampleSingle(texture, uv) SampleB(texture, uv)
#elif FD_SAMPLE_A
    #define SampleSingle(texture, uv) SampleA(texture, uv)
#else
    #define SampleSingle(texture, uv) SampleR(texture, uv)
#endif

#define SampleTexture(texture, uv)              (tex2D((texture), (uv)))

inline float4 SamplePalette(sampler2D palette, float normalizedIndex)
{
    return tex2D(palette, float2(normalizedIndex, 0.5));
}

inline float4 SamplePaletteRegion(sampler2D palette, float normalizedIndex, float2 regionStart, float regionWidth)
{
    return tex2D(palette, float2(regionStart.x + normalizedIndex * regionWidth, regionStart.y));
}

/*
inline float4 SamplePaletteArray(sampler2DArray palette, float normalizedIndex, float depth)
{
    return tex2DArray(palette, float3(normalizedIndex, 0.5, depth));
}
*/

#endif // FD_COMMON_INCLUDED