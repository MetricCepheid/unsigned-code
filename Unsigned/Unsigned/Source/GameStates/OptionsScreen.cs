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

        private float knobBroken;

        private float intro;

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
            OPT_CROWD,
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

        private int majorIndex, minorIndex;
        private bool selectedMajor, selectedMinor;
        private Vector3 camPos;

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
                o.options[2] = OPTIONS.OPT_CROWD;
                o.options[3] = OPTIONS.OPT_GUISTYLE;
                o.options[4] = OPTIONS.OPT_PARTICLELEVEL;
                o.options[5] = OPTIONS.OPT_WAVELEVEL;
                Options.Add(o);
            }
            {
                OptionsSet o = new OptionsSet("Audio");
                o.options[0] = OPTIONS.OPT_SFXVOL;
                o.options[1] = OPTIONS.OPT_MUSICVOL;
                Options.Add(o);
            } 
            camPos = new Vector3(-10f, 120f, 40f);
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
                    offset--;
                if (conts[i].WasPressed(PeripheralButton.UP))
                    offset++;
                if (conts[i].WasPressed(PeripheralButton.CONFIRM))
                    green = true;
                if (conts[i].WasPressed(PeripheralButton.BACK))
                    red = true;
            }
            if (!selectedMajor)
            {
                majorIndex -= offset;
                if (majorIndex < 0)
                    majorIndex = Options.Count-1;
                if (majorIndex >= Options.Count)
                    majorIndex = 0;
            }
            else if (!selectedMinor)
            {
                offset = -offset;
                if (offset > 0)
                    for (int i = 0; i < offset; i++)
                    {
                        minorIndex++;
                        while (minorIndex >= Options[majorIndex].options.Length || Options[majorIndex].options[minorIndex] == OPTIONS.NONE)
                        {
                            if (minorIndex >= Options[majorIndex].options.Length)
                                minorIndex = 0;
                            else
                                minorIndex++;
                        }
                    }

                if (offset < 0)
                    for (int i = 0; i < -offset; i++)
                    {
                        minorIndex--;
                        while (minorIndex < 0 || Options[majorIndex].options[minorIndex] == OPTIONS.NONE)
                        {
                            if (minorIndex < 0)
                                minorIndex = Options[majorIndex].options.Length - 1;
                            else
                                minorIndex--;
                        }
                    }
            }
            else
            {
                if (offset > 0)
                    for (int i = 0; i < offset; i++)
                        IncrementValue(Options[majorIndex].options[minorIndex]);
                if (offset < 0)
                    for (int i = 0; i < -offset; i++)
                        DecrementValue(Options[majorIndex].options[minorIndex]);
            }

            if (green)
            {
                if (!selectedMajor)
                    selectedMajor = true;
                else if (!selectedMinor)
                    selectedMinor = true;
                else
                    selectedMinor = false;
            }

            if (red)
            {
                if (selectedMinor)
                {
                    selectedMinor = false;
                }
                else if (selectedMajor)
                {
                    selectedMajor = false;
                    minorIndex = 0;
                }
                else
                    UnsignedGame.Singleton.SwitchState(new MainMenuScreen());
            }

            if (knobBroken > 0)
                knobBroken -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 targPos = new Vector3(-15f + (majorIndex * 15f), 120f, 40f);
            if(selectedMajor)
            {
                targPos.Z = 15f;
                if (minorIndex < 3)
                    targPos.X -= 2;
                else
                    targPos.X += 2;
                if (minorIndex % 3 == 0)
                    targPos.Y += 8;
                if (minorIndex % 3 == 1)
                    targPos.Y += 5;
                if (minorIndex % 3 == 2)
                    targPos.Y += 2;
            }
            float camLerp = (float)gameTime.ElapsedGameTime.TotalSeconds*10;
            camPos = ((1 - camLerp) * camPos) + ((camLerp) * targPos);
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
                effect.DirectionalLight = new DirectionalLight(true, new Vector3(1f, -0f, 3f), new Color(200, 200, 200), Color.White);
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

                    Matrix matView = Matrix.CreateLookAt(camPos, camPos-new Vector3(0,0,1), new Vector3(0, 1, 0));
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Global.ScreenWidth / (float)Global.ScreenHeight, 1f, 1000f);
                    //render the background graphics
                    effect.View = matView;

                    Matrix matRot, matScale, matTranslate;
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

                    {
                        matTranslate = Matrix.CreateTranslation(-15, 120, 0f);
                        matRot = Matrix.Identity;
                        matScale = Matrix.CreateScale(10, 10, 10);

                        effect.World = matScale * matRot * matTranslate;
                        effect.DiffuseTexture = gibsonTex;
                        effect.NormalMapTexture = Global.TexDefaultBM;
                        effect.Shininess = 32f;
                        effect.CommitChanges();

                        gibsonMdl.Draw();

                        matTranslate = Matrix.CreateTranslation(0, 120, 0f);

                        effect.World = matScale * matRot * matTranslate;
                        effect.CommitChanges();

                        gibsonMdl.Draw();

                        matTranslate = Matrix.CreateTranslation(15, 120, 0f);

                        effect.World = matScale * matRot * matTranslate;
                        effect.CommitChanges();

                        gibsonMdl.Draw();
                    }

                    pass.End();
                }
                effect.End();


                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

                if (selectedMajor)
                {
                    for (int i = 0; i < Options[majorIndex].options.Length; i++)
                    {
                        if (Options[majorIndex].options[i] != OPTIONS.NONE)
                        {
                            Vector3 textPos = new Vector3(-15f + (majorIndex * 15f), 120f, 0f);
                            if (i < 3)
                                textPos.X -= 2;
                            else
                                textPos.X += 2;
                            if (i % 3 == 0)
                                textPos.Y += 8;
                            if (i % 3 == 1)
                                textPos.Y += 5;
                            if (i % 3 == 2)
                                textPos.Y += 2;
                            textPos = Global.Graphics.GraphicsDevice.Viewport.Project(textPos, effect.Projection, effect.View, Matrix.Identity);
                            {
                                String str = GetValue(Options[majorIndex].options[i]);
                                spriteBatch.DrawString(Global.DefaultFont, str, new Vector2(textPos.X, textPos.Y), i == minorIndex ? (selectedMinor ? Color.Lime : Global.UnsignedYellow) : Global.UnsignedOrange, 0, new Vector2(i < 3 ? Global.DefaultFont.MeasureString(str).X : 0, 0), Global.ScreenHeight / 600f, SpriteEffects.None, 0);
                            }
                            {
                                String str = GetTitle(Options[majorIndex].options[i]);
                                spriteBatch.DrawString(Global.DefaultFont, str, new Vector2(textPos.X, textPos.Y), i == minorIndex ? (selectedMinor ? Color.Lime : Global.UnsignedYellow) : Global.UnsignedOrange, 0, new Vector2(i < 3 ? Global.DefaultFont.MeasureString(str).X : 0, Global.DefaultFont.MeasureString(str).Y), Global.ScreenHeight / 600f, SpriteEffects.None, 0);
                            }
                        }
                    }
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

        private String GetValue(OPTIONS option)
        {
            switch (option)
            {
                case OPTIONS.OPT_CROWD:
                    return Configuration.CrowdDetails[Configuration.CrowdDetail];
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

        private String GetTitle(OPTIONS option)
        {
            switch (option)
            {
                case OPTIONS.OPT_CROWD:
                    return "Crowd Detail?";
                case OPTIONS.OPT_FULLSCREEN:
                    return "Full Screen?";
                case OPTIONS.OPT_GUISTYLE:
                    return "HUD Style?";
                case OPTIONS.OPT_LANGUAGE:
                    return "Language?";
                case OPTIONS.OPT_LIGHTING:
                    return "Lighting?";
                case OPTIONS.OPT_MUSICVOL:
                    return "Music Volume?";
                case OPTIONS.OPT_NORMALMAPPING:
                    return "Normal Mapping?";
                case OPTIONS.OPT_PARTICLELEVEL:
                    return "Particle Detail Level?";
                case OPTIONS.OPT_RENDER3D:
                    return "3D or 2D?";
                case OPTIONS.OPT_RENDERVENUES:
                    return "Render Venues?";
                case OPTIONS.OPT_RESOLUTION:
                    return "Resolution?";
                case OPTIONS.OPT_ROCKLEVEL:
                    return "Rock Level?";
                case OPTIONS.OPT_SFXVOL:
                    return "Sound Effects Volume?";
                case OPTIONS.OPT_SPECULAR:
                    return "Specular Highlights?";
                case OPTIONS.OPT_WAVELEVEL:
                    return "Held Note Trails Detail?";
                case OPTIONS.OPT_WIDESCREEN:
                    return "Widescreen?";
            }
            return "INVALID";
        }

        private void IncrementValue(OPTIONS option)
        {
            switch (option)
            {
                case OPTIONS.OPT_CROWD:
                    {
                        Configuration.CrowdDetail++;
                        break;
                    }
                case OPTIONS.OPT_FULLSCREEN:
                    {
                        Configuration.FullScreen = !Configuration.FullScreen;
                        break;
                    }
                case OPTIONS.OPT_GUISTYLE:
                    {
                        const int MAX_GUI_STYLE = 1;
                        int gs = (int)Configuration.GUIStyle;
                        gs++;
                        if (gs > MAX_GUI_STYLE)
                            gs -= (MAX_GUI_STYLE + 1);
                        Configuration.GUIStyle = (GameUIMaster.GUIStyle)gs;
                        break;
                    }
                case OPTIONS.OPT_LANGUAGE:
                    {
                        int lang = (int)Configuration.CurrentLanguage;
                        lang++;
                        if (lang >= (int)Localizer.Language.Length)
                            lang -= (int)Localizer.Language.Length;
                        Configuration.CurrentLanguage = (Localizer.Language)lang;
                        break;
                    }
                case OPTIONS.OPT_LIGHTING:
                    {
                        Configuration.Lighting = !Configuration.Lighting;
                        break;
                    }
                case OPTIONS.OPT_MUSICVOL:
                    {
                        Configuration.MusicVolume += 10;
                        break;
                    }
                case OPTIONS.OPT_NORMALMAPPING:
                    {
                        Configuration.NormalMapping = !Configuration.NormalMapping;
                        break;
                    }
                case OPTIONS.OPT_PARTICLELEVEL:
                    {
                        Configuration.ParticleDetail++;
                        break;
                    }
                case OPTIONS.OPT_RENDER3D:
                    {
                        Configuration.TwoDimensionalMode = !Configuration.TwoDimensionalMode;
                        break;
                    }
                case OPTIONS.OPT_RENDERVENUES:
                    {
                        Configuration.RenderVenues = !Configuration.RenderVenues;
                        break;
                    }
                case OPTIONS.OPT_RESOLUTION:
                    {
                        Rectangle dr = new Rectangle(0, 0, Global.Graphics.GraphicsDevice.DisplayMode.Width, Global.Graphics.GraphicsDevice.DisplayMode.Height);
                        int cf = Configuration.ResIndex;
                        cf++;
                        if (cf >= Configuration.ResolutionWidthOptions.Length)
                            cf = Configuration.ResolutionWidthOptions.Length - 1;
                        while (true)
                        {
                            if (dr.Width < Configuration.ResolutionWidthOptions[cf])
                                cf--;
                            else if (Configuration.WideScreen && dr.Height < Configuration.ResolutionHeightWideOptions[cf])
                                cf--;
                            else if (!Configuration.WideScreen && dr.Height < Configuration.ResolutionHeightFullOptions[cf])
                                cf--;
                            else
                                break;
                        }
                        Configuration.ResIndex = cf;
                        break;
                    }
                case OPTIONS.OPT_ROCKLEVEL:
                    {
                        break;
                    }
                case OPTIONS.OPT_SFXVOL:
                    {
                        Configuration.SoundEffectsVolume += 10;
                        break;
                    }
                case OPTIONS.OPT_SPECULAR:
                    {
                        Configuration.Specular = !Configuration.Specular;
                        break;
                    }
                case OPTIONS.OPT_WAVELEVEL:
                    {
                        Configuration.WaveDetail++;
                        break;
                    }
                case OPTIONS.OPT_WIDESCREEN:
                    {
                        Configuration.WideScreen = !Configuration.WideScreen;
                        Rectangle dr = new Rectangle(0, 0, Global.Graphics.GraphicsDevice.DisplayMode.Width, Global.Graphics.GraphicsDevice.DisplayMode.Height);
                        int cf = Configuration.ResIndex;
                        while (true)
                        {
                            if (dr.Width < Configuration.ResolutionWidthOptions[cf])
                                cf--;
                            else if (Configuration.WideScreen && dr.Height < Configuration.ResolutionHeightWideOptions[cf])
                                cf--;
                            else if (!Configuration.WideScreen && dr.Height < Configuration.ResolutionHeightFullOptions[cf])
                                cf--;
                            else
                                break;
                        }
                        Configuration.ResIndex = cf;
                        break;
                    }
            }
        }

        private void DecrementValue(OPTIONS option)
        {
            switch (option)
            {
                case OPTIONS.OPT_CROWD:
                    {
                        Configuration.CrowdDetail--;
                        break;
                    }
                case OPTIONS.OPT_FULLSCREEN:
                    {
                        Configuration.FullScreen = !Configuration.FullScreen;
                        break;
                    }
                case OPTIONS.OPT_GUISTYLE:
                    {
                        const int MAX_GUI_STYLE = 1;
                        int gs = (int)Configuration.GUIStyle;
                        gs--;
                        if (gs < 0)
                            gs += (MAX_GUI_STYLE + 1);
                        Configuration.GUIStyle = (GameUIMaster.GUIStyle)gs;
                        break;
                    }
                case OPTIONS.OPT_LANGUAGE:
                    {
                        int lang = (int)Configuration.CurrentLanguage;
                        lang--;
                        if (lang < 0)
                            lang += (int)Localizer.Language.Length;
                        Configuration.CurrentLanguage = (Localizer.Language)lang;
                        break;
                    }
                case OPTIONS.OPT_LIGHTING:
                    {
                        Configuration.Lighting = !Configuration.Lighting;
                        break;
                    }
                case OPTIONS.OPT_MUSICVOL:
                    {
                        Configuration.MusicVolume -= 10;
                        break;
                    }
                case OPTIONS.OPT_NORMALMAPPING:
                    {
                        Configuration.NormalMapping = !Configuration.NormalMapping;
                        break;
                    }
                case OPTIONS.OPT_PARTICLELEVEL:
                    {
                        Configuration.ParticleDetail--;
                        break;
                    }
                case OPTIONS.OPT_RENDER3D:
                    {
                        Configuration.TwoDimensionalMode = !Configuration.TwoDimensionalMode;
                        break;
                    }
                case OPTIONS.OPT_RENDERVENUES:
                    {
                        Configuration.RenderVenues = !Configuration.RenderVenues;
                        break;
                    }
                case OPTIONS.OPT_RESOLUTION:
                    {
                        Rectangle dr = new Rectangle(0, 0, Global.Graphics.GraphicsDevice.DisplayMode.Width, Global.Graphics.GraphicsDevice.DisplayMode.Height);
                        int cf = Configuration.ResIndex;
                        cf--;
                        if (cf < 0)
                            cf = 0;
                        while (true)
                        {
                            if (dr.Width < Configuration.ResolutionWidthOptions[cf])
                                cf--;
                            else if (Configuration.WideScreen && dr.Height < Configuration.ResolutionHeightWideOptions[cf])
                                cf--;
                            else if (!Configuration.WideScreen && dr.Height < Configuration.ResolutionHeightFullOptions[cf])
                                cf--;
                            else
                                break;
                        }
                        Configuration.ResIndex = cf;
                        break;
                    }
                case OPTIONS.OPT_ROCKLEVEL:
                    {
                        break;
                    }
                case OPTIONS.OPT_SFXVOL:
                    {
                        Configuration.SoundEffectsVolume -= 10;
                        break;
                    }
                case OPTIONS.OPT_SPECULAR:
                    {
                        Configuration.Specular = !Configuration.Specular;
                        break;
                    }
                case OPTIONS.OPT_WAVELEVEL:
                    {
                        Configuration.WaveDetail--;
                        break;
                    }
                case OPTIONS.OPT_WIDESCREEN:
                    {
                        Configuration.WideScreen = !Configuration.WideScreen;
                        Rectangle dr = new Rectangle(0, 0, Global.Graphics.GraphicsDevice.DisplayMode.Width, Global.Graphics.GraphicsDevice.DisplayMode.Height);
                        int cf = Configuration.ResIndex;
                        while (true)
                        {
                            if (dr.Width < Configuration.ResolutionWidthOptions[cf])
                                cf--;
                            else if (Configuration.WideScreen && dr.Height < Configuration.ResolutionHeightWideOptions[cf])
                                cf--;
                            else if (!Configuration.WideScreen && dr.Height < Configuration.ResolutionHeightFullOptions[cf])
                                cf--;
                            else
                                break;
                        }
                        Configuration.ResIndex = cf;
                        break;
                    }
            }
        }
    }
}
