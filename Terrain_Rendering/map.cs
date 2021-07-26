using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Terrain_Rendering
{
    public class mapsettings
    {
        public string texname;
        public int texanz;
        public float water_height;

        public float snow_minheight;
        public float snow_intensity;

        public float grass_minheight;
        public float grass_maxheight;
        public float grass_fadein;
        public float grass_fadeout;

        public float dirt_minheight;
        public float dirt_maxheight;
        public float dirt_fadein;
        public float dirt_fadeout;

        public float sand_maxheight;
        public float sand_intensity;

        public mapsettings(string texname, int texanz, float water_height)
        {
            this.texname = texname;
            this.texanz = texanz;
            this.water_height = water_height;
        }
        // S A N D
        public void load_environment_sand(float sand_maxheight, float sand_intensity)
        {
            this.sand_maxheight = sand_maxheight;
            this.sand_intensity = sand_intensity;
        }
        // G R A S S
        public void load_environment_grass(float grass_minheight, float grass_maxheight, float grass_fadein, float grass_fadeout)
        {
            this.grass_minheight = grass_minheight;
            this.grass_maxheight = grass_maxheight;
            this.grass_fadein = grass_fadein;
            this.grass_fadeout = grass_fadeout;
        }
        // D I R T
        public void load_environment_dirt(float dirt_minheight, float dirt_maxheight, float dirt_fadein, float dirt_fadeout)
        {
            this.dirt_minheight = dirt_minheight;
            this.dirt_maxheight = dirt_maxheight;
            this.dirt_fadein = dirt_fadein;
            this.dirt_fadeout = dirt_fadeout;
        }
        // S N O W
        public void load_environment_snow(float snow_minheight, float snow_intensity)
        {
            this.snow_minheight = snow_minheight;
            this.snow_intensity = snow_intensity;
        }
    }

    public class map : IDisposable
    {
        bool IsDisposed;
        private SpriteBatch batch;
        private ContentManager Content;
        private GraphicsDevice device;
        public Water water;
        public RenderTarget2D reflectionmap, reflectiondepthmap;
        public Texture2D heights_rough;
        public Effect mapeffect, perlineffect;

        public int seed;
        public int size;
        public int mappower;
        public int chunksize;
        public Vector2 mpos;
        public float Smallestdistancetomap;
        public int chunkdrawanz;
        public bool IsLoaded;
        public float loadingprogress;
        public string loadingtype;
        public bool reflectionrendering;
        private mapsettings Settings; // ~~~
        public float minh, maxh, scale;
        public float[,] heights;
        private float[] texweights;
        private mapchunk mainchunk;
        public IndexBuffer ichunkbuffer; // Chunk Indexbuffer
        public Matrix World, View, Projection; // Camera
        public BoundingFrustum Frustum;

        public map(GraphicsDevice device, ContentManager Content, int ChS)
        {
            loadingtype = "Inizialising";
            IsDisposed = false;
            this.device = device;
            this.Content = Content;
            batch = new SpriteBatch(device);
            water = new Water(device, Content);
            Frustum = new BoundingFrustum(Matrix.Identity);
            IsLoaded = false;
            loadingprogress = 0;
            texweights = new float[4];
            chunksize = ChS;

            #region Generation Index Buffer
            List<ushort> ushorts = new List<ushort>();
            for (int x = 0; x < chunksize - 1; x++)
            {
                for (int y = 0; y < chunksize - 1; y++)
                {
                    ushorts.Add((ushort)(x + y * chunksize));
                    ushorts.Add((ushort)(x + 1 + y * chunksize));
                    ushorts.Add((ushort)(x + 1 + (y + 1) * chunksize));

                    ushorts.Add((ushort)(x + y * chunksize));
                    ushorts.Add((ushort)(x + 1 + (y + 1) * chunksize));
                    ushorts.Add((ushort)(x + (y + 1) * chunksize));
                }
            }
            ichunkbuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, ushorts.Count, BufferUsage.WriteOnly);
            ichunkbuffer.SetData<ushort>(ushorts.ToArray());
            #endregion

            #region Initialising Shader
            perlineffect = Content.Load<Effect>("heightgeneratoreffect");
            mapeffect = Content.Load<Effect>("maptextureshader");
            Texture2D tex = Content.Load<Texture2D>("sand");
            mapeffect.Parameters["Texture1"].SetValue(tex);
            tex = Content.Load<Texture2D>("sand_normal");
            mapeffect.Parameters["sandnormaltex"].SetValue(tex);

            tex = Content.Load<Texture2D>("wetsand_tex");
            mapeffect.Parameters["wetsand_tex"].SetValue(tex);
            tex = Content.Load<Texture2D>("wetsand_normal");
            mapeffect.Parameters["wetsand_normal"].SetValue(tex);
            tex = Content.Load<Texture2D>("wetsand_specular");
            mapeffect.Parameters["wetsand_specular"].SetValue(tex);

            tex = Content.Load<Texture2D>("gras");
            mapeffect.Parameters["Texture2"].SetValue(tex);
            tex = Content.Load<Texture2D>("rock2");
            mapeffect.Parameters["Texture3"].SetValue(tex);
            tex = Content.Load<Texture2D>("snow");
            mapeffect.Parameters["Texture4"].SetValue(tex);

            tex = Content.Load<Texture2D>("waves3");
            //mapeffect.Parameters["watertex"].SetValue(tex);
            mapeffect.Parameters["wellenhohe"].SetValue(Water.waveheight);
            mapeffect.Parameters["wellenbreite"].SetValue(Water.wavesize);

            mapeffect.Parameters["texanz"].SetValue(4);
            mapeffect.Parameters["AmbientIntensity"].SetValue(0.02f);
            mapeffect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
            mapeffect.Parameters["DiffuseIntensity"].SetValue(1f);
            mapeffect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector4());
            mapeffect.Parameters["SpecularColor"].SetValue(Color.White.ToVector4());
            mapeffect.Parameters["SpecularIntensity"].SetValue(0.05f);
            mapeffect.Parameters["Color"].SetValue(Color.White.ToVector4());
            mapeffect.Parameters["viewdeepness"].SetValue(Water.viewdeepness);
            mapeffect.Parameters["surfacetransparancy"].SetValue(Water.surfacetransparency);
            #endregion

            reflectionmap = new RenderTarget2D(device, Game1.Screenwidth, Game1.Screenheight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.PreserveContents);
            reflectiondepthmap = new RenderTarget2D(device, Game1.Screenwidth, Game1.Screenheight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.PreserveContents);
        }

        // Gets the amount of landscape of the height and grade of the terrain
        Vector4 getstaerke(float height, float grade)
        {
            Vector4 output = Vector4.Zero;
            float factor;
            // S A N D
            if (height < Settings.sand_maxheight)
            {
                output.X = Math.Min((Settings.sand_maxheight - height) / Settings.sand_intensity, 1);
            }
            // G R A S S
            if (height > Settings.grass_minheight && height < Settings.grass_maxheight)
            {
                factor = Game1.fade(Math.Min((height - Settings.grass_minheight) / Settings.grass_fadein, 1));
                factor *= Game1.fade(Math.Min((Settings.grass_maxheight - height) / Settings.grass_fadeout, 1));
                output *= 1 - factor;
                output.Y = factor;
            }
            // D I R T
            if (height > Settings.dirt_minheight && height < Settings.dirt_maxheight)
            {
                factor = Game1.fade(Math.Min((height - Settings.dirt_minheight) / Settings.dirt_fadein, 1) * (1 - ((float)Math.Pow(grade, 1.75f) * 0.9f + 0.1f)));
                factor *= Game1.fade(Math.Min((Settings.dirt_maxheight - height) / Settings.dirt_fadein, 1) * (1 - ((float)Math.Pow(grade, 1.75f) * 0.9f + 0.1f)));
                output *= 1 - factor;
                output.Z = factor;
            }
            // S N O W
            if (height > Settings.snow_minheight)
            {
                factor = Game1.fade(Math.Min((height - Settings.snow_minheight) / Settings.snow_intensity, 1) * (grade));
                output *= 1 - factor;
                output.W = factor;
            }
            return output;
        }
        public Vector3 get_normalatpos(int x, int y)
        {
            Vector3 gesvec = new Vector3(0, 1, 0);
            if (x - 1 >= 0)
            {
                gesvec.X -= (float)((heights[x, y] - heights[x - 1, y]) / scale);
            }
            if (y - 1 >= 0)
            {
                gesvec.Z -= (float)((heights[x, y] - heights[x, y - 1]) / scale);
            }
            if (x + 1 < (int)size)
            {
                gesvec.X += (float)((heights[x, y] - heights[x + 1, y]) / scale);
            }
            if (y + 1 < (int)size)
            {
                gesvec.Z += (float)((heights[x, y] - heights[x, y + 1]) / scale);
            }
            return Vector3.Normalize(gesvec);
        }
        public Vector4 get_texturestaerkenatpos(int x, int y)
        {
            float grade = get_normalatpos(x, y).Y;
            /*
             * X: Sand
             * Y: Grass
             * Z: Dirt
             * W: Snow
             */
            return getstaerke(heights[x, y], grade);
        }
        public bool loadmapsettings(int mappower, float Minh, float Maxh, Vector2 Mpos, float Scale, float Frequency, Perlin Perlin, int Seed, mapsettings Settings)
        {
            // returns false when size is smaller than one or the maxheight is smaller then the minheight 
            if (Math.Pow(2, mappower) <= 1 || Maxh <= Minh)
            {
                return false;
            }

            //Inizialising Variables
            this.Settings = Settings;
            size = (int)Math.Pow(2, mappower) + 1;
            scale = Scale;
            minh = Minh;
            maxh = Maxh;
            mpos = Mpos;
            this.seed = Seed;
            this.mappower = mappower;
            heights = new float[size, size];
            mpos = Mpos;
            float deepestheight = (float)(Game1.Map.minh + 0.05) * (Game1.Map.maxh - Game1.Map.minh) * Game1.Map.scale;
            water.watereffect.Parameters["deepestheight"].SetValue(deepestheight);
            return true;
        }

        public void generateheights()
        {
            loadingtype = "Generating Terrain";
            /*
             * 
             * G E N E R A T I N G   T E R R A I N
             * 
             * */
            // Inizialising Perlinshader
            float[] randfloats = new float[255];
            Random r = new Random(seed);
            for (int i = 0; i < 255; i++)
            {
                randfloats[i] = r.Next(0, 256);
            }
            Vector3[] grads = new Vector3[12];
            grads[0] = new Vector3(1, 1, 0);
            grads[1] = new Vector3(-1, 1, 0);
            grads[2] = new Vector3(1, -1, 0);
            grads[3] = new Vector3(-1, -1, 0);
            grads[4] = new Vector3(1, 0, 1);
            grads[5] = new Vector3(-1, 0, 1);
            grads[6] = new Vector3(1, 0, -1);
            grads[7] = new Vector3(-1, 0, -1);
            grads[8] = new Vector3(0, 1, 1);
            grads[9] = new Vector3(0, -1, 1);
            grads[10] = new Vector3(0, 1, -1);
            grads[11] = new Vector3(0, -1, -1);

            perlineffect.Parameters["grad3"].SetValue(grads);
            perlineffect.Parameters["p"].SetValue(randfloats);
            perlineffect.Parameters["worldsize"].SetValue(size);
            SpriteBatch batch = new SpriteBatch(device);

            // Calculating heigths with GPU
            if (size <= 4097) // Mapsize is small enough to render the heights in one texture
            {
                perlineffect.Parameters["texsize"].SetValue(new Vector2(size, size));
                Texture2D perlintex = new Texture2D(device, size, size, false, SurfaceFormat.Single);
                RenderTarget2D perlintarget = new RenderTarget2D(device, size, size, false, SurfaceFormat.Single, DepthFormat.Depth24);
                lock (device)
                {
                    device.SetRenderTarget(perlintarget);
                    batch.Begin(SpriteSortMode.Deferred, null, null, null, null, perlineffect, null);
                    batch.Draw(perlintex, Vector2.Zero, Color.White);
                    batch.End();
                    device.SetRenderTarget(null);
                }

                Single[] perlinheights = new Single[size * size];
                perlintarget.GetData<Single>(perlinheights);
                perlintarget.Dispose();
                perlintex.Dispose();
                float scalefactor = (maxh - minh) * scale;
                Parallel.For(0, size, i =>
                {
                    for (short j = 0; j < size; ++j)
                    {
                        heights[i, j] = (minh + perlinheights[i + j * (int)size]) * scalefactor;
                    }

                    loadingprogress = i / size;
                });
            }
            else // Mapsize to big, rendering heigths in multiple textures
            {
                Texture2D perlintex = new Texture2D(device, 4097, 4097, false, SurfaceFormat.Single);
                RenderTarget2D perlintarget = new RenderTarget2D(device, 4097, 4097, false, SurfaceFormat.Single, DepthFormat.Depth16);
                perlineffect.Parameters["texsize"].SetValue(new Vector2(4097));
                float[] perlinheights = new float[4097 * 4097];
                float mappow = (float)Math.Pow(2, mappower - 12);
                float scalefactor = (maxh - minh) * scale;
                for (int x = 0; x < (int)mappow; x++)
                {
                    for (int y = 0; y < (int)mappow; y++)
                    {
                        perlineffect.Parameters["rendercoos"].SetValue(new Vector2(x * 4096, y * 4096));
                        lock (device)
                        {
                            device.SetRenderTarget(perlintarget);
                            batch.Begin(SpriteSortMode.Deferred, null, null, null, null, perlineffect, Matrix.Identity);
                            batch.Draw(perlintex, Vector2.Zero, Color.White);
                            batch.End();
                            device.SetRenderTarget(null);
                        }

                        perlintarget.GetData<float>(perlinheights);

                        int x2 = x * 4096;
                        int y2 = y * 4096;
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        Parallel.For(0, 4097, j =>
                        {
                            int j2 = j * 4097;
                            int j2plusj = y2 + j;
                            for (short i = 0; i < 4097; ++i)
                            {
                                heights[x2 + i, j2plusj] = (minh + perlinheights[i + j2]) * scalefactor;
                                //heights[x2, y2] = ((int)((minh + perlinheights[i + j * 4097]) * (scalefactor / 100.0f))) * 100; //Minecraft Style
                            }

                            //loadingprogress = (j / 4079.0f) / (mappow * mappow) + (x * mappow + y) / (mappow * mappow);
                        });
                        loadingprogress = 1 / (mappow * mappow) + (x * mappow + y) / (mappow * mappow);
                        watch.Stop();
                    }
                }

                perlintarget.Dispose();
                perlintex.Dispose();
            }

            if (heights_rough != null && !heights_rough.IsDisposed)
                heights_rough.Dispose();
            heights_rough = new Texture2D(device, size / 8, size / 8, false, SurfaceFormat.Single);
            float[] DATA = new float[(size / 8) * (size / 8)];
            for (int x = 0; x < size / 8; ++x)
            {
                for (int y = 0; y < size / 8; ++y)
                {
                    DATA[x + y * (size / 8)] = heights[x * 8 + 4, y * 8 + 4];
                }
            }
            heights_rough.SetData(DATA);
            water.watereffect.Parameters["terrainheights"].SetValue(heights_rough);
            water.watereffect.Parameters["worldsize"].SetValue(size);
        }

        public bool generatetexweigths()
        {
            float[] stärke = new float[4];
            for (int x = 0; x < (int)size; x++)
            {
                for (int y = 0; y < (int)size; y++)
                {
                    int edgecounter = 0;
                    float istgrade = 0;
                    if (x - 1 >= 0)
                    {
                        float z = (float)Math.Atan(Math.Abs(heights[x, y] - heights[x - 1, y]) / scale);
                        if (!float.IsNaN(z))
                            istgrade += z;
                        edgecounter++;
                    }
                    if (y - 1 >= 0)
                    {
                        float z = (float)Math.Atan(Math.Abs(heights[x, y] - heights[x, y - 1]) / scale);
                        if (!float.IsNaN(z))
                            istgrade += z;
                        edgecounter++;
                    }
                    if (x + 1 < (int)size)
                    {
                        float z = (float)Math.Atan(Math.Abs(heights[x, y] - heights[x + 1, y]) / scale);
                        if (!float.IsNaN(z))
                            istgrade += z;
                        edgecounter++;
                    }
                    if (y + 1 < (int)size)
                    {
                        float z = (float)Math.Atan(Math.Abs(heights[x, y] - heights[x, y + 1]) / scale);
                        if (!float.IsNaN(z))
                            istgrade += z;
                        edgecounter++;
                    }
                    istgrade = istgrade / edgecounter;
                    for (int i = 0; i < 4; i++)
                    {

                        //texweights[x, y, i] = (getstaerke(ms.minheight[i], ms.maxheight[i], ms.mingrade[i], ms.maxgrade[i], ms.grademinwert[i], ms.grademaxwert[i], istgrade, heights[x, y]));

                    }
                }
            }

            return true;
        }
        public bool loadheightsfromfile(string filename)
        {
            try
            {
                byte[] size_byte = new byte[4];
                // Reading the Size of the Map
                BinaryReader file = new BinaryReader(File.Open(filename, FileMode.Open), Encoding.Default, false);
                file.Read(size_byte, 0, 4);
                size = BitConverter.ToInt32(size_byte, 0);
                // Break if the mapsize is zero
                if (size == 0)
                    return false;
                //Reading the heights of the map
                byte[] heightbytearray = new byte[(int)(size * size * 4)];
                file.Read(heightbytearray, 0, heightbytearray.Length);
                Console.WriteLine("{0}", size);
                heights = Game1.toRectangular<float>(Game1.ConvertByteToFloat(heightbytearray), (int)size);
                for (int x = 0; x < (int)size; x++)
                {
                    for (int y = 0; y < (int)size; y++)
                    {
                        float x2, y2;
                        x2 = mpos.X + (x / (size - 1)) * size;
                        y2 = mpos.Y + (y / (size - 1)) * size;
                        //pos[x, y] = new Vector3(x2, heights[x, y], y2);
                    }
                }
                Console.WriteLine("Loading suceeded. Filename: {0}", filename);

            }
            catch (Exception exp)
            {
                Console.WriteLine("Loading failed: {0}", exp);
                return false;
            }
            return true;
        }
        public bool saveheightstofile(string filename)
        {
            try
            {
                BinaryReader file = new BinaryReader(File.Open(filename, FileMode.Create), Encoding.Default, false);
                byte[] heightbytearray = new byte[heights.Length * 4];
                Buffer.BlockCopy(heights, 0, heightbytearray, 0, heightbytearray.Length);
                byte[] size_byte;
                // Writing mapsize
                size_byte = BitConverter.GetBytes(size);
                file.BaseStream.Write(size_byte, 0, 4);
                //Writing heights
                file.BaseStream.Write(heightbytearray, 0, heightbytearray.Length);
                Console.WriteLine("Saving suceeded. Filename: {0}", filename);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Saving failed: {0}", exp);
                return false;
            }
            return true;
        }
        public bool generatefirstchunk()
        {
            mainchunk = new mapchunk(device, mpos * scale, mappower - 3, (size / (chunksize - 1)) * scale, 0, 0);
            mainchunk.generatevertexbuffer(this, Game1.camerapos);
            IsLoaded = true;
            return true;
        }

        #region Update
        public void Update(long ticks)
        {
            Smallestdistancetomap = 1000000;
            mainchunk.Update(Game1.camerapos, this);
            if (Game1.camerafreeze == false)
            {
                water.Update(ticks);
            }
        }
        public void UpdateMatrixes(Matrix View, Matrix World, Matrix Projection)
        {
            this.View = View;
            this.World = World;
            this.Projection = Projection;
        }
        public void UpdateView(Matrix View)
        {
            this.View = View;
        }
        #endregion

        #region Draw
        public void Draw(Matrix matrix, bool renderreflections)
        {
            this.reflectionrendering = renderreflections;
            Frustum.Matrix = View * Projection;
            mapeffect.Parameters["reflectionrendering"].SetValue(renderreflections);
            mapeffect.Parameters["waveverschiebung"].SetValue(water.waveoffset);

            #region OLD LIGHT CALULATIONS
            /*Vector3 richtung = -Game1.ligthdirection; // Direction to sun
            float airdensity = (1 - richtung.Y); //Density of air to the pixel of the sky
            float blauint = (-Game1.ligthdirection.Y) + (1 - airdensity) * 0.05f;
            float sundot = (Vector3.Dot(-Game1.ligthdirection, richtung) + 1) / 2.0f;

            blauint = ((blauint / 1.4f) + 0.4f) * 0.5f;

            // Makes the sky dark when the sun goes down
            float verdunkelung = ((-Game1.ligthdirection.Y) + 1) / 2.0f;

            float sonne = (float)Math.Pow(sundot + 0.0005f, 10000);
            float luftpow = (1 / (float)Math.Pow((float)Math.Pow(airdensity, 2), 4));
            float orangeint = sonne + (float)Math.Pow(sundot * (1 + 0.002f * (float)Math.Pow(airdensity, 2)), luftpow) * verdunkelung * 1.5f + (float)Math.Pow(sundot + 0.0005f, 500) * 0.25f;

            Vector4 orange = new Vector4(1, 0.6f, 0.05f, 1); // Color of the sun
            Vector4 blue = new Vector4(0.25f, 0.52f, 1, 1); // Color of the sky at day
            blauint = (1 - (float)Math.Pow(1 - blauint, 2));
            blauint = MathHelper.Clamp(blauint, 0, 1);
            orangeint = MathHelper.Clamp(orangeint, 0, 1);
            Vector4 output = orange * orangeint + blue * blauint * blauint; // Combining the blue and orange Color
            output.W = 1;
            output.Normalize();
            float intensity = 1 - (float)Math.Pow(Math.Max(1 + ((Game1.ligthdirection.Y - 0.9f)/2.0f), 0), 2);
            intensity = MathHelper.Clamp(intensity, 0, 1);*/
            #endregion

            // Calculating Color of the Map
            float maporangeintensity = 1 - MathHelper.Clamp(-Game1.ligthdirection.Y, 0, 1);
            Vector4 mapcolor = new Vector4(1, 0.8f, 0.55f, 1) * maporangeintensity + new Vector4(1, 1, 1, 1) * (1 - maporangeintensity);

            // Calculating Color of Reflections and sun
            float orangeintensity = 1 - (float)Math.Pow(1 - (-Game1.ligthdirection.Y + 1) / 2.0f, 0.7f);
            Game1.lightcolor = Vector4.One * orangeintensity + new Vector4(1, 0.7f, 0.3f, 1) * (1 - orangeintensity);

            // Calculating Intensity of Light
            float lightintensity = 1 - (float)Math.Pow(1 - (-Game1.ligthdirection.Y + 0.75f) / 1.75f, 1.25f);

            mapeffect.Parameters["AmbientIntensity"].SetValue(0.015f);
            mapeffect.Parameters["DiffuseIntensity"].SetValue(lightintensity);
            mapeffect.Parameters["SpecularIntensity"].SetValue(lightintensity * 0.1f);
            mapeffect.Parameters["LightDirection"].SetValue(Game1.ligthdirection);
            mapeffect.Parameters["DiffuseColor"].SetValue(mapcolor);
            mapeffect.Parameters["AmbientColor"].SetValue(Vector4.One);
            mapeffect.Parameters["SpecularColor"].SetValue(mapcolor);

            // Setting Indexbuffer to the Map Index Buffer
            device.Indices = ichunkbuffer;
            mainchunk.Draw();
        }
        public void DrawWater(BasicEffect effect)
        {
            water.watereffect.Parameters["lightcolor"].SetValue(Game1.lightcolor);
            water.Draw(View, World, Projection, Matrix.Identity);
        }
        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {

                    mainchunk?.Dispose();
                    batch?.Dispose();
                    ichunkbuffer?.Dispose();
                    reflectionmap?.Dispose();
                    water?.Dispose();
                }
            }
            //dispose unmanaged resources
            IsDisposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
