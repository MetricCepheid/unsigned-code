using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Unsigned
{
    class FVLogoScreen : BaseState
    {
        private ContentManager content;
        private float logoTime;
        Texture2D texBarrel, texGoo1, texGoo2, texPresser;
        Texture2D texGooBM, texPresserBM;
        VertexBuffer BarrelVB, Goo1VB, Goo2VB, PresserVB;
        IndexBuffer BarrelIB, Goo1IB, Goo2IB, PresserIB;
        

        public FVLogoScreen()
        {
            
        }

        public override void Load()
        {
            content = new ContentManager(UnsignedGame.GetSingleton().Services);
            texBarrel = content.Load<Texture2D>("graphics\\barrel");
            texGoo1 = content.Load<Texture2D>("graphics\\goo1");
            texGoo2 = content.Load<Texture2D>("graphics\\goo2");
            texPresser = content.Load<Texture2D>("graphics\\presser");

            texGooBM = content.Load<Texture2D>("graphics\\goobm");
            texPresserBM = content.Load<Texture2D>("graphics\\presserbm");

            Model mBarrel = content.Load<Model>("meshes\\barrel");
            BarrelIB = mBarrel.Meshes[0].IndexBuffer;
            BarrelVB = ModelConverter.Convert(mBarrel.Meshes[0].VertexBuffer, mBarrel.Meshes[0].MeshParts[0].VertexDeclaration);
            Model mGoo1 = content.Load<Model>("meshes\\goo1");
            Goo1IB = mGoo1.Meshes[0].IndexBuffer;
            Goo1VB = ModelConverter.Convert(mGoo1.Meshes[0].VertexBuffer, mGoo1.Meshes[0].MeshParts[0].VertexDeclaration);
            Model mGoo2 = content.Load<Model>("meshes\\goo2");
            Goo2IB = mGoo2.Meshes[0].IndexBuffer;
            Goo2VB = ModelConverter.Convert(mGoo2.Meshes[0].VertexBuffer, mGoo2.Meshes[0].MeshParts[0].VertexDeclaration);
            Model mPresser = content.Load<Model>("meshes\\presser");
            PresserIB = mPresser.Meshes[0].IndexBuffer;
            PresserVB = ModelConverter.Convert(mPresser.Meshes[0].VertexBuffer, mPresser.Meshes[0].MeshParts[0].VertexDeclaration);
        }

        public override void Unload()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            if (logoTime >= 35)
                UnsignedGame.GetSingleton().PushState(new MainMenuScreen());
            if (logoTime < 35)
                logoTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 8;
        }

        public override void Render(GameTime gameTime)
        {
            RenderMaster rm = RenderMaster.GetSingleton();
            FVShader bEffect = rm.engine;

            rm.graphics.GraphicsDevice.Clear(Color.Black);


#if !DEBUG

                    try
                    {
#endif


            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphics.PreferMultiSampling = true;
            rm.graphics.ApplyChanges();

            rm.graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            rm.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            bEffect.AmbientMaterial = new Color(24, 24, 24);
            bEffect.DiffuseMaterial = Color.White;
            bEffect.SpecularMaterial = Color.White;
            bEffect.DirectionalLight = new DirectionalLight(true, new Vector3(-1, 3, -1), new Color(200, 200, 200), Color.White);
            bEffect.NormalMapTexture = Global.texDefaultBM;
            bEffect.LightingEnabled = GameSettings.Lighting;
            bEffect.SpecularEnabled = GameSettings.Specular;
            bEffect.NormalMapEnabled = GameSettings.NormalMapping;
            bEffect.SpecularMaterial = Color.White;
            bEffect.Shininess = 12.0f;
            bEffect.TextureEnabled = true;
            bEffect.CommitChanges();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }

                    try
                    {
#endif
            bEffect.Begin();
            foreach (EffectPass pass in bEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Matrix matProj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                  GameSettings.windowwidth / (float)GameSettings.windowheight,
                  1f, 40.0f);
                Vector3 camPos = new Vector3(20, 8, 0);
                if (logoTime < 20)
                    camPos.X = 20;
                else if (logoTime < 30)
                    camPos.X = ((((logoTime - 20) / 10f)) * 12) + ((1 - ((logoTime - 20) / 10f)) * 20);
                else
                    camPos.X = 12;
                if (logoTime < 20)
                    camPos.Y = 8;
                else if (logoTime < 30)
                    camPos.Y = ((((logoTime - 20) / 10f)) * 12) + ((1 - ((logoTime - 20) / 10f)) * 8);
                else
                    camPos.Y = 12;

                camPos = Vector3.Transform(camPos, Matrix.CreateRotationY(logoTime < 30 ? (float)(Math.PI * 3 / 4f) + (float)((logoTime / 30f) * (Math.PI * 3 / 4f)) : (float)(Math.PI * 1.5f)));

                Vector3 target = new Vector3(0, 0, 0);

                if (logoTime < 20)
                    camPos.X -= 0;
                else if (logoTime < 30)
                { target.X -= ((((logoTime - 20) / 10f)) * 1); camPos.X -= ((((logoTime - 20) / 10f)) * 1); }
                else
                { target.X -= 1; camPos.X -= 1; }

                Matrix matView = Matrix.CreateLookAt(camPos, target, new Vector3(0, 1, 0));
                //render the background graphics
                bEffect.View = matView;
                bEffect.Projection = matProj;

                Matrix matRot, matScale, matTranslate;
                {//goo1
                    matTranslate = Matrix.CreateTranslation(0, 0, 0);
                    matRot = Matrix.Identity;// Matrix.CreateRotationX((float)Math.PI);
                    matScale = Matrix.CreateScale(1, 1, 1);

                    bEffect.World = matScale * matRot * matTranslate;
                    if (logoTime < 17)
                        bEffect.DiffuseTexture = texGoo1;
                    else
                        bEffect.DiffuseTexture = texGoo2;

                    bEffect.NormalMapTexture = texGooBM;

                    bEffect.CommitChanges();

                    rm.graphics.GraphicsDevice.VertexDeclaration = GBVertexFormat.VertexDeclaration;
                    if (logoTime < 17)
                    {
                        rm.graphics.GraphicsDevice.Vertices[0].SetSource(Goo1VB, 0, GBVertexFormat.SizeInBytes);
                        rm.graphics.GraphicsDevice.Indices = Goo1IB;
                        rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Goo1IB.SizeInBytes / (Goo1IB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, Goo1IB.SizeInBytes / (Goo1IB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));
                    }
                    else
                    {
                        rm.graphics.GraphicsDevice.Vertices[0].SetSource(Goo2VB, 0, GBVertexFormat.SizeInBytes);
                        rm.graphics.GraphicsDevice.Indices = Goo2IB;
                        rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Goo2IB.SizeInBytes / (Goo2IB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, Goo2IB.SizeInBytes / (Goo2IB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));
                    }
                }
                {//barrel
                    matTranslate = Matrix.CreateTranslation(-10.2f, 0, -6.8f);
                    matRot = Matrix.CreateRotationY((float)Math.PI * 1.25f);
                    matScale = Matrix.CreateScale(1, 1, 1);

                    bEffect.World = matScale * matRot * matTranslate;
                    bEffect.DiffuseTexture = texBarrel;
                    bEffect.NormalMapTexture = texPresserBM;
                    bEffect.CommitChanges();

                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(BarrelVB, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.Indices = BarrelIB;
                    rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, BarrelIB.SizeInBytes / (BarrelIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, BarrelIB.SizeInBytes / (BarrelIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));
                }
                {//presser
                    float y = 30;
                    if (logoTime > 5 && logoTime < 15)
                        y = (10 - (logoTime - 5)) * 3;
                    else if (logoTime >= 15 && logoTime < 20)
                        y = 0;
                    else if (logoTime >= 20 && logoTime < 30)
                        y = (logoTime - 20) * 3;
                    matTranslate = Matrix.CreateTranslation(0, y, 0);
                    matRot = Matrix.Identity;//Matrix.CreateRotationY((float)Math.PI * 1.25f);
                    matScale = Matrix.CreateScale(1, 1, 1);

                    bEffect.World = matScale * matRot * matTranslate;
                    bEffect.DiffuseTexture = texPresser;
                    bEffect.NormalMapTexture = texPresserBM;
                    bEffect.CommitChanges();

                    rm.graphics.GraphicsDevice.Vertices[0].SetSource(PresserVB, 0, GBVertexFormat.SizeInBytes);
                    rm.graphics.GraphicsDevice.Indices = PresserIB;
                    rm.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, PresserIB.SizeInBytes / (PresserIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2), 0, PresserIB.SizeInBytes / (PresserIB.IndexElementSize == IndexElementSize.ThirtyTwoBits ? 12 : 6));
                }
                pass.End();
            }
            bEffect.End();

            rm.spritebatch.Begin();
            if (logoTime < 5)
                rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), new Color(0, 0, 0, (byte)(255 * (1 - ((logoTime) / 5f)))));
            if (logoTime > 30 && logoTime < 35)
                rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), new Color(0, 0, 0, (byte)(255 * (((logoTime - 30) / 5f)))));
            else if (logoTime >= 35)
                rm.spritebatch.Draw(Global.texWhite, new Rectangle(0, 0, GameSettings.windowwidth, GameSettings.windowheight), Color.Black);
            rm.spritebatch.End();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
#if WINDOWS
                        System.Windows.Forms.MessageBox.Show("Problem in Draw/MM/Pt1\n"+e.Message+"\n"+e.StackTrace);
#endif
                        UnsignedGame.GetSingleton().Exit();
                        return;
                    }

#endif
        }
    }
}
