#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif


Texture2D heightmap;

sampler2D normalTextureSampler = sampler_state
{
	Texture = <heightmap>;
};
float waveheight;
float wavesize;
int texsizex;
int texsizey;
float getheightatpos(int xi, int yi)
{
	int intx = xi;
	int inty = yi;
	uint2 pos00 = {intx, inty};
	float height = 0;
	height = heightmap[pos00];
	return height;
}

float fade(float t)
{
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}

float3 getnormalatpos(float x, float y)
{
	float3 gesvec = float3(0, 1, 0);
	float height = getheightatpos(x, y) * waveheight;


	float heightxp1;
	if (x + 1 >= texsizey)
		heightxp1 = getheightatpos(0, y) * waveheight;
	else
		heightxp1 = getheightatpos(x + 1, y) * waveheight;


	float heightxm1;
	if(x-1 < 0)
		heightxm1 = getheightatpos(texsizex - 1, y) * waveheight;
	else
		heightxm1 = getheightatpos(x - 1, y) * waveheight;


	float heightyp1;
	if(y + 1 >= texsizey)
		heightyp1 = getheightatpos(x, 0) * waveheight;
	else
		heightyp1 = getheightatpos(x, y + 1) * waveheight;


	float heightym1;
	if(y - 1 < 0)
		heightym1 = getheightatpos(x, texsizey - 1) * waveheight;
	else
		heightym1 = getheightatpos(x, y - 1) * waveheight;


	float xx = ((height - heightxm1) / wavesize) - ((height - heightxp1) / wavesize);
	float yy = ((height - heightym1) / wavesize) - ((height - heightyp1) / wavesize);

	gesvec.x -= xx / 2.0f;
	gesvec.z -= yy / 2.0f;

	float factor = 10;

	if (x <= factor)
	{
		gesvec.x *= fade(x / factor);
		gesvec.z *= fade(x / factor);
	}
	if (y <= factor)
	{
		gesvec.x *= fade(y / factor);
		gesvec.z *= fade(y / factor);
	}
	if (x >= texsizex - factor)
	{
		gesvec.x *= fade((texsizex - x) / factor);
		gesvec.z *= fade((texsizex - x) / factor);
	}
	if (y >= texsizey - factor)
	{
		gesvec.x *= fade((texsizey - y) / factor);
		gesvec.z *= fade((texsizey - y) / factor);
	}

	return normalize(gesvec);
}


struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	float3 normal = getnormalatpos(input.Position.x, input.Position.y);
	normal = float3((normal.x + 1) / 2.0f, (normal.y + 1) / 2.0f, (normal.z + 1) / 2.0f);
	return float4(normal, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};