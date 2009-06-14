using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FVProductions.Utility;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    public class RoundTextChooser
    {
        private float engage;
        private bool engaged;
        private float spin, spinSpeed;

        public static float Scale = 1.0f;

        public static String Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";

        private SpriteFont spFont;

        private Peripheral Peripheral;

        public RoundTextChooser(Peripheral p)
        {
            Peripheral = p;
            spFont = Global.DefaultFont;
            engage = 0;
            engaged = false;
        }

        public char? Update(GameTime gameTime)
        {
            if (engaged)
            {
                engage += (float)gameTime.ElapsedGameTime.TotalSeconds * 10;
                if (engage > 1)
                    engage = 1;
            }
            else
            {
                engage -= (float)gameTime.ElapsedGameTime.TotalSeconds * 10;
                if (engage < 0)
                    engage = 0;
            }

            if (engaged)
            {
                spinSpeed = 5*(Peripheral.IsPressed(PeripheralButton.DOWN) ? 1 : Peripheral.IsPressed(PeripheralButton.UP) ? -1 : 0);
            }

            {
                spin += spinSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                spinSpeed *= (1 - (float)gameTime.ElapsedGameTime.TotalSeconds*10);
                if (spin < 0)
                    spin += Alphabet.Length;
            }
            bool confirm = (Peripheral is KeyboardPeripheral) ? Peripheral.WasPressed(PeripheralButton.RIGHT) : Peripheral.WasPressed(PeripheralButton.CONFIRM);
            bool back = Peripheral.WasPressed(PeripheralButton.BACK) || Peripheral.WasPressed(PeripheralButton.LEFT);
            if (engaged && confirm)
            {
                return Alphabet[(int)(spin + 13) % Alphabet.Length];
            }
            else if (engaged && back)
            {
                return (char)0x08;
            }
            else if (engaged)
            {
                if (!(Peripheral is KeyboardPeripheral))
                    return GetKeyboardInput();
            }
            return null;
        }

        private static char[] MyChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '.', '_', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ';' };
        private static Keys[] MyKeys = { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z, Keys.OemPeriod, Keys.Space, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9, Keys.NumPad0,Keys.OemSemicolon };
        private static KeyboardState oldKBS;
        private char? GetKeyboardInput()
        {
            KeyboardState kbs = Keyboard.GetState();

            bool capital = false; // TODO: set to capslock state

            if (kbs.IsKeyDown(Keys.LeftShift) || kbs.IsKeyDown(Keys.RightShift))
                capital = !capital;

            char? returnValue = null;

            for (int i = 0; i < MyKeys.Length; i++)
            {
                if (kbs.IsKeyDown(MyKeys[i]) && oldKBS.IsKeyUp(MyKeys[i]))
                {
                    char retVal = MyChars[i];
                    if (retVal >= 'a' && retVal <= 'z' && capital)
                        retVal = (char)(retVal + ('A' - 'a'));
                    if (retVal == ';' && capital)
                        retVal = ':';
                    if (Alphabet.Contains(retVal))
                    {
                        returnValue = retVal;
                        break;
                    }
                }
            }
            if (returnValue.HasValue == false && kbs.IsKeyDown(Keys.Back) && oldKBS.IsKeyUp(Keys.Back))
                returnValue = (char)0x08;//backspace
            oldKBS = kbs;
            return returnValue;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int i = -1;
            while (true)
            {
                i++;
                if (i < spin)
                    continue;
                if (i - spin > 26)
                    break;
                Vector3 pos = new Vector3((((1-((i-spin)/26))*10*26)+(engage*100))*Scale,0,0);
                pos = Vector3.Transform(pos,Matrix.CreateRotationZ(((i-spin)/26)*MathHelper.Pi*3 + MathHelper.PiOver2));
                pos += new Vector3(Global.ScreenWidth / 2, Global.ScreenHeight / 2, 0);
                float alpha = engage;
                Color col = Color.White;
                if (i - spin < 6)
                    alpha *= (i - spin) / 6f;
                if (i - spin > 26 - 6)
                    alpha *= (26 - (i - spin)) / 6f;
                if (i - spin > 12.0f && i - spin <= 13.0f)
                    col = Color.Red;
                spriteBatch.DrawString(spFont, "" + Alphabet[i % Alphabet.Length], new Vector2(pos.X, pos.Y), new Color(col.R/256f,col.G/256f,col.B/256f,alpha),0,Vector2.Zero,Scale,SpriteEffects.None,0);
            }
        }

        public void TurnOn()
        {
            engaged = true;
        }

        public void TurnOff()
        {
            engaged = false;
        }

        public bool IsOn()
        {
            return engaged;
        }
    }
}
