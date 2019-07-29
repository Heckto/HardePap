#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler samplerState;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};


float v = 0.01f;
float a = 0.25f;
const float2 offsets[12] = {
-0.326212, -0.405805,
-0.840144, -0.073580,
-0.695914, 0.457137,
-0.203345, 0.620716,
0.962340, -0.194983,
0.473434, -0.480026,
0.519456, 0.767022,
0.185461, -0.893124,
0.507431, 0.064425,
0.896420, 0.412458,
-0.321940, -0.932615,
-0.791559, -0.597705,
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 Bloom(VertexShaderOutput Input) : COLOR0
{
	float4 col = tex2D(samplerState, Input.TextureCoordinates);
	for (int i = 0; i < 12; i++)
	col += tex2D(samplerState, Input.TextureCoordinates + v *
	offsets[i]);
	col /= 13.0f;
	col.a = a;
	return col;
}


technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL Bloom();
	}
};