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

namespace Ringify
{
    public enum MPEG_Version
    {
        MPEG_1 = 0,
        MPEG_2 = 1,
        MPEG_2_5 = 2,
        INVALID = 3
    }

    public enum MPEG_Layer
    {
        LAYER_I = 0,
        LAYER_II = 1,
        LAYER_III = 2,
        INVALID = 3
    }

    public enum MPEG_CHANNEL_MODE
    {
        STEREO,
        JOINT_STEREO,
        DUAL_CHANNEL,
        SINGLE_CHANNEL
    }

    public enum MPEG_Emphasis
    {
        NONE,
        MS_50_15,
        RESERVED,
        CCIT_J_17
    }

    public class MP3Header
    {
        // BitRates: 1. index = LSF, 2. index = Layer, 3. index = bitrate index
        int[][] numbers = new int[][] { new int[] {1, 2}, new int[] {3, 4, 5}};
        private static Int16[][][] BitRates = new Int16[][][]
        {
	        new Int16[][] // MPEG 1
            {	
		        new Int16[] {0,32,64,96,128,160,192,224,256,288,320,352,384,416,448,-1}, // Layer1
		        new Int16[] {0,32,48,56, 64, 80, 96,112,128,160,192,224,256,320,384,-1}, // Layer2
		        new Int16[] {0,32,40,48, 56, 64, 80, 96,112,128,160,192,224,256,320,-1}  // Layer3
	        },
            new Int16[][] // MPEG 2
	        {		
		        new Int16[] {0,32,48,56,64,80,96,112,128,144,160,176,192,224,256,-1}, // Layer1
		        new Int16[] {0, 8,16,24,32,40,48, 56, 64, 80, 96,112,128,144,160,-1}, // Layer2
		        new Int16[] {0, 8,16,24,32,40,48, 56, 64, 80, 96,112,128,144,160,-1}  // Layer3
	        },
            new Int16[][] // MPEG 2.5
	        {		
		        new Int16[] {0,32,48,56,64,80,96,112,128,144,160,176,192,224,256,-1}, // Layer1
		        new Int16[] {0, 8,16,24,32,40,48, 56, 64, 80, 96,112,128,144,160,-1}, // Layer2
		        new Int16[] {0, 8,16,24,32,40,48, 56, 64, 80, 96,112,128,144,160,-1}  // Layer3
	        }
        };

        // Samples per Frame: 1. index = LSF, 2. index = Layer
        private static Int16[][] SamplesPerFrames = new Int16[][]
        {
	        new Int16[] { // MPEG 1
		        384,      // Layer1
		        1152,     // Layer2	
		        1152      // Layer3
	        },
	        new Int16[] { // MPEG 2, 2.5
		        384,      // Layer1
		        1152,     // Layer2
		        576	      // Layer3
	        }	
        };


        // slot size per layer
        private static Int16[] SlotSizes = new Int16[]
        {
	        4,  // Layer1
	        1,  // Layer2
	        1   // Layer3
        };


        // 11 – (31-21)  -  Frame sync
        // 2  - (20, 19) -  MPEG audio version (MPEG-1, 2, etc.)  
        // 2  - (18,17)  -  MPEG layer (Layer I, II, III, etc.)
        // 1  - (16)     - Protection (if on, then checksum follows header)
        // 4  - (15,12)  - Bitrate index (lookup table used to specify bitrate for this MPEG version and layer)
        // 2  - (11, 10) - Sampling rate frequency (44.1kHz, etc., determined by lookup table)
        // 1  - (9)      - Padding bit (on or off, compensates for unfilled frames)
        // 1  - (8)      -Private bit (on or off, allows for application-specific triggers)
        // 2  - (7,6)    - Channel mode (stereo, joint stereo, dual channel, single channel)
        // 2  - (5,4)    - Mode extension (used only with joint stereo, to conjoin channel data)
        // 1  - (3)      - (on or off) 1
        // 1  - (2)      -  Original (off if copy of original, on if original)
        // 2  - (1,0)    - Emphasis (respects emphasis bit in the original recording; now largely obsolete)
        private bool m_Valid;
        private MPEG_Version m_Version;
        private MPEG_Layer m_Layer;
        private bool m_Protected;
        private int m_BitRateIndex;
        private int m_BitRate;
        private int m_SampleRate;
        private int m_Padding;
        private bool m_Private;
        private MPEG_CHANNEL_MODE m_ChannelMode;
        private int m_ModeExtension;
        private bool m_Copyright;
        private bool m_Original;
        private MPEG_Emphasis m_Emphasis;

