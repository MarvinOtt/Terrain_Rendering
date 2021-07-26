#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 EyePosition;
float3 LightDirection;
float abstand = 10;
Texture2D watertex;
Texture2D watertex2;
float wellenhohe;
float waveverschiebung;
float wellenbreite;
int texabm = 800;
float4 lightcolor;



// The input for the VertexShader
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float4 Color : COLOR;
};

// The output from the vertex shader, used for later processing
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
	// Fade function as defined by Ken Perlin.  This eases coordinate values
	// so that they will "ease" towards integral values.  This ends up smoothing
	// the final output.
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}
float getheightatpos(float xi, float yi)
{
	xi = xi / wellenbreite;
	yi = yi / wellenbreite;
	xi += 100000;
	yi += 100000;
	xi = abs(xi);
	yi = abs(yi);
	float y = (yi + waveverschiebung*1.05242f);
	float x = xi;
	float y2 = yi;
	float x2 = (xi + waveverschiebung);
	float y3 = yi;
	float x3 = (xi - waveverschiebung);
	int intx = (int)x;
	int inty = (int)y;
	int intx2 = (int)x2;
	int inty2 = (int)y2;
	int intx3 = (int)x3;
	int inty3 = (int)y3;

	uint2 pos00 = { intx % texabm, inty % texabm };
	uint2 pos10 = { (intx+1) % texabm, inty % texabm };
	uint2 pos01 = { intx % texabm, (inty+1) % texabm };
	uint2 pos11 = { (intx+1) % texabm, (inty+1) % texabm };

	uint2 pos002 = { intx2 % texabm, inty2 % texabm };
	uint2 pos102 = { (intx2 + 1) % texabm, inty2 % texabm };
	uint2 pos012 = { intx2 % texabm, (inty2 + 1) % texabm };
	uint2 pos112 = { (intx2 + 1) % texabm, (inty2 + 1) % texabm };

	uint2 pos003 = { intx3 % texabm, inty3 % texabm };
	uint2 pos103 = { (intx3 + 1) % texabm, inty3 % texabm };
	uint2 pos013 = { intx3 % texabm, (inty3 + 1) % texabm };
	uint2 pos113 = { (intx3 + 1) % texabm, (inty3 + 1) % texabm };

	float xanteil = x - ((int)x);
	float yanteil = y - ((int)y);
	float xanteil2 = x2 - ((int)x2);
	float yanteil2 = y2 - ((int)y2);
	float xanteil3 = x3 - ((int)x3);
	float yanteil3 = y3 - ((int)y3);
	float height = 0;
	height += (1 - xanteil) * watertex[pos00] * (1 - yanteil);
	height += (xanteil) * watertex[pos10] * (1 - yanteil);
	height += (1 - xanteil) * watertex[pos01] * (yanteil);
	height += (xanteil) * watertex[pos11] * (yanteil);

	height += (1 - xanteil2) * watertex[pos002] * (1 - yanteil2);
	height += (xanteil2)* watertex[pos102] * (1 - yanteil2);
	height += (1 - xanteil2) * watertex[pos012] * (yanteil2);
	height += (xanteil2)* watertex[pos112] * (yanteil2);

	height += (1 - xanteil3) * watertex[pos003] * (1 - yanteil3);
	height += (xanteil3)* watertex[pos103] * (1 - yanteil3);
	height += (1 - xanteil3) * watertex[pos013] * (yanteil3);
	height += (xanteil3)* watertex[pos113] * (yanteil3);
	return (height / 12.0f);
}
float3 getnormalatpos(float x, float y)
{
	float3 gesvec = float3(0, 1, 0);
	float height = getheightatpos(x, y) * wellenhohe;
	float heightxp1 = getheightatpos(x + abstand, y) * wellenhohe;
	float heightxm1 = getheightatpos(x - abstand, y) * wellenhohe;
	float heightyp1 = getheightatpos(x, y + abstand) * wellenhohe;
	float heightym1 = getheightatpos(x, y - abstand) * wellenhohe;

	float xx = ((height - heightxm1) / abstand) - ((height - heightxp1) / abstand);
	float yy = ((height - heightym1) / abstand) - ((height - heightyp1) / abstand);

	gesvec.x -= xx / 2.0f;
	gesvec.z -= yy / 2.0f;
	return normalize(gesvec);
}

// The VertexShader.
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float3 normal = normalize(mul(getnormalatpos(10000 + mul(input.Position, World).x, 10000 + mul(input.Position, World).z), World));
	output.Normal = normal;
	//float2 koo = float2((input.Position.x / 1000.0f) % 1, ((input.Position.z / 1000.0f) % 1));
	input.Position.y += getheightatpos(10000 + mul(input.Position, World).x, 10000 + mul(input.Position, World).z) * wellenhohe;
	//input.Position.y += (watertex.SampleLevel(wastex, koo, float1(1.0f))).r * 100 + 100;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.Position2 = worldPosition;
	output.Color = input.Color;
	output.View = normalize(float3(EyePosition)-worldPosition);
	return output;
}

// The Pixel Shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 normal = float4(input.Normal, 1.0);
	float4 diffuse = dot(-LightDirection, normal);
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
	float lightintens = 1 - pow(1 + LightDirection.y - 0.225f, 1);
	if (lightintens < 0)
	{
		lightintens = 0;
	}
	diffuse *= lightintens;
	if (diffuse.x < 0)
	{
		diffuse.x = 0;
		diffuse.y = 0;
		diffuse.z = 0;
	}
	float4 sss = float4(dotprodukt, dotprodukt, dotprodukt, 1);
	float4 lightcolors = sss * lightcolor;
	float4 dunkelblau = float4(input.Color.xyz*0.1f, 1);
	float4 output = ((input.Color * (pow(input.Normal.y, 2)) + dunkelblau * (1 - pow(input.Normal.y, 2))));
	output *= (-LightDirection.y);
	output += lightcolors * dotprodukt*2;
	float reflecstaerke = ((1 - pow(1 + LightDirection.y, 10)));
	if (reflecstaerke < 0)
	{
		reflecstaerke = 0;
	}
	return output * reflecstaerke;
}

// Our Techinique
technique Technique1
{
	pass Pass1
	{
		//AlphaBlendEnable = FALSE;
		/*
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
		*/
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}
