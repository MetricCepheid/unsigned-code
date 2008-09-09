using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Unsigned
{
    public struct CharacterIdol
    {
        public String name, filename;
    }

    public class CharacterMaster
    {
        private static CharacterMaster SINGLETON_CharacterMaster = null;

        private List<CharacterIdol> idols;

        private CharacterMaster()
        {
            String dir = Directory.GetCurrentDirectory()+"\\";
            String[] files = Directory.GetFiles(dir+"characters\\","*.unc");
            List<CharacterIdol> list = new List<CharacterIdol>();
            for (int i = 0; i < files.Length; i++)
            {
                CharacterIdol idol = new CharacterIdol();
                BinaryReader bin = new BinaryReader(File.OpenRead(files[i]));
                if (bin.ReadChar() == 'U' && bin.ReadChar() == 'N' && bin.ReadChar() == 'C')
                {
                    idol.filename = files[i];
                    idol.name = bin.ReadString();
                    list.Add(idol);
                }
                bin.Close();
            }
            idols = list;
        }

        public CharacterIdol GetCharacter(int index)
        {
            return idols[index];
        }

        public int GetNumCharacters()
        {
            return idols.Count;
        }

        public CharacterIdol GetCharacter(String index)
        {
            for (int i = 0; i < idols.Count; i++)
                if (idols[i].name.Equals(index))
                    return idols[i];
            throw new IndexOutOfRangeException();
        }

        public static void CreateSingleton()
        {
            if (SINGLETON_CharacterMaster == null)
                SINGLETON_CharacterMaster = new CharacterMaster();
            else
                throw new InvalidOperationException("Singleton has already been initialized");
        }

        public static void DestroySingleton()
        {
            if (SINGLETON_CharacterMaster != null)
                SINGLETON_CharacterMaster = null;
            else
                throw new InvalidOperationException("Singleton has already been destroyed");
        }

        public static CharacterMaster GetSingleton()
        {
            return SINGLETON_CharacterMaster;
        }
    }
}