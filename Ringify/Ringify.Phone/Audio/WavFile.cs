using System;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;


namespace Ringify
{
    public class WavFile : SongFile
    {
        // Chunk ID Strings
        private static string DATA_CHUNK_ID = "data";
        private static string WAVE_CHUNK_ID = "WAVE";
        private static string RIFF_CHUNK_ID = "RIFF";
        private static string FMT_CHUNK_ID = "fmt ";

        private int m_FormatTag;
        private int m_Channels;
        private int m_SamplesPerSec;
        private int m_AvgBytesPerSec;
        private int m_BlockAlign;
        private int m_BitsPerSample;

        public WavFile()
        {
            m_Initialized = false;
            m_FormatTag = -1;
            m_Channels = -1;
            m_SamplesPerSec = -1;
            m_AvgBytesPerSec = -1;
            m_BlockAlign = -1;
            m_BitsPerSample = -1;
        }

        public override bool Initialize(String i_FileName)
        {
            try
            {
                m_FileName = i_FileName;
                m_Initialized = false;
                Debugger.Trace("Initializing the wav file");

                IsolatedStorageFile Store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!Store.FileExists(m_FileName))
                {
                    Debugger.Trace("The Wav file does not exist");
                    return false;
                }

                using (IsolatedStorageFileStream stream = Store.OpenFile(m_FileName, FileMode.Open))
                {
                    stream.Seek(0, 0);

                    BinaryReader Reader = new BinaryReader(stream);
                    if (Reader == null)
                    {
                        Debugger.Trace("There was an error opening the BinaryReader to initialize the WavFile");
                        return false;
                    }

                    String RIFF_ChunkID = new String(Reader.ReadChars(4));
                    if (RIFF_ChunkID != RIFF_CHUNK_ID)
                    {
                        Debugger.Trace("There was no RIFF Chunk ID ({1})", RIFF_ChunkID);
                        return false;
                    }
                    long RIFF_ChunkSize = Reader.ReadInt32();
                    long Expected_RIFF_ChunkSize = Reader.BaseStream.Length - Reader.BaseStream.Position;
                    //if (RIFF_ChunkSize != Expected_RIFF_ChunkSize)
                    //{
                    //    Debugger.Trace("There an invalid RIFF Chunk Size of {0}. Expected {1}", RIFF_ChunkSize, Expected_RIFF_ChunkSize);
                    //    return false;
                    //}

                    String WAVE_ChunkID = new String(Reader.ReadChars(4));
                    if (WAVE_ChunkID != WAVE_CHUNK_ID)
                    {
                        Debugger.Trace("There was no WAVE Chunk ID ({1})", WAVE_ChunkID);
                        return false;
                    }

                    // Find the FMT chunk
                    char[] ChunkID = Reader.ReadChars(4);
                    Int32 ChunkSize = Reader.ReadInt32();
                    while (new String(ChunkID) != FMT_CHUNK_ID)
                    {
                        // Move to next chunk
                        ChunkID = Reader.ReadChars(4);
                        ChunkSize = Reader.ReadInt32();
                    }

                    if (!int_HandleFMTChunk(Reader, ChunkSize))
                        return false;

                    // Find the DATA chunk
                    ChunkID = Reader.ReadChars(4);
                    ChunkSize = Reader.ReadInt32();
                    while (new String(ChunkID) != DATA_CHUNK_ID)
                    {
                        // Move to next chunk
                        Reader.BaseStream.Position += ChunkSize;
                        ChunkID = Reader.ReadChars(4);
                        ChunkSize = Reader.ReadInt32();

                    }

                    // Set the SongLength
                    m_SongLength = new TimeSpan(0,0,(int)(ChunkSize / (double)m_AvgBytesPerSec));
                    m_Initialized = true;
                    return true;
                }
            }
            catch(Exception ex)
            {
                Debugger.Trace(ex);
                return false;
            }
        }

