using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SongDataIO;

namespace Unsigned
{
    /// <summary>
    /// Class to read song file headers
    /// </summary>
    public class SongFileHeader
    {
        private static string BROKEN = "<BROKEN>";
        protected string filePath = "";
        protected byte version;
        protected string songName = "";
        protected string artistName = "";
        protected uint year;
        protected string genre = "";
        protected TimeSpan length;
        protected bool isBrokenSongFile = false;
        protected String[] instruments;

        public bool IsBroken
        {
            get { return isBrokenSongFile; }
            set { isBrokenSongFile = value; }
        }
        public String SongName
        {
            get { return songName; }
        }
        public String ArtistName
        {
            get { return artistName; }
        }
        public String Genre
        {
            get { return genre; }
        }
        public uint Year
        {
            get { return year; }
        }
        public TimeSpan Length
        {
            get { return length; }
        }
        public String Filename
        {
            get { return filePath; }
        }

        /// <summary>
        /// Creates an empty SongFileHeader
        /// </summary>
        private SongFileHeader()
        {

        }

        /// <summary>
        /// Creates a version 12 GBA FileHeader
        /// </summary>
        /// <param name="filePath">the path to the .gba file</param>
        /// <returns>A new GBA file header, or a broken one</returns>
        public static SongFileHeader CreateGBA(string filePath)
        {
            SongFileHeader header = new SongFileHeader();
            header.OpenGBA(filePath);
            return header;
        }

        /// <summary>
        /// Loads a version 12 GBA
        /// </summary>
        /// <param name="filePath">The path to the .gba file</param>
        private void OpenGBA(string filePath)
        {
            BinaryReader binReader = null;
            try
            {
                this.filePath = filePath;

                binReader = new BinaryReader(File.OpenRead(filePath));

                this.version = binReader.ReadByte();
                songName = binReader.ReadString();
                artistName = binReader.ReadString();
                year = binReader.ReadUInt32();
                genre = binReader.ReadString();
                length = SongLoader.LengthStringToTimeSpan(binReader.ReadString());
                instruments = new string[4];
                instruments[0] = "LGT";
                instruments[1] = "LVX";
                instruments[2] = "SET";
                instruments[3] = "BAS";
            }
            finally
            {
                if (binReader != null)
                {
                    binReader.Close();
                }
            }
        }

        /// <summary>
        /// Creates a version 17+ UNS FileHeader
        /// </summary>
        /// <param name="filePath">the path to the .uns file</param>
        /// <returns>A new UNS file header, or a broken one</returns>
        public static SongFileHeader CreateUNS(string filePath)
        {
            SongFileHeader header = new SongFileHeader();
            header.OpenUNS(filePath);
            return header;
        }

        /// <summary>
        /// Loads a version 17+ UNS
        /// </summary>
        /// <param name="filePath">The path to the .uns file</param>
        private void OpenUNS(string filePath)
        {
            BinaryReader binReader = null;
            try
            {
                this.filePath = filePath;

                binReader = new BinaryReader(File.OpenRead(filePath));

                binReader.ReadBytes(3);// "UNS"

                binReader.ReadBytes(6 * 4);//redundant for header

                this.version = binReader.ReadByte();

                if (this.version == 17)
                {
                    songName = binReader.ReadString();
                    artistName = binReader.ReadString();
                    year = binReader.ReadUInt32();
                    genre = binReader.ReadString();
                    length = SongLoader.LengthStringToTimeSpan(binReader.ReadString());
                    instruments = new string[4];
                    instruments[0] = "LGT";
                    instruments[1] = "LVX";
                    instruments[2] = "SET";
                    instruments[3] = "BAS";
                }
                else if (this.version == 18)
                {
                    instruments = new string[binReader.ReadUInt32()];
                    int[] offsets = new int[instruments.Length];
                    for (int i = 0; i < offsets.Length; i++)
                        offsets[i] = binReader.ReadInt32();
                    binReader.ReadUInt32();
                    songName = binReader.ReadString();
                    artistName = binReader.ReadString();
                    year = binReader.ReadUInt32();
                    genre = binReader.ReadString();
                    length = SongLoader.LengthStringToTimeSpan(binReader.ReadString());
                    for (int i = 0; i < instruments.Length; i++)
                    {
                        int cPos = (int)binReader.BaseStream.Position;
                        binReader.ReadBytes(offsets[i] - cPos);
                        instruments[i] = new String(binReader.ReadChars(3));
                    }
                }
            }
            finally
            {
                if (binReader != null)
                {
                    binReader.Close();
                }
            }
        }

