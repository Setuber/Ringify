using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.IO.IsolatedStorage;

namespace Ringify
{
    public class MP3File : SongFile
    {
        
        public ID3 m_MetaData;
        private MP3Header m_FirstHeader;
        private int m_FrameCount;

        public MP3File()
        {
            m_Initialized = false;
            m_FirstHeader = new MP3Header();
            m_FileName = "";
            m_SongLength = new TimeSpan(0);
        }

        public override bool Initialize(String i_FileName)
        {
            m_FileName = i_FileName;
            m_Initialized = true;
            Debugger.Trace("Initializing the mp3 file");

            IsolatedStorageFile Store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!Store.FileExists(m_FileName))
            {
                Debugger.Trace("The mp3 file does not exist");
                return false;
            }

            using (IsolatedStorageFileStream stream = Store.OpenFile(m_FileName, FileMode.Open))
            {
                // Set the file length
                m_FileSize = stream.Length;
                Debugger.Trace("FileSize [" + m_FileSize + "]");

                // Check for an ID3 Header
                Debugger.Trace("Searching for the mp3 file's meta data");
                int_FindMetaData(stream);

                Debugger.Trace("Searching for the mp3 file's first header");
                if (!int_FindFirstHeader(stream))
                {
                    Debugger.Trace("There was an error finding the first Header of the MP3 File");
                    m_Initialized = false;
                    return m_Initialized;
                }

                Debugger.Trace("Calculating the song length");
                if (!int_CalculateSongLength(stream))
                {
                    Debugger.Trace("There was an error calculating the length of the MP3 File");
                    m_Initialized = false;
                    return m_Initialized;
                }
            }

            return m_Initialized;
        }

        private void int_FindMetaData(Stream i_Stream)
        {
            try
            {
                // Go to the first header
                i_Stream.Seek(0, SeekOrigin.Begin);
                BinaryReader Reader = new BinaryReader(i_Stream);

                string Header = new string(Reader.ReadChars(3));
                if (Header == "ID3")
                {
                    // The first 2 bytes are the version
                    int MajorVersion = System.Convert.ToInt32(Reader.ReadByte());
                    int MinorVerison = System.Convert.ToInt32(Reader.ReadByte());

                    byte  Flags   = Reader.ReadByte();
                    bool Unsynchronisation = (((Flags >> 6) & 0x01) == 0x01);
                    bool ExtendedHeader    = (((Flags >> 5) & 0x01) == 0x01);
                    bool Experimental      = (((Flags >> 4) & 0x01) == 0x01);

                    byte[] Size = Reader.ReadBytes(4);
                    int FrameSize = (Size[3] | (Size[2] << 7) | (Size[1] << 14) | (Size[0] << 21));


                    byte[] Frames = Reader.ReadBytes(FrameSize - 10);
                    m_MetaData = new ID3(Frames);
                }
                else if (Header == "TAG+")
                {

                }
                
            }
            catch (Exception ex)
            {
                Debugger.Trace(ex);
            }
        }

        private bool int_CalculateSongLength(Stream i_Stream)
        {
            if (m_FirstHeader == null)
            {
                Debugger.Trace("There was no first header");
                return false;
            }

            if (!m_FirstHeader.IsValid)
            {
                Debugger.Trace("The First header was not valid");
                return false;
            }

            try
            {
                // Go to the first header
                i_Stream.Seek(m_FirstHeader.StartLocation, SeekOrigin.Begin);
                BinaryReader Reader = new BinaryReader(i_Stream);
                if (Reader == null)
                {
                    Debugger.Trace("There was an error opening the BinaryReader to initialize the MP3File");
                    return false;
                }

                BufferedReader MyReader = new BufferedReader(Reader, 1024);
                MP3Header NextHeader = new MP3Header();

                byte[] HeaderBuffer = new byte[4];
                HeaderBuffer[0] = MyReader.ReadByte();
                HeaderBuffer[1] = MyReader.ReadByte();
                HeaderBuffer[2] = MyReader.ReadByte();
                HeaderBuffer[3] = MyReader.ReadByte();
                NextHeader.Initialize(HeaderBuffer, MyReader.Position - 4);

                m_FrameCount = 0;
                while (NextHeader.IsValid)
                {
                    m_FrameCount++;

                    if (!MyReader.MoveForward(NextHeader.FrameSize - 4))
                    {
                        break;
                    }

                    try
                    {
                        HeaderBuffer[0] = MyReader.ReadByte();
                        HeaderBuffer[1] = MyReader.ReadByte();
                        HeaderBuffer[2] = MyReader.ReadByte();
                        HeaderBuffer[3] = MyReader.ReadByte();
                        NextHeader.Initialize(HeaderBuffer, MyReader.Position - 4);
                    }
                    catch (EndOfStreamException ex)
                    {
                        // Reached the end of the file 
                        break;
                    }
                }

                int SongLength = m_FrameCount * m_FirstHeader.SamplesPerFrame / m_FirstHeader.SampleRate;
                m_SongLength = new TimeSpan(0, 0, SongLength);
                return true;
            }
            catch (Exception ex)
            {
                Debugger.Trace(ex);
                return false;
            }
        }

