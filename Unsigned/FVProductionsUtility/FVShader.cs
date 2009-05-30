using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

// Copyright 2008 FV Productions
// Do not redistribute this file without permission from FV Productions

namespace FVProductions.Utility
{
    /// <summary>
    /// A base light class
    /// </summary>
    public abstract class Light
    {
        /// <summary>
        /// Whether or not this light is turned on
        /// </summary>
        public bool Enabled;
        /// <summary>
        /// The diffuse value of this light (don't use alpha)
        /// </summary>
        public Color Diffuse;
        /// <summary>
        /// The specular value of this light (don't use alpha)
        /// </summary>
        public Color Specular;
    }

    /// <summary>
    /// Represents a position-less directional light
    /// </summary>
    public class DirectionalLight : Light
    {
        /// <summary>
        /// The direction of the light (must be unit-vector)
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Creates a new position-less directional light
        /// </summary>
        /// <param name="enabled">Whether or not this light is turned on</param>
        /// <param name="direction">The direction of the light (must be unit-vector)</param>
        /// <param name="diffuse">The diffuse value of this light (don't use alpha)</param>
        /// <param name="specular">The specular value of this light (don't use alpha)</param>
        public DirectionalLight(bool enabled, Vector3 direction, Color diffuse, Color specular)
        {
            Enabled = enabled;
            Direction = Vector3.Normalize(direction);
            Diffuse = diffuse;
            Specular = specular;
        }

        /// <summary>
        /// A generic light that is turned off
        /// </summary>
        public static DirectionalLight DisabledLight = new DirectionalLight(false, new Vector3(0,-1,0), Color.Black, Color.Black);
    }

    /// <summary>
    /// Represents a point light with a falloff
    /// </summary>
    public class PointLight : Light
    {
        /// <summary>
        /// The world-space position of this light
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// How far away a lit object must be to start losing light
        /// </summary>
        public float NearFade;
        /// <summary>
        /// How far away a lit object must be to recieve no light
        /// </summary>
        public float FarFade;

        /// <summary>
        /// Creates a new point light
        /// </summary>
        /// <param name="enabled">Whether or not this light is turned on</param>
        /// <param name="position">The world-space position of this light</param>
        /// <param name="near">How far away a lit object must be to start losing light</param>
        /// <param name="far">How far away a lit object must be to recieve no light</param>
        /// <param name="diffuse">The diffuse value of this light (don't use alpha)</param>
        /// <param name="specular">The specular value of this light (don't use alpha)</param>
        public PointLight(bool enabled, Vector3 position, float near, float far, Color diffuse, Color specular)
        {
            Enabled = enabled;
            Position = position;
            Diffuse = diffuse;
            Specular = specular;
            NearFade = near;
            FarFade = far;
        }

        /// <summary>
        /// A generic light that is turned off
        /// </summary>
        public static PointLight DisabledLight = new PointLight(false, Vector3.Zero, 0.1f, 1.0f, Color.Black, Color.Black);
    }

    /// <summary>
    /// Represents a spot light
    /// </summary>
    public class SpotLight : Light
    {
        /// <summary>
        /// The world-space position of this light
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// The world-space direction of this light
        /// MUST BE NORMALIZED
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// How different the angle to a lit object must be to start losing light (rad)
        /// </summary>
        public float InnerAngle;
        /// <summary>
        /// How different the angle to a lit object must be to recieve no light (rad)
        /// </summary>
        public float OuterAngle;

        /// <summary>
        /// Creates a new point light
        /// </summary>
        /// <param name="enabled">Whether or not this light is turned on</param>
        /// <param name="position">The world-space position of this light</param>
        /// <param name="position">The world-space direction of this light</param>
        /// <param name="near">How different the angle to a lit object must be to start losing light (rad)</param>
        /// <param name="far">How different the angle to a lit object must be to recieve no light (rad)</param>
        /// <param name="diffuse">The diffuse value of this light (don't use alpha)</param>
        /// <param name="specular">The specular value of this light (don't use alpha)</param>
        public SpotLight(bool enabled, Vector3 position, Vector3 direction, float near, float far, Color diffuse, Color specular)
        {
            Enabled = enabled;
            Position = position;
            Direction = Vector3.Normalize(direction);
            Diffuse = diffuse;
            Specular = specular;
            InnerAngle = near;
            OuterAngle = far;
        }

