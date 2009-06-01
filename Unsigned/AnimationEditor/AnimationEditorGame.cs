using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using FVProductions.Utility;

namespace AnimationEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class AnimationEditorGame : Game
    {
        private XNAComponentCollection components;
        private SpriteBatch spriteBatch;

        private RightClickMenu rcMenu;

        private IntPtr DrawSurface;

        public AnimationEditorGame(IntPtr drawSurface)
        {
            Global.Graphics = new GraphicsDeviceManager(this);
            Global.Services = Services;
            Global.Random = new Random();
            Global.Graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            System.Windows.Forms.Control.FromHandle((this.Window.Handle)).VisibleChanged += new EventHandler(Game1_VisibleChanged);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            base.Initialize();
            return;
            AnimationInfo info = new AnimationInfo();
            info.Add(new AnimationInfo.AnimationValue("Mouth Open", 0));
            info.Add(new AnimationInfo.AnimationValue("Mouth Pinch", 1));

            components = new XNAComponentCollection();
            components.Add(new AnimationEditorViewer("Viewer"));
            components.Add(new Menu("TopMenu"));
            //components.Add(new ControlPanel("ControlPanel",info));
            components["Viewer"].Bounds = new Rectangle(0, 24, Global.ScreenHeight - 24, Global.ScreenHeight - 24);
            //components["ControlPanel"].Bounds = new Rectangle(Global.ScreenHeight - 24, 24, Global.ScreenWidth - (Global.ScreenHeight - 24), Global.ScreenHeight - 24);
            components["TopMenu"].Bounds = new Rectangle(0,0,Global.ScreenWidth, 24);

            //((EditorViewer)components["Viewer"]).animInfo = info;

            //((ControlPanel)components["ControlPanel"]).EditorViewer = ((AnimationEditorViewer)components["Viewer"]);

            this.IsMouseVisible = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //for (int i = 0; i < components.Count; i++)
            //    components[i].Load(Content);

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);
            Global.TexWhite = Content.Load<Texture2D>("white");
            Global.TexDefaultBM = Content.Load<Texture2D>("defaultnm");
            Global.DefaultFont = Content.Load<SpriteFont>("defaultFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            return;
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            for (int i = 0; i < components.Count; i++)
                components[i].Update(gameTime);

            if (Mouse.GetState().RightButton == ButtonState.Pressed && rcMenu == null)
            {
                Point mousePos = new Point(Mouse.GetState().X, Mouse.GetState().Y);
                for (int i = 0; i < components.Count; i++)
                    if (components[i].Bounds.Contains(mousePos))
                        rcMenu = components[i].GetRightClickMenu(mousePos);
            }
            if (rcMenu != null)
            {
                rcMenu.Update(gameTime);
                if (rcMenu.ShouldDestroy)
                    rcMenu = null;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            return;

            for (int i = 0; i < components.Count; i++)
                components[i].Draw();

            Global.Graphics.GraphicsDevice.SetRenderTarget(0, null);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

            for (int i = 0; i < components.Count; i++)
            {
                Texture2D tex = components[i].GetTexture();
                spriteBatch.Draw(tex, components[i].Bounds, Color.White);
            }
            
            if (rcMenu != null)
                rcMenu.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>  
        /// Event capturing the construction of a draw surface and makes sure this gets redirected to  
        /// a predesignated drawsurface marked by pointer drawSurface  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)  
        {  
                e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle =  DrawSurface;  
        }  
          
        /// <summary>  
        /// Occurs when the original gamewindows' visibility changes and makes sure it stays invisible  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void Game1_VisibleChanged(object sender, EventArgs e)  
        {  
                if (System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible == true)  
                    System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible = false;  
        }  
    }
}