        private bool int_FindFirstHeader(Stream i_Stream)
        {
            try
            {
                // Go to start of file
                i_Stream.Seek(0, SeekOrigin.Begin);

                if (m_FirstHeader == null)
                    m_FirstHeader = new MP3Header();

                BinaryReader Reader = new BinaryReader(i_Stream);
                if (Reader == null)
                {
                    Debugger.Trace("There was an error opening the BinaryReader to initialize the MP3File");
                    return false;
                }

                BufferedReader MyReader = new BufferedReader(Reader, 1024);
                MP3Header NextHeader = new MP3Header();

                byte[] HeaderBuffer = new byte[4];
                HeaderBuffer[0] = MyReader.ReadByte();
                HeaderBuffer[1] = MyReader.ReadByte();
                HeaderBuffer[2] = MyReader.ReadByte();
                HeaderBuffer[3] = MyReader.ReadByte();

                // Find the first valid header
                while (!m_FirstHeader.IsValid || !NextHeader.IsValid)
                {
                    // Find the next header tag
                    while ((HeaderBuffer[0] != 0xFF) || (HeaderBuffer[1] & 0xE0) != 0xE0)
                    {
                        // Get the Next Byte
                        HeaderBuffer[0] = HeaderBuffer[1];
                        HeaderBuffer[1] = HeaderBuffer[2];
                        HeaderBuffer[2] = HeaderBuffer[3];
                        HeaderBuffer[3] = MyReader.ReadByte();
                    }

                    if (m_FirstHeader.Initialize(HeaderBuffer, MyReader.Position - 4))
                    {
                        // Move to the next header to check if it is the same format
                        // make sure the reader can move that far forward
                        if (MyReader.MoveForward(m_FirstHeader.FrameSize - 4))
                        {
                            byte[] NextHeaderBuffer = new byte[4];
                            NextHeaderBuffer[0] = MyReader.ReadByte();
                            NextHeaderBuffer[1] = MyReader.ReadByte();
                            NextHeaderBuffer[2] = MyReader.ReadByte();
                            NextHeaderBuffer[3] = MyReader.ReadByte();
                            if ((NextHeaderBuffer[0] == 0xFF) && (NextHeaderBuffer[1] & 0xE0) == 0xE0)
                            {
                                if (NextHeader.Initialize(NextHeaderBuffer, MyReader.Position - 4))
                                {
                                    // If it is not the same header as the previous one then reset it
                                    if (NextHeader != m_FirstHeader)
                                        NextHeader = new MP3Header();
                                }
                            }
                            MyReader.MoveBackward(m_FirstHeader.FrameSize);
                        }
                    }

                    // Get the Next Byte
                    HeaderBuffer[0] = HeaderBuffer[1];
                    HeaderBuffer[1] = HeaderBuffer[2];
                    HeaderBuffer[2] = HeaderBuffer[3];
                    HeaderBuffer[3] = MyReader.ReadByte();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Trace(ex);
                return false;
            }
        }

        public override bool Trim(TimeSpan i_Start, TimeSpan i_End, String i_FileDestination)
        {
            try
            {
                if (!m_Initialized)
                {
                    Debugger.Trace("MP3File.Trim() not initialized");
                    return false;
                }

                if (i_Start > i_End)
                {
                    Debugger.Trace("MP3File.Trim() The End time is before the Start time");
                    return false;
                }

                if (i_End > m_SongLength)
                {
                    Debugger.Trace("MP3File.Trim() The End time after the End of the song");
                    return false;
                }
                
                Debugger.Trace("Trimming the MP3 File");
                if (m_FirstHeader == null)
                    m_FirstHeader = new MP3Header();

                IsolatedStorageFile Store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!Store.FileExists(m_FileName))
                {
                    Debugger.Trace("The mp3 file does not exist");
                    return false;
                }

                Debugger.Trace("Opening File streams");
                using (IsolatedStorageFileStream OriginalStream = Store.OpenFile(m_FileName, FileMode.Open))
                {
                    using (IsolatedStorageFileStream DestinationStream = Store.OpenFile(i_FileDestination, FileMode.Create))
                    {
                        OriginalStream.Seek(0, 0);
                        DestinationStream.Seek(0, 0);

                        BinaryReader Reader = new BinaryReader(OriginalStream);
                        if (Reader == null)
                        {
                            Debugger.Trace("There was an error opening the BinaryReader to trim the MP3File");
                            return false;
                        }

                        BinaryWriter Writer = new BinaryWriter(DestinationStream);
                        if (Writer == null)
                        {
                            Debugger.Trace("There was an error opening the BinaryWriter to trim the MP3File");
                            return false;
                        }

                        BufferedReader MyReader = new BufferedReader(Reader, 1024);

                        // Copy the Header information
                        Debugger.Trace("Copying meta data");
                        MyReader.MoveForward(m_FirstHeader.StartLocation);
                        //MyReader.Copy(Writer, m_FirstHeader.StartLocation);

                        // Skip over headers until we get to the start time
                        double CurTime = 0.0;
                        MP3Header NextHeader = new MP3Header();
                        byte[] HeaderBuffer = new byte[4];
                        HeaderBuffer[0] = MyReader.ReadByte();
                        HeaderBuffer[1] = MyReader.ReadByte();
                        HeaderBuffer[2] = MyReader.ReadByte();
                        HeaderBuffer[3] = MyReader.ReadByte();
                        while (CurTime < i_Start.TotalSeconds && NextHeader.Initialize(HeaderBuffer, MyReader.Position - 4))
                        {
                            // Update the time
                            CurTime += NextHeader.SamplesPerFrame / (double)NextHeader.SampleRate;

                            if (!MyReader.MoveForward(NextHeader.FrameSize - 4))
                            {
                                Debugger.Trace("Reached the end of the mp3 file");
                                return false;
                            }

                            HeaderBuffer[0] = MyReader.ReadByte();
                            HeaderBuffer[1] = MyReader.ReadByte();
                            HeaderBuffer[2] = MyReader.ReadByte();
                            HeaderBuffer[3] = MyReader.ReadByte();
                        }

                        // Copy Frames until we get to the end position
                        while (CurTime < i_End.TotalSeconds && NextHeader.Initialize(HeaderBuffer, MyReader.Position - 4))
                        {
                            // Update the time
                            CurTime += NextHeader.SamplesPerFrame / (double)NextHeader.SampleRate;
                            MyReader.MoveBackward(4);
                            MyReader.Copy(Writer, NextHeader.FrameSize);

                            HeaderBuffer[0] = MyReader.ReadByte();
                            HeaderBuffer[1] = MyReader.ReadByte();
                            HeaderBuffer[2] = MyReader.ReadByte();
                            HeaderBuffer[3] = MyReader.ReadByte();
                        }

                        Writer.Flush();
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                Debugger.Trace(ex);
                return false;
            }
        }
    }

    public class BufferedReader
    {
        private byte[] m_Buffer;
        private int m_BufferIndex;
        private int m_BufferSize;
        private int m_NumBytes;
        private BinaryReader m_Reader;

        public BufferedReader(BinaryReader i_Reader, int i_BufferSize)
        {
            m_Reader = i_Reader;
            m_Buffer = new byte[i_BufferSize];
            m_BufferIndex = 0;
            m_BufferSize = i_BufferSize;
            m_NumBytes = 0;
        }

        public byte ReadByte()
        {
            if (m_BufferIndex >= m_NumBytes)
            {
                m_BufferIndex = 0;
                m_NumBytes = m_Reader.Read(m_Buffer, 0, m_BufferSize);
                if (m_NumBytes == 0)
                    throw new EndOfStreamException("Reached the end of the Binary Reader");
            }

            return m_Buffer[m_BufferIndex++];
        }

        public int Position 
        {
            get 
            {
                return (int)(m_Reader.BaseStream.Position - (m_NumBytes - m_BufferIndex));
            } 
        }

        public bool MoveForward(int i_NumBytes)
        {
            if(i_NumBytes < 0)
                return false;

            long CurPosition = m_BufferIndex + m_Reader.BaseStream.Position - m_NumBytes;
            long NewPosition = CurPosition + i_NumBytes;
            if (NewPosition > m_Reader.BaseStream.Length)
                return false;

            if(NewPosition > m_Reader.BaseStream.Position)
                m_Reader.BaseStream.Position = NewPosition;

            m_BufferIndex += i_NumBytes;
            return true;
        }

        public void MoveBackward(int i_NumBytes)
        {
            long CurPosition = m_BufferIndex + m_Reader.BaseStream.Position - m_NumBytes;
            long NewPosition = CurPosition - i_NumBytes;
            if (NewPosition < 0)
                NewPosition = 0;

            long StartPosition = m_Reader.BaseStream.Position - m_NumBytes;
            if (NewPosition < StartPosition)
            {
                m_Reader.BaseStream.Position = NewPosition;
                m_NumBytes = 0;
                m_BufferIndex = 0;
            }
            else
                m_BufferIndex -= i_NumBytes;
        }

        public void Copy(BinaryWriter i_Writer, int i_Length)
        {
            int BytesCopied = 0;
            int BytesRemaining = i_Length;
            while(BytesCopied < i_Length)
            {
                int NumBytes = m_NumBytes - m_BufferIndex;
                if(BytesRemaining < NumBytes)
                    NumBytes = BytesRemaining;

                i_Writer.Write(m_Buffer, m_BufferIndex, NumBytes);
                m_BufferIndex += NumBytes;

                BytesCopied    += NumBytes;
                BytesRemaining -= NumBytes;

                if(m_BufferIndex >= m_NumBytes)
                {
                    m_BufferIndex = 0;
                    m_NumBytes = m_Reader.Read(m_Buffer, 0, m_BufferSize);
                    if (m_NumBytes == 0)
                        throw new EndOfStreamException("Reached the end of the Binary Reader");
                }
            }
        }
    }
}
