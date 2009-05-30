using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using FVProductions.Utility;

namespace Unsigned
{
    static class Configuration
    {
        /// <summary>
        /// The filename to save out configuration data
        /// </summary>
        public static String ConfigFileName = "Configuration\\config.cfg";

        /// <summary>
        /// Whether the game is running in fullscreen or windowed
        /// </summary>
        public static bool FullScreen { get; set; }

        /// <summary>
        /// Whether the game window is fullscreen or widescreen
        /// is irrelevant if FullScreen is true
        /// </summary>
        public static bool WideScreen { get; set; }

        /// <summary>
        /// Whether or not the game should render the background venues
        /// </summary>
        static bool _rv;
        public static bool RenderVenues { get { return _rv; }
            set
            {
                _rv = value;
            }
        }

        /// <summary>
        /// If the screen should be rendered at half-res and scaled up for performance issues
        /// </summary>
        public static bool HalfRender { get; set; }

        /// <summary>
        /// The style of the Graphical User Interface
        /// </summary>
        public static GameUIMaster.GUIStyle GUIStyle { get; set; }

        /// <summary>
        /// An int from 0-10 describing how smooth the held-note waves are
        /// </summary>
        private static int _waveDetail;
        public static int WaveDetail
        {
            get { return _waveDetail; }
            set { _waveDetail = Math.Min(10, Math.Max(0, value)); }
        }

        /// <summary>
        /// An int from 0-5 describing how many particles can be on screen
        /// </summary>
        private static int _partDetail;
        public static int ParticleDetail
        {
            get { return _partDetail; }
            set { _partDetail = Math.Min(5, Math.Max(0, value)); }
        }

        /// <summary>
        /// The description Strings for each level of ParticleDetail
        /// </summary>
        public static String[] ParticleDetails =
        {
            "None", "Minimal", "Low", "Normal", "High", "Excessive",
        };

        /// <summary>
        /// Possible Resolution Widths
        /// </summary>
        public static int[] ResolutionWidthOptions = { 640, 800, 960, 1024, 1152, 1280, 1440, 1600, 1920, 2048};

        /// <summary>
        /// Possible Resolution Heights (Fullscreen)
        /// </summary>
        public static int[] ResolutionHeightFullOptions = { 480, 600, 720,  768,  864,  960, 1080, 1200, 1440, 1536};

        /// <summary>
        /// Possible Resolution Heights (Widescreen)
        /// </summary>
        public static int[] ResolutionHeightWideOptions = { 360, 480, 540,  576,  720,  720,  900, 1024, 1200, 1152};

        /// <summary>
        /// The current resolution index, matches above arrays
        /// </summary>
        private static int _resIndex;
        public static int ResIndex
        {
            get { return _resIndex; }
            set 
            {
                int res = value;
                while (res >= ResolutionWidthOptions.Length)
                    res -= ResolutionWidthOptions.Length;
                while (res < 0)
                    res += ResolutionWidthOptions.Length;
                _resIndex = res;
            }
        }
        
        /// <summary>
        /// Whether surfaces are normal mapped for additional detail
        /// </summary>
        private static bool _normalMapping;
        public static bool NormalMapping 
        {
            get { return Lighting ? _normalMapping : false; }
            set { _normalMapping = value; }
        }
        
        /// <summary>
        /// Whether surfaces have specular shine
        /// </summary>
        private static bool _specular;
        public static bool Specular
        {
            get { return Lighting ? _specular : false; }
            set { _specular = value; }
        }

        /// <summary>
        /// What language this is set to
        /// </summary>
        public static Localizer.Language CurrentLanguage;

        /// <summary>
        /// Whether lighting is enabled at all for rendering
        /// </summary>
        public static bool Lighting { get; set; }

