#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif
int height;
static float Pixels[31] =
{
	-15,
	-14,
	-13,
	-12,
	-11,
	-10,
	-9,
	-8,
	-7,
	-6,
	-5,
	-4,
	-3,
	-2,
	-1,
	0,
	1,
	2,
	3,
	4,
	5,
	6,
	7,
	8,
	9,
	10,
	11,
	12,
	13,
	14,
	15,
};
/*
static float Pixels[32] =
{
-30.5f,
-28.5f,
-26.5f,
-24.5f,
-22.5f,
-20.5f,
-18.5f,
-16.5f,
-14.5f,
-12.5f,
-10.5f,
-8.5f,
-6.5f,
-4.5f,
-2.5f,
-0.5f,
0.5f,
2.5f,
4.5f,
6.5f,
8.4f,
10.5f,
12.5f,
14.5f,
16.5f,
18.5f,
20.5f,
22.5f,
24.5f,
26.5f,
28.5f,
30.5f,
};*/

float BlurWeights[32];
/*=
{
	0.001924,
	0.002957,
	0.004419,
	0.006424,
	0.009084,
	0.012493,
	0.016713,
	0.021747,
	0.027524,
	0.033882,
	0.04057,
	0.04725,
	0.053526,
	0.058978,
	0.063209,
	0.065892,
	0.065892,
	0.063209,
	0.058978,
	0.053526,
	0.04725,
	0.04057,
	0.033882,
	0.027524,
	0.021747,
	0.016713,
	0.012493,
	0.009084,
	0.006424,
	0.004419,
	0.002957,
	0.001924,
};*/


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



float4 MainPS(VertexShaderOutput input) : COLOR
{
	// Pixel width
	float pixelHeight = (1 / (float)height);
	float4 color = { 0, 0, 0, 1 };

	float2 blur;
	blur.x = input.TextureCoordinates.x;

	for (int i = 0; i < 31; i++)
	{
		blur.y = input.TextureCoordinates.y + Pixels[i] * pixelHeight;
		color += tex2D(SpriteTextureSampler, blur) * BlurWeights[i];
	}

	return color;
}


technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}

};