        /// <summary>
        /// Creates a broken file header
        /// </summary>
        public static SongFileHeader BrokenFileHeader(string name)
        {
            SongFileHeader header = new SongFileHeader();
            header.filePath = BROKEN;
            header.isBrokenSongFile = true;
            header.songName = "BROKEN - " + name;
            header.artistName = "The Broken Band";
            return header;
        }

        /// <summary>
        /// Opens a gba song file header
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the GBA song file</param>
        /// <returns>A GBA song file, or a DummySongFile if the real file fails to open</returns>
        public static SongFileHeader OpenSongFileHeader(string filePath)
        {
            try
            {
                if (filePath.ToLower().EndsWith(".gba"))
                    return CreateGBA(filePath);
                else if (filePath.ToLower().EndsWith(".uns"))
                    return CreateUNS(filePath);
                else
                    return BrokenFileHeader("");
            }
            catch
            {
                return BrokenFileHeader("");
            }
        }

        /// <summary>
        /// Checks the file version in the GBA header to make sure it is a valid version
        /// </summary>
        public bool IsValidVersion()
        {
            if (isBrokenSongFile)
                return false;
            if (version >= 12)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the simple file name of this song file, excluding the path and extension.
        /// </summary>
        /// <returns>For example, if the song path is "songdata/mysong.uns", this method returns "mysong"</returns>
        public virtual string GetSimpleFileName()
        {
            int slashIndex = filePath.LastIndexOf("\\");
            int dotIndex = filePath.LastIndexOf(".");
            return filePath.Substring(slashIndex + 1, dotIndex - slashIndex - 1);
        }
    }

    /// <summary>
    /// Represents a sub-set within a setlist.  For example, each decade in the "by decade" setlist represents a subset,
    /// and each artist in a "by artist" setlist represents a subset.
    /// 
    /// </summary>
    public class SubSet
    {
        /// <summary>
        /// The name of this subset
        /// </summary>
        public String name;

        /// <summary>
        /// The internal collection of songs in this subset
        /// A dictionary is used to ensure that songs are unique in this subset
        /// </summary>
        private SortedDictionary<String, SongFileHeader> songs;

        /// <summary>
        /// The externally exposed collection of songs in this subset
        /// </summary>
        /// <returns>A <code>List</code> of song headers</returns>
        public List<SongFileHeader> GetSongList()
        {
            //TODO: could we maybe change this to expose the dictionary someday?
            List<SongFileHeader> songsList = new List<SongFileHeader>(songs.Values);
            return songsList;
        }

        /// <summary>
        /// Adds a song to this subset, ignoring entries with duplicate names
        /// </summary>
        /// <param name="songFile"></param>
        public void AddSong(SongFileHeader songFile)
        {
            if (songs.ContainsKey(songFile.GetSimpleFileName()) == false)
            {
                songs.Add(songFile.GetSimpleFileName(), songFile);
            }
        }

        /// <summary>
        /// Constructor for a new subset
        /// Initializes a new empty song header list
        /// </summary>
        /// <param name="name">The name for this subset</param>
        public SubSet(String name)
        {
            this.name = name;
            songs = new SortedDictionary<String, SongFileHeader>();
        }
    }

    /// <summary>
    /// A setlist used to display a list of songs available on the song selection screen
    /// Setlists are composed of <code>SubSets</code> which in turn contain songs.
    /// SubSets are used to group songs into categories like artist, genre, or decade
    /// </summary>
    public class SetList
    {
        /// <summary>
        /// Holds the unsorted list of songs
        /// </summary>
        private Dictionary<string, SongFileHeader> allSongs = new Dictionary<string, SongFileHeader>();

        /// <summary>
        /// Value in SetList files that specifies a new set
        /// </summary>
        private const string SET_DELIMITER = "set";

        /// <summary>
        /// The internal collection of subsets in this setlist
        /// A dictionary is used to ensure that subsets are unique in this setlist
        /// </summary>
        private IDictionary<String, SubSet> subSets = new SortedDictionary<string, SubSet>();

        /// <summary>
        /// A filepath to a custom setlist that can be used to order this setlist
        /// </summary>
        private string customSetListFilePath = "";

        /// <summary>
        /// The name of this setlist
        /// </summary>
        public String name;

        /// <summary>
        /// The current sort order for this set list
        /// </summary>
        private SortOrders currentSortOrder = SortOrders.Artist;