        /// <summary>
        /// A generic light that is turned off
        /// </summary>
        public static SpotLight DisabledLight = new SpotLight(false, Vector3.Zero, new Vector3(0,-1,0), 0.1f, 1.0f, Color.Black, Color.Black);
    }

    public class FVShader
    {
        // ******************************************************************************************************
        // Private Members
        // ******************************************************************************************************

        private const int NUM_LIGHTS = 8;

        private Effect innerShader;
        private Texture2D diffuseTex, normalTex, specularTex;
        private TextureCube cube;
        private bool textureEnabled, normalMappingEnabled, lightingEnabled, specularEnabled;
        private Matrix world, view, viewInverse, projection;
        private DirectionalLight dLight;
        private PointLight[] pLights;
        private SpotLight[] sLights;
        private float shininess;
        private Color ambient, diffuse, specular;
        private float alpha;
        private bool begun;

        // point light arrays
        private Vector3[] PLPosition, PLDiffuse, PLSpecular;
        private float[] PLNear, PLFar;
        private bool[] PLOn;

        // spot light arrays
        private Vector3[] SLPosition, SLDirection, SLDiffuse, SLSpecular;
        private float[] SLNear, SLFar;
        private bool[] SLOn;

        private String techniqueName;

        // ******************************************************************************************************
        // Constructors
        // ******************************************************************************************************

        /// <summary>
        /// Creates an FVShader with an XNA BasicEffect
        /// </summary>
        /// <param name="device">The GraphicsDevice to handle this Shader</param>
        public FVShader(GraphicsDevice device)
        {
            innerShader = new BasicEffect(device, new EffectPool());
            (innerShader as BasicEffect).PreferPerPixelLighting = true;

            dLight = DirectionalLight.DisabledLight;
            pLights = new PointLight[NUM_LIGHTS];
            for (int i = 0; i < NUM_LIGHTS; i++)
                pLights[i] = PointLight.DisabledLight;
            sLights = new SpotLight[NUM_LIGHTS];
            for (int i = 0; i < NUM_LIGHTS; i++)
                sLights[i] = SpotLight.DisabledLight;

            PLPosition = new Vector3[NUM_LIGHTS];
            PLDiffuse = new Vector3[NUM_LIGHTS];
            PLSpecular = new Vector3[NUM_LIGHTS];
            PLNear = new float[NUM_LIGHTS];
            PLFar = new float[NUM_LIGHTS];
            PLOn = new bool[NUM_LIGHTS];

            SLPosition = new Vector3[NUM_LIGHTS];
            SLDirection = new Vector3[NUM_LIGHTS];
            SLDiffuse = new Vector3[NUM_LIGHTS];
            SLSpecular = new Vector3[NUM_LIGHTS];
            SLNear = new float[NUM_LIGHTS];
            SLFar = new float[NUM_LIGHTS];
            SLOn = new bool[NUM_LIGHTS];
        }

