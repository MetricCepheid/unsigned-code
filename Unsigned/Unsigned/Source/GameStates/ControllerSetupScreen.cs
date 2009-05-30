using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;
using SongDataIO;
using FVProductions.Utility;

namespace Unsigned
{
    class ControllerSetupScreen : BaseState
    {
        private ContentManager Content;

        private SpriteBatch spriteBatch;

        private FVShader effect;

        private const float LEFT = -100.0f;
        private const float SPACING = 65.0f;
        private const float SCALE = 20.0f;

        private const float LEFT2 = 150.0f;
        private const float SPACING2 = 128.0f;
        private const float SCALE2 = 80.0f;

        private RenderTarget2D[] rtNote;
        private Texture2D[] texNote;
        private Texture2D[] nPadTex;
        private Texture2D texNoteBM;
        private Texture2D concrTex, concrBM;
        private Texture2D flameTex, hairr, hairl;
        private Texture2D texBorder;
        private FVModel mPad;

        private float joinAlpha, joinBlue, joinBlueDir;

        SessionInfo nugget;

        private float[] arrowTimer, arrowRot;
        private Vector3[][] flames;
        private bool notesNeedRefreshing;

        float borderScroll = 0;

        public ControllerSetupScreen()
        {
            texNote = new Texture2D[4];
            nPadTex = new Texture2D[4];
            arrowRot = new float[4];
            arrowTimer = new float[4];
            flames = new Vector3[4][];
            nugget = new SessionInfo();
            joinAlpha = 0;
            joinBlue = 0;
            joinBlueDir = 1;
        }

        public ControllerSetupScreen(SessionInfo sesInfo)
        {
            texNote = new Texture2D[4];
            nPadTex = new Texture2D[4];
            arrowRot = new float[4];
            arrowTimer = new float[4];
            flames = new Vector3[4][];
            nugget = sesInfo;
            for (int i = 0; i < nugget.confirmStates.Length; i++)
                if (nugget.confirmStates[i] == SessionInfo.ConfirmState.FULLY_CONFIRMED)
                    nugget.confirmStates[i] = SessionInfo.ConfirmState.CHOOSING_INSTRUMENT;
            joinAlpha = 0;
            joinBlue = 0;
            joinBlueDir = 1;
        }

        public override void Load()
        {
            Content = new ContentManager(Global.Services);
            Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);

            effect = new FVShader(Global.Graphics.GraphicsDevice, Content.Load<Effect>("shaders\\UnsignedEngineShader"), "maintechnique");

            mPad = ModelLoader.LoadModel("meshes\\ControllerSetup\\paper1");
            texNoteBM = Content.Load<Texture2D>("textures\\ControllerSetup\\notebm");
            nPadTex = new Texture2D[4];
            nPadTex[0] = Content.Load<Texture2D>("textures\\ControllerSetup\\paper1");
            nPadTex[1] = Content.Load<Texture2D>("textures\\ControllerSetup\\paper2");
            nPadTex[2] = Content.Load<Texture2D>("textures\\ControllerSetup\\paper3");
            nPadTex[3] = Content.Load<Texture2D>("textures\\ControllerSetup\\paper4");
            flames = new Vector3[4][];
            flames[0] = new Vector3[50];
            flames[1] = new Vector3[50];
            flames[2] = new Vector3[50];
            flames[3] = new Vector3[50];
            concrTex = Content.Load<Texture2D>("textures\\ControllerSetup\\concr");
            concrBM = Content.Load<Texture2D>("textures\\ControllerSetup\\concrBM");
            hairl = Content.Load<Texture2D>("textures\\ControllerSetup\\hairl");
            hairr = Content.Load<Texture2D>("textures\\ControllerSetup\\hairr");
            flameTex = Content.Load<Texture2D>("textures\\ControllerSetup\\flame");
            texBorder = Content.Load<Texture2D>("textures\\ControllerSetup\\songscreenbottom");
        }

