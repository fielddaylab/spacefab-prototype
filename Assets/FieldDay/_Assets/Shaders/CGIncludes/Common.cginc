#ifndef FD_COMMON_INCLUDED
#define FD_COMMON_INCLUDED

#include "UnityCG.cginc"

/// Quantization

#define QUANTIZE_PRECISION_8 half(0xff)
#define INV_QUANTIZE_PRECISION_8 half(1.0 / QUANTIZE_PRECISION_8)

inline float Quantize8(float value)
{
    return round(value * QUANTIZE_PRECISION_8) * INV_QUANTIZE_PRECISION_8;
}

inline half Quantize8(half value)
{
    return round(value * QUANTIZE_PRECISION_8) * INV_QUANTIZE_PRECISION_8;
}

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

#endif // FD_COMMON_INCLUDED