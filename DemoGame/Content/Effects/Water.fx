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
float _time;
float delta;
float theta;

const float PI = 3.14159265f;

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

	float delta = _time * 8.0 % (2 * PI);
	float theta = _time * 10.0 % (2 * PI);

	if (tex.y < 1.0)
	{
		tex.y = horizon - tex.y;
		tex.y += (cos(tex.x * 10.0 + delta) / 1500.0f);
		tex.y += (sin(tex.y * 100.0 + theta) / 240.0f);
		tex.x += (tex.y * (sin(tex.y * 150.0 + theta) / 150.0f));
		sum = tex2D(samplerState, tex);
	}
       if (Input.TextureCoordinates.y < 0.05f)
		sum.a = 0.0;
	if (Input.TextureCoordinates.y < 0.25f)
		sum.a = (Input.TextureCoordinates.y) * 20.0;
	else
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