using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;
using FVProductions.Utility;

namespace Unsigned
{
    class OptionsScreen : BaseState
    {
        private ContentManager Content;
        private SpriteBatch spriteBatch;

        private FVShader effect;

        private Texture2D concrTex, concrBM;
        private Texture2D tamp1, tamp1bm, tamp2, tamp2bm;
        private Texture2D tknob, ttape;
        private Texture2D tledon, tledoff, tswitchon, tswitchoff;

        private FVModel mAmp1, mAmp2;

        private String[] guiStyle = { "Rock Band", "Unsigned", "Guitar Hero", };
        private String[][] languageNames = { new[] { "English", "French", "Spanish", "German" },
                                             new[] { "Inglés", "Francés", "Español", "Alemán" },
                                             new[] { "Anglais", "Français", "Espagnol", "Allemand" },
                                             new[] { "Englisch", "Französisch", "Spanisch", "Deutsch" },
                                           };

        private float optionsOffset = 0;
        private OPTIONS optionsSelected;

        private float knobBroken;

        private float intro;

        private int rockLevel;

        private enum OPTIONS
        {
            OPT_ROCKLEVEL = 1,
            OPT_LANGUAGE,
            OPT_RESOLUTION,
            OPT_FULLSCREEN,
            OPT_GUISTYLE,
            OPT_RENDER3D,
            OPT_LIGHTING,
            OPT_NORMALMAPPING,
            OPT_SPECULAR,
            OPT_PARTICLELEVEL,
            MAX,
        };

        public OptionsScreen()
        {

        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            mAmp1 = ModelLoader.LoadModel("meshes\\Options\\amp");
            tamp1 = Content.Load<Texture2D>("textures\\Options\\amp");
            tamp1bm = Content.Load<Texture2D>("textures\\Options\\ampBM");
            mAmp2 = ModelLoader.LoadModel("meshes\\Options\\amppanel");
            tamp2 = Content.Load<Texture2D>("textures\\Options\\amppanel");
            tamp2bm = Content.Load<Texture2D>("textures\\Options\\amppanelBM");
            tknob = Content.Load<Texture2D>("textures\\Options\\knob");
            ttape = Content.Load<Texture2D>("textures\\Options\\tape");
            tledon = Content.Load<Texture2D>("textures\\Options\\ledon");
            tledoff = Content.Load<Texture2D>("textures\\Options\\ledoff");
            tswitchon = Content.Load<Texture2D>("textures\\Options\\switchon");
            tswitchoff = Content.Load<Texture2D>("textures\\Options\\switchoff");
            concrTex = Content.Load<Texture2D>("textures\\Options\\concr");
            concrBM = Content.Load<Texture2D>("textures\\Options\\concrBM");

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");

            optionsSelected = OPTIONS.OPT_RESOLUTION;
            rockLevel = 11;
        }

        public override void Unload()
        {
            Content.Unload();
            Global.Graphics.ApplyScreenConfiguration();
        }

