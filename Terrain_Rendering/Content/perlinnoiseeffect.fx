#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

texture2D SpriteTexture;
float4x4 Matrix;
float2 rendercoos;
float p[255];
float2 texsize;
int worldsize;
float pointvalue00;
float pointvalue10;
float pointvalue01;
float pointvalue11;
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float fade(float t)
{
	// Fade function as defined by Ken Perlin.  This eases coordinate values
	// so that they will "ease" towards integral values.  This ends up smoothing
	// the final output.
	return (t * t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}
float fade2(float t)
{
	// Fade function as defined by Ken Perlin.  This eases coordinate values
	// so that they will "ease" towards integral values.  This ends up smoothing
	// the final output.
	return (t * t * (t * (t * 6 - 15) + 10));         // 6t^5 - 15t^4 + 10t^3
}

float speedperlin2D(float x, float y)
{

	uint xcoo, ycoo;
	xcoo = (int)x;
	ycoo = (int)y;
	pointvalue00 = p[((p[(((xcoo) * 734) % 256)] * 0.5f + p[(((ycoo) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue01 = p[((p[(((xcoo) * 734) % 256)] * 0.5f + p[(((ycoo + 1) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue10 = p[((p[(((xcoo + 1) * 734) % 256)] * 0.5f + p[(((ycoo) * 346) % 256)] * 0.5f))] * 0.00390625f;
	pointvalue11 = p[((p[(((xcoo + 1) * 734) % 256)] * 0.5f + p[(((ycoo + 1) * 346) % 256)] * 0.5f))] * 0.00390625f;
	float hohe = 0;
	hohe += fade(1 - (x - xcoo)) * pointvalue00 * fade(1 - (y - ycoo));
	hohe += fade(x - xcoo) * pointvalue10 * fade(1 - (y - ycoo));
	hohe += fade(1 - (x - xcoo)) * pointvalue01 * fade(y - ycoo);
	hohe += fade(x - xcoo) * pointvalue11 * fade(y - ycoo);
	return hohe;

	/*x += 100;
	y += 100;
	int xcoo, ycoo;
	xcoo = (int)x;
	ycoo = (int)y;
	Vector2 point = new Vector2(x, y);

	pointvector00 = vecs[p2[((p2[(((xcoo) * 734) % 1024)] + p2[(((ycoo) * 734) % 1024)]) / 2)]];
	pointvector01 = vecs[p2[((p2[(((xcoo) * 734) % 1024)] + p2[(((ycoo + 1) * 734) % 1024)]) / 2)]];
	pointvector10 = vecs[p2[((p2[(((xcoo + 1) * 734) % 1024)] + p2[(((ycoo) * 734) % 1024)]) / 2)]];
	pointvector11 = vecs[p2[((p2[(((xcoo + 1) * 734) % 1024)] + p2[(((ycoo + 1) * 734) % 1024)]) / 2)]];

	pointvalue00 = Vector2.Dot(pointvector00, point - new Vector2(xcoo, ycoo));
	pointvalue01 = Vector2.Dot(pointvector01, point - new Vector2(xcoo, ycoo + 1));
	pointvalue10 = Vector2.Dot(pointvector10, point - new Vector2(xcoo + 1, ycoo));
	pointvalue11 = Vector2.Dot(pointvector11, point - new Vector2(xcoo + 1, ycoo + 1));
	float höhe = 0;
	höhe += fade(1 - (x - xcoo)) * pointvalue00 * fade(1 - (y - ycoo));
	höhe += fade(x - xcoo) * pointvalue10 * fade(1 - (y - ycoo));
	höhe += fade(1 - (x - xcoo)) * pointvalue01 * fade(y - ycoo);
	höhe += fade(x - xcoo) * pointvalue11 * fade(y - ycoo);*/
	return hohe;
}
float speedOctavePerlin2D(float x, float y, int octaves, float persistence)
{
	persistence *= 1;
	float total = 0;
	float frequency = 0.5;
	float amplitude = 1;
	float maxValue = 0;
	for (int i = 0; i < octaves; i++)
	{
		total += speedperlin2D(x * frequency, y * frequency) * amplitude;
		maxValue += amplitude;
		amplitude *= persistence;
		frequency *= 2;
	}
	return total / maxValue;
}
struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input, float2 coords: TEXCOORD0) : COLOR0
{
	float height = 1;
	float bergheight = (speedOctavePerlin2D((coords.x*texsize.x + rendercoos.x) / 300.0f, (coords.y*texsize.y + rendercoos.y) / 300.0f, 10, 0.35f))/2.0f;
	float mediumheight = (speedOctavePerlin2D((coords.x*texsize.x + rendercoos.x) / 300.0f+3, (coords.y*texsize.y + rendercoos.y) / 300.0f+3, 10, 0.38f)-0.2f)/7.0f;
	float flachlandheight = (speedOctavePerlin2D((coords.x*texsize.x + rendercoos.x) / 1000.0f, (coords.y*texsize.y + rendercoos.y) / 1000.0f, 10, 0.45f) - 0.2f) / 10.0f;
	float biomperlin = (speedOctavePerlin2D((coords.x*texsize.x + rendercoos.x) / 1200.0f, (coords.y*texsize.y + rendercoos.y) / 1200.0f, 4, 0.2f) - 0.5f);
	float meerperlin = (speedOctavePerlin2D((coords.x*texsize.x + rendercoos.x) / 1200.0f+5, (coords.y*texsize.y + rendercoos.y) / 1200.0f+5, 4, 0.2f) - 0.5f);
	float mediumperlin = (speedOctavePerlin2D((coords.x*texsize.x + rendercoos.x) /400.0f + 4, (coords.y*texsize.y + rendercoos.y) / 400.0f + 4, 4, 0.2f) - 0.4f);
	float meerheight = (speedOctavePerlin2D((coords.x*texsize.x + rendercoos.x) / 500.0f + 5, (coords.y*texsize.y + rendercoos.y) / 500.0f + 5, 8, 0.4f) - 0.85f)/5.0f;
	if (biomperlin > 0.4)
	{
		height = bergheight;
	}
	else if (biomperlin > 0)
	{
		if (meerperlin > 0.4)
		{
			height = meerheight;
		}
		else if (meerperlin > -0.1)
		{
			height = meerheight*fade((meerperlin + 0.1) * (1 / 0.5)) + flachlandheight*fade(1 - ((meerperlin + 0.1) * (1 / 0.5)));
		}
		else
		{
			height = flachlandheight;
		}

		if (mediumperlin > 0.4)
		{
			height += mediumheight;
		}
		else if (mediumperlin > -0.1)
		{
			height += mediumheight*fade((mediumperlin + 0.1) * (1 / 0.5));
		}

		height = bergheight*fade((biomperlin+0) * (1 / 0.4)) + height*fade(1 - ((biomperlin + 0) * (1 / 0.4)));
	}
	else
	{
		if (meerperlin > 0.4)
		{
			height = meerheight;
		}
		else if (meerperlin > -0.1)
		{
			height = meerheight*fade((meerperlin + 0.1) * (1 / 0.5)) + flachlandheight*fade(1 - ((meerperlin + 0.1) * (1 / 0.5)));
		}
		else
		{
			height = flachlandheight;
		}

		if (mediumperlin > 0.4)
		{
			height += mediumheight;
		}
		else if (mediumperlin > -0.1)
		{
			height += mediumheight*fade((mediumperlin + 0.1) * (1 / 0.5));
		}
	}
	float xcoo = coords.x*texsize.x + rendercoos.x;
	float ycoo = coords.y*texsize.y + rendercoos.y;
	float smoothsize = 1000;
	float factor;
	if (xcoo < smoothsize)
	{
		factor = fade(pow((smoothsize - xcoo) / smoothsize, 2));
		height = (factor * -0.05f) + (1 - factor) * height;
	}
	if (ycoo < smoothsize)
	{
		factor = fade(pow((smoothsize - ycoo) / smoothsize, 2));
		height = (factor * -0.05f) + (1 - factor) * height;
	}
	if (xcoo > worldsize - smoothsize)
	{
		factor = fade(pow((smoothsize-(worldsize - xcoo)) / smoothsize, 2));
		height = (factor * -0.05f) + (1 - factor) * height;
	}
	if (ycoo > worldsize - smoothsize)
	{
		factor = fade(pow((smoothsize - (worldsize - ycoo)) / smoothsize, 2));
		height = (factor * -0.05f) + (1 - factor) * height;
	}

	return float4(height, 0, 0.5, 1);

}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};