using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain_Rendering
{
    public class menü
    {
        Texture2D punkthinztex, verbindunghinztex, radhinztex, mausauswahltex;
        GraphicsDevice Graphicsdevice;
        SpriteBatch Spritebatch;

        public menü(ContentManager Content, GraphicsDevice device, SpriteBatch batch)
        {
            punkthinztex = Content.Load<Texture2D>("punkt hinzufügen");
            verbindunghinztex = Content.Load<Texture2D>("verbindung hinzufügen");
            radhinztex = Content.Load<Texture2D>("rad hinzufügen");
            mausauswahltex = Content.Load<Texture2D>("maus auswahl");
            Graphicsdevice = device;
            Spritebatch = batch;
        }
        public bool Update()
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            if (mousePosition.X > 10 && mousePosition.X < 50 && mousePosition.Y > 10 && mousePosition.Y < 50 && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Game1.modus = 1;
                return false;
            }
            else if (mousePosition.X > 10 && mousePosition.X < 50 && mousePosition.Y > 60 && mousePosition.Y < 100 && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Game1.modus = 2;
                Game1.Punktfenster.aktiv = false;
                return false;
            }
            else if (mousePosition.X > 10 && mousePosition.X < 50 && mousePosition.Y > 110 && mousePosition.Y < 150 && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Game1.modus = 3;
                Game1.Punktfenster.aktiv = false;
                return false;
            }
            else if (mousePosition.X > 10 && mousePosition.X < 50 && mousePosition.Y > 160 && mousePosition.Y < 200 && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Game1.modus = 4;
                Game1.Punktfenster.aktiv = false;
                return false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Game1.modus = 1;
                Game1.verbmod = 0;
            }
            return true;
        }
        public void Draw()
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);

            if (mousePosition.X > 10 && mousePosition.X < 50 && mousePosition.Y > 10 && mousePosition.Y < 50)
                Spritebatch.Draw(mausauswahltex, new Vector2(10, 10), Color.LightGray);
            else
                Spritebatch.Draw(mausauswahltex, new Vector2(10, 10), Color.White);

            if (mousePosition.X > 10 && mousePosition.X < 50 && mousePosition.Y > 60 && mousePosition.Y < 100)
                Spritebatch.Draw(punkthinztex, new Vector2(10, 60), Color.LightGray);
            else
                Spritebatch.Draw(punkthinztex, new Vector2(10, 60), Color.White);


            if (mousePosition.X > 10 && mousePosition.X < 50 && mousePosition.Y > 110 && mousePosition.Y < 150)
                Spritebatch.Draw(verbindunghinztex, new Vector2(10, 110), Color.LightGray);
            else
                Spritebatch.Draw(verbindunghinztex, new Vector2(10, 110), Color.White);


            if (mousePosition.X > 10 && mousePosition.X < 50 && mousePosition.Y > 160 && mousePosition.Y < 200)
                Spritebatch.Draw(radhinztex, new Vector2(10, 160), Color.LightGray);
            else
                Spritebatch.Draw(radhinztex, new Vector2(10, 160), Color.White);
        }
    }
}