        private int m_SamplesPerFrame;
        private int m_FrameSize;
        private int m_StartLocation;

        public MP3Header()
        {
            m_Valid = false;
            m_Version = MPEG_Version.INVALID;
            m_Layer = MPEG_Layer.INVALID;
            m_Protected = false;
            m_BitRateIndex = -1;
            m_BitRate = -1;
            m_SampleRate = -1;
            m_Padding = -1;
            m_Private = false;
            m_ChannelMode = MPEG_CHANNEL_MODE.STEREO;
            m_ModeExtension = -1;
            m_Copyright = false;
            m_Original = false;
            m_Emphasis = MPEG_Emphasis.NONE;
            m_FrameSize = -1;
            m_StartLocation = -1;
        }

        public bool IsValid { get { return m_Valid; } }
        public MPEG_Version Version { get { return m_Version; } }
        public MPEG_Layer Layer { get { return m_Layer; } }
        public bool IsProtected { get { return m_Protected; } }
        public int BitRateIndex { get { return m_BitRateIndex; } }
        public int BitRate { get { return m_BitRate; } }
        public int SampleRate { get { return m_SampleRate; } }
        public int Padding { get { return m_Padding; } }
        public bool IsPrivate { get { return m_Private; } }
        public MPEG_CHANNEL_MODE ChannelMode { get { return m_ChannelMode; } }
        public int ModeExtension { get { return m_ModeExtension; } }
        public bool IsCopyright { get { return m_Copyright; } }
        public bool IsOriginal { get { return m_Original; } }
        public MPEG_Emphasis Emphasis { get { return m_Emphasis; } }
        public int FrameSize { get { return m_FrameSize; } }
        public int SamplesPerFrame { get { return m_SamplesPerFrame; } }
        public int StartLocation { get { return m_StartLocation; } }

