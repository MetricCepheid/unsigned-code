using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnsignedPeripheralPlugins;
using SongDataIO;

namespace Unsigned
{
    class ControllerSetupScreen : BaseState
    {
        private enum ConfirmState
        {
            EMPTY_SLOT=0,
            CHOOSING_NAME,
            CHOOSING_INSTRUMENT,
            FULLY_CONFIRMED
        };

        private ContentManager content;

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
        private VertexBuffer nPadVB;
        private IndexBuffer nPadIB;

        private float joinAlpha, joinBlue, joinBlueDir;

        private ConfirmState[] confirmStates;
        PlayerConfigNugget nugget;

        private float idleTime;
        private float[] arrowTimer, arrowRot;
        private Vector3[][] flames;
        private bool notesNeedRefreshing;

        float borderScroll = 0;

        float yaw = 0, pitch = 0;

        public ControllerSetupScreen()
        {
            texNote = new Texture2D[4];
            nPadTex = new Texture2D[4];
            arrowRot = new float[4];
            arrowTimer = new float[4];
            flames = new Vector3[4][];
            confirmStates = new ConfirmState[4];
            for (int i = 0; i < confirmStates.Length; i++)
                confirmStates[i] = ConfirmState.EMPTY_SLOT;
            nugget = new PlayerConfigNugget();
            joinAlpha = 0;
            joinBlue = 0;
            joinBlueDir = 1;
        }

        public override void Load()
        {
            content = new ContentManager(UnsignedGame.GetSingleton().Services);
            Model PadMdl = content.Load<Model>("meshes\\paper1");
            ModelConverter.Convert(out nPadVB, PadMdl.Meshes[0].VertexBuffer, PadMdl.Meshes[0].MeshParts[0].VertexDeclaration);
            nPadIB = PadMdl.Meshes[0].IndexBuffer;
            texNoteBM = content.Load<Texture2D>("graphics\\notebm");
            nPadTex = new Texture2D[4];
            nPadTex[0] = content.Load<Texture2D>("graphics\\paper1");
            nPadTex[1] = content.Load<Texture2D>("graphics\\paper2");
            nPadTex[2] = content.Load<Texture2D>("graphics\\paper3");
            nPadTex[3] = content.Load<Texture2D>("graphics\\paper4");
            flames = new Vector3[4][];
            flames[0] = new Vector3[50];
            flames[1] = new Vector3[50];
            flames[2] = new Vector3[50];
            flames[3] = new Vector3[50];
            concrTex = content.Load<Texture2D>("graphics\\concr");
            concrBM = content.Load<Texture2D>("graphics\\concrBM");
            hairl = content.Load<Texture2D>("graphics\\hairl");
            hairr = content.Load<Texture2D>("graphics\\hairr");
            flameTex = content.Load<Texture2D>("graphics\\flame");
            texBorder = content.Load<Texture2D>("graphics\\songscreenbottom");
        }

