#ifndef FD_DXCOMPAT_INCLUDED
#define FD_DXCOMPAT_INCLUDED

#include "UnityCG.cginc"

#if defined(UNITY_FIXED_IS_HALF)
#define FD_SUPPORTS_HALF 1
#else
#define FD_SUPPORTS_HALF 0
#endif // defined(UNITY_FIXED_IS_HALF)

/*
#if !defined(sampler2DArray)

struct sampler2DArray                           { Texture2DArray t; SamplerState s; };
float4 tex2DArray(sampler2DArray x, float3 v)   { return x.t.Sample(x.s, v); }

#endif // !defined(sampler2DArray)
*/

#endif // FD_DXCOMPAT_INCLUDED