#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 lightcolor;

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};
Texture2D tex;
sampler2D texsamp = sampler_state
{
	Texture = <tex>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float strength;
	float4 col1 = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float4 col2 = tex2D(texsamp, input.TextureCoordinates);
	strength = clamp(col2.x, 0, 1);
	float4 coll = lightcolor * strength + float4(1, 0.6f, 0.25f, 1) * (1 - strength);
	return col1 * (1 - strength) + coll * strength;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};