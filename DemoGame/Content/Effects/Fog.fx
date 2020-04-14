#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

Texture2D SpriteTexture;

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


//float3 color = float3(0.35, 0.18, 0.95);
float3 color = float3(1.0, 1.0, 1.0);
int OCTAVES = 4;
float _time;
float2 offset;
float2 res;

float rand(float2 coord) {
	return frac(sin(dot(coord, float2(56, 78)) * 1000.0) * 1000.0);
}

float noise(float2 coord) {
	float2 i = floor(coord);
	float2 f = frac(coord);

	// 4 corners of a rectangle surrounding our point
	float a = rand(i);
	float b = rand(i + float2(1.0, 0.0));
	float c = rand(i + float2(0.0, 1.0));
	float d = rand(i + float2(1.0, 1.0));

	float2 cubic = f * f * (3.0 - 2.0 * f);

	return lerp(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float fbm(float2 coord) {
	float value = 0.0;
	float scale = 0.5;

	for (int i = 0; i < 4; i++) {
		value += noise(coord) * scale;
		coord *= 2.0;
		scale *= 0.5;
	}
	return value;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{

	float4 c = float4(color,0);
	
	float2 coord = input.TextureCoordinates * 20;
	float f = fbm(coord + float2(_time * 2.0 * -0.2, _time * 2.0 * 0.2));
	float2 motion = float2(f,f);
	float final = fbm(coord + motion);
	c.a = final * 1.5 * (input.TextureCoordinates.y);
	c.rgb = (c.rgb * final * 1.25 * (input.TextureCoordinates.y));
	return c;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};