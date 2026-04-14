#ifndef FD_COLORMOD_INCLUDED
#define FD_COLORMOD_INCLUDED

#include "./Common.cginc"

/// Configuration Defines

// UNITY_INSTANCING_ENABLED (Unity) Enables instanced rendering
// FD_COLORMOD_LERP         Interpolates to the provided color
// FD_COLORMOD_ADDITIVE     Adds the provided color

/// Instancing

/// Types

#ifdef IS_UI_SHADER
    #if FD_COLORMOD_LERP
        #define AttributesUILerpColor(channel)  fixed4 lerpColor : TEXCOORD##channel;
        #define VaryingsUILerpColor(channel)    fixed4 lerpColor : TEXCOORD##channel;
        #define UITransferLerpColor(input, output)  (output).lerpColor = (input).lerpColor;
    #else
        #define AttributesUILerpColor(channel)
        #define VaryingsUILerpColor(channel)
        #define UITransferLerpColor(input, output)
    #endif // FD_COLORMOD_LERP

    #if FD_COLORMOD_ADDITIVE
        #define AttributesUIAdditiveColor(channel)  fixed4 additiveColor : TEXCOORD##channel;
        #define VaryingsUIAdditiveColor(channel)    fixed4 additiveColor : TEXCOORD##channel;
        #define UITransferAdditiveColor(input, output)  (output).additiveColor = (input).additiveColor;
    #else
        #define AttributesUIAdditiveColor(channel)
        #define VaryingsUIAdditiveColor(channel)
        #define UITransferAdditiveColor(input, output)
    #endif // FD_COLORMOD_ADDITIVE
#else

    #ifdef UNITY_INSTANCING_ENABLED
        UNITY_INSTANCING_BUFFER_START(InstancedColorMod)
            #if FD_COLORMOD_LERP
            UNITY_DEFINE_INSTANCED_PROP(fixed4, fd_RendererLerpColor)
            #endif // FD_COLORMOD_LERP
            
            #if FD_COLORMOD_ADDITIVE
            UNITY_DEFINE_INSTANCED_PROP(fixed4, fd_RendererAdditiveColor)
            #endif // FD_COLORMOD_ADDITIVE
        UNITY_INSTANCING_BUFFER_END(InstancedColorMod)

        #define _LerpColor      UNITY_ACCESS_INSTANCED_PROP(InstancedColorMod, fd_RendererLerpColor)
        #define _AdditiveColor  UNITY_ACCESS_INSTANCED_PROP(InstancedColorMod, fd_RendererAdditiveColor)
    #else
        #if FD_COLORMOD_LERP
        half4 _LerpColor;
        #endif // FD_COLORMOD_LERP

        #if FD_COLORMOD_ADDITIVE
        half4 _AdditiveColor;
        #endif // FD_COLORMOD_ADDITIVE
    #endif // UNITY_INSTANCING_ENABLED
#endif // IS_UI_SHADER

/// Uniforms

/// Helpers

#ifdef IS_UI_SHADER
    #if FD_COLORMOD_LERP
        #define UIApplyLerpColor(baseColor, input)   (baseColor).rgb = lerp((baseColor).rgb, (input).lerpColor.rgb, (input).lerpColor.a)
    #else
        #define UIApplyLerpColor(baseColor, input)
    #endif // FD_COLORMOD_LERP

    #if FD_COLORMOD_ADDITIVE
        #define UIApplyAdditiveColor(baseColor, input)   (baseColor).rgb += (input).additiveColor.rgb * (input).additiveColor.a
    #else
        #define UIApplyAdditiveColor(baseColor, input)
    #endif // FD_COLORMOD_ADDITIVE
#else
    #if FD_COLORMOD_LERP
        #define LayerApplyLerpColor(baseColor)   (baseColor).rgb = lerp((baseColor).rgb, _LerpColor.rgb, _LerpColor.a)
    #else
        #define LayerApplyLerpColor(baseColor)
    #endif // FD_COLORMOD_LERP

    #if FD_COLORMOD_ADDITIVE
        #define LayerApplyAdditiveColor(baseColor)   (baseColor).rgb += _AdditiveColor.rgb * _AdditiveColor.a
    #else
        #define LayerApplyAdditiveColor(baseColor)
    #endif // FD_COLORMOD_ADDITIVE
#endif // IS_UI_SHADER

#endif // FD_COLORMOD_INCLUDED