        /// <summary>
        /// Creates an FVShader based on a custom effect
        /// </summary>
        /// <param name="device">The GraphicsDevice to handle this Shader</param>
        /// <param name="effect">The Shader to build this FVShader on</param>
        public FVShader(GraphicsDevice device, Effect effect, String techniqueName)
        {
            innerShader = effect;

            this.techniqueName = techniqueName;

            Texture2D white = new Texture2D(device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] cWhite = { Color.White };
            white.SetData<Color>(cWhite);
            Texture2D blue = new Texture2D(device, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] cBlue = { new Color(128, 128, 255) };
            blue.SetData<Color>(cBlue);
            TextureCube c = new TextureCube(device, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            c.SetData<Color>(CubeMapFace.NegativeX, cWhite);
            c.SetData<Color>(CubeMapFace.PositiveX, cWhite);
            c.SetData<Color>(CubeMapFace.NegativeY, cWhite);
            c.SetData<Color>(CubeMapFace.PositiveY, cWhite);
            c.SetData<Color>(CubeMapFace.NegativeZ, cWhite);
            c.SetData<Color>(CubeMapFace.PositiveZ, cWhite);

            DiffuseTexture = white;
            NormalMapTexture = blue;
            SpecularMapTexture = white;
            CubeMapTexture = c;

            dLight = DirectionalLight.DisabledLight;
            pLights = new PointLight[NUM_LIGHTS];
            for (int i = 0; i < NUM_LIGHTS; i++)
                pLights[i] = PointLight.DisabledLight;
            sLights = new SpotLight[NUM_LIGHTS];
            for (int i = 0; i < NUM_LIGHTS; i++)
                sLights[i] = SpotLight.DisabledLight;

            PLPosition = new Vector3[NUM_LIGHTS];
            PLDiffuse = new Vector3[NUM_LIGHTS];
            PLSpecular = new Vector3[NUM_LIGHTS];
            PLNear = new float[NUM_LIGHTS];
            PLFar = new float[NUM_LIGHTS];
            PLOn = new bool[NUM_LIGHTS];

            SLPosition = new Vector3[NUM_LIGHTS];
            SLDirection = new Vector3[NUM_LIGHTS];
            SLDiffuse = new Vector3[NUM_LIGHTS];
            SLSpecular = new Vector3[NUM_LIGHTS];
            SLNear = new float[NUM_LIGHTS];
            SLFar = new float[NUM_LIGHTS];
            SLOn = new bool[NUM_LIGHTS];

            SetDirectionalLight(dLight);
            SetPointLight(0, pLights[0]);
            SetSpotLight(0, sLights[0]);
        }

        // ******************************************************************************************************
        // Public Mutators
        // ******************************************************************************************************

        /// <summary>
        /// Gets this FVShader's inner effect (for setting model Effects, etc)
        /// </summary>
        public Effect InnerEffect
        {
            get { return innerShader; }
        }

        /// <summary>
        /// If false, all geometry is "fullbright"
        /// </summary>
        public bool LightingEnabled
        {
            set 
            { 
                lightingEnabled = value;
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).LightingEnabled = lightingEnabled;
                if (lightingEnabled)
                {
                    SetDirectionalLight(dLight);
                    for (int i = 0; i < NUM_LIGHTS; i++)
                        SetPointLight(i, pLights[i]);
                }
                else
                {
                    SetDirectionalLight(DirectionalLight.DisabledLight);
                    for (int i = 0; i < NUM_LIGHTS; i++)
                        SetPointLight(i, PointLight.DisabledLight);
                }
            }
            get { return lightingEnabled; }
        }

        /// <summary>
        /// This is the unlit light value
        /// Note there is no "ambient light"
        /// Please, no alpha
        /// </summary>
        public Color AmbientMaterial
        {
            get { return ambient; }
            set 
            { 
                ambient = value;
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).AmbientLightColor = value.ToVector3();
                else
                    innerShader.Parameters["Ambient"].SetValue(value.ToVector4());
            }
        }

