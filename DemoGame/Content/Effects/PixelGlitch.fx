﻿#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0; // from SpriteBatch

float _verticalSize; // vertical size in pixels or each row. default 5.0
float _horizontalOffset; // horizontal shift in pixels. default 10.0
float2 _screenSize; // screen width/height

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

float hash11( float p )
{
	float3 p3  = frac( float3( p, p, p ) * 0.1031 );
    p3 += dot( p3, p3.yzx + 19.19 );
    return frac( ( p3.x + p3.y ) * p3.z );
}


float4 MainPS( float2 coords:TEXCOORD0 ) : COLOR
{
    // convert verticalSize and horizontalOffset from pixels
    float _pixels = _screenSize.x / _verticalSize;
    float offset = _horizontalOffset / _screenSize.y;

    // get a number between -1 and 1 to offset the row of pixels by that is dependent on the y position
    float r = hash11( floor( coords.y * _pixels ) ) * 2.0 - 1.0;
    return tex2D( s0, float2( coords.x + r * offset, coords.y ) );
 //   return tex2D( s0,coords);
}



technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};