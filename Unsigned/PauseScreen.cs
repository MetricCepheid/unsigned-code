using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using UnsignedPeripheralPlugins;

namespace Unsigned
{
    class PauseScreen : BaseState
    {
        private ContentManager content;
        private Vector2 pauseMenuPos, pauseMenuVel, pauseWingRot;
        private float pauseRot;
        private int pauseSelected;
        private Peripheral pauseSelectOwner;
        private String[][] pauseTextDisp;
        private int currentState;
        private Texture2D texPauseBorder, texPauseWings, texPausePick;
        private Vector3[] textPos = new Vector3[16];

        private GameState gameState;

        public void SetOwner(Peripheral p)
        {
            pauseSelectOwner = p;
        }

        public void SetGameState(GameState gs)
        {
            gameState = gs;
        }

        public override void Load()
        {
            content = new ContentManager(UnsignedGame.GetSingleton().Services);
            texPauseBorder = content.Load<Texture2D>("graphics\\pauseborder");
            texPauseWings = content.Load<Texture2D>("graphics\\pausewing");
            texPausePick = content.Load<Texture2D>("graphics\\pickofselect");
            pauseMenuPos = new Vector2(GameSettings.windowwidth / 2, GameSettings.windowheight / 2);
            pauseMenuVel = new Vector2(10, 0);
            pauseWingRot.Y = 30;

            pauseTextDisp = new String[2][];

            pauseTextDisp[0] = new String[4];
            pauseTextDisp[0][0] = Localizer.Get("Continue");
            pauseTextDisp[0][1] = Localizer.Get("Retry");
            pauseTextDisp[0][2] = Localizer.Get("Options");
            pauseTextDisp[0][3] = Localizer.Get("Exit");

            pauseTextDisp[1] = new String[2];
            pauseTextDisp[1][0] = Localizer.Get("Lefty") + ": " + Localizer.Get("Maybe");
            pauseTextDisp[1][1] = Localizer.Get("Back");
            //song.pause();
        }

        public override void Unload()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            pauseMenuPos += pauseMenuVel * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (pauseMenuPos.X > (GameSettings.windowwidth * 0.6f))
                pauseMenuVel.X = -Math.Abs(pauseMenuVel.X);
            if (pauseMenuPos.X < (GameSettings.windowwidth * 0.4f))
                pauseMenuVel.X = Math.Abs(pauseMenuVel.X);
            pauseMenuVel.Y = pauseWingRot.Y;
            pauseWingRot.X += (float)gameTime.ElapsedGameTime.TotalSeconds * pauseWingRot.Y * 0.1f;
            if (pauseWingRot.Y < 0)
                pauseRot += (pauseMenuVel.X * (float)gameTime.ElapsedGameTime.TotalSeconds * 0.01f) * (Math.Sign(pauseMenuVel.X) == Math.Sign(pauseRot) ? 1 : 3);
            if (pauseWingRot.Y > 0 && pauseWingRot.X > MathHelper.Pi / 4)
                pauseWingRot.Y = -160;
            else if (pauseWingRot.Y < 0 && pauseWingRot.X < -MathHelper.Pi / 2)
                pauseWingRot.Y = 30;

            int offset = 0;
            if (pauseSelectOwner.WasPressed(PeripheralButton.DOWN))
                offset++;
            if (pauseSelectOwner.WasPressed(PeripheralButton.UP))
                offset--;
            pauseSelected = Math.Max(0, Math.Min(pauseTextDisp[currentState].Length - 1, pauseSelected + offset));
            if (currentState > 0 && pauseSelectOwner.WasPressed(PeripheralButton.BACK))
                currentState = 0;
            else if (pauseSelectOwner.WasPressed(PeripheralButton.BACK))
                gameState.TogglePause();
            if (pauseSelectOwner.WasPressed(PeripheralButton.CONFIRM))
                ApplyPauseOption();
        }

