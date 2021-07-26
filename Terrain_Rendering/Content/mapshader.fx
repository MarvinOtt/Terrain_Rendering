#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

/*matrix WorldViewProjection;

struct VertexShaderInput
{
float4 Position : SV_POSITION;
float4 Color : COLOR0;
};

struct VertexShaderOutput
{
float4 Position : SV_POSITION;
float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
VertexShaderOutput output = (VertexShaderOutput)0;

output.Position = mul(input.Position, WorldViewProjection);
output.Color = input.Color;

return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
return input.Color;
}

technique BasicColorDrawing
{
pass P0
{
VertexShader = compile VS_SHADERMODEL MainVS();
PixelShader = compile PS_SHADERMODEL MainPS();
}
};*/

float4x4 World;
float4x4 View;
float4x4 Projection;
static const float PI = 3.14159265f;
// Light related
float4 AmbientColor;
float AmbientIntensity;
float4 Color;
float3 LightDirection;
float4 DiffuseColor;
float DiffuseIntensity, DiffuseIntensity2;

float4 SpecularColor;
float SpecularIntensity;
float3 EyePosition;


// The input for the VertexShader
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 View : TEXCOORD1;
	float4 Color : COLOR0;
};

// The VertexShader.
VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	float3 normal = normalize(mul(Normal, World));
	output.Normal = normal;
	output.View = normalize(float3(EyePosition)-worldPosition);
	output.Color = input.Color;
	return output;
}

// The Pixel Shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	/*float4 normal = float4(input.Normal, 1.0);
	float4 diffuse = saturate(dot(-LightDirection,normal));
	float4 reflect = normalize(2 * diffuse * normal - float4(LightDirection, 1.0));
	float4 specular = pow(saturate(dot(reflect,input.View)),30) * 5;
	return AmbientColor*AmbientIntensity + DiffuseIntensity*DiffuseColor*diffuse + SpecularColor*specular;*/
	/*float4 normal = float4(input.Normal, 1.0);
	float4 diffuse = dot(-LightDirection,normal);
	//float4 reflect = normalize(2 * diffuse * normal - float4(-LightDirection, 1.0));
	float3 reflect2 = normalize(reflect(-LightDirection, normal));
	float abschwacher = 1;
	if (dot(normal, -LightDirection) < 0)
	{
	abschwacher = 1 + dot(normal, -LightDirection) * 100;
	if (abschwacher < 0)
	{
	abschwacher = 0;
	}
	}
	float dotprodukt = saturate(pow(dot(reflect2, input.View)*1, 6));
	float4 sss = SpecularColor * pow(dotprodukt, 1) * abschwacher;
	return saturate(AmbientColor*AmbientIntensity + DiffuseIntensity*DiffuseColor*diffuse + sss);*/
	float4 normal = float4(input.Normal, 1.0);
	float4 diffuse = dot(-LightDirection,normal);
	float3 allgemeineslicht = float3(0, -1, 0);
	float4 diffuse2 = dot(-allgemeineslicht, normal);
	//float4 reflect = normalize(2 * diffuse * normal - float4(-LightDirection, 1.0));
	float3 reflect2 = normalize(reflect(-LightDirection, normal));
	float abschwacher = 1;
	if (dot(normal, -LightDirection) < 0)
	{
		abschwacher = 1 + dot(normal, -LightDirection) * 10;
		if (abschwacher < 0)
		{
			abschwacher = 0;
		}
	}
	float dotprodukt = pow(saturate(dot(-reflect2, input.View)*0.999),100);
	if (dot(normal, -LightDirection) < -0.1)
	{
		//dotprodukt = 0;
	}
	if (diffuse.x < 0)
	{
		diffuse.x = 0;
		diffuse.y = 0;
		diffuse.z = 0;
	}
	if (diffuse2.x < 0)
	{
		diffuse2.x = 0;
		diffuse2.y = 0;
		diffuse2.z = 0;
	}



	float4 sss = float4(dotprodukt, dotprodukt, dotprodukt, 1);
	//return saturate(AmbientColor*AmbientIntensity + DiffuseIntensity*DiffuseColor*diffuse + sss * SpecularColor);
	float4 lightcolors = (AmbientColor * AmbientIntensity + DiffuseIntensity * DiffuseColor * diffuse + DiffuseIntensity2 * DiffuseColor * diffuse2 + sss * SpecularColor * SpecularIntensity);
	return saturate(((input.Color*normal.y + (Color - float4(1,1,1,1))*(-1)) * lightcolors));
}

// Our Techinique
technique Technique1
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}