        public static void LoadDefaults()
        {
            FullScreen = false;
            RenderVenues = true;
            HalfRender = false;
            GUIStyle = GameUIMaster.GUIStyle.UN;
            WaveDetail = 10;
            ParticleDetail = 5;
            ResIndex = 0;
            Lighting = true;
            NormalMapping = true;
            Specular = true;
            CurrentLanguage = Localizer.Language.ENGLISH;
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower() == "es")
                CurrentLanguage = Localizer.Language.SPANISH;
            else if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower() == "fr")
                CurrentLanguage = Localizer.Language.FRENCH;
            else if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower() == "de")
                CurrentLanguage = Localizer.Language.GERMAN;
        }

        public static void Load()
        {
            StreamReader fin;
            bool l = true, n = false, s = false;

            try
            {
                fin = new System.IO.StreamReader(ConfigFileName);
            }
            catch (Exception)
            {
                Debug.Error("The configuration file \""+ConfigFileName+"\" cannot be found.\nLoading default values");
                LoadDefaults();
                return;
            }

            do
            {
                String str = fin.ReadLine();
                if (str.Length > 10)
                {
                    String varName = str.Substring(0, 10).ToLower();
                    try
                    {
                        if (varName == "wavedetail")
                        {
                            WaveDetail = Int32.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "resolution")
                        {
                            int windowwidth = Int32.Parse(str.Substring(str.IndexOf('=') + 1, Math.Max(str.IndexOf('x'), str.IndexOf('X')) - (str.IndexOf('=') + 1)).Trim());
                            int windowheight = Int32.Parse(str.Substring(Math.Max(str.IndexOf('x'), str.IndexOf('X')) + 1).Trim());
                            ResIndex = 0;
                            for (int i = 0; i < ResolutionWidthOptions.Length; i++)
                                if (ResolutionWidthOptions[i] == windowwidth)
                                    ResIndex = i;
                        }
                        else if (varName == "fullscreen")
                        {
                            FullScreen = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "widescreen")
                        {
                            WideScreen = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "halfrender")
                        {
                            HalfRender = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "rendervnus")
                        {
                            RenderVenues = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "igguistyle")
                        {
                            String strn = str.Substring(str.IndexOf('=') + 1).Trim();
                            GUIStyle = strn.ToLower().Equals("rockband") ? GameUIMaster.GUIStyle.RB : GameUIMaster.GUIStyle.UN;
                        }
                        else if (varName == "particldtl")
                        {
                            ParticleDetail = Int32.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "lightingon")
                        {
                            l = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "nrmmapping")
                        {
                            n = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "specularhl")
                        {
                            s = Boolean.Parse(str.Substring(str.IndexOf('=') + 1).Trim());
                        }
                        else if (varName == "txlanguage")
                        {
                            String vl = str.Substring(str.IndexOf('=') + 1).Trim().ToUpper();
                            for (int i = 0; i < Localizer.LangCodes.Length; i++)
                                if (vl.Equals(Localizer.LangCodes[i]))
                                    CurrentLanguage = (Localizer.Language)i;
                        }
                    }
                    catch (FormatException)
                    {
                        Debug.Warning("Configuration value parse failed ("+varName+")");
                    }
                    catch (Exception)
                    {
                        Debug.Error("General Configuration Load Error\nLoading default values");
                        fin.Close();
                        LoadDefaults();
                        return;
                    }
                }
            } while (!fin.EndOfStream);

            Lighting = l;
            if (Lighting)
            {
                NormalMapping = n;
                if (NormalMapping)
                    Specular = s;
                else
                    Specular = false;
            }
            else
            {
                NormalMapping = false;
                Specular = false;
            }

            fin.Close();
        }

        public static void Save()
        {
            System.IO.StreamWriter fout;

            try
            {
                fout = new System.IO.StreamWriter(ConfigFileName);
            }
            catch (Exception)
            {
                Debug.Error("Configuration file could not be opened for writing");
                return;
            }

            try
            {
                fout.WriteLine("WaveDetail = " + WaveDetail);
                fout.WriteLine("Resolution = " + Global.ScreenWidth + "x" + Global.ScreenHeight);
                fout.WriteLine("FullScreen = " + FullScreen);
                fout.WriteLine("WideScreen = " + WideScreen);
                fout.WriteLine("HalfRender = " + HalfRender);
                fout.WriteLine("RenderVnus = " + RenderVenues);
                fout.WriteLine("IGGUIStyle = " + (GUIStyle == GameUIMaster.GUIStyle.GH ? "guitarhero" : GUIStyle == GameUIMaster.GUIStyle.RB ? "rockband" : "unsigned"));
                fout.WriteLine("particldtl = " + ParticleDetail);
                fout.WriteLine("LightingOn = " + Lighting);
                fout.WriteLine("NrmMapping = " + NormalMapping);
                fout.WriteLine("SpecularHl = " + Specular);
                fout.WriteLine("TxLanguage = " + Localizer.LangCodes[(int)CurrentLanguage]);
            }
            catch (Exception e)
            {
                Debug.Error("General Configuration Save Error", e);
            }
            finally
            {
                fout.Close();
            }
        }
    }
}