        public override bool Trim(TimeSpan i_Start, TimeSpan i_End, String i_FileDestination)
        {
            try
            {
                Debugger.Trace("Trimming Wav File {0}", m_FileName);

                if (!m_Initialized)
                {
                    Debugger.Trace("WavFile.Trim() not initialized");
                    return false;
                }

                if (i_Start > i_End)
                {
                    Debugger.Trace("WavFile.Trim() The End time is before the Start time");
                    return false;
                }

                if (i_End > m_SongLength)
                {
                    Debugger.Trace("WavFile.Trim() The End time after the End of the song");
                    return false;
                }

                double bytesPerMillisecond = m_AvgBytesPerSec / 1000.0;

                int StartPos = (int)(i_Start.TotalMilliseconds * bytesPerMillisecond);
                StartPos = StartPos - StartPos % m_BlockAlign;

                int NumBytes = (int)((i_End.TotalMilliseconds - i_Start.TotalMilliseconds) * bytesPerMillisecond);
                NumBytes = NumBytes - NumBytes % m_BlockAlign;

                // Open the file
                IsolatedStorageFile Store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!Store.FileExists(m_FileName))
                {
                    Debugger.Trace("The Wav file does not exist");
                    return false;
                }

                BinaryWriter Writer = new BinaryWriter(new MemoryStream());
                if (Writer == null)
                {
                    Debugger.Trace("WavFile.Trim() Writer was null");
                    return false;
                }

                using (IsolatedStorageFileStream stream = Store.OpenFile(m_FileName, FileMode.Open))
                {
                    stream.Seek(0, 0);
                    if (stream == null)
                    {
                        Debugger.Trace("WavFile.Trim() m_Stream was null");
                        return false;
                    }
                    stream.Seek(0, 0);

                    BinaryReader Reader = new BinaryReader(stream);
                    if (Reader == null)
                    {
                        Debugger.Trace("WavFile.Trim() Reader was null");
                        return false;
                    }

                    char[] RIFF_ChunkID = Reader.ReadChars(4);
                    if (new String(RIFF_ChunkID) != RIFF_CHUNK_ID)
                    {
                        Debugger.Trace("There was no RIFF Chunk ID ({0})", RIFF_ChunkID);
                        return false;
                    }
                    Writer.Write(RIFF_ChunkID);
                    Int32 RIFF_ChunkSize = Reader.ReadInt32();
                    Int32 Expected_RIFF_ChunkSize = (Int32)(Reader.BaseStream.Length - Reader.BaseStream.Position);
                    //if (RIFF_ChunkSize != Expected_RIFF_ChunkSize)
                    //{
                    //   Debug.WriteLine("[{0}] There an invalid RIFF Chunk Size of {1}. Expected {2}", DateTime.Now.ToString(datePatt), RIFF_ChunkSize, Expected_RIFF_ChunkSize);
                    //   return false;
                    //}
                    Writer.Write(RIFF_ChunkSize);

                    char[] WAVE_ChunkID = Reader.ReadChars(4);
                    if (new String(WAVE_ChunkID) != WAVE_CHUNK_ID)
                    {
                        Debugger.Trace("There was no WAVE Chunk ID ({0})", WAVE_ChunkID);
                        return false;
                    }
                    Writer.Write(WAVE_ChunkID);

                    // Continue copying Chunks until we find the data chunk
                    char[] ChunkID = Reader.ReadChars(4);
                    Int32 ChunkSize = Reader.ReadInt32();
                    while (new String(ChunkID) != DATA_CHUNK_ID)
                    {
                        // Copy current chunk
                        Writer.Write(ChunkID);
                        Writer.Write(ChunkSize);
                        Writer.Write(Reader.ReadBytes(ChunkSize));

                        // Move to next chunk
                        ChunkID = Reader.ReadChars(4);
                        ChunkSize = Reader.ReadInt32();
                    }

                    // We are now at the Data
                    Writer.Write(ChunkID);
                    Writer.Write((Int32)NumBytes);

                    // Skip over the starting audio until the start position
                    Reader.BaseStream.Position += StartPos;

                    // Copy over the desired length of audio
                    int BytesCopied = 0;
                    byte[] Buffer = new byte[1024];
                    int Count = Buffer.Length;
                    if (Count > (NumBytes - BytesCopied))
                        Count = NumBytes - BytesCopied;

                    int Result = Reader.Read(Buffer, 0, Count);

                    while (BytesCopied < NumBytes)
                    {
                        if (Result == 0)
                        {
                            Debugger.Trace("WavFile.Trim() There was an error copying the data");
                            return false;
                        }

                        Writer.Write(Buffer, 0, Result);
                        BytesCopied += Result;

                        if (Count > (NumBytes - BytesCopied))
                            Count = NumBytes - BytesCopied;

                        Result = Reader.Read(Buffer, 0, Count);
                    }

                    // Update the RIFF size to match the new files size
                    Writer.Seek(4, 0);
                    Writer.Write((Int32)(Writer.BaseStream.Length - 4));

                    // Copy new file to old
                    m_SongLength = new TimeSpan(0,0, (int)(NumBytes / (double)m_AvgBytesPerSec));
                }


                using (IsolatedStorageFileStream stream = Store.OpenFile(m_FileName, FileMode.Truncate))
                {
                    // Go to the start of the new file and write it to the WAV file
                    Writer.Seek(0, 0);
                    Writer.BaseStream.CopyTo(stream);
                    return true;
                }

            }
            catch(Exception ex)
            {
                Debugger.Trace(ex);
                return false;
            }
        }

        private bool int_HandleFMTChunk(BinaryReader i_Reader, int i_Size)
        {
            // Make sure it is a valid length
            if (i_Size < 16)
                return false;

            // 2	Format code
            // 2	Number of interleaved channels
            // 4	Sampling rate (blocks per second)
            // 4	Data rate
            // 2	Data block size (bytes)
            // 2	Bits per sample
            m_FormatTag      = i_Reader.ReadInt16();
            m_Channels       = i_Reader.ReadInt16();
            m_SamplesPerSec  = i_Reader.ReadInt32();
            m_AvgBytesPerSec = i_Reader.ReadInt32();
            m_BlockAlign     = i_Reader.ReadInt16();
            m_BitsPerSample  = i_Reader.ReadInt16();

            if (i_Size > 16)
            {
                // Ignore the extra format data
                byte[] SubFormat = i_Reader.ReadBytes(i_Size - 16);
            }

            Debugger.Trace("Done parsing the FMT Chunk");
            return true;
        }
    }
}
