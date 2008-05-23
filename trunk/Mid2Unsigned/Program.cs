using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace Mid2Unsigned
{
    public struct Pair
    {
        public int time;
        public int value;

        public Pair(int t, int v)
        {
            time = t;
            value = v;
        }
    }

    public struct Note : IComparable
    {
        public byte value;
        public uint time, len;
        public Note(byte v, uint t, uint l)
        {
            value = v;
            time = t;
            len = l;
        }
        public int CompareTo(object o)
        {
            return time.CompareTo(((Note)o).time);
        }
    }

    public struct SPPH
    {
        public byte value;
        public uint time, len;
        public int start1, start2;
        public int end1, end2;
        public SPPH(byte v, uint t, uint l)
        {
            value = v;
            time = t;
            len = l;
            start1 = 0;
            start2 = 0;
            end1 = 0;
            end2 = 0;
        }
    }

    public struct Event
    {
        public string value;
        public uint time;
        public Event(string v, uint t)
        {
            value = v;
            time = t;
        }
    }

    public class BarLine
    {
        public uint time;
        public int beats;
        public uint sBeat;
        public BarLine(uint t, int b, uint s)
        {
            time = t;
            beats = b;
            sBeat = s;
        }
    }

    public class VocalWord
    {
        public uint time, length;
        public short note;
        public string value;
    }

    public class VocalPhrase
    {
        public enum TYPE { REGULAR = 0, BLANK = 1, RHYTHM = 2, TALKIE = 3 };
        public enum RTYPE { TAMBOURINE = 0, COWBELL = 1, CLAP = 2 };
        public TYPE type;
        public RTYPE rType;
        public bool SP;
        public List<VocalWord> words;
        public uint time;
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Error("Must pass a file to convert!");
                return;
            }

            string thisPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            thisPath = thisPath.Substring(0, thisPath.LastIndexOf('\\') + 1);

            for (int i = 0; i < args.Length; i++)
                Console.WriteLine(args[i]);

            byte VERSION = 16;

            String songInternalName;//aka midi track name

            Console.WriteLine("Initial setup complete");

            for (int files = 0; files < args.Length; files++)
            {
                System.IO.BinaryReader bin;


                try
                {
                    bin = new System.IO.BinaryReader(System.IO.File.OpenRead(args[files]));
                    
                }
                catch (Exception e)
                {
                    Error("Error while trying to open file\n" +
                        args[files] + "\n" +
                        e.Message);
                    return;
                }

                Console.WriteLine("File Opened. Beginning Read");

                ushort MIDI_TYPE = ushort.MaxValue;
                ushort MIDI_numTracks = 0;

                ushort MIDI_timeDivision = 0;
                int[] info = new int[15];

                Console.WriteLine("Variables initialised");

                ulong cBO = 0;//Current Byte Offset, how far into the file we are

                try
                {

                    //read header
                    char[] mthd = bin.ReadChars(4); cBO += 4;
                    for (int i = 0; i < 4; i++)
                        if (mthd[i] != "MThd".ToCharArray()[i])
                            throw new Exception("Error... file not midi file.  MThd header missing");
                    uint header_size = Endian.Invert(bin.ReadUInt32()); cBO += 4;
                    if (header_size != 6)
                        throw new Exception("Unsupported MIDI format: Header is wrong size; expected size = 6");

                    MIDI_TYPE = Endian.Invert(bin.ReadUInt16()); cBO += 2;

                    if (MIDI_TYPE < 0 || MIDI_TYPE > 2)
                        throw new Exception("Unsupported MIDI format: Format not recognised; expected value = 0-2");

                    MIDI_numTracks = Endian.Invert(bin.ReadUInt16()); cBO += 2;
                    if (MIDI_TYPE == 0 && MIDI_numTracks > 1)
                        throw new Exception("MIDI type 0 should not have more than one track");

                    MIDI_timeDivision = Endian.Invert(bin.ReadUInt16()); cBO += 2;

                    String trackName = "";

                    float time = 0;
                    //read the tracks
                    for (int track = 0; track < MIDI_numTracks; track++)
                    {
                        Console.Write("Reading Track " + track + " ");
                        //read track header
                        char[] mtrk = bin.ReadChars(4); cBO += 4;
                        for (int r = 0; r < 4; r++)
                            if (mtrk[r] != "MTrk".ToCharArray()[r])
                                throw new Exception("Error... file corrupted.  MTrk header missing");

                        uint TRACK_size = Endian.Invert(bin.ReadUInt32()); cBO += 4;
                        int i;
                        float percent = 0;
                        for (i = 0; i < TRACK_size; )
                        {
                            if ((float)i / (float)TRACK_size > percent + 0.1)
                            {
                                Console.Write(".");
                                percent += 0.1f;
                            }
                            ReadVLengthInt(bin);
                            int dtime = VLengthValue;
                            i += VLengthSize;

                            time += dtime;

                            byte evchan = bin.ReadByte(); i++;
                            if (evchan == 0xFF)//meta
                            {
                                //Console.Write("M");
                                byte meta_type = bin.ReadByte(); i++;
                                if (meta_type == 0x01)//text event
                                {
                                    ReadVLengthInt(bin); i += VLengthSize;
                                    bin.ReadChars(VLengthValue);
                                    i += VLengthValue;
                                    info[0]++;
                                }
                                else if (meta_type == 0x03)//track name
                                {
                                    ReadVLengthInt(bin); i += VLengthSize;
                                    trackName = new string(bin.ReadChars(VLengthValue));
                                    i += VLengthValue;
                                    info[1]++;
                                }
                                else if (meta_type == 0x05)//lyrics
                                {
                                    ReadVLengthInt(bin); i += VLengthSize;
                                    bin.ReadChars(VLengthValue);
                                    i += VLengthValue;
                                    info[2]++;
                                }
                                else if (meta_type == 0x51)//tempo
                                {
                                    ReadVLengthInt(bin); i += VLengthSize;
                                    if (VLengthValue != 3)
                                        Error("Tempo should be 3 bytes long");
                                    bin.ReadBytes(3); i += 3;
                                    info[3]++;
                                }
                                else if (meta_type == 0x58)//time signature
                                {
                                    ReadVLengthInt(bin); i += VLengthSize;
                                    if (VLengthValue != 4)
                                        Error("Time sig should be 4 bytes long");
                                    info[4]++;
                                }
                                else if (meta_type == 0x2F)//end of track
                                {
                                    ReadVLengthInt(bin); i += VLengthSize;
                                    if (VLengthValue != 0)
                                        Error("EoT should be 0 bytes long");
                                    info[5]++;
                                }
                                else
                                    Console.Write("?");
                            }
                            else if (evchan == 0xF0)//sysex
                            {
                                Console.Write("S");
                            }
                            else //midi
                            {

                                //Console.Write("E");
                                byte ev = (byte)(evchan >> 4);
                                byte ch = (byte)(evchan & 0x0F);
                                ev = (byte)(ev << 1);

                                switch (ev)
                                {
                                    case 0x08://note off
                                        bin.ReadInt16(); i += 2;
                                        info[6]++;
                                        break;
                                    case 0x09://note on
                                        bin.ReadInt16(); i += 2;
                                        info[7]++;
                                        break;
                                    case 0x0A://note aftertouch
                                        bin.ReadInt16(); i += 2;
                                        info[8]++;
                                        break;
                                    case 0x0B://controller value
                                        bin.ReadInt16(); i += 2;
                                        info[9]++;
                                        break;
                                    case 0x0C://program change
                                        bin.ReadByte(); i += 1;
                                        info[10]++;
                                        break;
                                    case 0x0D://channel aftertouch
                                        bin.ReadByte(); i += 1;
                                        info[11]++;
                                        break;
                                    case 0x0E://pitch bend
                                        bin.ReadInt16(); i += 2;
                                        info[12]++;
                                        break;
                                    default:
                                        info[13]++;
                                        break;
                                }
                            }
                        }
                        cBO += (ulong)i;
                        Console.WriteLine("Done!");
                        Console.WriteLine("\nTrack Name: " + trackName + "\n");
                        Console.WriteLine("Text Events: "+info[0]);
                        Console.WriteLine("Track Names: "+info[1]);
                        Console.WriteLine("Lyrics     : "+info[2]);
                        Console.WriteLine("Tempo      : "+info[3]);
                        Console.WriteLine("Time Signat: "+info[4]);
                        Console.WriteLine("End of Trck: "+info[5]);
                        Console.WriteLine("Note Off   : "+info[6]);
                        Console.WriteLine("Note On    : "+info[7]);
                        Console.WriteLine("Note Afrtch: "+info[8]);
                        Console.WriteLine("Controllers: "+info[9]);
                        Console.WriteLine("Prgm Change: "+info[10]);
                        Console.WriteLine("Chnl Afrtch: "+info[11]);
                        Console.WriteLine("Pitch Bend : "+info[12]);
                        Console.WriteLine("Unknown    : " + info[13]);
                        Console.WriteLine("\n");
                        for (int y = 0; y < 15; y++)
                            info[y] = 0;
                    }
                    Console.WriteLine("\n\n\nComplete!\nPress any key to continue");

                    Console.ReadKey();
                }
                catch (System.Threading.ThreadAbortException e)
                {
                    return;
                }
#if !DEBUG
                catch (Exception e)
                {
                    Error("Error while trying to read file\n" +
                        args[files] + "\n" +
                        e.Message);
                    return;
                }
#endif

            }
        }

        private static void Error(String a)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n\n"+a);
            Console.Beep(100, 200);
            Console.Beep(200, 400);
            Console.Beep(150, 400);
            Console.Beep(200, 150);
            Console.Beep(100, 150);
            Console.Beep(200, 150);
            Console.Beep(100, 150);
            Console.WriteLine("Closing...");
            Console.WriteLine("Press any key to Continue");
            Console.ReadKey();
            System.Threading.Thread.CurrentThread.Abort();
            return;
        }

        private static int VLengthValue, VLengthSize;
        private static void ReadVLengthInt(System.IO.BinaryReader bin)
        {
            VLengthValue = 0;
            VLengthSize = 0;
            for (int i = 0; i < 4; i++)
            {
                byte read = bin.ReadByte(); VLengthSize++;
                int readi = (int)read;
                readi = readi & 0x0000007F;
                VLengthValue = VLengthValue << (i * 7);
                VLengthValue |= readi;
                if ((read & 0x80) == 0)
                    break;
            }
        }
    }
}