        public bool Initialize(byte[] i_Header, int i_StartLocation)
        {
            m_StartLocation = i_StartLocation;
            m_Valid = true;
            if (i_Header.Length != 4)
            {
                Debugger.Trace("Received an invalid sized Header");
                m_Valid = false;
            }

            if (i_Header[0] != 0xFF || (i_Header[1] & 0xE0) != 0xE0)
            {
                Debugger.Trace("Received an invalid Header");
                m_Valid = false;
            }

            // MPEG Audio version ID
            // 00 - MPEG Version 2.5 (later extension of MPEG 2)
            // 01 - reserved
            // 10 - MPEG Version 2 (ISO/IEC 13818-3)
            // 11 - MPEG Version 1 (ISO/IEC 11172-3)
            // Note: MPEG Version 2.5 was added lately to the MPEG 2 standard. 
            // It is an extension used for very low bitrate files, allowing the use of lower sampling frequencies.
            // If your decoder does not support this extension, it is recommended for you to use 12 bits for synchronization instead of 11 bits.
            int VersionID = (i_Header[1] >> 3) & 0x3;
            switch (VersionID)
            {
                case 0:
                    m_Version = MPEG_Version.MPEG_2_5;
                    break;
                case 1:
                    m_Version = MPEG_Version.INVALID;
                    m_Valid = false;
                    break;
                case 2:
                    m_Version = MPEG_Version.MPEG_2;
                    break;
                case 3:
                    m_Version = MPEG_Version.MPEG_1;
                    break;
                default:
                    m_Version = MPEG_Version.INVALID;
                    m_Valid = false;
                    break;
            }

            // Layer description
            // 00 - reserved
            // 01 - Layer III
            // 10 - Layer II
            // 11 - Layer I
            int LayerID = (i_Header[1] & 0x06) >> 1;
            switch (LayerID)
            {
                case 0:
                    m_Layer = MPEG_Layer.INVALID;
                    m_Valid = false;
                    break;
                case 1:
                    m_Layer = MPEG_Layer.LAYER_III;
                    break;
                case 2:
                    m_Layer = MPEG_Layer.LAYER_II;
                    break;
                case 3:
                    m_Layer = MPEG_Layer.LAYER_I;
                    break;
                default:
                    m_Layer = MPEG_Layer.INVALID;
                    m_Valid = false;
                    break;
            }

            // Protection bit
            // 0 - Protected by CRC (16bit CRC follows header)
            // 1 - Not protected
            m_Protected = (i_Header[1] & 0x01) == 0x00;

            // Bitrate index
            m_BitRateIndex = (i_Header[2] & 0xF0) >> 4;
            if (m_Valid && (int)m_Version < 3 && (int)m_Layer < 3)
                m_BitRate = BitRates[(int)m_Version][(int)m_Layer][m_BitRateIndex];
            else
                m_BitRate = -1;

            if (m_BitRate < 0)
                m_Valid = false;

            // MPEG layer II doesnt allow some bitrates
            // bitrate single stereo intensity dual
            // free    yes    yes    yes    yes
            // 32 	   yes    no     no     no
            // 48      yes    no     no     no
            // 56      yes    no     no     no
            // 64 	   yes    yes    yes    yes
            // 80      yes    no     no     no
            // 96 	   yes	  yes    yes    yes
            // 112     yes    yes    yes    yes
            // 128     yes    yes    yes    yes
            // 160     yes    yes    yes    yes
            // 192     yes    yes    yes    yes
            // 224 	   no     yes    yes    yes
            // 256     no     yes    yes    yes
            // 320     no     yes    yes    yes
            // 384     no     yes    yes    yes
            if (m_Valid && m_Layer == MPEG_Layer.LAYER_II)
            {
                if (m_ChannelMode == MPEG_CHANNEL_MODE.SINGLE_CHANNEL)
                {
                    if (m_BitRate == 224)
                        m_Valid = false;
                    else if (m_BitRate == 256)
                        m_Valid = false;
                    else if (m_BitRate == 320)
                        m_Valid = false;
                    else if (m_BitRate == 384)
                        m_Valid = false;
                }
                else
                {
                    if (m_BitRate == 32)
                        m_Valid = false;
                    else if (m_BitRate == 48)
                        m_Valid = false;
                    else if (m_BitRate == 56)
                        m_Valid = false;
                    else if (m_BitRate == 80)
                        m_Valid = false;
                }
            }

            // Convert bitrate to Hz
            m_BitRate *= 1000;

            // bits  MPEG1      MPEG2       MPEG2.5
            // 00 	 44100 Hz 	22050 Hz 	11025 Hz
            // 01 	 48000 Hz 	24000 Hz 	12000 Hz
            // 10 	 32000 Hz 	16000 Hz 	8000 Hz
            // 11 	 reserved 	reserved	reserved
            int SampleRateIndex = (i_Header[2] & 0x0C) >> 2;
            switch (SampleRateIndex)
            {
                case 0:
                    switch (m_Version)
                    {
                        case MPEG_Version.MPEG_1:
                            m_SampleRate = 44100;
                            break;
                        case MPEG_Version.MPEG_2:
                            m_SampleRate = 22050;
                            break;
                        case MPEG_Version.MPEG_2_5:
                            m_SampleRate = 11025;
                            break;
                    }
                    break;
                case 1:
                    switch (m_Version)
                    {
                        case MPEG_Version.MPEG_1:
                            m_SampleRate = 48000;
                            break;
                        case MPEG_Version.MPEG_2:
                            m_SampleRate = 24000;
                            break;
                        case MPEG_Version.MPEG_2_5:
                            m_SampleRate = 12000;
                            break;
                    }
                    break;
                case 2:
                    switch (m_Version)
                    {
                        case MPEG_Version.MPEG_1:
                            m_SampleRate = 32000;
                            break;
                        case MPEG_Version.MPEG_2:
                            m_SampleRate = 16000;
                            break;
                        case MPEG_Version.MPEG_2_5:
                            m_SampleRate = 8000;
                            break;
                    }
                    break;
                case 3:
                    m_SampleRate = -1;
                    m_Valid = false;
                    break;
                default:
                    m_SampleRate = -1;
                    m_Valid = false;
                    break;
            }

            // Padding bit
            // 0 - frame is not padded
            // 1 - frame is padded with one extra slot
            // Padding is used to exactly fit the bitrate.As an example: 128kbps 44.1kHz layer II uses a 
            // lot of 418 bytes and some of 417 bytes long frames to get the exact 128k bitrate. For Layer 
            // I slot is 32 bits long, for Layer II and Layer III slot is 8 bits long.
            m_Padding = (i_Header[2] >> 1) & 0x01;

            // Private bit
            m_Private = (i_Header[2] & 0x01) == 0x01;

            // Channel Mode
            // 00 - Stereo
            // 01 - Joint stereo (Stereo)
            // 10 - Dual channel (2 mono channels)
            // 11 - Single channel (Mono)
            int ChannelModeID = (i_Header[3] & 0xC0) >> 6;
            switch (ChannelModeID)
            {
                case 0:
                    m_ChannelMode = MPEG_CHANNEL_MODE.STEREO;
                    break;
                case 1:
                    m_ChannelMode = MPEG_CHANNEL_MODE.JOINT_STEREO;
                    break;
                case 2:
                    m_ChannelMode = MPEG_CHANNEL_MODE.DUAL_CHANNEL;
                    break;
                case 3:
                    m_ChannelMode = MPEG_CHANNEL_MODE.SINGLE_CHANNEL;
                    break;
            }

            // Mode extension (used by joint stereo)
            m_ModeExtension = (i_Header[3] & 0x30) >> 4;

            // Copyright
            // 0 - Audio is not copyrighted
            // 1 - Audio is copyrighted
            m_Copyright = (i_Header[3] & 0x08) != 0x00;

            // Original
            // 0 - Copy of original media
            // 1 - Original media
            m_Original = (i_Header[3] & 0x04) != 0x00;

            // Emphasis
            // 00 - none
            // 01 - 50/15 ms
            // 10 - reserved
            // 11 - CCIT J.17
            int EmphasisID = i_Header[3] & 0x03;
            switch (EmphasisID)
            {
                case 0:
                    m_Emphasis = MPEG_Emphasis.NONE;
                    break;
                case 1:
                    m_Emphasis = MPEG_Emphasis.MS_50_15;
                    break;
                case 2:
                    m_Emphasis = MPEG_Emphasis.RESERVED;
                    break;
                case 3:
                    m_Emphasis = MPEG_Emphasis.CCIT_J_17;
                    break;
            }

            // Determine the samples per Frame
            //            MPEG 1    MPEG 2 (LSF)  MPEG 2.5 (LSF)
            // Layer I    384       384           384
            // Layer II   1152 	    1152          1152
            // Layer III  1152 	    576           576
            switch(m_Layer)
            {
                case MPEG_Layer.LAYER_I:
                    m_SamplesPerFrame = 384;
                break;
                case MPEG_Layer.LAYER_II:
                    m_SamplesPerFrame = 1152;
                break;
                case MPEG_Layer.LAYER_III:
                    if (m_Version == MPEG_Version.MPEG_1)
                        m_SamplesPerFrame = 1152;
                    else
                        m_SamplesPerFrame = 576;
                break;
            }

            // Calculate the Frame Size
            if (m_Valid && m_SampleRate != 0)
            {
                m_FrameSize = ( (m_SamplesPerFrame / 8 * m_BitRate) / m_SampleRate) + m_Padding;
            }
            else
            {
                m_FrameSize = -1;
            }

            if (m_FrameSize <= 0)
                m_Valid = false;

            return m_Valid;
        }

        public static bool operator ==(MP3Header i_H1, MP3Header i_H2)
        {
            try
            {
                return i_H1.Equals(i_H2);
            }
            catch
            {
                return false;
            }
        }

        public static bool operator !=(MP3Header i_H1, MP3Header i_H2)
        {
            try
            {
                return !i_H1.Equals(i_H2);
            }
            catch
            {
                return false;
            }
        }

        // Override the Object.Equals(object o) method:
        public override bool Equals(object i_Header)
        {
            MP3Header Header;
            try
            {
                Header = (MP3Header)i_Header;
            }
            catch
            {
                return false;
            }

            if (this.m_Version != Header.m_Version)
                return false;

            if (this.m_Layer != Header.m_Layer)
                return false;

            if (this.m_ChannelMode != Header.m_ChannelMode)
                return false;

            return true;
        }

        // Override the Object.GetHashCode() method:
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
