#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;
float3 LightDirection;

int output2type;

Texture2D obstaclemap;
sampler2D obstaclesamp = sampler_state {
	Texture = (obstaclemap);
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

// The input for the VertexShader
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float4 Color : COLOR;
};

// The output from the vertex shader || input for the pixel shader
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Position2 : POSITION1;
	float3 Normal : TEXCOORD0;
	float3 View : TEXCOORD1;
	float4 Color : COLOR0;
};

float fade(float t)
{
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}

float Dither(float3 uv)
{
	float4 magic = { 0.06711056, 0.00583715, 0.739567264, 52.9829189 };
	return frac(magic.w * frac(dot(uv, magic.xyz)));
}

float airdensityatheight(float h)
{
	return 1.225f * pow((288.16f - 0.0065f * h) * 0.00347f, 4.2561f);
}

// The Vertex Shader
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.Position2 = worldPosition;
	output.Color = input.Color;
	float3 normal = normalize(mul(input.Normal, World));
	output.Normal = normal;
	output.View = normalize(float3(EyePosition)-worldPosition);
	return output;
}


struct PixelOut
{
	float4 Col1 : COLOR0;
	float4 Col2 : COLOR1;
};

// The Pixel Shader
PixelOut PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	PixelOut OUT;
	float3 richtung = normalize(input.Position2 - EyePosition); // Direction to sky
	float airdensity = (1 - richtung.y); //Density of air to the pixel of the sky
	float blauint = (-LightDirection.y) + (1 - airdensity) * 0.05f;
	float sundot = (dot(-LightDirection, richtung) + 1) / 2.0f;

	blauint = ((blauint / 1.4f) + 0.4f)*0.5f;

	// Makes the sky dark when the sun goes down
	float verdunkelung = ((-LightDirection.y) + 1) / 2.0f;

	float sonne = pow(sundot + 0.00075f, 100000) * 1000;

	float luftpow = (1 / pow(pow(airdensity, 2), 4));
	float orangeint = pow(sundot * (1 + 0.002f * pow(airdensity, 2)), luftpow) * verdunkelung * 1.5f + pow(sundot + 0.0005f, 500) * 0.25f;

	float4 orange = float4(1, 0.6f, 0.05f, 1); // Color of the sun
	float4 blue = float4(0.25f, 0.52f, 1, 1); // Color of the sky at day

	float4 output = orange * orangeint + blue * (1 - pow(1 - blauint, 2)); // Combining the blue and orange Color
	OUT.Col1 = output;

	if (output2type == 1) //Bloom Effect
	{
		float brightness = sonne;
		brightness = pow(clamp(brightness / 1.0f, 0, 1), 1) * 2.0f;
		OUT.Col2 = float4(brightness, 0, 0, 1);
	}
	else if (output2type == 2) // Depth Effect
	{
		OUT.Col2 = float4(input.Position2.y, 0, 0, 1);
	}
	else if (output2type == 3) // God Ray Effect
	{
		float4 _col = tex2D(obstaclesamp, float2(input.Position.x / 1920.0f, input.Position.y / 1080.0f));
		if (_col.x > 0.015f)
			OUT.Col2 = _col + float4(clamp(sonne, 0, 1), clamp(sonne, 0, 1), clamp(sonne, 0, 1), 1.0f);
		else
			OUT.Col2 = _col;
		OUT.Col2.a = 1.0f;
	}
	float noise = lerp(-0.5, 0.5, Dither(input.Position2.xyz));
	OUT.Col1.xyz = OUT.Col1 + noise / 511.0f;

	//OUT.Col1.xyz = pow(pow(OUT.Col1.xyz, 1.0 / 2.2) + noise / 255.0, 2.2);

	return OUT;
}

technique Rendering
{
	pass Pass_1
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}
