#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace Unsigned
{
    public abstract class Entity
    {
        protected Entity parent;
        protected Vector3 relLoc;
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(Effect engine, GraphicsDeviceManager graphics, Vector3 CamPos);
        public virtual Matrix GetTransform()
        {
            if(parent!=null)
                return Matrix.CreateTranslation(relLoc)*parent.GetTransform();
            return Matrix.CreateTranslation(relLoc);
        }
    }
    public class SwingingEntity : Entity
    {
        private int model, texture;
        private float swingAmt, rotSpd, ovalish;
        private bool fullBright, emmissive;
        private Matrix staticRot;
        private float rotVal;
        private bool glow;

        public SwingingEntity(int model,int tex, Vector3 loc)
        {
            this.model = model;
            texture = tex;
            relLoc = loc;
            staticRot = Matrix.Identity;
            swingAmt = 0;
            rotSpd = 0;
            ovalish = 1;
            fullBright=false;
            emmissive=false;
            rotVal = 0;
            glow = false;
        }

        public void SetSwing(float amt, float rSpd, float oval)
        {
            swingAmt = amt;
            rotSpd = rSpd;
            ovalish = oval;
        }

        public override void Update(GameTime gameTime)
        {
            rotVal += rotSpd * (gameTime.ElapsedGameTime.Milliseconds / 1000f);
        }

        /*public LightData GetLight()
        {
            if (emmissive)
            {
                Matrix matRot = Matrix.CreateRotationX(swingAmt) * Matrix.CreateRotationY(rotVal);
                LightData l = new LightData();
                l.On = true;
                l.Near = 128;
                l.Far = 512;
                l.Diffuse = new Vector3(1, 1, 0.9f);
                l.Specular = new Vector3(1, 1, 0.8f);
                l.Pos = relLightLoc;
                l.Pos = Vector3.Transform(l.Pos, matRot);
                l.Pos += loc;
                return l;
            }
            return new LightData();
        }*/
        public override Matrix GetTransform()
        {
            Matrix matRot = Matrix.CreateRotationX(swingAmt) * Matrix.CreateRotationY(rotVal);
            return matRot * Matrix.CreateTranslation(relLoc) * parent.GetTransform();
        }

        public override void Draw(Effect engine, GraphicsDeviceManager graphics, Vector3 CamPos)
        {
            Matrix matIdentity = Matrix.Identity;
            float xval = (float)Game1.dirdistTOhdist(rotVal * 180 / Math.PI,swingAmt);
            float yval = (float)Game1.dirdistTOvdist(rotVal * 180 / Math.PI, swingAmt);
            xval *= ovalish;
            Matrix transform = GetTransform();
            Matrix matScale = Matrix.CreateScale(Venue.SCALE);

            // identity, scale, rotate, orbit(translate & rotate), translate
            Matrix matWorld = matIdentity * matScale * transform;
            if (fullBright)
                engine.Parameters["fullbright"].SetValue(true);
            engine.Parameters["world"].SetValue(matWorld);
            engine.Parameters["wRot"].SetValue(transform);
            engine.Parameters["diffuseTexture"].SetValue(Venue.StaticTexture[texture].tex);
            engine.Parameters["bumpTexture"].SetValue(Game1.texDefaultBM);
            engine.CommitChanges();

            foreach (ModelMesh mesh in Venue.Models[model].Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    graphics.GraphicsDevice.VertexDeclaration = meshpart.VertexDeclaration;
                    graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, meshpart.StreamOffset, meshpart.VertexStride);
                    graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshpart.BaseVertex, 0, meshpart.NumVertices, meshpart.StartIndex, meshpart.PrimitiveCount);
                }
            }
            engine.Parameters["fullbright"].SetValue(false);
        }
    }
    public class LightEntity : Entity
    {
        private float sFade, eFade;
        private Vector3 dif, spc;

        public LightData GetLight()
        {
            Vector3 pos = relLoc;
            pos = Vector3.Transform(pos, GetTransform());
            LightData ret = new LightData();
            ret.Diffuse = dif;
            ret.Far = eFade;
            ret.Near = sFade;
            ret.On = true;
            ret.Pos = pos;
            ret.Specular = spc;
            return ret;
        }

        public override void Draw(Effect engine, GraphicsDeviceManager graphics, Vector3 CamPos)
        {
            //lights do not draw
            //maybe make a glow?
        }

        public override void Update(GameTime gameTime)
        {
            //TODO: update for effects (strobe, etc)
        }
    }
}
