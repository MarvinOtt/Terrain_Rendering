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
    public class punktfenster
    {
        Texture2D whitepixeltex;
        public bool aktiv;
        Vector2 pos;
        Vector2 size;
        int punktID = 0;
        Game1.Floatbox[] textboxen = new Game1.Floatbox[3];
        public punktfenster(ContentManager manager)
        {
            textboxen[0] = new Game1.Floatbox(Game1.Box_Style.Default, new Vector2(20, 20));
            textboxen[1] = new Game1.Floatbox(Game1.Box_Style.Default, new Vector2(20, 70));
            textboxen[2] = new Game1.Floatbox(Game1.Box_Style.Default, new Vector2(20, 120));
            whitepixeltex = manager.Load<Texture2D>("white pixel");
            pos = new Vector2(Game1.Screenwidth - 300, 0);
            size = new Vector2(300, Game1.Screenheight);
            aktiv = false;
            //pos = new Vector2()
        }
        public void punktübergabe(int ID)
        {
            Game1.punkt punkt = Game1.model.punkte[ID];
            punktID = ID;
            textboxen[0].text = punkt.pos.X.ToString();
            textboxen[1].text = punkt.pos.Y.ToString();
            textboxen[2].text = punkt.pos.Z.ToString();
            textboxen[0].text = punkt.pos.X.ToString();
            textboxen[1].text = punkt.pos.Y.ToString();
            textboxen[2].text = punkt.pos.Z.ToString();
        }
        private void updatepoint(int ID)
        {
            if (ID == 0)
            {
                Game1.model.punkte[punktID].pos.X = (float)Convert.ToDouble(textboxen[ID].text);
            }
            else if (ID == 1)
            {
                Game1.model.punkte[punktID].pos.Y = (float)Convert.ToDouble(textboxen[ID].text);
            }
            else if (ID == 2)
            {
                Game1.model.punkte[punktID].pos.Z = (float)Convert.ToDouble(textboxen[ID].text);
            }
        }
        public int Update(GameTime time)
        {
            Vector2 mousepos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            bool textboxabbruch = false;
            for (int i = 0; i < textboxen.Length; i++)
            {
                //textboxen[i].verschiebung = pos;
                /*int keypressed = textboxen[i].Update(time);
                if (keypressed == 1)
                    updatepoint(i);
                if(keypressed == -1)
                {
                    textboxabbruch = true;
                }*/
            }
            if (textboxabbruch == false && Game1.keyhandler.IsKeyActive(Keys.Escape))
            {
                aktiv = false;
            }
            if (new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y).Intersects(new Rectangle((int)mousepos.X, (int)mousepos.Y, 1, 1)))
            {
                return -1;
            }
            return 0;
        }
        public void Draw(SpriteBatch batch, GameTime time)
        {
            batch.Draw(whitepixeltex, pos, new Rectangle(0, 0, (int)size.X, (int)size.Y), Color.White * 0.5f);
            Game1.DrawRectangle(batch, pos, pos + size, Color.Black * 0.5f);
            Game1.DrawRectangle(batch, new Vector2(pos.X + 1, pos.Y + 1), pos + size - new Vector2(1, 1), Color.Black * 0.5f);
            Game1.DrawRectangle(batch, new Vector2(pos.X + 2, pos.Y + 2), pos + size - new Vector2(2, 2), Color.Gray * 0.5f);
            Game1.DrawRectangle(batch, new Vector2(pos.X + 3, pos.Y + 3), pos + size - new Vector2(3, 3), Color.Gray * 0.5f);
            for (int i = 0; i < textboxen.Length; i++)
            {
                textboxen[i].Draw(batch, time);
            }
        }
    }
}
