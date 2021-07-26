using System;
using System.IO;
using System.Collections.Generic;
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
    public class Water : IDisposable
    {
        public bool IsDisposed;
        private GraphicsDevice device;
        private VertexBuffer vbufferwaves;
        private IndexBuffer ibufferwaves;
        private Random r;
        public Effect watereffect;
        public Texture2D watertex;
        public RenderTarget2D reflectionmap, normalmap, normalmap2;
        public float waveoffset, waveoffset2;
        public static float viewdeepness = 5.5f, surfacetransparency = 0.15f, waveheight = 0.75f, wavesize = 0.075f;


        public Water(GraphicsDevice device, ContentManager content)
        {
            IsDisposed = false;
            this.device = device;
            r = new Random();
            watertex = content.Load<Texture2D>("waves3");

            #region Loading and Inizialising Water Shader
            watereffect = content.Load<Effect>("watershader3");
            watereffect.Parameters["watertex"].SetValue(watertex);
            watereffect.Parameters["waveheight"].SetValue(waveheight);
            watereffect.Parameters["wellenbreite"].SetValue(wavesize);

            watereffect.Parameters["wellenbreite2"].SetValue(0.075f);

            watereffect.Parameters["wavemeshsizeX"].SetValue(Game1.settings.water_meshsize);
            watereffect.Parameters["wavemeshsizeY"].SetValue(Game1.settings.water_meshsize);
            watereffect.Parameters["viewdeepness"].SetValue(viewdeepness);
            watereffect.Parameters["Screenwidth"].SetValue(Game1.Screenwidth);
            watereffect.Parameters["Screenheight"].SetValue(Game1.Screenheight);
            waveoffset = 0;
            #endregion

            #region generating normalmap
            normalmap = new RenderTarget2D(device, watertex.Width, watertex.Height, false, SurfaceFormat.Color, DepthFormat.None);
            normalmap2 = new RenderTarget2D(device, watertex.Width, watertex.Height, false, SurfaceFormat.Color, DepthFormat.None);
            SpriteBatch batch = new SpriteBatch(device);
            Effect heighttonormal = content.Load<Effect>("heighttonormalmap");

            heighttonormal.Parameters["waveheight"].SetValue(waveheight);
            heighttonormal.Parameters["wavesize"].SetValue(wavesize);
            heighttonormal.Parameters["texsizex"].SetValue(watertex.Width);
            heighttonormal.Parameters["texsizey"].SetValue(watertex.Height);
            device.SetRenderTarget(normalmap);
            batch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, heighttonormal, Matrix.Identity);
            batch.Draw(watertex, Vector2.Zero, Color.White);
            batch.End();

            heighttonormal.Parameters["waveheight"].SetValue(0.125f);
            heighttonormal.Parameters["wavesize"].SetValue(0.0075f);
            heighttonormal.Parameters["texsizex"].SetValue(watertex.Width);
            heighttonormal.Parameters["texsizey"].SetValue(watertex.Height);
            device.SetRenderTarget(normalmap2);
            batch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, heighttonormal, Matrix.Identity);
            batch.Draw(watertex, Vector2.Zero, Color.White);
            batch.End();

            device.SetRenderTarget(null);
            #endregion
            //Stream stream = File.Create("wavenormals.png");
            //normalmap.SaveAsPng(stream, normalmap.Width, normalmap.Height);
            //stream.Close();
            // Giving the shader the normalmap

            //watereffect.Parameters["normalmap"].SetValue(content.Load<Texture2D>("wavenormals"));
            //watereffect.Parameters["normalmap2"].SetValue(content.Load<Texture2D>("wavenormals2"));

            watereffect.Parameters["normalmap"].SetValue(normalmap);
            //watereffect.Parameters["normalmap2"].SetValue(normalmap2);
            // Generating Water Mesh
            generatewatermesh();
        }
        public void generatewatermesh()
        {
            List<VertexPositionColorNormal_noTexCoo> vertexes = new List<VertexPositionColorNormal_noTexCoo>();
            // Color and Brightness of Water
            float breightness = 0.6f;
            Color watercolor = new Color((1 / 255.0f) * breightness, (60 / 255.0f) * breightness, (150 / 255.0f) * breightness, 1f);
            for (int x = 0; x < Game1.settings.water_meshsize; x++)
            {
                for (int y = 0; y < Game1.settings.water_meshsize; y++)
                {
                    //vertexes.Add(new VertexPositionColorNormal_noTexCoo(new Vector3(x, 0, y), Vector3.Zero, new Color(r.Next(0,255), r.Next(0, 255), r.Next(0, 255))));
                    vertexes.Add(new VertexPositionColorNormal_noTexCoo(new Vector3(x, 0, y), Vector3.Zero, watercolor));
                }
            }
            // Generating Vertex Buffer
            vbufferwaves = new VertexBuffer(device, VertexPositionColorNormal_noTexCoo.VertexDeclaration, vertexes.Count, BufferUsage.WriteOnly);
            vbufferwaves.SetData(vertexes.ToArray());

            List<int> indicis = new List<int>();
            for (int x = 0; x < Game1.settings.water_meshsize - 1; x++)
            {
                for (int y = 0; y < Game1.settings.water_meshsize - 1; y++)
                {
                    indicis.Add((int)(x + y * Game1.settings.water_meshsize));
                    indicis.Add((int)(x + 1 + y * Game1.settings.water_meshsize));
                    indicis.Add((int)(x + 1 + (y + 1) * Game1.settings.water_meshsize));

                    indicis.Add((int)(x + y * Game1.settings.water_meshsize));
                    indicis.Add((int)(x + 1 + (y + 1) * Game1.settings.water_meshsize));
                    indicis.Add((int)(x + (y + 1) * Game1.settings.water_meshsize));
                }
            }
            // Generating Index Buffer
            ibufferwaves = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indicis.Count, BufferUsage.WriteOnly);
            ibufferwaves.SetData(indicis.ToArray());
        }
        private static Vector3 Unproject(Vector2 mousepos, GraphicsDevice device, int tiefe)
        {
            Matrix viewmatrix = Matrix.CreateLookAt(Vector3.Zero, Game1.camerarichtung - Game1.camerapos, Vector3.Up);
            Vector3 vec1 = device.Viewport.Unproject(new Vector3(mousepos.X, mousepos.Y, 0), Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(85), device.Viewport.AspectRatio, 10, 10000), viewmatrix, Game1.cameraeffect.World);
            Vector3 vec2 = -device.Viewport.Unproject(new Vector3(mousepos.X, mousepos.Y, 1.000015f), Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(85), device.Viewport.AspectRatio, 10, 10000), viewmatrix, Matrix.Identity);
            Vector3 vecrichtung = -Vector3.Normalize(vec2 - vec1);
            return Game1.camerapos - vec1 - vecrichtung * tiefe;
        }
        public void Update(long ticks)
        {
            // Moving Water
            waveoffset += (float)(0.15 * ticks / (TimeSpan.TicksPerSecond / 144.0));// * 0.02f;
            waveoffset2 += (float)(0.4 * ticks / (TimeSpan.TicksPerSecond / 144.0));// * 0.02f;

            // Changing Projection so that the edge of the watermesh is not going inside the view.
            float screenposoffset = (1 / ((Math.Abs(Game1.camerapos.Y) / 100000.0f) + 0.00015f));
            Vector3 leftup = -Vector3.Normalize(Unproject(new Vector2(-screenposoffset / 20, 0), device, 100) - Game1.camerapos);
            Vector3 leftdown = -Vector3.Normalize(Unproject(new Vector2(-screenposoffset / 20, Game1.Screenheight + screenposoffset), device, 100) - Game1.camerapos);
            Vector3 rightup = -Vector3.Normalize(Unproject(new Vector2(Game1.Screenwidth + screenposoffset / 20, 0), device, 100) - Game1.camerapos);
            Vector3 rightdown = -Vector3.Normalize(Unproject(new Vector2(Game1.Screenwidth + screenposoffset / 20, Game1.Screenheight + screenposoffset), device, 100) - Game1.camerapos);
            // Giving the Shader the Screen vectors
            watereffect.Parameters["leftup"].SetValue(leftup);
            watereffect.Parameters["leftdown"].SetValue(leftdown);
            watereffect.Parameters["rightup"].SetValue(rightup);
            watereffect.Parameters["rightdown"].SetValue(rightdown);

            //Updating Smallest Distance
            Game1.Map.Smallestdistancetomap = Math.Min(Game1.Map.Smallestdistancetomap, Math.Abs(Game1.camerapos.Y) - waveheight / 2.0f);
        }
        public void Draw(Matrix View, Matrix World, Matrix Projection, Matrix matrix)
        {
            watereffect.Parameters["World"].SetValue(World * matrix);
            watereffect.Parameters["View"].SetValue(View);
            watereffect.Parameters["Projection"].SetValue(Projection);
            watereffect.Parameters["EyePosition"].SetValue(Game1.camerapos);
            watereffect.Parameters["LightDirection"].SetValue(Game1.ligthdirection);
            watereffect.Parameters["waveverschiebung"].SetValue(waveoffset);
            watereffect.Parameters["waveverschiebung2"].SetValue(waveoffset2);

            watereffect.Parameters["maptex"].SetValue((Texture2D)Game1.maptarget);
            watereffect.Parameters["depthtex"].SetValue((Texture2D)Game1.heighttarget);
            //watereffect.Parameters["reflectionmap"].SetValue((Texture2D)Game1.Map.reflectionmap);
            //watereffect.Parameters["reflectiondepthmap"].SetValue((Texture2D)Game1.Map.reflectiondepthmap);

            watereffect.CurrentTechnique.Passes[0].Apply();
            device.SetVertexBuffer(vbufferwaves);
            device.Indices = ibufferwaves;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ibufferwaves.IndexCount);
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (ibufferwaves != null && !ibufferwaves.IsDisposed)
                        ibufferwaves.Dispose();
                    if (normalmap != null && !normalmap.IsDisposed)
                        normalmap.Dispose();
                    if (reflectionmap != null && !reflectionmap.IsDisposed)
                        reflectionmap.Dispose();
                    if (watereffect != null && !watereffect.IsDisposed)
                        watereffect.Dispose();
                    if (vbufferwaves != null && !vbufferwaves.IsDisposed)
                        vbufferwaves.Dispose();
                    if (watertex != null && !watertex.IsDisposed)
                        watertex.Dispose();
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
