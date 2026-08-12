#ifndef OW_FOG_INCLUDED
#define OW_FOG_INCLUDED

// ------------------------------------------------------------------
//  Fog helpers
//
//	multi_compile_fog Will compile fog variants.
//	UNITY_FOG_COORDS(texcoordindex) Declares the fog data interpolator.
//	UNITY_TRANSFER_FOG(outputStruct,clipspacePos) Outputs fog data from the vertex shader.
//	UNITY_APPLY_FOG(fogData,col) Applies fog to color "col". Automatically applies black fog when in forward-additive pass.
//	Can also use UNITY_APPLY_FOG_COLOR to supply your own fog color.



// OW Fog Variables

sampler3D _FogLookupTex;
float4 _FogPosition; // xyz = world position, w = radius
float4 _FogDirRight; // DLC: xyz = fog right direction, w = unused
float4 _FogDirForward; // DLC: xyz = fog forward direction, w = unused
float4 _FogParams; // x = density scale, y = fog color ramp scale, z = density exponent, w = skybox factor
sampler2D _FogColorRampTex; // rgb = main color, a = density scale
float4 _FogTint; // rgb = tint, a = density scale



// OW Fog Functions

inline float4 CalcFogCoords( float3 worldVertex )
{
	float3 vertPos = worldVertex.xyz - _FogPosition.xyz;
	float3 cameraPos = _WorldSpaceCameraPos.xyz - _FogPosition.xyz;
	float3 sunDir = normalize( _OWSunPositionRange.xyz - _FogPosition.xyz );
	
	float3 lookDir = vertPos - cameraPos;
	float vertDepth = length( lookDir );
	lookDir /= vertDepth;
	
	float cameraHeight = length( cameraPos );
	float normCameraHeight = cameraHeight / _FogPosition.w;
	
	// Calc the near and far distances on the sphere
	float b = 2 * dot( lookDir, cameraPos );
	float c = dot( cameraPos, cameraPos ) - ( _FogPosition.w * _FogPosition.w );
	float det = max( b*b - 4*c, 0 );
	float sqrtDet = sqrt( det );
	float distNear = max( ( -b - sqrtDet ) * 0.5, 0 );
	float distFar = ( -b + sqrtDet ) * 0.5;
	
	float normDepth = (vertDepth-distNear) / (distFar-distNear);
	float3 norm = normalize( cameraPos + lookDir * distFar );
	float lookAngle = dot( norm, cameraPos / cameraHeight );
#if USE_RINGWORLD_LIGHTING
	// DLC: for ringworld fog, instead of using the angle to the sun for the lookup, we use the angle along the ring 
	float localVertPosX = dot( _FogDirRight.xyz, vertPos );
	float localVertPosY = dot( _FogDirForward.xyz, vertPos );
	float sunAngle = atan2( localVertPosX, localVertPosY ) * UNITY_INV_TWO_PI;
	sunAngle = sunAngle * 2 + 1; // remap to [-1,1] to compensate for the remap to [0,1] below 
#else
	float sunAngle = dot( sunDir, normalize(vertPos) );
#endif
	
	return float4( lookAngle*0.5+0.5, normCameraHeight, saturate(1-normDepth), sunAngle*0.5+0.5 );
}

inline float4 LookupFog( float4 fogCoord )
{
	float4 tex = tex3D( _FogLookupTex, fogCoord.xyz );
	float texDensity = DecodeFloatRG( tex.rg ) / DecodeFloatRG( tex.ba ); // RGBS encoding

	fixed4 colorRamp = tex2D( _FogColorRampTex, float2(fogCoord.w, 1-fogCoord.z) );
	colorRamp = lerp( float4(1,1,1,1), colorRamp, _FogParams.y );
	
	float density = texDensity * _FogParams.x * colorRamp.a * _FogTint.a;
	density = pow( density, _FogParams.z );
	return float4( colorRamp.rgb * _FogTint.rgb, density );
}

inline float4 CalcFogColor( float3 worldVertex )
{
	float4 coords = CalcFogCoords( worldVertex );
	return LookupFog( coords );
}

inline float3 ApplyFog( float3 col, float3 fogCol, float fogFac )
{
	return lerp( col.rgb, fogCol.rgb, saturate(fogFac) );
}

inline float4 ApplyFogPremult( float4 col, float fogFac )
{
	return lerp( col, fixed4(0,0,0,0), saturate(fogFac) );
}



// OW Fog Defines

#define UNITY_FOG_COORDS_PACKED(idx, vectype) vectype fogCoord : TEXCOORD##idx;

#if defined(UNITY_PASS_PREPASSBASE) || defined(UNITY_PASS_DEFERRED) || defined(UNITY_PASS_SHADOWCASTER) || defined(UNITY_PASS_META)
    #define UNITY_FOG_COORDS(idx)
	#define UNITY_TRANSFER_FOG(o,outpos)
	#define UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,outpos)
	#define UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,outpos)
    #define UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,outpos)
#else
    #define UNITY_FOG_COORDS(idx) UNITY_FOG_COORDS_PACKED(idx, float3)
    #define UNITY_TRANSFER_FOG(o,outpos) o.fogCoord.xyz = mul( unity_ObjectToWorld, v.vertex ).xyz
    #define UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,outpos)
    #define UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,outpos)
    #define UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,outpos) ERROR_UNSUPPORTED_FOG_PACKING_MODE // Unsupported fog packing mode.  This shouldn't happen.  Get Logan.
#endif

#define UNITY_FOG_LERP_COLOR(col,fogCol,fogFac) col.rgb = lerp((fogCol).rgb, (col).rgb, saturate(fogFac))


#if defined(UNITY_PASS_PREPASSBASE) || defined(UNITY_PASS_DEFERRED) || defined(UNITY_PASS_SHADOWCASTER) || defined(UNITY_PASS_META)
	#define UNITY_APPLY_FOG_COLOR(coord,col,fogCol)
    #define UNITY_EXTRACT_FOG(name)
    #define UNITY_EXTRACT_FOG_FROM_TSPACE(name)
    #define UNITY_EXTRACT_FOG_FROM_WORLD_POS(name)
    #define UNITY_EXTRACT_FOG_FROM_EYE_VEC(name)
#else
    #define UNITY_APPLY_FOG_COLOR(coord,col,fogCol) col.rgb = ApplyFog( col.rgb, fogCol.rgb, CalcFogColor(coord).a )
    #define UNITY_EXTRACT_FOG(name) float3 _unity_fogCoord = name.fogCoord
    #define UNITY_EXTRACT_FOG_FROM_TSPACE(name) float3 _unity_fogCoord = float3( name.tSpace0.w, name.tSpace1.w, name.tSpace2.w )
    #define UNITY_EXTRACT_FOG_FROM_WORLD_POS(name) float3 _unity_fogCoord = name.worldPos.xyz
	#define UNITY_EXTRACT_FOG_FROM_EYE_VEC(name) ERROR_UNSUPPORTED_FOG_PACKING_MODE // Unsupported fog packing mode.  This shouldn't happen.  Get Logan.
#endif

#ifdef UNITY_PASS_FORWARDADD
    #define UNITY_APPLY_FOG(coord,col) UNITY_APPLY_FOG_COLOR(coord,col,fixed4(0,0,0,0))
#else
    #define UNITY_APPLY_FOG(coord,col) float4 fogCol = CalcFogColor(coord); col.rgb = ApplyFog( col.rgb, fogCol.rgb, fogCol.a )
#endif

#endif // OW_FOG_INCLUDED