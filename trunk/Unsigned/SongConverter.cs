using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    class SongConverter
    {
        public static bool ConvertSong(String fn)
        {

            if (!System.IO.File.Exists("songdata\\" + fn + ".gba"))
            {
#if WINDOWS
                System.Windows.Forms.MessageBox.Show("songdata not found");
#endif
                return false;
            }
//            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite("songdata\\" + fn + ".uns"));
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + fn + ".gba"));
            byte version = reader.ReadByte();
            if (version < 12)
            {

#if WINDOWS
                System.Windows.Forms.MessageBox.Show("SongData Version too old");
#endif
                return false;
            }
            String SongName = reader.ReadString();
            String ArtistName = reader.ReadString();
            int year = reader.ReadInt32();
            String genre = reader.ReadString();
            String songLength = reader.ReadString();

            String z = songLength;
            uint iSongLength = 0;
            iSongLength += 3600u*UInt32.Parse(z.Substring(0, z.IndexOf(':')));
            z = z.Substring(z.IndexOf(':') + 1);
            iSongLength += 60u*UInt32.Parse(z.Substring(0, z.IndexOf(':')));
            iSongLength += UInt32.Parse(z.Substring(z.IndexOf(':') + 1));

            String[] quotes = new string[8];
            for (int i = 0; i < quotes.Length; i++)
                quotes[i] = reader.ReadString();

            int numCharters = 6;
            String[] charters = new String[numCharters];
            for (int i = 0; i < numCharters; i++)
                charters[i] = reader.ReadString();

            byte[] diffs = new byte[4];
            for(int i=0;i<4;i++)
                diffs[i] = reader.ReadByte();
            Vector2[] Bars = new Vector2[reader.ReadInt32()];
            for (int c = 0; c < Bars.Length; c++)
            {
                Bars[c] = new Vector2(reader.ReadInt32(), reader.ReadInt32());
            }
            int endLength = reader.ReadInt32();

            reader.Close();
            reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + fn + ".gbg"));

            reader.ReadByte();//version
            uint[] GuitarSPS = new uint[reader.ReadInt32()];
            uint[] GuitarSPE = new uint[GuitarSPS.Length];
            for (int i = 0; i < GuitarSPS.Length; i++)
            {
                GuitarSPS[i] = reader.ReadUInt32();
                GuitarSPE[i] = reader.ReadUInt32();
            }
            NoteSet[][] GuitarNotes = new NoteSet[4][];
            uint[][] GuitarStarLevels = new uint[4][];
            for (int i = 0; i < 4; i++)
            {
                int k = reader.ReadInt32();
                GuitarNotes[k] = new NoteSet[reader.ReadInt32()];
                for (int j = 0; j < GuitarNotes[k].Length; j++)
                {
                    GuitarNotes[k][j] = new NoteSet();
                    GuitarNotes[k][j].type = reader.ReadByte();
                    GuitarNotes[k][j].time = reader.ReadUInt32();
                    GuitarNotes[k][j].length = reader.ReadInt32();
                }
                GuitarStarLevels[k] = new uint[5];
                for (int j = 0; j < 5; j++)
                    GuitarStarLevels[k][j] = reader.ReadUInt32();
            }
            reader.Close();

            reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + fn + ".gbb"));

            reader.ReadByte();//version
            uint[] BassSPS = new uint[reader.ReadInt32()];
            uint[] BassSPE = new uint[BassSPS.Length];
            for (int i = 0; i < BassSPS.Length; i++)
            {
                BassSPS[i] = reader.ReadUInt32();
                BassSPE[i] = reader.ReadUInt32();
            }
            NoteSet[][] BassNotes = new NoteSet[4][];
            uint[][] BassStarLevels = new uint[4][];
            for (int i = 0; i < 4; i++)
            {
                int k = reader.ReadInt32();
                BassNotes[k] = new NoteSet[reader.ReadInt32()];
                for (int j = 0; j < BassNotes[k].Length; j++)
                {
                    BassNotes[k][j] = new NoteSet();
                    BassNotes[k][j].type = reader.ReadByte();
                    BassNotes[k][j].time = reader.ReadUInt32();
                    BassNotes[k][j].length = reader.ReadInt32();
                }
                BassStarLevels[k] = new uint[5];
                for (int j = 0; j < 5; j++)
                    BassStarLevels[k][j] = reader.ReadUInt32();
            }
            reader.Close();

            reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + fn + ".gbd"));

            reader.ReadByte();//version
            uint[] DrumsSPS = new uint[reader.ReadInt32()];
            uint[] DrumsSPE = new uint[DrumsSPS.Length];
            for (int i = 0; i < DrumsSPS.Length; i++)
            {
                DrumsSPS[i] = reader.ReadUInt32();
                DrumsSPE[i] = reader.ReadUInt32();
            }
            uint[] DrumsDFS = new uint[reader.ReadInt32()];
            uint[] DrumsDFE = new uint[DrumsDFS.Length];
            for (int i = 0; i < DrumsDFS.Length; i++)
            {
                DrumsDFS[i] = reader.ReadUInt32();
                DrumsDFE[i] = reader.ReadUInt32();
            }
            NoteSet[][] DrumsNotes = new NoteSet[4][];
            uint[][] DrumsStarLevels = new uint[4][];
            for (int i = 0; i < 4; i++)
            {
                int k = reader.ReadInt32();
                DrumsNotes[k] = new NoteSet[reader.ReadInt32()];
                for (int j = 0; j < DrumsNotes[k].Length; j++)
                {
                    DrumsNotes[k][j] = new NoteSet();
                    DrumsNotes[k][j].type = reader.ReadByte();
                    DrumsNotes[k][j].time = reader.ReadUInt32();
                }
                DrumsStarLevels[k] = new uint[5];
                for (int j = 0; j < 5; j++)
                    DrumsStarLevels[k][j] = reader.ReadUInt32();
            }
            reader.Close();

            reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + fn + ".gbv"));

            reader.ReadByte();//version

            VocalPhrase[] vocals = new VocalPhrase[reader.ReadUInt32()];
            uint[][] VocalStarLevels = new uint[4][];
            for (int i = 0; i < vocals.Length; i++)
            {
                vocals[i].time = reader.ReadUInt32();
                vocals[i].type = (VocalPhrase.TYPE)reader.ReadByte();
                vocals[i].words = new VocalWord[reader.ReadInt32()];
                if (vocals[i].type == VocalPhrase.TYPE.REGULAR)
                {
                    for (int k = 0; k < vocals[i].words.Length; k++)
                    {
                        vocals[i].words[k] = new VocalWord();
                        vocals[i].words[k].time = reader.ReadUInt32();
                        vocals[i].words[k].len = reader.ReadUInt32();
                        vocals[i].words[k].sNote = reader.ReadInt16();
                        vocals[i].words[k].value = reader.ReadString();
                    }
                }
                else if (vocals[i].type == VocalPhrase.TYPE.RHYTHM)
                {
                    vocals[i].rType = (VocalPhrase.RTYPE)reader.ReadByte();
                    for (int k = 0; k < vocals[i].words.Length; k++)
                    {
                        vocals[i].words[k] = new VocalWord();
                        vocals[i].words[k].time = reader.ReadUInt32();
                    }
                }
            }
            VocalStarLevels[0] = new uint[5];
            for (int j = 0; j < 5; j++)
                VocalStarLevels[0][j] = reader.ReadUInt32();
            reader.Close();

            reader = new System.IO.BinaryReader(System.IO.File.OpenRead("songdata\\" + fn + ".gbe"));

            reader.ReadByte();//version
            uint[] camSwitches = new uint[reader.ReadInt32()];
            for (int i = 0; i < camSwitches.Length; i++)
                camSwitches[i] = reader.ReadUInt32();

            reader.Close();

            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite("songdata\\" + fn + ".uns"));

            writer.Write('U');
            writer.Write('N');
            writer.Write('S');

            int offsetToGBA = 3 + (4 * 6);

            int offsetToGBG = offsetToGBA;
            offsetToGBG += 1 + 1 + 1 + 1 + 1 + 4;
            offsetToGBG += SongName.Length + ArtistName.Length
                        + genre.Length + songLength.Length;
            for (int i = 0; i < 8; i++)
                offsetToGBG += ((quotes[i].Length/128)+1)+quotes[i].Length;
            offsetToGBG += 6 * 1;
            for (int i = 0; i < 6; i++)
                offsetToGBG += charters[i].Length;
            offsetToGBG += 1 * 4;
            offsetToGBG += 4;
            offsetToGBG += 8 * Bars.Length;
            offsetToGBG += 4;
            offsetToGBG += 1 + 4 + 4;
            offsetToGBG += 4;

            int offsetToGBB = offsetToGBG;
            offsetToGBB += 4 + 4;
            offsetToGBB += GuitarSPS.Length * 8;
            for (int i = 0; i < 4; i++)
            {
                offsetToGBB += 4 * 7;
                offsetToGBB += 9 * GuitarNotes[i].Length;
            }

            int offsetToGBD = offsetToGBB;
            offsetToGBD += 4;
            offsetToGBD += BassSPS.Length * 8;
            for (int i = 0; i < 4; i++)
            {
                offsetToGBD += 4 * 7;
                offsetToGBD += 9 * BassNotes[i].Length;
            }

            int offsetToGBV = offsetToGBD;
            offsetToGBV += 4 + 4;
            offsetToGBV += DrumsSPS.Length * 8;
            offsetToGBV += DrumsDFS.Length * 8;
            for (int i = 0; i < 4; i++)
            {
                offsetToGBV += 4 * 7;
                offsetToGBV += 5 * DrumsNotes[i].Length;
            }

            int offsetToGBE = offsetToGBV;
            offsetToGBE += 4;
            for (int i = 0; i < vocals.Length; i++)
            {
                offsetToGBE += 10;
                if (vocals[i].type == VocalPhrase.TYPE.REGULAR)
                {
                    for (int k = 0; k < vocals[i].words.Length; k++)
                    {
                        offsetToGBE += 13;
                        offsetToGBE += vocals[i].words[k].value.Length;
                    }
                }
                else if (vocals[i].type == VocalPhrase.TYPE.RHYTHM)
                {
                    offsetToGBE += 1;
                    offsetToGBE += 4 * vocals[i].words.Length;
                }
            }
            offsetToGBE += 4 * 4 * 5;

            //GBA

            writer.Write(offsetToGBA);
            writer.Write(offsetToGBG);
            writer.Write(offsetToGBB);
            writer.Write(offsetToGBD);
            writer.Write(offsetToGBV);
            writer.Write(offsetToGBE);

            writer.Write((byte)17);

            writer.Write(SongName);
            writer.Write(ArtistName);
            writer.Write(year);
            writer.Write(genre);
            writer.Write(songLength);
            for (int i = 0; i < 8; i++)
                writer.Write(quotes[i]);

            for (int i = 0; i < charters.Length; i++)
                writer.Write(charters[i]);

            for (int i = 0; i < diffs.Length; i++)
                writer.Write(diffs[i]);

            writer.Write(Bars.Length);
            for (int i = 0; i < Bars.Length; i++)
            {
                writer.Write((int)Bars[i].X);
                writer.Write((int)Bars[i].Y);
            }
            writer.Write(endLength);

            writer.Write(false);
            writer.Write(0);
            writer.Write(0);

            writer.Write(0);

            //GBG

            writer.Write(GuitarSPS.Length);
            for (int i = 0; i < GuitarSPS.Length; i++)
            {
                writer.Write(GuitarSPS[i]);
                writer.Write(GuitarSPE[i]);
            }

            writer.Write(0);

            for (int i = 3; i >= 0; i--)
            {
                writer.Write(i);
                writer.Write(GuitarNotes[i].Length);
                for (int k = 0; k < GuitarNotes[i].Length; k++)
                {
                    writer.Write(GuitarNotes[i][k].type);
                    writer.Write(GuitarNotes[i][k].time);
                    writer.Write(GuitarNotes[i][k].length);
                }
                for (int k = 0; k < 5; k++)
                    writer.Write(GuitarStarLevels[i][k]);
            }

            //GBB

            writer.Write(BassSPS.Length);
            for (int i = 0; i < BassSPS.Length; i++)
            {
                writer.Write(BassSPS[i]);
                writer.Write(BassSPE[i]);
            }

            for (int i = 3; i >= 0; i--)
            {
                writer.Write(i);
                writer.Write(BassNotes[i].Length);
                for (int k = 0; k < BassNotes[i].Length; k++)
                {
                    writer.Write(BassNotes[i][k].type);
                    writer.Write(BassNotes[i][k].time);
                    writer.Write(BassNotes[i][k].length);
                }
                for (int k = 0; k < 5; k++)
                    writer.Write(BassStarLevels[i][k]);
            }

            //GBD

            writer.Write(DrumsSPS.Length);
            for (int i = 0; i < DrumsSPS.Length; i++)
            {
                writer.Write(DrumsSPS[i]);
                writer.Write(DrumsSPE[i]);
            }

            writer.Write(DrumsDFS.Length);
            for (int i = 0; i < DrumsDFS.Length; i++)
            {
                writer.Write(DrumsDFS[i]);
                writer.Write(DrumsDFE[i]);
            }

            for (int i = 3; i >= 0; i--)
            {
                writer.Write(i);
                writer.Write(DrumsNotes[i].Length);
                for (int k = 0; k < DrumsNotes[i].Length; k++)
                {
                    writer.Write(DrumsNotes[i][k].type);
                    writer.Write(DrumsNotes[i][k].time);
                }
                for (int k = 0; k < 5; k++)
                    writer.Write(DrumsStarLevels[i][k]);
            }

            //GBV

            writer.Write(vocals.Length);

            for (int i = 0; i < vocals.Length; i++)
            {
                writer.Write(vocals[i].time);
                writer.Write((byte)vocals[i].type);
                writer.Write(false);
                writer.Write(vocals[i].words.Length);
                if (vocals[i].type == VocalPhrase.TYPE.REGULAR)
                {
                    for (int k = 0; k < vocals[i].words.Length; k++)
                    {
                        writer.Write(vocals[i].words[k].time);
                        writer.Write(vocals[i].words[k].len);
                        writer.Write(vocals[i].words[k].sNote);
                        writer.Write(vocals[i].words[k].sNote);
                        writer.Write(vocals[i].words[k].value);
                    }
                }
                else if (vocals[i].type == VocalPhrase.TYPE.RHYTHM)
                {
                    writer.Write((byte)vocals[i].rType);
                    for (int k = 0; k < vocals[i].words.Length; k++)
                        writer.Write(vocals[i].words[k].time);
                }

            }
            for (int i = 0; i < 4; i++)
                for (int k = 0; k < 5; k++)
                    writer.Write(VocalStarLevels[0][k]);

            //GBE

            writer.Write(camSwitches.Length);

            for (int i = 0; i < camSwitches.Length; i++)
                writer.Write(camSwitches[i]);

            writer.Write(0);

            writer.Write(0);
            writer.Write('l');
            writer.Write('n');
            writer.Write(iSongLength);
            writer.Write(0xFFFFFFFF);

            writer.Close();

            return true;
        }
    }
}