        public override void Update(GameTime gameTime)
        {
            Peripheral[] conts = PeripheralManager.Singleton.GetPeripherals();

            int offset = 0;
            bool green = false, red = false;
            for (int i = 0; i < conts.Length; i++)
            {
                if (conts[i].WasPressed(PeripheralButton.DOWN))
                    offset++;
                if (conts[i].WasPressed(PeripheralButton.UP))
                    offset--;
                if (conts[i].WasPressed(PeripheralButton.CONFIRM))
                    green = true;
                if (conts[i].WasPressed(PeripheralButton.BACK))
                    red = true;
            }
            optionsSelected += offset;
            if (optionsSelected < (OPTIONS)1)
                optionsSelected = OPTIONS.MAX - 1;
            if (optionsSelected >= OPTIONS.MAX)
                optionsSelected = (OPTIONS)1;

            if (green)
            {
                switch (optionsSelected)
                {
                    case OPTIONS.OPT_ROCKLEVEL:
                        knobBroken = 5;
                        break;
                    case OPTIONS.OPT_RESOLUTION:
                        Configuration.ResIndex++;
                        break;
                    case OPTIONS.OPT_GUISTYLE:
                        if (Configuration.GUIStyle == GameUIMaster.GUIStyle.RB)
                            Configuration.GUIStyle = GameUIMaster.GUIStyle.UN;
                        else if (Configuration.GUIStyle == GameUIMaster.GUIStyle.UN)
                            Configuration.GUIStyle = GameUIMaster.GUIStyle.RB;
                        break;
                    case OPTIONS.OPT_RENDER3D:
                        Configuration.RenderVenues = !Configuration.RenderVenues;
                        break;
                    case OPTIONS.OPT_FULLSCREEN:
                        Configuration.FullScreen = !Configuration.FullScreen;
                        break;
                    case OPTIONS.OPT_LIGHTING:
                        Configuration.Lighting = !Configuration.Lighting;
                        break;
                    case OPTIONS.OPT_NORMALMAPPING:
                        Configuration.NormalMapping = !Configuration.NormalMapping;
                        break;
                    case OPTIONS.OPT_SPECULAR:
                        Configuration.Specular = !Configuration.Specular;
                        break;
                    case OPTIONS.OPT_LANGUAGE:
                        Configuration.CurrentLanguage++;
                        if (Configuration.CurrentLanguage >= Localizer.Language.Length)
                            Configuration.CurrentLanguage = Localizer.Language.ENGLISH;
                        break;
                    case OPTIONS.OPT_PARTICLELEVEL:
                        Configuration.ParticleDetail = (Configuration.ParticleDetail + 1) % 6;
                        break;
                }
            }

            if (red)
                UnsignedGame.Singleton.SwitchState(new MainMenuScreen());

            optionsOffset = (((int)optionsSelected-3) * 0.2f) + (optionsOffset * 0.8f);

            if (knobBroken > 0)
                knobBroken -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public override void Render(GameTime gameTime)
        {
            UnsignedGame game = UnsignedGame.Singleton;

#if !DEBUG
            try
            {
#endif
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                //graphics.PreferMultiSampling = true;
                Global.Graphics.ApplyChanges();

                VertexDeclaration vd = new VertexDeclaration(Global.Graphics.GraphicsDevice, VertexTangentBinormal.Elements);
                Global.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
                //graphics.GraphicsDevice.

                effect.NormalMapTexture = Global.TexDefaultBM;
                effect.AmbientMaterial = new Color(24, 24, 24);
                effect.DiffuseMaterial = new Color(200, 200, 200);
                effect.SpecularMaterial = Color.White;
                effect.DirectionalLight = new DirectionalLight(true, new Vector3(0f, -1f, 1f), new Color(200, 200, 200), Color.White);
                Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                effect.TextureEnabled = true;
                effect.LightingEnabled = Configuration.Lighting;
                effect.SpecularEnabled = Configuration.Specular;
                effect.NormalMapEnabled = Configuration.NormalMapping;

                Random r = new Random();

                effect.CommitChanges();
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in OptionScreen.Draw[1]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }

            try
            {
#endif
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    intro += (float)gameTime.ElapsedGameTime.TotalSeconds*0.1f;
                    float introlerp = (Math.Min(intro, 0.5f) * 2);
                    Vector3 campos = ((1 - introlerp) * (new Vector3(-8, 112, 24))) + ((introlerp) * (new Vector3(-0.2f+(optionsOffset*0.2f), 106.1f, 1f)));
                    Matrix matView = Matrix.CreateLookAt(campos, new Vector3(-0.2f + (optionsOffset * 0.2f), 106.3f, 0), new Vector3(0, 1, 0));
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Global.Graphics.GraphicsDevice.Viewport.Height / (float)Global.Graphics.GraphicsDevice.Viewport.Height, 1.0f, 50);
                    //render the background graphics
                    effect.View = matView;

                    Matrix matRot, matScale, matTranslate;
                    if (introlerp < 0.95f)
                    {
                        {//basebottom
                            matTranslate = Matrix.CreateTranslation(0, 100, 0);
                            matRot = Matrix.CreateRotationX((float)Math.PI);
                            matScale = Matrix.CreateScale(40, 0, 32);

                            effect.World = matScale * matRot * matTranslate;
                            effect.DiffuseTexture = concrTex;
                            effect.NormalMapTexture = concrBM;
                            effect.Shininess = 0.25f;
                            effect.CommitChanges();

                            Global.Graphics.GraphicsDevice.DrawSquare();
                        }
                        {//basewall
                            matTranslate = Matrix.CreateTranslation(0, 132, -4.6f);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                            matScale = Matrix.CreateScale(40, 0, 32);

                            effect.World = matScale * matRot * matTranslate;
                            effect.DiffuseTexture = concrTex;
                            effect.NormalMapTexture = concrBM;
                            effect.Shininess = 0.25f;
                            effect.CommitChanges();

                            Global.Graphics.GraphicsDevice.DrawSquare();
                        }
                        {//rightwall
                            matTranslate = Matrix.CreateTranslation(6.8f, 132, 0f);
                            matRot = Matrix.CreateRotationX((float)Math.PI / 2) * Matrix.CreateRotationY(MathHelper.PiOver2);
                            matScale = Matrix.CreateScale(40, 0, 32);

                            effect.World = matScale * matRot * matTranslate;
                            effect.DiffuseTexture = concrTex;
                            effect.NormalMapTexture = concrBM;
                            effect.Shininess = 0.25f;
                            effect.CommitChanges();

                            Global.Graphics.GraphicsDevice.DrawSquare();
                        }
                    }

                    {//amp
                        matTranslate = Matrix.CreateTranslation(0, 103.6f, 0);
                        matRot = Matrix.Identity;
                        matScale = Matrix.CreateScale(1, 1, 1);

                        effect.World = matScale * matRot * matTranslate;

                        effect.SpecularMaterial = Color.Gray;
                        Global.Graphics.GraphicsDevice.VertexDeclaration = VertexTangentBinormal.VertexDeclaration;

                        effect.Shininess = 32f;
                        if (introlerp < 0.95f)
                        {
                            effect.DiffuseTexture = tamp1;
                            effect.NormalMapTexture = tamp1bm;
                            effect.CommitChanges();

                            mAmp1.Draw();
                        }

                        effect.DiffuseTexture = tamp2;
                        effect.NormalMapTexture = tamp2bm;
                        effect.CommitChanges(); 
                        
                        mAmp2.Draw();
                    }

                    pass.End();
                }
                effect.End();
                float xscale = -Global.ScreenWidth * 0.2f, scale = (Global.ScreenWidth / 1024f);
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

                {//rocklev
                    float x = 0.2f * (int)OPTIONS.OPT_ROCKLEVEL, y = 0.3f+(((int)OPTIONS.OPT_ROCKLEVEL%2)*0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * Global.ScreenWidth + optionsOffset * xscale), (int)(y * Global.ScreenHeight), (int)(0.02f*Global.ScreenWidth+Global.DefaultFont.MeasureString(Localizer.Get("Rock Level")).X*scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Rock Level"), new Vector2(((x + 0.08f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.01f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, "" + rockLevel, new Vector2(((x + 0.1f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.05f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(tknob, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_ROCKLEVEL ? Color.White : Color.Gray, (rockLevel / 10f) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//res
                    float x = 0.2f * (int)OPTIONS.OPT_RESOLUTION, y = 0.3f + (((int)OPTIONS.OPT_RESOLUTION % 2) * 0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * Global.ScreenWidth + optionsOffset * xscale), (int)(y * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Global.DefaultFont.MeasureString(Localizer.Get("Resolution")).X * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Resolution"), new Vector2(((x + 0.08f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.01f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, "" + Configuration.ResolutionWidthOptions[Configuration.ResIndex] + "x" + (Configuration.WideScreen?Configuration.ResolutionHeightWideOptions[Configuration.ResIndex]:Configuration.ResolutionHeightFullOptions[Configuration.ResIndex]), new Vector2(((x + 0.08f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.05f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(tknob, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_RESOLUTION ? Color.White : Color.Gray, (Configuration.ResIndex / (float)(Configuration.ResolutionWidthOptions.Length - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//gui
                    float x = 0.2f * (int)OPTIONS.OPT_GUISTYLE, y = 0.3f + (((int)OPTIONS.OPT_GUISTYLE % 2) * 0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * Global.ScreenWidth + optionsOffset * xscale), (int)(y * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Math.Max(Global.DefaultFont.MeasureString(Localizer.Get("HUD Style")).X, Global.DefaultFont.MeasureString(guiStyle[(int)Configuration.GUIStyle]).X) * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("HUD Style"), new Vector2(((x + 0.08f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.01f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, guiStyle[(int)Configuration.GUIStyle], new Vector2(((x + 0.08f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.05f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(tknob, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_GUISTYLE ? Color.White : Color.Gray, ((float)Configuration.GUIStyle / (guiStyle.Length - 1)) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//3don
                    float x = 0.2f * (int)OPTIONS.OPT_RENDER3D, y = 0.3f + (((int)OPTIONS.OPT_RENDER3D % 2) * 0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * Global.ScreenWidth + optionsOffset * xscale), (int)((y - 0.05f) * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Global.DefaultFont.MeasureString(Localizer.Get("3D Mode")).X * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("3D Mode"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y - 0.04f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, Configuration.RenderVenues ? Localizer.Get("On") : Localizer.Get("Off"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.RenderVenues ? tswitchon : tswitchoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_RENDER3D ? Color.White : Color.Gray, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.RenderVenues ? tledon : tledoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), (y - 0.13f) * Global.ScreenHeight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//fulls
                    float x = 0.2f * (int)OPTIONS.OPT_FULLSCREEN, y = 0.3f + (((int)OPTIONS.OPT_FULLSCREEN % 2) * 0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * Global.ScreenWidth + optionsOffset * xscale), (int)((y - 0.05f) * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Global.DefaultFont.MeasureString(Localizer.Get("Full Screen")).X * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Full Screen"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y - 0.04f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, Configuration.FullScreen ? Localizer.Get("On") : Localizer.Get("Off"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.FullScreen ? tswitchon : tswitchoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_FULLSCREEN ? Color.White : Color.Gray, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.FullScreen ? tledon : tledoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), (y - 0.13f) * Global.ScreenHeight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//light
                    float x = 0.2f * (int)OPTIONS.OPT_LIGHTING, y = 0.3f+(((int)OPTIONS.OPT_LIGHTING%2)*0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * Global.ScreenWidth + optionsOffset * xscale), (int)((y - 0.05f) * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Global.DefaultFont.MeasureString(Localizer.Get("Lighting")).X * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Lighting"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y - 0.04f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, Configuration.Lighting ? Localizer.Get("On") : Localizer.Get("Off"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.Lighting ? tswitchon : tswitchoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_LIGHTING ? Color.White : Color.Gray, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.Lighting ? tledon : tledoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), (y - 0.13f) * Global.ScreenHeight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//normalmapping
                    float x = 0.2f * (int)OPTIONS.OPT_NORMALMAPPING, y = 0.3f + (((int)OPTIONS.OPT_NORMALMAPPING % 2) * 0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * Global.ScreenWidth + optionsOffset * xscale), (int)((y - 0.05f) * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Global.DefaultFont.MeasureString(Localizer.Get("Normal Mapping")).X * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Normal Mapping"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y - 0.04f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, Configuration.NormalMapping ? Localizer.Get("On") : Localizer.Get("Off"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.NormalMapping ? tswitchon : tswitchoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_NORMALMAPPING ? Color.White : Color.Gray, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.NormalMapping ? tledon : tledoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), (y - 0.13f) * Global.ScreenHeight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//specular
                    float x = 0.2f * (int)OPTIONS.OPT_SPECULAR, y = 0.3f + (((int)OPTIONS.OPT_SPECULAR % 2) * 0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.03f) * Global.ScreenWidth + optionsOffset * xscale), (int)((y - 0.05f) * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Global.DefaultFont.MeasureString(Localizer.Get("Specular Highlights")).X * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Specular Highlights"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y - 0.04f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, Configuration.Specular ? Localizer.Get("On") : Localizer.Get("Off"), new Vector2(((x + 0.04f) * Global.ScreenWidth + optionsOffset * xscale), (y) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.Specular ? tswitchon : tswitchoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_SPECULAR ? Color.White : Color.Gray, 0, new Vector2(tswitchon.Width / 2, tswitchon.Height / 2), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(Configuration.Specular ? tledon : tledoff, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), (y - 0.13f) * Global.ScreenHeight), null, Color.White, 0, new Vector2(tledon.Width / 2, tledon.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//language
                    float x = 0.2f * (int)OPTIONS.OPT_LANGUAGE, y = 0.3f + (((int)OPTIONS.OPT_LANGUAGE % 2) * 0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * Global.ScreenWidth + optionsOffset * xscale), (int)(y * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Global.DefaultFont.MeasureString(Localizer.Get("Language")).X * 1.5f * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Language"), new Vector2(((x + 0.08f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.01f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, "" + languageNames[(int)Configuration.CurrentLanguage][(int)Configuration.CurrentLanguage], new Vector2(((x + 0.1f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.05f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(tknob, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_LANGUAGE ? Color.White : Color.Gray, ((int)Configuration.CurrentLanguage / (float)Localizer.Language.Length) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale, SpriteEffects.None, 0);
                }
                {//particle detail
                    float x = 0.2f * (int)OPTIONS.OPT_PARTICLELEVEL, y = 0.3f + (((int)OPTIONS.OPT_PARTICLELEVEL % 2) * 0.3f);
                    spriteBatch.Draw(ttape, new Rectangle((int)((x + 0.07f) * Global.ScreenWidth + optionsOffset * xscale), (int)(y * Global.ScreenHeight), (int)(0.02f * Global.ScreenWidth + Global.DefaultFont.MeasureString(Localizer.Get("Particle Detail")).X * 1.5f * scale), (int)(0.1f * Global.ScreenHeight)), Color.White);
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Particle Detail"), new Vector2(((x + 0.08f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.01f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), Global.ScreenWidth / 1024f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(Global.DefaultFont, "" + Configuration.ParticleDetails[(int)Configuration.ParticleDetail], new Vector2(((x + 0.1f) * Global.ScreenWidth + optionsOffset * xscale), (y + 0.05f) * Global.ScreenHeight), Color.Black, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(tknob, new Vector2((x * Global.ScreenWidth + optionsOffset * xscale), y * Global.ScreenHeight), null, optionsSelected == OPTIONS.OPT_PARTICLELEVEL ? Color.White : Color.Gray, ((int)Configuration.ParticleDetail / (float)Configuration.ParticleDetails.Length) * -MathHelper.Pi, new Vector2(tknob.Width / 2, tknob.Height / 2), scale, SpriteEffects.None, 0);
                }


                if (knobBroken > 4)
                {
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("This knob appears to be broken"), new Vector2(Global.ScreenWidth * 0.1f, Global.ScreenHeight * 0.7f), new Color(Color.Orange,(byte)((1-(knobBroken-4))*255)));
                }
                else if (knobBroken > 1)
                {
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("This knob appears to be broken"), new Vector2(Global.ScreenWidth * 0.1f, Global.ScreenHeight * 0.7f), Color.Orange);
                }
                else if (knobBroken > 0)
                {
                    spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("This knob appears to be broken"), new Vector2(Global.ScreenWidth * 0.1f, Global.ScreenHeight * 0.7f), new Color(Color.Orange, (byte)(knobBroken * 255)));
                }

                /*spriteBatch.Draw(GameUIMaster.Singleton.texButtonGreen, new Rectangle((int)(0.1f * Global.ScreenWidth), (int)(0.80f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Select"), new Vector2((0.1f * Global.ScreenWidth) + (0.10f * Global.ScreenHeight), (0.80f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString(Localizer.Get("Select")).Y)), Color.White);
                spriteBatch.Draw(GameUIMaster.Singleton.texButtonRed, new Rectangle((int)(0.9f * Global.ScreenWidth) - (int)(0.09f * Global.ScreenHeight), (int)(0.80f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Back"), new Vector2((0.9f * Global.ScreenWidth) - (0.10f * Global.ScreenHeight) - Global.DefaultFont.MeasureString(Localizer.Get("Back")).X, (0.80f * Global.ScreenHeight) + (0.09f * Global.ScreenHeight) - (Global.DefaultFont.MeasureString(Localizer.Get("Back")).Y)), Color.White);*/
                spriteBatch.End();
#if !DEBUG
            }
            catch(Exception e)
            {
                Debug.Error("Problem in OptionScreen.Draw[2]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
#endif
        }
    }
}
