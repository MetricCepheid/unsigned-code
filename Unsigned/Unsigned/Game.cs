using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{
    #region publicstructs

    public struct WaveNode
    {
        public float X, Y;
        public byte Z;
        public bool White;
    }
#endregion

    public class UnsignedGame : Microsoft.Xna.Framework.Game
    {
        public static UnsignedGame Singleton { get; private set; }

        private BaseState currentState;

#if DEBUG
        private SpriteBatch fpsSpriteBatch;
        private float[] lastframes = new float[60];
        private int frameIndex;
        RenderTarget2D screenshotRT;
#endif

        private bool SHOULD_TAKE_SCREENSHOT;

        public UnsignedGame()
        {
            Singleton = this;
            Global.Graphics = new GraphicsDeviceManager(this);
            Global.Services = Services;
            Global.Random = new Random();

            String[] Keys = { "Game Name:" };
            String[] Values = { "Unsigned" };
            //XFireClient.SetCustomGameData(Keys.Length, Keys, Values);
        }

        protected override void Initialize()
        {
#if DEBUG
            for (int i = 0; i < lastframes.Length; i++)
                lastframes[i] = 1 / 30f;
#endif

            SHOULD_TAKE_SCREENSHOT = false;

            Version SM = Global.Graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;

            if (SM.Major < 1 || (SM.Major == 1 && SM.Minor < 1))
            {
                Debug.Error("Your graphics card does not meet the minimum requirements.\nIt only supports SM" + SM.Major + "." + SM.Minor + "\nYou need at least 1.1 to run Unsigned");
                Exit();
                return;
            }

            Localizer.Load();

            Configuration.Load();

            Window.Title = "Unsigned";  

            KeyboardPeripheral.LoadMapping("Configuration\\keymapping.xml");

            PeripheralManager.Singleton.CheckConnections();

            InstrumentMaster.Singleton.Load("Configuration\\Instruments\\");

            currentState = new FVLogoScreen();

            PeripheralManager.Singleton.ReloadDLLs();
            PeripheralManager.Singleton.CheckConnections();

            //TEST CODE, takes you right into the action!
            SessionInfo info = new SessionInfo();
            info.characterIndices[0] = -1;
            info.difficulties[0] = Difficulty.Easy;
            info.instruments[0] = 1;
            info.peripherals[0] = PeripheralManager.Singleton.GetPeripherals()[0];
            info.characterIndices[1] = -1;
            info.difficulties[1] = Difficulty.Easy;
            info.instruments[1] = 3;
            info.peripherals[1] = PeripheralManager.Singleton.GetPeripherals()[1];
            info.songFileName = "songdata\\dontstop.uns";
            //info.songFileName = "C:\\projects\\Unsigned3.0\\Unsigned\\Unsigned\\bin\\x86\\Debug\\songdata\\Destroyer of Senses.gba";
            currentState = new GameState(info);
            //END TEST CODE */

#if DEBUG
            this.IsFixedTimeStep = false;
#endif

            base.Initialize();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            Configuration.Save();
            base.OnExiting(sender, args);
        }
        
        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";
            Global.TexDefaultBM = Content.Load<Texture2D>("textures\\global\\blankbm");
            Global.DefaultFont = Content.Load<SpriteFont>("fonts\\BasicFont");
            Global.TexWhite = Content.Load<Texture2D>("textures\\global\\white");
            currentState.Load();

#if DEBUG
            fpsSpriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);
            screenshotRT = new RenderTarget2D(Global.Graphics.GraphicsDevice, Global.ScreenWidth, Global.ScreenHeight, 0, SurfaceFormat.Color);
#endif
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            PeripheralManager.Singleton.QueryAll();

            currentState.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
#if DEBUG
            if (SHOULD_TAKE_SCREENSHOT)
            {
                Global.Graphics.GraphicsDevice.SetRenderTarget(0, screenshotRT);
            }
#endif
            currentState.Render(gameTime);
#if DEBUG
            if (SHOULD_TAKE_SCREENSHOT)
            {
                Global.Graphics.GraphicsDevice.SetRenderTarget(0, null);
                screenshotRT.GetTexture().Save("screenshot.jpg", ImageFileFormat.Jpg);
                SHOULD_TAKE_SCREENSHOT = false;
            }
#endif

#if DEBUG
            fpsSpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            {
                float fps = 0;
                lastframes[frameIndex % lastframes.Length] = (float)gameTime.ElapsedRealTime.TotalSeconds;

                frameIndex++;
                for (int i = 0; i < lastframes.Length; i++)
                    fps += lastframes[i];
                fps /= lastframes.Length;
                fps = 1 / fps;
                fpsSpriteBatch.DrawString(Global.DefaultFont, "" + (int)fps, new Vector2(Global.ScreenWidth - 40, Global.ScreenHeight - 40), Color.Red);
            }
            fpsSpriteBatch.End();
#endif

            base.Draw(gameTime);
        }

        public void SwitchState(BaseState state)
        {
            currentState.Unload();
            currentState = state;
            currentState.Load();
        }

        internal void TakeScreenshot()
        {
            SHOULD_TAKE_SCREENSHOT = true;
        }
    }
}