        /// <summary>
        /// Sets the material for Diffuse lighting
        /// Please, no alpha
        /// </summary>
        public Color DiffuseMaterial
        {
            get { return diffuse; }
            set 
            {
                diffuse = value;
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).DiffuseColor = value.ToVector3();
                else
                    innerShader.Parameters["Diffuse"].SetValue(value.ToVector4());
            }
        }

        /// <summary>
        /// Sets the material for Specular lighting
        /// Please, no alpha
        /// </summary>
        public EffectParameter _ep_spec = null;
        public Color SpecularMaterial
        {
            get { return specular; }
            set 
            {
                specular = value;
                if (_ep_spec == null && !(innerShader is BasicEffect))
                    _ep_spec = innerShader.Parameters["Specular"];
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).SpecularColor = value.ToVector3();
                else
                    _ep_spec.SetValue(value.ToVector4());
            }
        }

        /// <summary>
        /// Whether specular lighting is enabled or not
        /// </summary>
        public bool SpecularEnabled
        {
            set
            {
                specularEnabled = value;
                SetDirectionalLight(dLight);
                for (int i = 0; i < NUM_LIGHTS; i++)
                    SetPointLight(i, pLights[i]);
                innerShader.Parameters["specularTex"].SetValue(specularTex);
            }
            get { return specularEnabled; }
        }

        /// <summary>
        /// The Specular Power Variable
        /// </summary>
        public EffectParameter _ep_shin = null;
        public float Shininess
        {
            set
            {
                shininess = value;
                if (_ep_shin == null && !(innerShader is BasicEffect))
                    _ep_shin = innerShader.Parameters["SpecularPower"];
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).SpecularPower = shininess;
                else
                    _ep_shin.SetValue(shininess);
            }
            get { return shininess; }
        }

        /// <summary>
        /// The World Alpha value
        /// Multiplied by texture and vertex alpha
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            set 
            { 
                alpha = value;
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).Alpha = value;
                else
                    innerShader.Parameters["Alpha"].SetValue(value);
            }
        }

        /// <summary>
        /// Gets or Sets this Shader's Directional Light
        /// </summary>
        public DirectionalLight DirectionalLight
        {
            set
            {
                dLight = value;
                if (lightingEnabled)
                {
                    SetDirectionalLight(dLight);
                }
            }
            get { return dLight; }
        }

        public SpotLight SpotLight0
        {
            set
            {
                sLights[0] = value;
                if (lightingEnabled)
                {
                    SetSpotLight(0, sLights[0]);
                }
            }
            get { return sLights[0]; }
        }

        public SpotLight SpotLight1
        {
            set
            {
                sLights[1] = value;
                if (lightingEnabled)
                {
                    SetSpotLight(1, sLights[1]);
                }
            }
            get { return sLights[1]; }
        }

        public SpotLight SpotLight2
        {
            set
            {
                sLights[2] = value;
                if (lightingEnabled)
                {
                    SetSpotLight(2, sLights[2]);
                }
            }
            get { return sLights[2]; }
        }

        public SpotLight SpotLight3
        {
            set
            {
                sLights[3] = value;
                if (lightingEnabled)
                {
                    SetSpotLight(3, sLights[3]);
                }
            }
            get { return sLights[3]; }
        }

        public PointLight PointLight0
        {
            set
            {
                pLights[0] = value;
                if (lightingEnabled)
                {
                    SetPointLight(0,pLights[0]);
                }
            }
            get { return pLights[0]; }
        }

        public PointLight PointLight1
        {
            set
            {
                pLights[1] = value;
                if (lightingEnabled)
                {
                    SetPointLight(1, pLights[1]);
                }
            }
            get { return pLights[1]; }
        }

        public PointLight PointLight2
        {
            set
            {
                pLights[2] = value;
                if (lightingEnabled)
                {
                    SetPointLight(2, pLights[2]);
                }
            }
            get { return pLights[2]; }
        }

        public PointLight PointLight3
        {
            set
            {
                pLights[3] = value;
                if (lightingEnabled)
                {
                    SetPointLight(3, pLights[3]);
                }
            }
            get { return pLights[3]; }
        }

        public Texture2D DiffuseTexture
        {
            set
            {
                diffuseTex = value;
                if (textureEnabled)
                {
                    if (innerShader is BasicEffect)
                        (innerShader as BasicEffect).Texture = value;
                    else
                        innerShader.Parameters["diffuseTex"].SetValue(value);
                }
            }
        }

        public TextureCube CubeMapTexture
        {
            set
            {
                cube = value;
                if (textureEnabled)
                {
                    if (!(innerShader is BasicEffect))
                        innerShader.Parameters["cubeTex"].SetValue(value);
                }
            }
        }

        public Texture2D NormalMapTexture
        {
            set
            {
                normalTex = value;
                if (normalMappingEnabled)
                {
                    if (!(innerShader is BasicEffect))
                        innerShader.Parameters["normalTex"].SetValue(value);
                }
            }
        }

        public Texture2D SpecularMapTexture
        {
            set
            {
                specularTex = value;
                if (specularEnabled)
                {
                    if (!(innerShader is BasicEffect))
                        innerShader.Parameters["specularTex"].SetValue(value);
                }
            }
        }

        public bool TextureEnabled
        {
            set
            {
                textureEnabled = value;
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).TextureEnabled = value;
                if (textureEnabled && diffuseTex!=null && !diffuseTex.IsDisposed)
                {
                    if (innerShader is BasicEffect)
                        (innerShader as BasicEffect).Texture = diffuseTex;
                    else
                        innerShader.Parameters["diffuseTex"].SetValue(diffuseTex);
                }
            }
            get { return textureEnabled; }
        }

        public bool NormalMapEnabled
        {
            set
            {
                if (!normalMappingEnabled && value && normalTex != null && !normalTex.IsDisposed)
                {
                    if (!(innerShader is BasicEffect))
                    {
                        innerShader.Parameters["normalTex"].SetValue(normalTex);
                    }
                }
                if (!UpdateTechnique() && normalMappingEnabled && !value)
                {
                    innerShader.Parameters["normalTex"].SetValue(Global.TexDefaultBM);
                }
                normalMappingEnabled = value;
            }
            get { return normalMappingEnabled; }
        }

        private EffectParameter _ep_world=null;
        public Matrix World
        {
            set 
            {
                if (_ep_world == null && !(innerShader is BasicEffect))
                {
                    _ep_world = innerShader.Parameters["World"];
                }
                world = value;
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).World = world;
                else
                    _ep_world.SetValue(world);
            }
            get { return world; }
        }

        public Matrix View
        {
            set 
            { 
                view = value; 
                viewInverse = Matrix.Invert(value);
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).View = view;
                else
                {
                    innerShader.Parameters["View"].SetValue(view);

                    innerShader.Parameters["EyePos"].SetValue(new Vector3(viewInverse.M41,viewInverse.M42,viewInverse.M43));
                }
            }
            get { return view; }
        }

        public Matrix ViewInverse
        {
            get { return viewInverse; }
        }

        public Matrix Projection
        {
            set 
            {
                projection = value;
                if (innerShader is BasicEffect)
                    (innerShader as BasicEffect).Projection = projection;
                else
                    innerShader.Parameters["Projection"].SetValue(projection);
            }
            get { return projection; }
        }

        public EffectTechnique CurrentTechnique
        {
            set { innerShader.CurrentTechnique = value; }
            get { return innerShader.CurrentTechnique; }
        }

        public EffectTechniqueCollection Techniques
        {
            get { return innerShader.Techniques; }
        }

        // ******************************************************************************************************
        // Functions
        // ******************************************************************************************************

        public void Begin()
        {
            //UpdateTechnique();
            begun = true;
            innerShader.Begin();
        }

        public void End()
        {
            innerShader.End();
            begun = false;
        }

        public void CommitChanges()
        {
            innerShader.CommitChanges();
        }

        private void SetPointLight(int index, PointLight light)
        {
            if (innerShader.Parameters["pLightPos"] != null)
            {
                PLPosition[index] = light.Position;
                PLDiffuse[index] = light.Diffuse.ToVector3();
                PLSpecular[index] = SpecularEnabled ? light.Specular.ToVector3() : new Vector3(0, 0, 0);
                PLNear[index] = light.NearFade;
                PLFar[index] = light.FarFade;
                PLOn[index] = light.Enabled;
                int numPL = 0;
                for (int i = 0; i < PLOn.Length; i++)
                    if (PLOn[i])
                        numPL = i + 1;

                if (!(innerShader is BasicEffect))
                {
                    innerShader.Parameters["pLightPos"].SetValue(PLPosition);
                    innerShader.Parameters["pLightDiffuse"].SetValue(PLDiffuse);
                    innerShader.Parameters["pLightSpecular"].SetValue(PLSpecular);
                    innerShader.Parameters["pLightNear"].SetValue(PLNear);
                    innerShader.Parameters["pLightFar"].SetValue(PLFar);
                    if (innerShader.Parameters["pLightOn"] != null)
                        innerShader.Parameters["pLightOn"].SetValue(PLOn);
                    if (innerShader.Parameters["pLightNum"] != null)
                        innerShader.Parameters["pLightNum"].SetValue(numPL);
                }
            }
        }

        private void SetSpotLight(int index, SpotLight light)
        {
            if (innerShader.Parameters["sLightPos"] != null)
            {
                SLPosition[index] = light.Position;
                SLDirection[index] = Vector3.Normalize(light.Direction);
                SLDiffuse[index] = light.Diffuse.ToVector3();
                SLSpecular[index] = SpecularEnabled ? light.Specular.ToVector3() : new Vector3(0, 0, 0);
                SLNear[index] = light.InnerAngle;
                SLFar[index] = light.OuterAngle;
                SLOn[index] = light.Enabled;
                int numSL = 0;
                for (int i = 0; i < SLOn.Length; i++)
                    if (SLOn[i])
                        numSL = i + 1;

                if (!(innerShader is BasicEffect))
                {
                    innerShader.Parameters["sLightPos"].SetValue(SLPosition);
                    innerShader.Parameters["sLightDir"].SetValue(SLDirection);
                    innerShader.Parameters["sLightDiffuse"].SetValue(SLDiffuse);
                    innerShader.Parameters["sLightSpecular"].SetValue(SLSpecular);
                    innerShader.Parameters["sLightNarrow"].SetValue(SLNear);
                    innerShader.Parameters["sLightWide"].SetValue(SLFar);
                    if (innerShader.Parameters["sLightOn"] != null)
                        innerShader.Parameters["sLightOn"].SetValue(SLOn);
                    if (innerShader.Parameters["sLightNum"] != null)
                        innerShader.Parameters["sLightNum"].SetValue(numSL);
                }
            }
        }

        private void SetDirectionalLight(DirectionalLight light)
        {
            if (innerShader is BasicEffect)
            {
                (innerShader as BasicEffect).DirectionalLight0.DiffuseColor = light.Diffuse.ToVector3();
                (innerShader as BasicEffect).DirectionalLight0.SpecularColor = SpecularEnabled ? light.Specular.ToVector3() : new Vector3(0, 0, 0);
                (innerShader as BasicEffect).DirectionalLight0.Direction = Vector3.Normalize(light.Direction);
                (innerShader as BasicEffect).DirectionalLight0.Enabled = light.Enabled;
            }
            else
            {
                if (light.Enabled)
                {
                    innerShader.Parameters["LightDirection"].SetValue(light.Direction);
                    innerShader.Parameters["LightDiffuse"].SetValue(light.Diffuse.ToVector4());
                    innerShader.Parameters["LightSpecular"].SetValue(SpecularEnabled ? light.Specular.ToVector4() : new Vector4(0, 0, 0, 1));
                }
                else
                {
                    innerShader.Parameters["LightDiffuse"].SetValue(new Vector4(0, 0, 0, 1));
                    innerShader.Parameters["LightSpecular"].SetValue(new Vector4(0, 0, 0, 1));
                }
            }
        }

        public bool UpdateTechnique()
        {
            if (!begun)
            {
                if (!(innerShader is BasicEffect))
                {
                    if (lightingEnabled)
                    {
                        if (NormalMapEnabled)
                        {
                            if (SpecularEnabled)
                                innerShader.CurrentTechnique = innerShader.Techniques[techniqueName];
                            else
                                innerShader.CurrentTechnique = innerShader.Techniques[techniqueName + "_NO_SP"];
                        }
                        else
                        {
                            if (SpecularEnabled)
                                innerShader.CurrentTechnique = innerShader.Techniques[techniqueName + "_NO_NM"];
                            else
                                innerShader.CurrentTechnique = innerShader.Techniques[techniqueName + "_NO_NMSP"];
                        }
                    }
                    else
                        innerShader.CurrentTechnique = innerShader.Techniques[techniqueName + "_NO_LT"];
                }
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            innerShader.Dispose();
        }
    }
}
