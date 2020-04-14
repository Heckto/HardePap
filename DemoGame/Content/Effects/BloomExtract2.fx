#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler TextureSampler: register(s0); // from SpriteBatch

float _bloomThreshold;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	// Look up the original image color.
	float4 c = tex2D(TextureSampler, texCoord);

	// Adjust it to keep only values brighter than the specified threshold.
	return saturate((c - _bloomThreshold) / (1 - _bloomThreshold));
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};