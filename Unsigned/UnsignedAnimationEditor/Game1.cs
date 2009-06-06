using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

namespace UnsignedAnimationEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public AnimationInfo AnimationInfo;

        private SpriteBatch spriteBatch;
        private IntPtr DrawSurface;

        private float yaw, pitch;

        private Point lastMousePos;
        private float yCamPos;

        private BasicEffect effect;

        private float Scale;

        private Texture2D texGuitar;
        private FVModel mdlGuitar;
        private Dictionary<String, FVModel> mdlDrums;

        private Texture2D texCharacter;

        private Dictionary<String, FVModel> models;

        public bool CanUseMouse { get; set; }

        public Game1(IntPtr drawSurface)
        {
            DrawSurface = drawSurface;
            Global.Graphics = new GraphicsDeviceManager(this);
            Global.Services = Services;
            Global.Random = new Random();
            Content.RootDirectory = "Content";
            Global.Graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            System.Windows.Forms.Control.FromHandle((this.Window.Handle)).VisibleChanged += new EventHandler(Game1_VisibleChanged);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            yaw = 0;
            pitch = 0;
            Scale = 1f;
            yCamPos = 4.3f;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Global.TexWhite = Content.Load<Texture2D>("white");
            effect = new BasicEffect(Global.Graphics.GraphicsDevice, null);
            models = new Dictionary<String, FVModel>();
            String[] files = Directory.GetFiles("Content\\bodyparts\\");
            for (int i = 0; i < files.Length; i++)
            {
                String str = files[i].Substring(files[i].LastIndexOf('\\') + 1);
                str = str.Substring(0, str.LastIndexOf('.'));
                models.Add(str.ToUpper(), ModelLoader.LoadModel("bodyparts\\" + str));
            }

            {
                mdlDrums = new Dictionary<String, FVModel>();
                String[] dfs = Directory.GetFiles("Content\\instruments\\drums\\yahama\\");
                for (int i = 0; i < dfs.Length; i++)
                {
                    String subName = dfs[i].Substring(dfs[i].LastIndexOf('\\') + 1);
                    subName = subName.Substring(0, subName.LastIndexOf('.'));
                    if (subName.StartsWith("mdl0"))
                    {
                        mdlDrums.Add(subName.Substring(5).ToUpper(), ModelLoader.LoadModel("instruments\\drums\\yahama\\" + subName));
                    }
                }
            }

            mdlGuitar = ModelLoader.LoadModel("instruments\\guitar\\gibson\\mdl0");
            texGuitar = Content.Load<Texture2D>("instruments\\guitar\\gibson\\tex0");

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            Global.DefaultFont = Content.Load<SpriteFont>("defaultFont");
            texCharacter = Content.Load<Texture2D>("fanTex");
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            Point mousePos = new Point(Mouse.GetState().X, Mouse.GetState().Y);

            if (CanUseMouse)
            {
                if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
                {
                    float dx = mousePos.X - lastMousePos.X;
                    float dy = mousePos.Y - lastMousePos.Y;
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                    {
                        float scaleChange = 1.0f;
                        while (dy > 0)
                        {
                            scaleChange *= 1.001f;
                            dy--;
                        }
                        while (dy < 0)
                        {
                            scaleChange *= 0.999f;
                            dy++;
                        }
                        Scale *= scaleChange;
                    }
                    else
                    {
                        yaw += dx * 0.01f;
                        pitch += dy * 0.01f;
                    }
                }
                else if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    if (AnimationInfo != null)
                    {
                        Frame ck = AnimationInfo.CurrentKeyframe;
                        if (ck != null)
                        {
                            if (AnimationInfo.SelectedJointIndex >= 0)
                            {
                                float dx = mousePos.X - lastMousePos.X;
                                float dy = mousePos.Y - lastMousePos.Y;

                                if (!Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                                {
                                    ck.Matrices[AnimationInfo.SelectedJointIndex] = ck.Matrices[AnimationInfo.SelectedJointIndex] * Matrix.CreateRotationY(dx * 0.01f);
                                    ck.Matrices[AnimationInfo.SelectedJointIndex] = ck.Matrices[AnimationInfo.SelectedJointIndex] * Matrix.CreateRotationZ(dy * 0.01f);
                                }
                                else
                                    ck.Matrices[AnimationInfo.SelectedJointIndex] = Matrix.CreateRotationX(dy * 0.01f) * ck.Matrices[AnimationInfo.SelectedJointIndex];
                            }
                        }
                    }
                }
            }

            

            lastMousePos = mousePos;

            while (pitch > MathHelper.Pi)
                pitch -= MathHelper.TwoPi;
            while (pitch < -MathHelper.Pi)
                pitch += MathHelper.TwoPi;
            while (yaw > MathHelper.Pi)
                yaw -= MathHelper.TwoPi;
            while (yaw < -MathHelper.Pi)
                yaw += MathHelper.TwoPi;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;

            effect.LightingEnabled = true;
            effect.TextureEnabled = true;

            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(0.1f, -0.2f, -1));
            effect.DirectionalLight0.DiffuseColor = new Vector3(1, 1, 1);
            effect.DirectionalLight0.SpecularColor = new Vector3(1, 1, 1);
            effect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            effect.DiffuseColor = new Vector3(1f, 1f, 1f);
            effect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);
            effect.SpecularPower = 24f;
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Global.ScreenWidth / (float)Global.ScreenHeight, 0.1f, 100f);
            effect.View = Matrix.CreateLookAt(new Vector3(0,yCamPos,12),new Vector3(0,yCamPos,0),Vector3.Up);
            Matrix World = Matrix.CreateRotationY(yaw) * Matrix.CreateRotationX(pitch) * Matrix.CreateScale(Scale);
            effect.World = World;
            effect.Texture = texCharacter;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                if (AnimationInfo != null)
                {
                    Joint j = AnimationInfo.Skeleton.RootJoint;

                    DrawJoint(j, Matrix.CreateRotationY(yaw) * Matrix.CreateRotationX(pitch));
                }

                pass.End();
            }
            effect.End();

            spriteBatch.Begin();

            spriteBatch.DrawString(Global.DefaultFont, "" + lastMousePos.X + ", " + lastMousePos.Y, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(Global.DefaultFont, "" + Scale, new Vector2(0, 20), Color.White);
            spriteBatch.Draw(Global.TexWhite, new Rectangle(0, Global.ScreenHeight - 20, 20, 20), CanUseMouse ? Color.Lime : Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawJoint(Joint j, Matrix world)
        {
            Matrix JointMatrix = (AnimationInfo==null || AnimationInfo.CurrentAnimation==null) ? 
                                 Matrix.Identity : 
                                 AnimationInfo.CurrentAnimation.GetJointMatrix(AnimationInfo.CurrentAnimationTimeValue, j.JointMatrix);
            Matrix postWorld = JointMatrix * Matrix.CreateTranslation(j.postOffset) * world;
            if (j.Name.ToUpper() == "GUITAR")
            {
                effect.World = Matrix.CreateTranslation(-j.preOffset) * postWorld * Matrix.CreateScale(Scale);
                effect.Texture = texGuitar;
                effect.CommitChanges();
                mdlGuitar.Draw();
            }
            else if (models.ContainsKey(j.Name.ToUpper()))
            {
                effect.World = Matrix.CreateTranslation(-j.preOffset) * postWorld * Matrix.CreateScale(Scale);
                effect.Texture = texCharacter;
                effect.CommitChanges();
                models[j.Name.ToUpper()].Draw();
            }
            else if (mdlDrums.ContainsKey(j.Name.ToUpper()))
            {
                effect.World = Matrix.CreateTranslation(-j.preOffset) * postWorld * Matrix.CreateScale(Scale);
                effect.Texture = Global.TexWhite;
                effect.CommitChanges();
                mdlDrums[j.Name.ToUpper()].Draw();
            }
            for (int i = 0; i < j.Children.Count; i++)
                DrawJoint(j.Children[i], postWorld);
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)  
        {
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = DrawSurface;  
        }  
          
        private void Game1_VisibleChanged(object sender, EventArgs e)  
        {  
            if (System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible == true)  
                System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible = false;  
        }  
    }
}
