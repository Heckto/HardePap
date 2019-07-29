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

float horizon = 0.5;
float delta;
float theta;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};



struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 Water(VertexShaderOutput Input) : COLOR0
{
	float4 sum = 0;
	float2 tex = Input.TextureCoordinates;
	if (tex.y < 1.0)
	{
		tex.y = horizon - tex.y;
		tex.y += (cos(tex.x * 50.0 + delta) / 500.0f);
		tex.y += (sin(tex.y * 250.0 + theta) / 120.0f);
		tex.x += (tex.y * (sin(tex.y * 750.0 + theta) / 250.0f));
		sum = tex2D(samplerState, tex);
	}
	//if (Input.TextureCoordinates.y < 0.2f)
		//sum.a = 0.0;
	//else if (Input.TextureCoordinates.y < 0.25f)
		//sum.a = (Input.TextureCoordinates.y - 0.2) * 20.0;
	//else 
		sum.a = 1.0;	
	return sum;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL Water();
	}
};