        /// <summary>
        /// The externally available collection of subsets in this setlist
        /// </summary>
        /// <returns></returns>
        public List<SubSet> GetSubsets()
        {
            List<SubSet> subSetsList = new List<SubSet>(subSets.Values);
            return subSetsList;
        }

        /// <summary>
        /// Adds a song file to the setList.  The setList must be re-ordered for the new song to show up.
        /// </summary>
        public void AddSong(SongFileHeader songFileHeader)
        {
            allSongs.Add(songFileHeader.GetSimpleFileName(), songFileHeader);
        }

        /// <summary>
        /// Sets up a custom setlist file, which allows ordering by a custom setlist
        /// </summary>
        /// <param name="filePath">The path to a custom setlist file</param>
        public void SetCustomSetList(string filePath)
        {
            customSetListFilePath = filePath;
        }

        /// <summary>
        /// Prints the contents of this setlist to the console for debugging
        /// </summary>
        public void PrintSetList()
        {
            foreach (SubSet subSet in subSets.Values)
            {
                Console.WriteLine(subSet.name);
                foreach (SongFileHeader song in subSet.GetSongList())
                {
                    Console.WriteLine("\t" + song.SongName);
                }
            }
        }

        /// <summary>
        /// The various ways a setlist can be sorted
        /// </summary>
        private enum SortOrders
        {
            CustomSetList,
            Artist,
            SongName,
            Decade,
            Genre
        }

        /// <summary>
        /// Re-orders this setlist into the next available sorting configuration
        /// </summary>
        public void NextSortOrder()
        {
            Console.WriteLine("Old Sort Order: {0}", currentSortOrder.ToString());
            switch (currentSortOrder)
            {
                case SortOrders.CustomSetList:
                    OrderByArtist();
                    break;
                case SortOrders.Artist:
                    OrderBySongName();
                    break;
                case SortOrders.SongName:
                    OrderByDecade();
                    break;
                case SortOrders.Decade:
                    OrderByGenre();
                    break;
                case SortOrders.Genre:
                    OrderByCustomSetList();
                    break;
            }
            Console.WriteLine("New Sort Order: {0}", currentSortOrder.ToString());

        }

        /// <summary>
        /// Re-initializes this setList object from a custom setList file
        /// </summary>        
        public void OrderByCustomSetList()
        {
            //If there is no custom set list, order by artist instead            
            if (customSetListFilePath == "")
            {
                OrderByArtist();
                return;
            }
            currentSortOrder = SortOrders.CustomSetList;

            StreamReader reader = new StreamReader(customSetListFilePath);

            subSets = new SortedDictionary<String, SubSet>();

            String line;
            SubSet currentSubSet = null;

            while (reader.EndOfStream == false)
            {
                line = reader.ReadLine();
                //Console.WriteLine("Current line: {0}", line);
                //If this row is a set() row, add a new subset to the subset list
                if (line.Length >= 3 && line.Substring(0, 3).ToLower().Equals(SET_DELIMITER))
                {
                    //Parse the set name from between the parens
                    String name = line.Substring(line.IndexOf("(") + 1);
                    name = name.Substring(0, name.LastIndexOf(')'));

                    //Create a new SongSet with the given name
                    currentSubSet = new SubSet(name);
                    subSets.Add(currentSubSet.name, currentSubSet);
                }
                else
                {
                    if (allSongs.ContainsKey(line) == true)
                    {
                        SongFileHeader song = allSongs[line];
                        currentSubSet.AddSong(song);
                    }
                    else
                    {
                        SongFileHeader dummySongFile = SongFileHeader.BrokenFileHeader(line);
                        currentSubSet.AddSong(dummySongFile);
                    }
                }
            }
            reader.Close();
        }

        /// <summary>
        /// Loads all of the headers from the song data folder
        /// </summary>
        /// <returns></returns>
        public void LoadSongFileHeaders()
        {
            allSongs = new Dictionary<string, SongFileHeader>();

            //Grab the .uns files, and add them first
            String unsSearchString = "*.uns";
            String[] unsFilePaths = Directory.GetFiles(Directory.GetCurrentDirectory()+"\\songdata", unsSearchString);
            foreach (string unsFilePath in unsFilePaths)
            {
                SongFileHeader unsSongFile = SongFileHeader.CreateUNS(unsFilePath);
                allSongs.Add(unsSongFile.GetSimpleFileName(), unsSongFile);
            }

            //Then grab the .gba files
            String gbaSearchString = "*.gba";
            String[] gbaFilePaths = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\songdata", gbaSearchString);
            foreach (string gbaFilePath in gbaFilePaths)
            {
                SongFileHeader gbaFile = SongFileHeader.CreateGBA(gbaFilePath);
                if (allSongs.ContainsKey(gbaFile.GetSimpleFileName()) == false)
                {
                    allSongs.Add(gbaFile.GetSimpleFileName(), gbaFile);
                }
            }
            OrderBySongName();
        }

