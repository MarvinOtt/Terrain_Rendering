#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D SpriteTexture, lightmap;
float3 transformedLightDirection;
int Screenwidth, Screenheight;
float Aspectratio;

float4 blue = float4(0.25f, 0.52f, 1, 1); // Color of the sky at day

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

sampler2D lightsamp = sampler_state
{
	Texture = <lightmap>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 col = tex2D(SpriteTextureSampler,input.TextureCoordinates);
	float4 OUT;
	if (col.a > 0.05f)
		OUT = float4(1, 0, 0, 1);
	else
		OUT = float4(0, 0, 0, 0);
	//else
		//OUT = float4(blue.xyz * 0.015f, 0);

	float2 screen = float2(input.Position.x / (float)Screenwidth, input.Position.y / (float)Screenheight) * 2 - float2(1, 1);
	//float2 screennorm = (((input.Position.xy / 1080.0f) * 5.67f) *2) - float2(1, 1);
	float3 projecteddirection = normalize(float3(0.839f * screen.x * Aspectratio, 0.839f * screen.y, 1.0f));
	//float3 richtung = normalize(pos2 - EyePosition); // Direction to sky
	float sundot = (dot(transformedLightDirection, projecteddirection) + 1) / 2.0f;

	float sonne = pow(abs(sundot) + 0.00020f, 3200);

	float4 light = tex2D(lightsamp, float2(input.Position.x / (float)Screenwidth, input.Position.y / (float)Screenheight));

	if(col.a <= 0.05f)
		OUT += light * 0.5f;
	return OUT;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};