        public override void Render(GameTime gameTime)
        {
            SpriteBatch spritebatch = RenderMaster.GetSingleton().spritebatch;

            float scale = 1.5f;

            Vector2 origin = new Vector2(texPauseBorder.Width / 2, texPauseBorder.Height / 2);
            Vector3 wingPosR = new Vector3(455, 79, 0);
            Vector3 wingPosL = new Vector3(25, 59, 0);
            wingPosR -= new Vector3(origin, 0);
            wingPosL -= new Vector3(origin, 0);
            wingPosR *= scale;
            wingPosL *= scale;
            wingPosR = Vector3.Transform(wingPosR, Matrix.CreateRotationZ(pauseRot));
            wingPosL = Vector3.Transform(wingPosL, Matrix.CreateRotationZ(pauseRot));

            for (int i = 0; i < pauseTextDisp[currentState].Length; i++)
            {
                textPos[i] = new Vector3(texPauseBorder.Width / 2, ((350 - 68) * ((i + 1f) / (pauseTextDisp[currentState].Length + 1f))) + 68, 0);
                textPos[i] -= new Vector3(origin, 0);
                textPos[i] *= scale;
                textPos[i] = Vector3.Transform(textPos[i], Matrix.CreateRotationZ(pauseRot));
            }

            Vector3 pickPosL, pickPosR;

            pickPosL = new Vector3(texPauseBorder.Width * 0.25f, ((350 - 68) * ((pauseSelected + 1f) / (pauseTextDisp[currentState].Length + 1f))) + 68, 0);
            pickPosL -= new Vector3(origin, 0);
            pickPosL *= scale;
            pickPosL = Vector3.Transform(pickPosL, Matrix.CreateRotationZ(pauseRot));

            pickPosR = new Vector3(texPauseBorder.Width * 0.75f, ((350 - 68) * ((pauseSelected + 1f) / (pauseTextDisp[currentState].Length + 1f))) + 68, 0);
            pickPosR -= new Vector3(origin, 0);
            pickPosR *= scale;
            pickPosR = Vector3.Transform(pickPosR, Matrix.CreateRotationZ(pauseRot));

            spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            spritebatch.Draw(texPauseBorder, pauseMenuPos, null, Color.White, pauseRot, origin, scale, SpriteEffects.None, 0);
            spritebatch.Draw(texPauseWings, new Vector2(wingPosR.X, wingPosR.Y) + pauseMenuPos, null, Color.White, -pauseWingRot.X / 2, new Vector2(32, 69), scale, SpriteEffects.None, 0);
            spritebatch.Draw(texPauseWings, new Vector2(wingPosL.X, wingPosL.Y) + pauseMenuPos, null, Color.White, pauseWingRot.X / 2, new Vector2(texPauseWings.Width - 32, 69), scale, SpriteEffects.FlipHorizontally, 0);
            for (int i = 0; i < pauseTextDisp[currentState].Length; i++)
                spritebatch.DrawString(Global.DefaultFont, pauseTextDisp[currentState][i], new Vector2(textPos[i].X, textPos[i].Y) + pauseMenuPos, pauseSelected == i ? Color.Red : new Color(100, 128, 100), pauseRot, Global.DefaultFont.MeasureString(pauseTextDisp[currentState][i]) * 0.5f, scale * 2, SpriteEffects.None, 0);
            spritebatch.Draw(texPausePick, new Vector2(pickPosL.X, pickPosL.Y) + pauseMenuPos, null, Color.White, pauseRot, new Vector2(texPausePick.Width, texPausePick.Height / 2), scale / 3, SpriteEffects.None, 0);
            spritebatch.Draw(texPausePick, new Vector2(pickPosR.X, pickPosR.Y) + pauseMenuPos, null, Color.White, pauseRot + MathHelper.Pi, new Vector2(texPausePick.Width, texPausePick.Height / 2), scale / 3, SpriteEffects.None, 0);

            spritebatch.End();
        }

        private void ApplyPauseOption()
        {
            if (currentState == 0)
            {
                if (pauseSelected == 0)
                    gameState.TogglePause();
                else if (pauseSelected == 1)
                { gameState.TogglePause(); RhythmMaster.GetSingleton().Restart(); }
                else if (pauseSelected == 2)
                {
                    currentState = 1;
                    pauseTextDisp[1][0] = Localizer.Get("Lefty") + ": " + (pauseSelectOwner.LeftySwitch ? Localizer.Get("On") : Localizer.Get("Off"));
                }
                else if (pauseSelected == 3)
                { 
                    UnsignedGame.GetSingleton().PopState();
                    UnsignedGame.GetSingleton().PopState();
                    UnsignedGame.GetSingleton().PopState();
                }
            }
            else if (currentState == 1)
            {
                if (pauseSelected == 0)
                { 
                    pauseSelectOwner.LeftySwitch = !pauseSelectOwner.LeftySwitch; 
                    pauseTextDisp[1][0] = Localizer.Get("Lefty") + ": " + (pauseSelectOwner.LeftySwitch ? Localizer.Get("On") : Localizer.Get("Off")); 
                }
                else if (pauseSelected == 1)
                {
                    currentState = 0;
                }
            }
        }
    }
}