        public override void Unload()
        {
            content.Unload();
        }

        
        public override void Update(GameTime gameTime)
        {

            if (joinBlue >= 1)
                joinBlueDir = -1;
            if (joinBlue <= 0)
                joinBlueDir = 1;
            joinBlue += (float)gameTime.ElapsedGameTime.TotalSeconds * joinBlueDir;

            bool anyfree = false;
            for (int k = 0; k < 4; k++)
                if (confirmStates[k] == ConfirmState.EMPTY_SLOT)
                    anyfree = true;

            if (anyfree)
                joinAlpha = (float)Math.Min(1.0, joinAlpha + gameTime.ElapsedGameTime.TotalSeconds);
            else
                joinAlpha = (float)Math.Max(0.0, joinAlpha - gameTime.ElapsedGameTime.TotalSeconds);

            RenderMaster rm = RenderMaster.GetSingleton();
            if (rtNote == null || notesNeedRefreshing)
            {
                if (rtNote == null)
                {
                    rtNote = new RenderTarget2D[4];
                    rtNote[0] = new RenderTarget2D(rm.graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[1] = new RenderTarget2D(rm.graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[2] = new RenderTarget2D(rm.graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                    rtNote[3] = new RenderTarget2D(rm.graphics.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color);
                }

                for (int i = 0; i < 4; i++)
                {
                    rm.graphics.GraphicsDevice.SetRenderTarget(0, rtNote[i]);
                    rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

                    rm.spritebatch.Draw(nPadTex[i], new Rectangle(0, 0, 256, 256), confirmStates[i]==ConfirmState.EMPTY_SLOT?Color.Gray:Color.White);
                    if (confirmStates[i] >= ConfirmState.CHOOSING_NAME)
                    {
                        bool alsochosen = false;
                        for (int k = 0; k < 4; k++)
                            if (k == i)
                                continue;
                            else if (confirmStates[k] == ConfirmState.EMPTY_SLOT)
                                continue;
                            else if (nugget.characterIndices[i] == nugget.characterIndices[k])
                                alsochosen = true;
                        if(confirmStates[i] >= ConfirmState.CHOOSING_INSTRUMENT)
                            for(int r=0;r<20;r++)
                            rm.spritebatch.DrawString(Global.HandwrittenFont, CharacterMaster.GetSingleton().GetCharacter(nugget.characterIndices[i]).name, new Vector2(70-r, 20-(r/2)),
                                new Color(128,85,0,(byte)(50-(r*2))), 0, new Vector2(0, 0),
                                (rtNote[i].Width - (80-(r*2))) / Global.HandwrittenFont.MeasureString(CharacterMaster.GetSingleton().GetCharacter(nugget.characterIndices[i]).name).X,
                                SpriteEffects.None, 0);
                        rm.spritebatch.DrawString(Global.HandwrittenFont, CharacterMaster.GetSingleton().GetCharacter(nugget.characterIndices[i]).name, new Vector2(70, 20),
                            alsochosen ? Color.Red : Color.Black, 0, new Vector2(0, 0),
                            (rtNote[i].Width - 80) / Global.HandwrittenFont.MeasureString(CharacterMaster.GetSingleton().GetCharacter(nugget.characterIndices[i]).name).X,
                            SpriteEffects.None, 0);
                    }

                    if (confirmStates[i] >= ConfirmState.CHOOSING_INSTRUMENT)
                    {
                        if (confirmStates[i] >= ConfirmState.FULLY_CONFIRMED)
                            for (int r = 0; r < 20; r++)
                                rm.spritebatch.DrawString(Global.HandwrittenFont, InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).FullName, new Vector2(70 - r, 200 + (r / 2)),
                                    new Color(128, 85, 0, (byte)(50 - (r * 2))), 0, new Vector2(0, 0),
                                    (rtNote[i].Width - (80 - (r * 2))) / Global.HandwrittenFont.MeasureString(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).FullName).X,
                                    SpriteEffects.None, 0);
                        rm.spritebatch.DrawString(Global.HandwrittenFont, InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).FullName, new Vector2(70, 200),
                            Color.Black, 0, new Vector2(0, 0),
                            (rtNote[i].Width - 80) / Global.HandwrittenFont.MeasureString(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).FullName).X,
                            SpriteEffects.None, 0);
                    }

                    rm.spritebatch.End();
                    rm.graphics.GraphicsDevice.SetRenderTarget(0, null);
                    texNote[i] = rtNote[i].GetTexture();
                }
                notesNeedRefreshing = false;
            }
#if !DEBUG
            try
            {
#endif
            Peripheral[] controllers = PeripheralManager.GetSingleton().GetPeripherals();

            borderScroll += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (borderScroll >= MathHelper.Pi * 2)
                borderScroll -= MathHelper.Pi * 2;

            for (int i = 0; i < nugget.peripherals.Length; i++)
            {
                if (confirmStates[i] == ConfirmState.FULLY_CONFIRMED)
                {
                    if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
                    {
                        confirmStates[i] = ConfirmState.CHOOSING_INSTRUMENT;
                        notesNeedRefreshing = true;
                        continue;
                    }
                    if (nugget.peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                    {
                        bool isLeader = true;
                        for (int k = i-1; k >= 0; k--)
                            if (confirmStates[k] >= ConfirmState.EMPTY_SLOT)
                                isLeader = false;
                        if (isLeader)
                        {
                            bool allReady = true;
                            for (int k = 0; k < 4; k++)
                                if (confirmStates[k] > ConfirmState.EMPTY_SLOT && confirmStates[k] < ConfirmState.FULLY_CONFIRMED)
                                    allReady = false;
                            if (allReady)
                            {
                                UnsignedGame.GetSingleton().PushState(new SongSelectScreen(nugget));
                            }
                        }
                    }
                    for (int k = 0; k < flames[i].Length; k++)
                        if (flames[i][k].Z <= 0)
                        {
                            flames[i][k] = new Vector3((GameSettings.windowwidth * 0.1625f) + (i * 0.1875f * GameSettings.windowwidth) + ((float)(Global.random.NextDouble() * 0.1719f * GameSettings.windowwidth)), (GameSettings.windowheight*0.1667f) + (float)(Global.random.NextDouble() * 0.28f * GameSettings.windowheight), 1);
                            break;
                        }
                }
                else if (confirmStates[i] == ConfirmState.CHOOSING_INSTRUMENT)
                {
                    if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
                    {
                        nugget.instruments[i] = 0;
                        confirmStates[i] = ConfirmState.CHOOSING_NAME;
                        notesNeedRefreshing = true;
                        continue;
                    }
                    if (nugget.peripherals[i].WasPressed(PeripheralButton.CONFIRM))
                    {
                        confirmStates[i] = ConfirmState.FULLY_CONFIRMED;
                        notesNeedRefreshing = true;
                        continue;
                    }
                    if (nugget.peripherals[i].WasPressed(PeripheralButton.DOWN))
                    {
                        nugget.instruments[i]++;
                        if(nugget.instruments[i]>=InstrumentMaster.GetSingleton().GetNumInstruments())
                            nugget.instruments[i]=0;
                        String[] possibles = nugget.peripherals[i].GetSupportedInstruments();
                        bool good = false;
                        for(int k=0;k<possibles.Length;k++)
                            if (possibles[k].Equals(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).CodeName))
                            {good = true;break;}
                        while (!good)
                        {
                            nugget.instruments[i]++;
                            if (nugget.instruments[i] >= InstrumentMaster.GetSingleton().GetNumInstruments())
                                nugget.instruments[i] = 0;
                            good = false;
                            for (int k = 0; k < possibles.Length; k++)
                                if (possibles[k].Equals(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).CodeName))
                                { good = true; break; }
                        }
                        notesNeedRefreshing = true;
                    }
                    if (nugget.peripherals[i].WasPressed(PeripheralButton.UP))
                    {
                        nugget.instruments[i]--;
                        if (nugget.instruments[i] < 0)
                            nugget.instruments[i] = InstrumentMaster.GetSingleton().GetNumInstruments() - 1;
                        String[] possibles = nugget.peripherals[i].GetSupportedInstruments();
                        bool good = false;
                        for (int k = 0; k < possibles.Length; k++)
                            if (possibles[k].Equals(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).CodeName))
                            { good = true; break; }
                        while (!good)
                        {
                            nugget.instruments[i]--;
                            if (nugget.instruments[i] < 0)
                                nugget.instruments[i] = InstrumentMaster.GetSingleton().GetNumInstruments() - 1;
                            good = false;
                            for (int k = 0; k < possibles.Length; k++)
                                if (possibles[k].Equals(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).CodeName))
                                { good = true; break; }
                        }
                        notesNeedRefreshing = true;
                    }
                }
                else if (confirmStates[i] == ConfirmState.CHOOSING_NAME)
                {
                    if (nugget.peripherals[i].WasPressed(PeripheralButton.BACK))
                    {
                        nugget.peripherals[i] = null;
                        confirmStates[i] = ConfirmState.EMPTY_SLOT;
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
                            else if (confirmStates[k] == ConfirmState.EMPTY_SLOT)
                                continue;
                            else if (nugget.characterIndices[k] == nugget.characterIndices[i])
                                hassamename = true;
                        // TODO: check if character has role compatible with peripheral
                        if (!hassamename)
                        {
                            confirmStates[i] = ConfirmState.CHOOSING_INSTRUMENT;
                            String[] possibles = nugget.peripherals[i].GetSupportedInstruments();
                            bool good = false;
                            for (int k = 0; k < possibles.Length; k++)
                                if (possibles[k].Equals(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).CodeName))
                                { good = true; break; }
                            while (!good)
                            {
                                nugget.instruments[i]++;
                                if (nugget.instruments[i] >= InstrumentMaster.GetSingleton().GetNumInstruments())
                                    nugget.instruments[i] = 0;
                                good = false;
                                for (int k = 0; k < possibles.Length; k++)
                                    if (possibles[k].Equals(InstrumentMaster.GetSingleton().GetInstrument(nugget.instruments[i]).CodeName))
                                    { good = true; break; }
                            }
                            notesNeedRefreshing = true;
                            continue;
                        }
                    }
                    if (nugget.peripherals[i].WasPressed(PeripheralButton.DOWN))
                    {
                        if (nugget.characterIndices[i] < CharacterMaster.GetSingleton().GetNumCharacters() - 1)
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
                        if (confirmStates[k]==ConfirmState.EMPTY_SLOT)
                        {
                            nugget.peripherals[k] = controllers[i];
                            confirmStates[k] = ConfirmState.CHOOSING_NAME;
                            nugget.characterIndices[k] = 0;
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
                if (confirmStates[k] != ConfirmState.EMPTY_SLOT)
                    allfree = false;

            if (red)
                if(allfree && !notesNeedRefreshing)
                    UnsignedGame.GetSingleton().PopState();
#if !DEBUG
                }
                catch(Exception e)
                {
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                    UnsignedGame.GetSingleton().Exit();
                    return;
                }
                    try
                    {
#endif
                for (int k = 0; k < 4; k++)
                    for (int i = 0; i < flames[k].Length; i++)
                        if (flames[k][i].Z > 0)
                        {
                            flames[k][i].Z -= gameTime.ElapsedGameTime.Milliseconds / 1000f;
                            flames[k][i].Y -= (k + 1) * 1.5f * gameTime.ElapsedGameTime.Milliseconds / 100f;
                            flames[k][i].X += (float)(Global.random.NextDouble() - 0.5) * gameTime.ElapsedGameTime.Milliseconds / 50f;
                        }
#if !DEBUG
                }
                catch(Exception e)
                {
                    
#if WINDOWS
                    System.Windows.Forms.MessageBox.Show("Problem in Update/CCS/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                    UnsignedGame.GetSingleton().Exit();
                    return;
                }
#endif
        }

        public override void Render(GameTime gameTime)
        {
            UnsignedGame game = UnsignedGame.GetSingleton();
            RenderMaster rm = RenderMaster.GetSingleton();
            FVShader effect = rm.engine;

            float vmul = 2f, hmul = 2f;
            bool[] plo = new bool[16];
            Vector3[] plp = new Vector3[16];
            float[] pln = new float[16];
            float[] plf = new float[16];
            Vector3[] pld = new Vector3[16];
            Vector3[] pls = new Vector3[16];
#if !DEBUG
                    try
                    {
#endif
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            rm.graphics.ApplyChanges();

            rm.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.NormalMapTexture = Global.texDefaultBM;
            effect.AmbientMaterial = new Color(24, 24, 24);
            effect.DiffuseMaterial = new Color(200, 200, 200);
            effect.SpecularMaterial = Color.White;

            Random r = Global.random;

            rm.ResetLighting();
            effect.TextureEnabled = true;
            effect.LightingEnabled = GameSettings.Lighting;
            effect.SpecularEnabled = GameSettings.Specular;
            effect.NormalMapEnabled = GameSettings.NormalMapping;
            effect.DirectionalLight = new DirectionalLight(true, new Vector3(0, 1, 1), new Color(100, 100, 100), Color.White);

            Version SM =RenderMaster.GetSingleton().graphics.GraphicsDevice.GraphicsDeviceCapabilities.PixelShaderVersion;
            if(SM.Major<1 || (SM.Major==1 && SM.Minor<1))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("Whoops! Your graphics card only supports Shader Model "+SM.Major+"."+SM.Minor+"\nYou need at least 2.0 to run Unsigned");
#endif
                game.Exit();
            }
            effect.CommitChanges();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
                    try
                    {
#endif
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                UnsignedGame.SetProjMatrix(GameSettings.windowwidth, GameSettings.windowheight);

                Matrix matView;
                Vector3 tgt = new Vector3(0, 0, -1);
                if (idleTime < 29.5)
                    matView = Matrix.CreateLookAt(new Vector3(-8, 128, 200), new Vector3(-8, 128, 200)+tgt, new Vector3(0, 1, 0));
                else
                    matView = Matrix.CreateLookAt(new Vector3(-8, 128, 150), new Vector3(-24 + ((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * 32 : (1 - ((idleTime * hmul) % .5f * 2)) * 16), 128 - ((idleTime * vmul) % 1 < 0.5 ? (idleTime * vmul) % 0.5f * 32 : (1 - ((idleTime * vmul) % .5f * 2)) * 16), 0), new Vector3(0, 1, 0));
                rm.View = matView;

                Matrix matRot, matScale, matTranslate;
                {
                    matTranslate = Matrix.CreateTranslation(-32, 120, -128);
                    matRot = Matrix.CreateRotationX((float)Math.PI / 2);
                    matScale = Matrix.CreateScale(300, 1, 192);

                    effect.World = matScale * matRot * matTranslate;
                    effect.DiffuseTexture = concrTex;
                    effect.NormalMapTexture =concrBM;
                    effect.SpecularMaterial = new Color(30, 30, 30);
                    effect.Shininess = 0.25f;
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    rm.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    rm.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(Global.square, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;

                for (int i = 0; i < 4; i++)
                {
                    matTranslate = Matrix.CreateTranslation(LEFT + (SPACING * i), 192, -126);
                    matRot = Matrix.Identity;
                    matScale = Matrix.CreateScale(SCALE);

                    effect.World = matScale * matRot * matTranslate;
                    effect.DiffuseTexture = texNote[i];
                    effect.NormalMapTexture =texNoteBM;
                    effect.SpecularMaterial = Color.Black;
                    effect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;
                   rm.graphics.GraphicsDevice.Vertices[0].SetSource(nPadVB, 0, GBVertexFormat.SizeInBytes);
                   rm.graphics.GraphicsDevice.Indices = nPadIB;
                   rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, nPadIB.SizeInBytes / (nPadIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, nPadIB.SizeInBytes / (nPadIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));
                }
                rm.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                pass.End();
            }
            effect.End();
#if !DEBUG
                    }
                    catch (Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt2\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
                    try
                    {
#endif
            rm.spritebatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            //if (leader >= 0 && contguis[leader].status == 2)
                //spritebatch.Draw(texContinue, new Rectangle((int)(Window.ClientBounds.Width * (contguis[leader].loc) / 6), Window.ClientBounds.Height - (Window.ClientBounds.Height / 5), Window.ClientBounds.Width / 6, Window.ClientBounds.Height / 6), Color.Red);

            if (idleTime > 30)
            {
                rm.spritebatch.Draw(hairl, new Rectangle(-20, 0, (int)game.Window.ClientBounds.Height - (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (game.Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * game.Window.ClientBounds.Height), (int)game.Window.ClientBounds.Height), Color.White);
                rm.spritebatch.Draw(hairr, 
                    new Rectangle(20 + (int)game.Window.ClientBounds.Width - (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (game.Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * game.Window.ClientBounds.Height), 
                                  0, 
                                  (int)((idleTime * hmul) % 1 < 0.5 ? (idleTime * hmul) % 0.5f * (game.Window.ClientBounds.Height * 2) : (1 - ((idleTime * hmul) % .5f * 2)) * game.Window.ClientBounds.Height), 
                                  (int)game.Window.ClientBounds.Height), 
                    Color.White);
            }

            for (int i = -1; i < 3; i++)
                rm.spritebatch.Draw(texBorder, new Rectangle((int)((i * GameSettings.windowwidth) + (((borderScroll) / (MathHelper.Pi * 4)) * GameSettings.windowwidth)), (int)(GameSettings.windowheight * (0.6f + (-Math.Sin(borderScroll) * 0.01f))), GameSettings.windowwidth, GameSettings.windowwidth / 4), Color.White);
            for (int i = 0; i < 4; i++)
                rm.spritebatch.Draw(texBorder, new Rectangle((int)((i * GameSettings.windowwidth) + (((-borderScroll) / (MathHelper.Pi*4)) * GameSettings.windowwidth)), (int)(GameSettings.windowheight * (0.75f+(Math.Sin(borderScroll)*0.05f))), GameSettings.windowwidth, GameSettings.windowwidth / 4), Color.White);
            rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, (int)(GameSettings.windowheight * 0.05f)), new Color(255, 190, 0));

            for (int i = 0; i < 4; i++)
                for (int k = 0; k < flames[i].Length; k++)
                {
                    float scale = (1 - flames[i][k].Z) * 32;
                    if(flames[i][k].Z<1 && flames[i][k].Z>0)
                        rm.spritebatch.Draw(flameTex, new Rectangle((int)(flames[i][k].X - 4 - scale), (int)(flames[i][k].Y - 8 - scale), (int)(8+(scale*2)), (int)(12+(scale*1.5f))), new Color(255, 255, 255, (byte)(flames[i][k].Z * 255)));
                }

            rm.spritebatch.DrawString(Global.DefaultFont, Localizer.Get("Press Green to Join"), new Vector2((GameSettings.windowwidth / 2) - (Global.DefaultFont.MeasureString(Localizer.Get("Press Green to Join")).X / 2), GameSettings.windowheight * 0.8f), new Color(0, (byte)(joinBlue * 128), 0, (byte)(joinAlpha * 255)));

            /*rm.spritebatch.Draw(GameUIMaster.GetSingleton().texButtonGreen, new Rectangle((int)(0.1f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            if (leader >= 0 && contguis[leader].status == 2)
                rm.spritebatch.DrawString(Global.DefaultFont, "Continue", new Vector2((0.10f * GameSettings.windowwidth) + (int)(0.1f * GameSettings.windowheight), (0.90f * GameSettings.windowheight) - (DefaultFont.MeasureString("Continue").Y)), Color.White);
            else
                rm.spritebatch.DrawString(Global.DefaultFont, "Select", new Vector2((0.10f * GameSettings.windowwidth) + (int)(0.1f * GameSettings.windowheight), (0.90f * GameSettings.windowheight) - (DefaultFont.MeasureString("Select").Y)), Color.White);*/
            /*rm.spritebatch.Draw(tbgRed, new Rectangle((int)(0.80f * GameSettings.windowwidth), (int)(0.80f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight), (int)(0.09f * GameSettings.windowheight)), Color.White);
            rm.spritebatch.DrawString(DefaultFont, "Back", new Vector2((0.80f * GameSettings.windowwidth) - DefaultFont.MeasureString("Back").X, (0.90f * GameSettings.windowheight) - (DefaultFont.MeasureString("Back").Y)), Color.White);*/
            //spritebatch.DrawString(DefaultFont, "" + contguis[0].type+","+GamePad.GetState(PlayerIndex.One).IsConnected + ","+ GamePad.GetCapabilities(PlayerIndex.One).GamePadType, new Vector2(100, 100), Color.Red);

            //spritebatch.DrawString(DefaultFont, "" + contguis[0].loc + "::" + contguis[0].info, new Vector2(10, 10), Color.White);
            if (Global.DemoMode)
            {
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.15f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.4f), new Color(255, 0, 0, 64));
                rm.spritebatch.DrawString(Global.BigFont, Localizer.Get("Demo Mode"), new Vector2((GameSettings.windowwidth / 2) - (Global.BigFont.MeasureString(Localizer.Get("Demo Mode")).X / 2), GameSettings.windowheight * 0.65f), new Color(255, 0, 0, 64));
            }
            rm.spritebatch.End();
#if !DEBUG
                    }
                    catch (Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/CCS/Pt3\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }
#endif
        }
    }
}
