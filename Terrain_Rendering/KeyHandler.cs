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
    public class KeyHandler
    {
        #region Tasten
        public KeyboardState oldstate, newstate;
        #endregion
        public string CurrentInput;
        public KeyHandler()
        {
            newstate = Keyboard.GetState();
            oldstate = newstate;
        }
        public bool IsKeyActive(Keys key)
        {
            if (newstate.IsKeyDown(key) && !oldstate.IsKeyDown(key))
                return true;
            else
                return false;
        }
        public char KeytoChar_Number(Keys Key)
        {
            bool Shift = newstate.IsKeyDown(Keys.LeftShift) || newstate.IsKeyDown(Keys.RightShift);
            bool Caps = System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock);
            if (Caps)
                Shift = !Shift;
            switch (Key)
            {
                case Keys.D0:
                    if (Shift) { return '='; } else { return '0'; }
                case Keys.D1:
                    if (Shift) { return '!'; } else { return '1'; }
                case Keys.D2:
                    if (Shift) { return '"'; } else { return '2'; }
                case Keys.D3:
                    if (Shift) { return '§'; } else { return '3'; }
                case Keys.D4:
                    if (Shift) { return '$'; } else { return '4'; }
                case Keys.D5:
                    if (Shift) { return '%'; } else { return '5'; }
                case Keys.D6:
                    if (Shift) { return '&'; } else { return '6'; }
                case Keys.D7:
                    if (Shift) { return '/'; } else { return '7'; }
                case Keys.D8:
                    if (Shift) { return '('; } else { return '8'; }
                case Keys.D9:
                    if (Shift) { return ')'; } else { return '9'; }

                case Keys.NumPad0: return '0';
                case Keys.NumPad1: return '1';
                case Keys.NumPad2: return '2';
                case Keys.NumPad3: return '3';
                case Keys.NumPad4: return '4';
                case Keys.NumPad5: return '5';
                case Keys.NumPad6: return '6';
                case Keys.NumPad7: return '7';
                case Keys.NumPad8: return '8';
                case Keys.NumPad9: return '9';
                case Keys.Add: return '+';
                case Keys.Subtract: return '-';

                case Keys.OemPlus:
                    if (Shift) { return '*'; } else { return '+'; }
                case Keys.OemPeriod:
                    if (Shift) { return ':'; } else { return ','; }
                case Keys.OemMinus:
                    if (Shift) { return '_'; } else { return '-'; }
                case Keys.OemComma:
                    if (Shift) { return ';'; } else { return ','; }
            }
            return (char)0;

        }
        public void Update()
        {
            oldstate = newstate;
            newstate = Keyboard.GetState();
            CurrentInput = "";
            Keys[] keys = newstate.GetPressedKeys();
            Keys[] oldkeys = oldstate.GetPressedKeys();
            bool IsSameKey = false;
            for (int i = 0; i < keys.Length; i++)
            {
                for (int j = 0; j < oldkeys.Length; j++)
                {
                    if (keys[i] == oldkeys[j])
                        IsSameKey = true;
                }
                if (!IsSameKey)
                {
                    char OUT = KeytoChar_Number(keys[i]);
                    if (OUT != 0)
                        CurrentInput += OUT.ToString();
                }
                IsSameKey = false;
            }
        }
    }
}