        public override void Unload()
        {
            Content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            try
            {
                if (joinBlue >= 1)
                    joinBlueDir = -1;
                if (joinBlue <= 0)
                    joinBlueDir = 1;
                joinBlue += (float)gameTime.ElapsedGameTime.TotalSeconds * joinBlueDir;

                bool anyfree = false;
                for (int k = 0; k < 4; k++)
                    if (nugget.confirmStates[k] == SessionInfo.ConfirmState.EMPTY_SLOT)
                        anyfree = true;

                if (anyfree)
                    joinAlpha = (float)Math.Min(1.0, joinAlpha + gameTime.ElapsedGameTime.TotalSeconds);
                else
                    joinAlpha = (float)Math.Max(0.0, joinAlpha - gameTime.ElapsedGameTime.TotalSeconds);

                if (rtNote == null || notesNeedRefreshing)
                {
                    if (rtNote == null)
                    {
                        rtNote = new RenderTarget2D[4];
                        rtNote[0] = new RenderTarget2D(Global.Graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtNote[1] = new RenderTarget2D(Global.Graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtNote[2] = new RenderTarget2D(Global.Graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                        rtNote[3] = new RenderTarget2D(Global.Graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        Global.Graphics.GraphicsDevice.SetRenderTarget(0, rtNote[i]);
                        spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None);

                        spriteBatch.Draw(nPadTex[i], new Rectangle(0, 0, 256, 256), nugget.confirmStates[i] == SessionInfo.ConfirmState.EMPTY_SLOT ? Color.Gray : Color.White);
                        if (nugget.confirmStates[i] >= SessionInfo.ConfirmState.CHOOSING_NAME)
                        {
                            bool alsochosen = false;
                            for (int k = 0; k < 4; k++)
                                if (k == i)
                                    continue;
                                else if (nugget.confirmStates[k] == SessionInfo.ConfirmState.EMPTY_SLOT)
                                    continue;
                                else if (nugget.characterIndices[i] == nugget.characterIndices[k])
                                    alsochosen = true;
                            if (nugget.confirmStates[i] >= SessionInfo.ConfirmState.CHOOSING_INSTRUMENT)
                                for (int r = 0; r < 20; r++)
                                    spriteBatch.DrawString(Global.DefaultFont, nugget.characterIndices[i] < 0 ? "[Create New]" : CharacterMaster.Singleton.GetCharacter(nugget.characterIndices[i]).name, new Vector2(70 - r, 20 - (r / 2)),
                                        new Color(128, 85, 0, (byte)(50 - (r * 2))), 0, new Vector2(0, 0),
                                        (rtNote[i].Width - (80 - (r * 2))) / Global.DefaultFont.MeasureString(nugget.characterIndices[i] < 0 ? "[Create New]" : CharacterMaster.Singleton.GetCharacter(nugget.characterIndices[i]).name).X,
                                        SpriteEffects.None, 0);
                            spriteBatch.DrawString(Global.DefaultFont, nugget.characterIndices[i]<0?"[Create New]":CharacterMaster.Singleton.GetCharacter(nugget.characterIndices[i]).name, new Vector2(70, 20),
                                alsochosen ? Color.Red : Color.Black, 0, new Vector2(0, 0),
                                (rtNote[i].Width - 80) / Global.DefaultFont.MeasureString(nugget.characterIndices[i] < 0 ? "[Create New]" : CharacterMaster.Singleton.GetCharacter(nugget.characterIndices[i]).name).X,
                                SpriteEffects.None, 0);
                        }

                        if (nugget.confirmStates[i] >= SessionInfo.ConfirmState.CHOOSING_INSTRUMENT)
                        {
                            if (nugget.confirmStates[i] >= SessionInfo.ConfirmState.FULLY_CONFIRMED)
                                for (int r = 0; r < 20; r++)
                                    spriteBatch.DrawString(Global.DefaultFont, InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).FullName, new Vector2(70 - r, 200 + (r / 2)),
                                        new Color(128, 85, 0, (byte)(50 - (r * 2))), 0, new Vector2(0, 0),
                                        (rtNote[i].Width - (80 - (r * 2))) / Global.DefaultFont.MeasureString(InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).FullName).X,
                                        SpriteEffects.None, 0);
                            spriteBatch.DrawString(Global.DefaultFont, InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).FullName, new Vector2(70, 200),
                                Color.Black, 0, new Vector2(0, 0),
                                (rtNote[i].Width - 80) / Global.DefaultFont.MeasureString(InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).FullName).X,
                                SpriteEffects.None, 0);
                        }

                        spriteBatch.End();
                        Global.Graphics.GraphicsDevice.SetRenderTarget(0, null);
                        texNote[i] = rtNote[i].GetTexture();
                    }
                    notesNeedRefreshing = false;
                }
            }
            catch(Exception e)
            {
                Debug.Error("Problem in ControllerSetupScreen.Update[1]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
                Peripheral[] controllers = PeripheralManager.Singleton.GetPeripherals();

                borderScroll += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (borderScroll >= MathHelper.Pi * 2)
                    borderScroll -= MathHelper.Pi * 2;

                for (int i = 0; i < nugget.peripherals.Length; i++)
                {
                    if (nugget.confirmStates[i] == SessionInfo.ConfirmState.FULLY_CONFIRMED)
                    {
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
                        {
                            nugget.confirmStates[i] = SessionInfo.ConfirmState.CHOOSING_INSTRUMENT;
                            notesNeedRefreshing = true;
                            continue;
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                        {
                            bool isLeader = true;
                            for (int k = i-1; k >= 0; k--)
                                if (nugget.confirmStates[k] >= SessionInfo.ConfirmState.EMPTY_SLOT)
                                    isLeader = false;
                            if (isLeader)
                            {
                                bool allReady = true;
                                for (int k = 0; k < 4; k++)
                                    if (nugget.confirmStates[k] > SessionInfo.ConfirmState.EMPTY_SLOT && nugget.confirmStates[k] < SessionInfo.ConfirmState.FULLY_CONFIRMED)
                                        allReady = false;
                                if (allReady)
                                {
                                    UnsignedGame.Singleton.SwitchState(new SongSelectScreen(nugget));
                                }
                            }
                        }
                        for (int k = 0; k < flames[i].Length; k++)
                            if (flames[i][k].Z <= 0)
                            {
                                flames[i][k] = new Vector3((Global.ScreenWidth * 0.1625f) + (i * 0.1875f * Global.ScreenWidth) + ((float)(Global.Random.NextDouble() * 0.1719f * Global.ScreenWidth)), (Global.ScreenHeight*0.1667f) + (float)(Global.Random.NextDouble() * 0.28f * Global.ScreenHeight), 1);
                                break;
                            }
                    }
                    else if (nugget.confirmStates[i] == SessionInfo.ConfirmState.CHOOSING_INSTRUMENT)
                    {
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
                        {
                            nugget.instruments[i] = 0;
                            nugget.confirmStates[i] = SessionInfo.ConfirmState.CHOOSING_NAME;
                            notesNeedRefreshing = true;
                            continue;
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                        {
                            nugget.confirmStates[i] = SessionInfo.ConfirmState.FULLY_CONFIRMED;
                            notesNeedRefreshing = true;
                            continue;
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.DOWN))
                        {
                            nugget.instruments[i]++;
                            if(nugget.instruments[i]>=InstrumentMaster.Singleton.GetNumInstruments())
                                nugget.instruments[i]=0;
                            String[] possibles = nugget.peripherals[i].GetSupportedInstruments();
                            bool good = false;
                            for(int k=0;k<possibles.Length;k++)
                                if (possibles[k].Equals(InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).CodeName))
                                {good = true;break;}
                            while (!good)
                            {
                                nugget.instruments[i]++;
                                if (nugget.instruments[i] >= InstrumentMaster.Singleton.GetNumInstruments())
                                    nugget.instruments[i] = 0;
                                good = false;
                                for (int k = 0; k < possibles.Length; k++)
                                    if (possibles[k].Equals(InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).CodeName))
                                    { good = true; break; }
                            }
                            notesNeedRefreshing = true;
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.UP))
                        {
                            nugget.instruments[i]--;
                            if (nugget.instruments[i] < 0)
                                nugget.instruments[i] = InstrumentMaster.Singleton.GetNumInstruments() - 1;
                            String[] possibles = nugget.peripherals[i].GetSupportedInstruments();
                            bool good = false;
                            for (int k = 0; k < possibles.Length; k++)
                                if (possibles[k].Equals(InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).CodeName))
                                { good = true; break; }
                            while (!good)
                            {
                                nugget.instruments[i]--;
                                if (nugget.instruments[i] < 0)
                                    nugget.instruments[i] = InstrumentMaster.Singleton.GetNumInstruments() - 1;
                                good = false;
                                for (int k = 0; k < possibles.Length; k++)
                                    if (possibles[k].Equals(InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).CodeName))
                                    { good = true; break; }
                            }
                            notesNeedRefreshing = true;
                        }
                    }
                    else if (nugget.confirmStates[i] == SessionInfo.ConfirmState.CHOOSING_NAME)
                    {
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
                        {
                            nugget.peripherals[i] = null;
                            nugget.confirmStates[i] = SessionInfo.ConfirmState.EMPTY_SLOT;
                            notesNeedRefreshing = true;
                            continue;
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                        {
                            // determing whether or not this player is trying to confirm with
                            // a name that someone else wants
                            bool hassamename = false;
                            for (int k = 0; k < nugget.peripherals.Length; k++)
                                if (k == i)
                                    continue;
                                else if (nugget.confirmStates[k] == SessionInfo.ConfirmState.EMPTY_SLOT)
                                    continue;
                                else if (nugget.characterIndices[k] == nugget.characterIndices[i])
                                    hassamename = true;
                            // TODO: check if character has role compatible with peripheral
                            if (!hassamename)
                            {
                                nugget.confirmStates[i] = SessionInfo.ConfirmState.CHOOSING_INSTRUMENT;
                                String[] possibles = nugget.peripherals[i].GetSupportedInstruments();
                                bool good = false;
                                for (int k = 0; k < possibles.Length; k++)
                                    if (possibles[k].Equals(InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).CodeName))
                                    { good = true; break; }
                                while (!good)
                                {
                                    nugget.instruments[i]++;
                                    if (nugget.instruments[i] >= InstrumentMaster.Singleton.GetNumInstruments())
                                        nugget.instruments[i] = 0;
                                    good = false;
                                    for (int k = 0; k < possibles.Length; k++)
                                        if (possibles[k].Equals(InstrumentMaster.Singleton.GetInstrument(nugget.instruments[i]).CodeName))
                                        { good = true; break; }
                                }
                                notesNeedRefreshing = true;
                                continue;
                            }
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.DOWN))
                        {
                            if (nugget.characterIndices[i] < CharacterMaster.Singleton.GetNumCharacters() - 1)
                                nugget.characterIndices[i]++;
                            notesNeedRefreshing = true;
                        }
                        if (nugget.peripherals[i].WasPressed(PeripheralButton.UP))
                        {
                            if (nugget.characterIndices[i] > 0)
                                nugget.characterIndices[i]--;
                            notesNeedRefreshing = true;
                        }
                    }
                }

                bool red = false;
                bool allfree = true;

                // For EMPTY_SLOTs
                for (int i = 0; i < controllers.Length; i++)
                {
                    bool alreadyowned = false;

                    // if this controller is already assigned, ignore it
                    for (int k = 0; k < nugget.peripherals.Length; k++)
                        if (nugget.peripherals[k] == controllers[i])
                            alreadyowned = true;
                    if (alreadyowned)
                        continue;

                    // if this unassigned controller presses confirm, put it in the next empty slot
                    if (controllers[i].WasPressed(PeripheralButton.CONFIRM))
                    {
                        for (int k = 0; k < nugget.peripherals.Length; k++)
                            if (nugget.confirmStates[k]==SessionInfo.ConfirmState.EMPTY_SLOT)
                            {
                                nugget.peripherals[k] = controllers[i];
                                nugget.confirmStates[k] = SessionInfo.ConfirmState.CHOOSING_NAME;
                                nugget.characterIndices[k] = -1;
                                notesNeedRefreshing = true;
                                break;
                            }
                    }
                    if (controllers[i].WasPressed(PeripheralButton.BACK))
                    {
                        red = true;
                    }
                }

                for (int k = 0; k < 4; k++)
                    if (nugget.confirmStates[k] != SessionInfo.ConfirmState.EMPTY_SLOT)
                        allfree = false;

                if (red)
                    if(allfree && !notesNeedRefreshing)
                        UnsignedGame.Singleton.SwitchState(new MainMenuScreen());
            }
            catch(Exception e)
            {
                Debug.Error("Problem in ControllerSetupScreen.Update[2]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
                for (int k = 0; k < 4; k++)
                    for (int i = 0; i < flames[k].Length; i++)
                        if (flames[k][i].Z > 0)
                        {
                            flames[k][i].Z -= gameTime.ElapsedGameTime.Milliseconds / 500f;
                            flames[k][i].Y -= (k + 1) * 1.5f * gameTime.ElapsedGameTime.Milliseconds / 50f;
                            flames[k][i].X += (float)(Global.Random.NextDouble() - 0.5) * gameTime.ElapsedGameTime.Milliseconds / 25f;
                        }
            }
            catch(Exception e)
            {
                Debug.Error("Problem in ControllerSetupScreen.Update[3]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
        }

        public override void Render(GameTime gameTime)
        {
            bool[] plo = new bool[16];
            Vector3[] plp = new Vector3[16];
            float[] pln = new float[16];
            float[] plf = new float[16];
            Vector3[] pld = new Vector3[16];
            Vector3[] pls = new Vector3[16];
            try
            {
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                Global.Graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                Global.Graphics.ApplyChanges();

                Global.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

                effect.NormalMapTexture = Global.TexDefaultBM;
                effect.AmbientMaterial = new Color(24, 24, 24);
                effect.DiffuseMaterial = Color.White;
                effect.SpecularMaterial = Color.White;
                effect.SpecularMapTexture = Global.TexWhite;

                Random r = Global.Random;

                effect.TextureEnabled = true;
                effect.LightingEnabled = Configuration.Lighting;
                effect.SpecularEnabled = Configuration.Specular;
                effect.NormalMapEnabled = Configuration.NormalMapping;
                effect.DirectionalLight = new DirectionalLight(true, new Vector3(0, -0.1f, 1), Color.White, Color.White);

                effect.CommitChanges();
            }
            catch(Exception e)
            {
                Debug.Error("Problem in ControllerSetupScreen.Draw[1]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Global.ScreenWidth / (float)Global.ScreenHeight, 1f, 1000f);

                    effect.View = Matrix.CreateLookAt(new Vector3(-8, 128, 200), new Vector3(-8, 128, 200-1), new Vector3(0, 1, 0)); ;

                    Matrix matRot, matScale, matTranslate;
                    {
                        matTranslate = Matrix.CreateTranslation(-32, 120, -128);
                        matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                        matScale = Matrix.CreateScale(300, 1, 192);

                        effect.World = matScale * matRot * matTranslate;
                        effect.DiffuseTexture = concrTex;
                        effect.NormalMapTexture = concrBM;
                        effect.SpecularMaterial = new Color(0.01f, 0.01f, 0.01f);
                        effect.Shininess = 1f;
                        effect.CommitChanges();

                        Global.Graphics.GraphicsDevice.DrawSquare();
                    }

                    Global.Graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;

                    for (int i = 0; i < 4; i++)
                    {
                        matTranslate = Matrix.CreateTranslation(LEFT + (SPACING * i), 192, -126);
                        matRot = Matrix.Identity;
                        matScale = Matrix.CreateScale(SCALE);

                        effect.World = matScale * matRot * matTranslate;
                        effect.DiffuseTexture = texNote[i];
                        effect.NormalMapTexture =texNoteBM;
                        effect.SpecularMaterial = new Color(0.1f,0.1f,0.1f);
                        effect.CommitChanges();

                        mPad.Draw();
                    }
                    Global.Graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                    pass.End();
                }
                effect.End();
            }
            catch (Exception e)
            {
                Debug.Error("Problem in ControllerSetupScreen.Draw[2]", e);
                UnsignedGame.Singleton.Exit();
                return;
            }
            try
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);

                //if (leader >= 0 && contguis[leader].status == 2)
                    //spriteBatch.Draw(texContinue, new Rectangle((int)(Window.ClientBounds.Width * (contguis[leader].loc) / 6), Window.ClientBounds.Height - (Window.ClientBounds.Height / 5), Window.ClientBounds.Width / 6, Window.ClientBounds.Height / 6), Color.Red);

                for (int i = -1; i < 3; i++)
                    spriteBatch.Draw(texBorder, new Rectangle((int)((i * Global.ScreenWidth) + (((borderScroll) / (MathHelper.Pi * 4)) * Global.ScreenWidth)), (int)(Global.ScreenHeight * (0.8f + (-Math.Sin(borderScroll) * 0.01f))), Global.ScreenWidth, Global.ScreenWidth / 4), Color.White);
                for (int i = 0; i < 4; i++)
                    spriteBatch.Draw(texBorder, new Rectangle((int)((i * Global.ScreenWidth) + (((-borderScroll) / (MathHelper.Pi*4)) * Global.ScreenWidth)), (int)(Global.ScreenHeight * (0.9f+(Math.Sin(borderScroll)*0.02f))), Global.ScreenWidth, Global.ScreenWidth / 4), Color.White);
                spriteBatch.Draw(Global.TexWhite, new Rectangle(0, 0, Global.ScreenWidth, (int)(Global.ScreenHeight * 0.05f)), new Color(255, 190, 0));

                Global.Graphics.GraphicsDevice.EnableAdditiveBlending();
                for (int i = 0; i < 4; i++)
                    for (int k = 0; k < flames[i].Length; k++)
                    {
                        float scale = (1 - flames[i][k].Z) * 128;
                        if(flames[i][k].Z<1 && flames[i][k].Z>0)
                            spriteBatch.Draw(flameTex, new Rectangle((int)(flames[i][k].X - 4 - scale), (int)(flames[i][k].Y - 8 - scale), (int)(8+(scale*2)), (int)(12+(scale*1.5f))), new Color(255, 255, 255, (byte)(flames[i][k].Z * 255)));
                    }
                Global.Graphics.GraphicsDevice.EnableAlphaBlending();

                spriteBatch.DrawString(Global.DefaultFont, Localizer.Get("Press Green to Join"), new Vector2((Global.ScreenWidth / 2) - (Global.DefaultFont.MeasureString(Localizer.Get("Press Green to Join")).X / 2), Global.ScreenHeight * 0.9f), new Color(0, (byte)(joinBlue * 128), 0, (byte)(joinAlpha * 255)));

                /*spriteBatch.Draw(GameUIMaster.Singleton.texButtonGreen, new Rectangle((int)(0.1f * Global.ScreenWidth), (int)(0.80f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                if (leader >= 0 && contguis[leader].status == 2)
                    spriteBatch.DrawString(Global.DefaultFont, "Continue", new Vector2((0.10f * Global.ScreenWidth) + (int)(0.1f * Global.ScreenHeight), (0.90f * Global.ScreenHeight) - (DefaultFont.MeasureString("Continue").Y)), Color.White);
                else
                    spriteBatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.10f * Global.ScreenWidth) + (int)(0.1f * Global.ScreenHeight), (0.90f * Global.ScreenHeight) - (DefaultFont.MeasureString("Select").Y)), Color.White);*/
                /*spriteBatch.Draw(tbgRed, new Rectangle((int)(0.80f * Global.ScreenWidth), (int)(0.80f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight), (int)(0.09f * Global.ScreenHeight)), Color.White);
                spriteBatch.DrawString(DefaultFont, "Back", new Vector2((0.80f * Global.ScreenWidth) - DefaultFont.MeasureString("Back").X, (0.90f * Global.ScreenHeight) - (DefaultFont.MeasureString("Back").Y)), Color.White);*/
                //spriteBatch.DrawString(DefaultFont, "" + contguis[0].type+","+GamePad.GetState(PlayerIndex.One).IsConnected + ","+ GamePad.GetCapabilities(PlayerIndex.One).GamePadType, new Vector2(100, 100), Color.Red);

                //spriteBatch.DrawString(DefaultFont, "" + contguis[0].loc + "::" + contguis[0].info, new Vector2(10, 10), Color.White);

                spriteBatch.End();
            }
            catch (Exception e)
            {
                Debug.Error("Problem in ControllerSetupScreen.Draw[3]",e);
                UnsignedGame.Singleton.Exit();
                return;
            }
        }
    }
}
