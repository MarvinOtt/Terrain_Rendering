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
    public class mapchunk : IDisposable
    {
        public bool IsDisposed;
        private GraphicsDevice device;

        private static mapvertexdecleration[] vertexes = new mapvertexdecleration[Game1.Map.chunksize * Game1.Map.chunksize];
        private static BoundingSphere sphere;
        private VertexBuffer vbuffer;
        public Vector3 mittelpunkt;
        private Vector2 pos;
        private int tiefe;
        private float maxheight;
        private bool generated_innerchunks, vertexes_generated;
        private int xid, yid;
        private float punktabstand;
        private int tiefepow;
        private float camtochunklength;
        private float maxradius;
        private bool IsUnderwater;
        private mapchunk[,] innerchunks;

        public mapchunk(GraphicsDevice device, Vector2 pos, int tiefe, float punktabstand, int xid, int yid)
        {
            IsDisposed = false;
            this.pos = pos;
            this.tiefe = tiefe;
            this.device = device;
            this.punktabstand = punktabstand;
            this.xid = xid;
            this.yid = yid;
            innerchunks = new mapchunk[2, 2];
            generated_innerchunks = false;
            vertexes_generated = false;
            maxradius = 0;

            tiefepow = (int)Math.Pow(2, tiefe - 2);
        }

        public bool generatevertexbuffer(map map, Vector3 campos)
        {
            //List<VertexPositionNormaltexCoo_weights> vertexes = new List<VertexPositionNormaltexCoo_weights>();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            IsUnderwater = true;
            int xidpunkt1 = xid + (map.chunksize / 2) * (int)Math.Pow(2, tiefe - 2);
            int yidpunkt1 = yid + (map.chunksize / 2) * (int)Math.Pow(2, tiefe - 2);
            mittelpunkt = new Vector3(pos.X, 0, pos.Y) + new Vector3(punktabstand * map.chunksize / 2, map.heights[xidpunkt1, yidpunkt1], punktabstand * map.chunksize / 2);
            float camtochunklength = (campos - mittelpunkt).Length();
            for (int y = 0; y < map.chunksize; y++)
            {
                for (int x = 0; x < map.chunksize; x++)
                {
                    xidpunkt1 = xid + x * tiefepow;
                    yidpunkt1 = yid + y * tiefepow;
                    vertexes[x + y * map.chunksize] = new mapvertexdecleration(new Vector3(pos.X, 0, pos.Y) + new Vector3(punktabstand * x, map.heights[xidpunkt1, yidpunkt1], punktabstand * y), map.get_normalatpos(xidpunkt1, yidpunkt1), map.get_texturestaerkenatpos(xidpunkt1, yidpunkt1));
                    // Get Max Radius of chunk for perfect frustum-Culling
                    float mittelpunkttopos = (mittelpunkt - vertexes[x + y * map.chunksize].Position).Length();
                    if (mittelpunkttopos > maxradius)
                        maxradius = mittelpunkttopos;
                    if (vertexes[x + y * map.chunksize].Position.Y > -Water.viewdeepness)
                        IsUnderwater = false;


                }
            }
            // Creating Vertex Buffer
            vbuffer = new VertexBuffer(device, mapvertexdecleration.VertexDeclaration, vertexes.Length, BufferUsage.WriteOnly);
            vbuffer.SetData(vertexes);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            vertexes_generated = true;
            return true;
        }
        public int Update(Vector3 campos, map map)
        {
            camtochunklength = (campos - mittelpunkt).Length();
            if (generated_innerchunks == false)
            {
                if (camtochunklength - maxradius < map.Smallestdistancetomap)
                    map.Smallestdistancetomap = camtochunklength - maxradius;
                if ((camtochunklength < ((60f * Game1.settings.map_drawquality) * ((map.scale)) * (float)Math.Pow(2, tiefe - 1)) && tiefe > 2))
                {
                    innerchunks[0, 0] = new mapchunk(device, pos, tiefe - 1, punktabstand / 2, xid, yid);
                    innerchunks[0, 0].generatevertexbuffer(map, campos);
                    innerchunks[1, 0] = new mapchunk(device, pos + new Vector2(((punktabstand * (map.chunksize - 1)) / 2.0f), 0), tiefe - 1, punktabstand / 2, xid + (int)((map.chunksize / 2.0f) * tiefepow - tiefepow / 2), yid);
                    innerchunks[1, 0].generatevertexbuffer(map, campos);
                    innerchunks[0, 1] = new mapchunk(device, pos + new Vector2(0, ((punktabstand * (map.chunksize - 1)) / 2.0f)), tiefe - 1, punktabstand / 2, xid, yid + (int)((map.chunksize / 2.0f) * tiefepow - tiefepow / 2));
                    innerchunks[0, 1].generatevertexbuffer(map, campos);
                    innerchunks[1, 1] = new mapchunk(device, pos + new Vector2(((punktabstand * (map.chunksize - 1)) / 2.0f), ((punktabstand * (map.chunksize - 1)) / 2.0f)), tiefe - 1, punktabstand / 2, xid + (int)((map.chunksize / 2.0f) * tiefepow - tiefepow / 2), yid + (int)((map.chunksize / 2.0f) * tiefepow - tiefepow / 2));
                    innerchunks[1, 1].generatevertexbuffer(map, campos);

                    generated_innerchunks = true;
                }
                else if ((camtochunklength > ((60 * 1.05f * Game1.settings.map_drawquality) * ((map.scale)) * (float)Math.Pow(2, tiefe)) && tiefe < map.mappower - 2))
                {
                    return 1;
                }
            }
            else if (Game1.camerafreeze == false)
            {
                int zuweitweg = 0;
                zuweitweg += innerchunks[0, 0].Update(campos, map);
                zuweitweg += innerchunks[1, 0].Update(campos, map);
                zuweitweg += innerchunks[0, 1].Update(campos, map);
                zuweitweg += innerchunks[1, 1].Update(campos, map);

                if (zuweitweg == 4)
                {
                    generated_innerchunks = false;
                    if (innerchunks[0, 0] != null && !innerchunks[0, 0].IsDisposed)
                        innerchunks[0, 0].Dispose();
                    if (innerchunks[0, 1] != null && !innerchunks[0, 1].IsDisposed)
                        innerchunks[0, 1].Dispose();
                    if (innerchunks[1, 0] != null && !innerchunks[1, 0].IsDisposed)
                        innerchunks[1, 0].Dispose();
                    if (innerchunks[1, 1] != null && !innerchunks[1, 1].IsDisposed)
                        innerchunks[1, 1].Dispose();
                    if (vertexes_generated == false)
                        generatevertexbuffer(map, campos);
                }
            }

            return 0;
        }
        public void Draw()
        {
            // Drawing Reflections with selected quality
            if (Game1.Map.reflectionrendering && vertexes_generated == true && (camtochunklength > ((60f * (float)Math.Pow(Game1.settings.water_ref_quality, 2)) * (Game1.Map.scale) * (float)Math.Pow(2, tiefe - 1)) || generated_innerchunks == false))
            {
                sphere.Center = mittelpunkt;
                sphere.Radius = maxradius;
                if (Game1.Map.Frustum.Intersects(sphere))
                {
                    Game1.Map.mapeffect.Parameters["World"].SetValue(Game1.Map.World);
                    Game1.Map.mapeffect.Parameters["View"].SetValue(Game1.Map.View);
                    Game1.Map.mapeffect.Parameters["Projection"].SetValue(Game1.Map.Projection);
                    Game1.Map.mapeffect.Parameters["EyePosition"].SetValue(Game1.camerapos);

                    Game1.Map.mapeffect.CurrentTechnique.Passes[0].Apply();
                    device.SetVertexBuffer(vbuffer);
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Game1.Map.ichunkbuffer.IndexCount);

                    Game1.Map.chunkdrawanz += 1;
                }
            }
            // Drawing Map because no inner chunks are generated
            else if (generated_innerchunks == false && vertexes_generated == true && Game1.Map.reflectionrendering == false)
            {
                if ((mittelpunkt - Game1.campos2).Length() < Game1.settings.map_drawdistance)
                {
                    sphere.Center = mittelpunkt;
                    sphere.Radius = maxradius;
                    /*Vector3 camtochunkrichtung = mittelpunkt - Game1.campos2;
                    double dotproduct = Vector3.Dot(Vector3.Normalize(Game1.camrichtung2 - Game1.campos2), Vector3.Normalize(camtochunkrichtung));
                    if (dotproduct < -1)
                        dotproduct = -1;
                    if (dotproduct > 1)
                        dotproduct = 1;
                    float angle = (float)Math.Acos(dotproduct);

                    if (angle < Math.PI / 2.6f)*/
                    if (Game1.Map.Frustum.Intersects(sphere))
                    {
                        Game1.Map.mapeffect.Parameters["World"].SetValue(Game1.Map.World);
                        Game1.Map.mapeffect.Parameters["View"].SetValue(Game1.Map.View);
                        Game1.Map.mapeffect.Parameters["Projection"].SetValue(Game1.Map.Projection);
                        Game1.Map.mapeffect.Parameters["EyePosition"].SetValue(Game1.camerapos);

                        Game1.Map.mapeffect.CurrentTechnique.Passes[0].Apply();
                        device.SetVertexBuffer(vbuffer);
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Game1.Map.ichunkbuffer.IndexCount);

                        Game1.Map.chunkdrawanz += 1;
                    }
                }

            }
            //Drawing inner chunks
            else if (generated_innerchunks)// && IsUnderwater == false)
            {
                //Drawing more detailed chunks
                innerchunks[0, 0].Draw();
                innerchunks[1, 0].Draw();
                innerchunks[0, 1].Draw();
                innerchunks[1, 1].Draw();
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (vbuffer != null && vbuffer.IsDisposed == false)
                        vbuffer.Dispose();
                    if (innerchunks[0, 0] != null && !innerchunks[0, 0].IsDisposed)
                        innerchunks[0, 0].Dispose();
                    if (innerchunks[0, 1] != null && !innerchunks[0, 1].IsDisposed)
                        innerchunks[0, 1].Dispose();
                    if (innerchunks[1, 0] != null && !innerchunks[1, 0].IsDisposed)
                        innerchunks[1, 0].Dispose();
                    if (innerchunks[1, 1] != null && !innerchunks[1, 1].IsDisposed)
                        innerchunks[1, 1].Dispose();
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
    }
}