        /// <summary>
        /// Loads all songs from the songdata folder, and groups them by decade
        /// </summary>
        public void OrderByDecade()
        {
            currentSortOrder = SortOrders.Decade;
            subSets = new SortedDictionary<String, SubSet>();

            SubSet unknownYearSubSet = new SubSet("<Unknown Year>");

            //Build a subset for each decade from 1900s to 2000s
            SubSet[] sets = new SubSet[20];
            for (int i = 0; i < 20; i++)
            {
                sets[i] = new SubSet((i < 10 ? "19" : "20") + (i % 10) + "0s");
            }

            foreach (SongFileHeader header in allSongs.Values)
            {
                if (header.Year > 0)
                {
                    uint decadeSetListIndex = (header.Year - 1900) / 10;
                    sets[decadeSetListIndex].AddSong(header);
                }
                else
                {
                    unknownYearSubSet.AddSong(header);
                }
            }

            if (unknownYearSubSet.GetSongList().Count > 0)
            {
                subSets.Add(unknownYearSubSet.name, unknownYearSubSet);
            }

            //Add only the non-empty song sets to our setlist
            for (int i = 0; i < 20; i++)
            {
                if (sets[i].GetSongList().Count > 0)
                {
                    subSets.Add(sets[i].name, sets[i]);
                }
            }
        }

        /// <summary>
        /// Loads all songs from the songdata folder by genre
        /// </summary>
        public void OrderByGenre()
        {
            currentSortOrder = SortOrders.Genre;

            const string UNKNOWN_GENRE = "<Unknown Genre>";
            SubSet noGenreSubSet = new SubSet(UNKNOWN_GENRE);

            subSets = new SortedDictionary<String, SubSet>();

            foreach (SongFileHeader header in allSongs.Values)
            {
                if (header.Genre != "")
                {
                    if (subSets.ContainsKey(header.Genre) == true)
                    {
                        subSets[header.Genre].AddSong(header);
                    }
                    else
                    {
                        SubSet subSet = new SubSet(header.Genre);
                        subSets.Add(subSet.name, subSet);
                        subSet.AddSong(header);
                    }
                }
                else
                {
                    noGenreSubSet.AddSong(header);
                }
            }

            if (noGenreSubSet.GetSongList().Count > 0)
            {
                subSets.Add(noGenreSubSet.name, noGenreSubSet);
            }
        }

        /// <summary>
        /// Orders this setlist alphabetically by song name
        /// </summary>
        public void OrderBySongName()
        {
            currentSortOrder = SortOrders.SongName;
            subSets = new SortedDictionary<String, SubSet>();

            foreach (SongFileHeader song in allSongs.Values)
            {
                string firstCharacter = song.SongName.Substring(0, 1);

                if (subSets.ContainsKey(firstCharacter) == false)
                {
                    SubSet newSubSet = new SubSet(firstCharacter);
                    subSets.Add(newSubSet.name, newSubSet);
                }
                subSets[firstCharacter].AddSong(song);
            }
        }

        /// <summary>
        /// Orders this set list alphatbetically by artist
        /// </summary>
        public void OrderByArtist()
        {
            currentSortOrder = SortOrders.Artist;
            const string ARTIST_NOT_DEFINED = "<Unknown Artist>";
            SubSet noArtistSubSet = new SubSet(ARTIST_NOT_DEFINED);

            subSets = new SortedDictionary<String, SubSet>();

            foreach (SongFileHeader header in allSongs.Values)
            {
                if (header.ArtistName != "")
                {
                    if (subSets.ContainsKey(header.ArtistName) == true)
                    {
                        subSets[header.ArtistName].AddSong(header);
                    }
                    else
                    {
                        SubSet subSet = new SubSet(header.ArtistName);
                        subSets.Add(subSet.name, subSet);
                        subSet.AddSong(header);
                    }
                }
                else
                {
                    noArtistSubSet.AddSong(header);
                }
            }

            if (noArtistSubSet.GetSongList().Count > 0)
            {
                subSets.Add(noArtistSubSet.name, noArtistSubSet);
            }
        }
    }
}
