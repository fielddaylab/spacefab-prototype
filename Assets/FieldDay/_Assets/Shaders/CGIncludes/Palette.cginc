#ifndef FD_PALETTIZED_INCLUDED
#define FD_PALETTIZED_INCLUDED

#include "./Common.cginc"

/// Configuration Defines

// FD_PALETTE_ATTENUATE_ALPHA   Attenuates alpha below a certain threshold

/// Uniforms

sampler2D _PaletteTex;
// sampler2DArray _PaletteTexArray;

half _PaletteColorThreshold;
half _PaletteAlphaThreshold;

/// Helpers

inline void LayerPaletteTextureComponents(sampler2D intensityTexture, float2 intensityUV, out float paletteValue, out float alphaComponent)
{
    float intensity = SampleSingle(intensityTexture, intensityUV);
    paletteValue = saturate(intensity / _PaletteColorThreshold);
    
#if FD_PALETTE_ATTENUATE_ALPHA
    alphaComponent = saturate(intensity / _PaletteAlphaThreshold);
#else
    alphaComponent = 1;
#endif // FD_PALETTE_ATTENUATE_ALPHA
}

inline void LayerPaletteARrayTextureComponents(sampler2D intensityTexture, float2 intensityUV, out float paletteValue, out float alphaComponent)
{
    float intensity = SampleSingle(intensityTexture, intensityUV);
    paletteValue = saturate(intensity / _PaletteColorThreshold);
    
#if FD_PALETTE_ATTENUATE_ALPHA
    alphaComponent = saturate(intensity / _PaletteAlphaThreshold);
#else
    alphaComponent = 1;
#endif // FD_PALETTE_ATTENUATE_ALPHA
}

inline float4 LayerPaletteTexture(sampler2D intensityTexture, float2 uv, sampler2D paletteTexture)
{
    float paletteValue, alphaComponent;
    LayerPaletteTextureComponents(intensityTexture, uv, paletteValue, alphaComponent);
    float4 paletteEntry = SamplePalette(paletteTexture, paletteValue);
    return float4(paletteEntry.rgb, paletteEntry.a * alphaComponent);
}

inline float4 LayerPaletteTextureRegion(sampler2D intensityTexture, float2 uv, sampler2D paletteTexture, float2 paletteTextureStart, float paletteTextureWidth)
{
    float paletteValue, alphaComponent;
    LayerPaletteTextureComponents(intensityTexture, uv, paletteValue, alphaComponent);
    float4 paletteEntry = SamplePaletteRegion(paletteTexture, paletteValue, paletteTextureStart, paletteTextureWidth);
    return float4(paletteEntry.rgb, paletteEntry.a * alphaComponent);
}

#endif // FD_PALETTIZED_INCLUDED