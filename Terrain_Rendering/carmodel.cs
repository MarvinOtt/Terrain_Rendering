using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain_Rendering
{
    /// <summary>
    /// This is the Model Class
    /// </summary>
    public class carmodel
    {
        public Model smodel, rmodel;
        GraphicsDevice graphicsdevice;
        public Matrix[] Soriginalmatrix, Roriginalmatrix;
        public List<Game1.punkt> punktelist = new List<Game1.punkt>();
        public List<Game1.verbindung> verbindungenlist = new List<Game1.verbindung>();
        public Game1.punkt[] punkte;
        public Game1.verbindung[] verbindungen;
        public carmodel(GraphicsDevice GD)
        {
            smodel = Game1.spheremodel;
            rmodel = Game1.ringmodel;
            graphicsdevice = GD;
            Soriginalmatrix = new Matrix[smodel.Bones.Count];
            for (int i = 0; i < smodel.Bones.Count; i++)
            {
                Soriginalmatrix[i] = smodel.Bones[i].Transform;
            }
            Roriginalmatrix = new Matrix[rmodel.Bones.Count];
            for (int i = 0; i < rmodel.Bones.Count; i++)
            {
                Roriginalmatrix[i] = rmodel.Bones[i].Transform;
            }
        }
        public void UpdateArray()
        {
            punkte = punktelist.ToArray();
            verbindungen = verbindungenlist.ToArray();
        }
        public void UpdateList()
        {
            punktelist = punkte.ToList();
            verbindungenlist = verbindungen.ToList();
        }
        public void Draw(BasicEffect effect)
        {
            for (int i = 0; i < punkte.Length; i++)
            {
                effect.LightingEnabled = true;
                effect.EmissiveColor = Color.Black.ToVector3();
                Game1.MeshMatrix(ref smodel, Soriginalmatrix[0], 0, Matrix.CreateTranslation(punkte[i].pos));
                Game1.DrawMesh(effect, smodel, Game1.Shader);
                if (punkte[i].aktiviert == 1)
                {
                    Vector3 target = Vector3.Normalize(Game1.camerapos - punkte[i].pos);
                    target = new Vector3(-target.X, target.Y, -target.Z);
                    float angle = (float)Math.Acos(Vector3.Dot(Vector3.Up, target));
                    Vector3 axis = Vector3.Normalize(Vector3.Cross(target, Vector3.Up));
                    effect.LightingEnabled = false;
                    effect.EmissiveColor = Color.White.ToVector3();
                    Game1.MeshMatrix(ref rmodel, Roriginalmatrix[0], 0, Matrix.CreateFromAxisAngle(axis, angle) * Matrix.CreateTranslation(punkte[i].pos));
                    Game1.DrawMesh(effect, rmodel);
                }
            }
            for (int i = 0; i < verbindungen.Length; i++)
            {
                Game1.DrawLine3d(graphicsdevice, effect, punkte[verbindungen[i].punkt1].pos, punkte[verbindungen[i].punkt2].pos, Color.White, Color.Black);
            }
        }
        public int mausaufpunkt(BasicEffect effect, Vector2 pos)
        {
            Vector3 nearsource = new Vector3(pos.X, pos.Y, 0);
            Vector3 farsource = new Vector3(pos.X, pos.Y, 1);
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            float nächstedist = -5;
            int nächsteID = -5;
            Vector3 nearPoint = graphicsdevice.Viewport.Unproject(nearsource, effect.Projection, effect.View, world);

            Vector3 farPoint = graphicsdevice.Viewport.Unproject(farsource, effect.Projection, effect.View, world);
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            for (int i = 0; i < punkte.Length; i++)
            {
                punkte[i].aktiviert = 0;
                BoundingSphere sphere = new BoundingSphere(punkte[i].pos, 10);
                Ray pickRay = new Ray(nearPoint, direction);
                var x = pickRay.Intersects(sphere);
                if (x.HasValue)
                {
                    if (nächstedist == -5 || x < nächstedist)
                    {
                        nächstedist = x.GetValueOrDefault();
                        nächsteID = i;
                    }
                }
            }
            if (nächsteID != -5)
            {
                punkte[nächsteID].aktiviert = 1;
                return nächsteID;
            }
            return -1;
        }
    }
}
