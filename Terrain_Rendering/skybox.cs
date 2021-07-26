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
    public class skybox : IDisposable
    {
        public bool Disposed;
        private GraphicsDevice device;

        public VertexBuffer vbuffer;
        public Effect shader;
        public skybox(ContentManager manager, GraphicsDevice device)
        {
            Disposed = false;
            this.device = device;
            shader = manager.Load<Effect>("skyboxshader");
            List<VertexPositionColorNormal_noTexCoo> vertexes = new List<VertexPositionColorNormal_noTexCoo>();

            //Inizialising Corner Positions of Skybox Cube
            Vector3 pos1 = new Vector3(-0.5f, -0.5f, -0.5f);
            Vector3 pos2 = new Vector3(-0.5f, -0.5f, 0.5f);
            Vector3 pos3 = new Vector3(0.5f, -0.5f, 0.5f);
            Vector3 pos4 = new Vector3(0.5f, -0.5f, -0.5f);

            Vector3 pos5 = new Vector3(-0.5f, 0.5f, -0.5f);
            Vector3 pos6 = new Vector3(-0.5f, 0.5f, 0.5f);
            Vector3 pos7 = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 pos8 = new Vector3(0.5f, 0.5f, -0.5f);
            Color color = Color.Red;

            //Adding Vertexes of Skybox Cube
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos1, new Vector3(1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos2, new Vector3(1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos6, new Vector3(1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos1, new Vector3(1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos6, new Vector3(1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos5, new Vector3(1, 0, 0), color));
            color = Color.Blue;
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos2, new Vector3(0, 0, -1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos3, new Vector3(0, 0, -1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos7, new Vector3(0, 0, -1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos2, new Vector3(0, 0, -1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos7, new Vector3(0, 0, -1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos6, new Vector3(0, 0, -1), color));
            color = Color.Green;
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos3, new Vector3(-1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos4, new Vector3(-1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos8, new Vector3(-1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos3, new Vector3(-1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos8, new Vector3(-1, 0, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos7, new Vector3(-1, 0, 0), color));
            color = Color.White;
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos1, new Vector3(0, 0, 1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos5, new Vector3(0, 0, 1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos8, new Vector3(0, 0, 1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos1, new Vector3(0, 0, 1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos8, new Vector3(0, 0, 1), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos4, new Vector3(0, 0, 1), color));
            color = Color.Black;
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos7, new Vector3(0, -1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos5, new Vector3(0, -1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos6, new Vector3(0, -1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos5, new Vector3(0, -1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos7, new Vector3(0, -1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos8, new Vector3(0, -1, 0), color));
            color = Color.Orange;
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos1, new Vector3(0, 1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos3, new Vector3(0, 1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos2, new Vector3(0, 1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos3, new Vector3(0, 1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos1, new Vector3(0, 1, 0), color));
            vertexes.Add(new VertexPositionColorNormal_noTexCoo(pos4, new Vector3(0, 1, 0), color));

            //Inizialising VertexBuffer
            vbuffer = new VertexBuffer(device, VertexPositionColorNormal_noTexCoo.VertexDeclaration, vertexes.Count, BufferUsage.WriteOnly);
            vbuffer.SetData(vertexes.ToArray());
        }


        public void Draw(Matrix world, Matrix view, Matrix matrix, Vector3 EyePosition)
        {
            shader.Parameters["World"].SetValue(world * matrix);
            shader.Parameters["View"].SetValue(view);
            shader.Parameters["Projection"].SetValue(Game1.cameraeffect.Projection);
            shader.Parameters["EyePosition"].SetValue(EyePosition);
            shader.Parameters["LightDirection"].SetValue(Game1.ligthdirection);

            shader.CurrentTechnique.Passes[0].Apply();
            device.SetVertexBuffer(vbuffer);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, vbuffer.VertexCount / 3);
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (!vbuffer.IsDisposed)
                        vbuffer.Dispose();
                    if (!shader.IsDisposed)
                        shader.Dispose();
                }
            }
            //dispose unmanaged resources
            Disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
