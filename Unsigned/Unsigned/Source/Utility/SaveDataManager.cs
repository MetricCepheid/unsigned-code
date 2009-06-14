using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Unsigned
{
    public class SaveDataManager
    {
        public static String SaveDataFilename = "Content\\SaveData\\SaveData.sav";

        private static SaveDataManager _singleton;
        public static SaveDataManager Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new SaveDataManager();
                return _singleton;
            }
        }

        private class InstrOwner
        {
            public String InstrCode;
            public String Owner;

            public InstrOwner(String iC, String o)
            {
                InstrCode = iC;
                Owner = o;
            }
        }

        private class SaveSlot
        {
            public List<InstrOwner> Instrs;
            public int Score;
            public int NumStars;

            public SaveSlot()
            {
                Score = 0;
                NumStars = 0;
                Instrs = new List<InstrOwner>();
            }
        }

        private class InstrSave
        {
            public String InstrCode;
            public String BestStreakOwner;
            public int BestStreak;

            public InstrSave()
            {
                InstrCode = "AAA";
                BestStreak = 0;
                BestStreakOwner = "";
            }
        }

        private class SongSlot
        {
            public String SongCodeName;
            public List<SaveSlot> Saves;
            public List<InstrSave> InstrBests;

            public SongSlot(String songCodeName)
            {
                SongCodeName = songCodeName;
                Saves = new List<SaveSlot>();
                InstrBests = new List<InstrSave>();
            }
        }

        private List<SongSlot> SongSlots;

        private Dictionary<String, float> SongOffsets;

        private SaveDataManager()
        {
        }

        public void Load()
        {
            BinaryReader br = new BinaryReader(File.OpenRead(SaveDataFilename));

            SongSlots = new List<SongSlot>();
            SongOffsets = new Dictionary<String, float>();

            int numSlots = br.ReadInt32();

            for (int i = 0; i < numSlots; i++)
            {
                SongSlot songSlot = new SongSlot(br.ReadString());
                int numInstrBests = br.ReadInt32();
                for (int k = 0; k < numInstrBests; k++)
                {
                    InstrSave instrSave = new InstrSave();
                    instrSave.InstrCode = new String(br.ReadChars(3));
                    instrSave.BestStreakOwner = br.ReadString();
                    instrSave.BestStreak = br.ReadInt32();
                    songSlot.InstrBests.Add(instrSave);
                }
                int numSaveSlots = br.ReadInt32();
                for (int k = 0; k < numSaveSlots; k++)
                {
                    SaveSlot saveSlot = new SaveSlot();
                    int numSaveInstrs = br.ReadInt32();
                    for (int j = 0; j < numSaveInstrs; j++)
                    {
                        InstrOwner instrOwner = new InstrOwner(new String(br.ReadChars(3)), br.ReadString());
                        saveSlot.Instrs.Add(instrOwner);
                    }
                    saveSlot.NumStars = br.ReadInt32();
                    saveSlot.Score = br.ReadInt32();
                    songSlot.Saves.Add(saveSlot);
                }
                SongSlots.Add(songSlot);
            }

            int numSongOffsets = br.ReadInt32();
            for (int i = 0; i < numSongOffsets; i++)
            {
                String key = br.ReadString();
                float offset = br.ReadSingle();
                SongOffsets.Add(key, offset);
            }

            br.Close();
        }

        public void Save()
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(SaveDataFilename));

            bw.Write(SongSlots.Count);
            for (int i = 0; i < SongSlots.Count; i++)
            {
                bw.Write(SongSlots[i].SongCodeName);
                bw.Write(SongSlots[i].InstrBests.Count);
                for (int k = 0; k < SongSlots[i].InstrBests.Count; k++)
                {
                    InstrSave instrSave = SongSlots[i].InstrBests[k];
                    bw.Write(instrSave.InstrCode[0]);
                    bw.Write(instrSave.InstrCode[1]);
                    bw.Write(instrSave.InstrCode[2]);
                    bw.Write(instrSave.BestStreakOwner);
                    bw.Write(instrSave.BestStreak);
                }
                bw.Write(SongSlots[i].Saves.Count);
                for (int k = 0; k < SongSlots[i].Saves.Count; k++)
                {
                    SaveSlot saveSlot = SongSlots[i].Saves[k];
                    bw.Write(saveSlot.Instrs.Count);
                    for (int j = 0; j < saveSlot.Instrs.Count; j++)
                    {
                        bw.Write(saveSlot.Instrs[j].InstrCode[0]);
                        bw.Write(saveSlot.Instrs[j].InstrCode[1]);
                        bw.Write(saveSlot.Instrs[j].InstrCode[2]);
                        bw.Write(saveSlot.Instrs[j].Owner);
                    }
                    bw.Write(saveSlot.NumStars);
                    bw.Write(saveSlot.Score);
                }
            }

            bw.Write(SongOffsets.Keys.Count);
            foreach (String key in SongOffsets.Keys)
            {
                bw.Write(key);
                bw.Write(SongOffsets[key]);
            }

            bw.Close();
        }

        public void AddEntry(String songCodeName, String[] InstrCodes, String[] InstrOwners, int score, int numStars, int[] streaks)
        {
            bool foundSong = false;
            for (int i = 0; i < SongSlots.Count; i++)
            {
                if (SongSlots[i].SongCodeName == songCodeName)
                {
                    foundSong = true;
                    {
                        SaveSlot ss = new SaveSlot();
                        ss.Score = score;
                        ss.NumStars = numStars;
                        for (int k = 0; k < InstrCodes.Length; k++)
                            ss.Instrs.Add(new InstrOwner(InstrCodes[k], InstrOwners[k]));
                        SongSlots[i].Saves.Add(ss);
                    }
                    for(int k=0;k<InstrCodes.Length;k++)
                    {
                        bool foundInstrCode = false;
                        for(int j=0;j<SongSlots[i].InstrBests.Count;j++)
                            if (SongSlots[i].InstrBests[j].InstrCode == InstrCodes[k])
                            {
                                foundInstrCode = true;
                                if (SongSlots[i].InstrBests[j].BestStreak < streaks[k])
                                {
                                    SongSlots[i].InstrBests[j].BestStreak = streaks[k];
                                    SongSlots[i].InstrBests[j].BestStreakOwner = InstrOwners[k];
                                }
                            }
                        if (!foundInstrCode)
                        {
                            InstrSave s = new InstrSave();
                            s.BestStreakOwner = InstrOwners[k];
                            s.BestStreak = streaks[k];
                            s.InstrCode = InstrCodes[k];
                            SongSlots[i].InstrBests.Add(s);
                        }
                    }
                    break;
                }
            }
            if(!foundSong)
            {
                SongSlots.Add(new SongSlot(songCodeName));
                int i = SongSlots.Count - 1;
                {
                    SaveSlot ss = new SaveSlot();
                    ss.Score = score;
                    ss.NumStars = numStars;
                    for (int k = 0; k < InstrCodes.Length; k++)
                        ss.Instrs.Add(new InstrOwner(InstrCodes[k], InstrOwners[k]));
                    SongSlots[i].Saves.Add(ss);
                }
                for (int k = 0; k < InstrCodes.Length; k++)
                {
                    bool foundInstrCode = false;
                    for (int j = 0; j < SongSlots[i].InstrBests.Count; j++)
                        if (SongSlots[i].InstrBests[j].InstrCode == InstrCodes[k])
                        {
                            foundInstrCode = true;
                            if (SongSlots[i].InstrBests[j].BestStreak < streaks[k])
                            {
                                SongSlots[i].InstrBests[j].BestStreak = streaks[k];
                                SongSlots[i].InstrBests[j].BestStreakOwner = InstrOwners[k];
                            }
                        }
                    if (!foundInstrCode)
                    {
                        InstrSave s = new InstrSave();
                        s.BestStreakOwner = InstrOwners[k];
                        s.BestStreak = streaks[k];
                        s.InstrCode = InstrCodes[k];
                        SongSlots[i].InstrBests.Add(s);
                    }
                }
            }
        }

        public void IncrementSongOffset(String songCodeName, float incr)
        {
            if (!SongOffsets.ContainsKey(songCodeName))
            {
                SongOffsets.Add(songCodeName, incr);
            }
            else
            {
                SongOffsets[songCodeName] = SongOffsets[songCodeName] + incr;
            }
        }

        public float GetSongOffset(String songCodeName)
        {
            if (SongOffsets.ContainsKey(songCodeName))
                return SongOffsets[songCodeName];
            return 0;
        }
    }
}
