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
        private Texture2D gibsonTex;
        private FVModel gibsonMdl;

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
            NONE,
            OPT_ROCKLEVEL,
            OPT_LANGUAGE,
            OPT_WIDESCREEN,
            OPT_RESOLUTION,
            OPT_FULLSCREEN,
            OPT_GUISTYLE,
            OPT_RENDER3D,
            OPT_LIGHTING,
            OPT_NORMALMAPPING,
            OPT_SPECULAR,
            OPT_PARTICLELEVEL,
            OPT_RENDERVENUES,
            OPT_WAVELEVEL,
            OPT_MUSICVOL,
            OPT_SFXVOL,
            MAX,
        };

        private struct OptionsSet
        {
            public String Name;
            public OPTIONS[] options;

            public OptionsSet(String name)
            {
                Name = name;
                options = new OPTIONS[6];
            }
        }

        private List<OptionsSet> Options;

        public OptionsScreen()
        {
            Options = new List<OptionsSet>();
            {
                OptionsSet o = new OptionsSet("Graphics");
                o.options[0] = OPTIONS.OPT_FULLSCREEN;
                o.options[1] = OPTIONS.OPT_WIDESCREEN;
                o.options[2] = OPTIONS.OPT_RESOLUTION;
                o.options[3] = OPTIONS.OPT_LIGHTING;
                o.options[4] = OPTIONS.OPT_SPECULAR;
                o.options[5] = OPTIONS.OPT_NORMALMAPPING;
                Options.Add(o);
            }
            {
                OptionsSet o = new OptionsSet("Gameplay");
                o.options[0] = OPTIONS.OPT_RENDER3D;
                o.options[1] = OPTIONS.OPT_RENDERVENUES;
                o.options[2] = OPTIONS.OPT_GUISTYLE;
                o.options[3] = OPTIONS.OPT_PARTICLELEVEL;
                o.options[4] = OPTIONS.OPT_WAVELEVEL;
                Options.Add(o);
            }
            {
                OptionsSet o = new OptionsSet("Audio");
                o.options[0] = OPTIONS.OPT_SFXVOL;
                o.options[1] = OPTIONS.OPT_MUSICVOL;
                Options.Add(o);
            }
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            concrTex = Content.Load<Texture2D>("textures\\Options\\concr");
            concrBM = Content.Load<Texture2D>("textures\\Options\\concrBM");
            gibsonTex = Content.Load<Texture2D>("textures\\Options\\header_gameplay");
            gibsonMdl = ModelLoader.LoadModel("meshes\\Options\\gibsonHeader");

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");
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
                Global.Graphics.ApplyChanges();

                VertexDeclaration vd = new VertexDeclaration(Global.Graphics.GraphicsDevice, VertexTangentBinormal.Elements);
                Global.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

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

                    pass.End();
                }
                effect.End();


                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

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

        private String GetValue(OPTIONS option)
        {
            switch (option)
            {
                case OPTIONS.OPT_FULLSCREEN:
                    return Configuration.FullScreen ? "Full" : "Windowed";
                case OPTIONS.OPT_GUISTYLE:
                    return guiStyle[(int)Configuration.GUIStyle];
                case OPTIONS.OPT_LANGUAGE:
                    return languageNames[(int)Configuration.CurrentLanguage][(int)Configuration.CurrentLanguage];
                case OPTIONS.OPT_LIGHTING:
                    return Configuration.Lighting ? "On" : "Off";
                case OPTIONS.OPT_MUSICVOL:
                    return "" + Configuration.MusicVolume;
                case OPTIONS.OPT_NORMALMAPPING:
                    return Configuration.NormalMapping ? "On" : "Off";
                case OPTIONS.OPT_PARTICLELEVEL:
                    return Configuration.ParticleDetails[Configuration.ParticleDetail];
                case OPTIONS.OPT_RENDER3D:
                    return Configuration.TwoDimensionalMode ? "2D" : "3D";
                case OPTIONS.OPT_RENDERVENUES:
                    return Configuration.RenderVenues ? "On" : "Off";
                case OPTIONS.OPT_RESOLUTION:
                    return Configuration.ResolutionWidthOptions[Configuration.ResIndex] + "x" + (Configuration.WideScreen ? Configuration.ResolutionHeightWideOptions[Configuration.ResIndex] : Configuration.ResolutionHeightFullOptions[Configuration.ResIndex]);
                case OPTIONS.OPT_ROCKLEVEL:
                    return "11";
                case OPTIONS.OPT_SFXVOL:
                    return "" + Configuration.SoundEffectsVolume;
                case OPTIONS.OPT_SPECULAR:
                    return Configuration.Specular ? "On" : "Off";
                case OPTIONS.OPT_WAVELEVEL:
                    return "" + Configuration.WaveDetail;
                case OPTIONS.OPT_WIDESCREEN:
                    return Configuration.WideScreen ? "Wide (16:9)" : "Standard (4:3)";
            }
            return "INVALID";
        }

        private void IncrementValue(OPTIONS option)
        {
            switch (option)
            {
                case OPTIONS.OPT_FULLSCREEN:
                    return Configuration.FullScreen ? "Full" : "Windowed";
                case OPTIONS.OPT_GUISTYLE:
                    return guiStyle[(int)Configuration.GUIStyle];
                case OPTIONS.OPT_LANGUAGE:
                    return languageNames[(int)Configuration.CurrentLanguage][(int)Configuration.CurrentLanguage];
                case OPTIONS.OPT_LIGHTING:
                    return Configuration.Lighting ? "On" : "Off";
                case OPTIONS.OPT_MUSICVOL:
                    return "" + Configuration.MusicVolume;
                case OPTIONS.OPT_NORMALMAPPING:
                    return Configuration.NormalMapping ? "On" : "Off";
                case OPTIONS.OPT_PARTICLELEVEL:
                    return Configuration.ParticleDetails[Configuration.ParticleDetail];
                case OPTIONS.OPT_RENDER3D:
                    return Configuration.TwoDimensionalMode ? "2D" : "3D";
                case OPTIONS.OPT_RENDERVENUES:
                    return Configuration.RenderVenues ? "On" : "Off";
                case OPTIONS.OPT_RESOLUTION:
                    return Configuration.ResolutionWidthOptions[Configuration.ResIndex] + "x" + (Configuration.WideScreen ? Configuration.ResolutionHeightWideOptions[Configuration.ResIndex] : Configuration.ResolutionHeightFullOptions[Configuration.ResIndex]);
                case OPTIONS.OPT_ROCKLEVEL:
                    return "11";
                case OPTIONS.OPT_SFXVOL:
                    return "" + Configuration.SoundEffectsVolume;
                case OPTIONS.OPT_SPECULAR:
                    return Configuration.Specular ? "On" : "Off";
                case OPTIONS.OPT_WAVELEVEL:
                    return "" + Configuration.WaveDetail;
                case OPTIONS.OPT_WIDESCREEN:
                    return Configuration.WideScreen ? "Wide (16:9)" : "Standard (4:3)";
            }
            return "INVALID";
        }
    }
}
