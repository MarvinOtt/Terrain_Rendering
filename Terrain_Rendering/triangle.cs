using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Terrain_Rendering
{
    public class triangle
    {
        Vector3 pos1, pos2, pos3;
        public Vector3 normal;
        //float grade;
        public triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            pos1 = p1;
            pos2 = p2;
            pos3 = p3;
            Vector3 p21, p23;
            p21 = p1 - p2;
            p23 = p3 - p2;
            normal = Vector3.Normalize(Vector3.Cross(p21, p23));
            //grade = normal.Y;
        }
        public triangle(Vector3 p1, Vector3 p2, Vector3 p3, int nvr)
        {
            pos1 = p1;
            pos2 = p2;
            pos3 = p3;
            Vector3 p21, p23;
            p21 = p1 - p2;
            p23 = p3 - p2;
            normal = Vector3.Normalize(Vector3.Cross(p21, p23));
            //grade = normal.Y;
            if (nvr == 1)
            {
                if (normal.Y < 0)
                {
                    invert();
                }
            }
            else if (nvr == -1)
            {
                if (normal.Y > 0)
                {
                    invert();
                }
            }
        }
        public void invert()
        {
            Vector3 ZS;
            ZS = pos2;
            pos2 = pos3;
            pos3 = ZS;
            normal *= -1;
            //grade = normal.Y;
        } // Flips the normal
    }
}
