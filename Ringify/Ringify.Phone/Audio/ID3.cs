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
using System.Text;
using System.IO;

namespace Ringify
{
    public class ID3
    {
        public MemoryStream m_Image;

        public ID3(byte[] i_Frames)
        {
            char[] Test = new char[i_Frames.Length];
            for (int i = 0; i < Test.Length; i++)
                Test[i] = (char)i_Frames[i];

            int Index = 0;
            int PrevIndex;
            int FrameSize;
            char[] FrameID;
            byte[] Flags;

            try
            {
                //    Frames

                //        All ID3v2 frames consists of one frame header followed by one or more
                //        fields containing the actual information. The header is always 10
                //        bytes and laid out as follows:
                //
                //        Frame ID      $xx xx xx xx  (four characters)
                //        Size      4 * %0xxxxxxx
                //        Flags         $xx xx
                while (Index < i_Frames.Length)
                {
                    FrameID = new char[] { (char)i_Frames[Index++], (char)i_Frames[Index++], (char)i_Frames[Index++], (char)i_Frames[Index++] };
                    FrameSize = ((i_Frames[Index++] << 24) | (i_Frames[Index++] << 16) | (i_Frames[Index++] << 8) | i_Frames[Index++]);
                    Flags = new byte[] { i_Frames[Index++], i_Frames[Index++] };

                    int_HandleFrame(new string(FrameID), FrameSize, Index, i_Frames);
                    PrevIndex = Index;
                    Index += FrameSize;
                }

                Index = 0;
            }
            catch (Exception ex)
            {
                Debugger.Trace(ex);
            }
        }

        private void int_HandleFrame(string i_FrameID, int i_FrameSize, int i_Index, byte[] i_Frames)
        {
            /////////////////////////////////////
            // AENC Audio encryption
            // APIC Attached picture

            // COMM Comments
            // COMR Commercial frame

            // ENCR Encryption method registration
            // EQUA Equalization (replaced by EQU2 in v2.4)
            // ETCO Event timing codes

            // GEOB General encapsulated object
            // GRID Group identification registration

            // IPLS Involved people list (replaced by TMCL and TIPL in v2.4)

            // LINK Linked information

            // MCDI Music CD identifier
            // MLLT MPEG location lookup table

            // OWNE Ownership frame

            // PRIV Private frame
            // PCNT Play counter
            // POPM Popularimeter
            // POSS Position synchronisation frame

            // RBUF Recommended buffer size
            // RVAD Relative volume adjustment (replaced by RVA2 in v2.4)
            // RVRB Reverb

            // SYLT Synchronized lyric/text
            // SYTC Synchronized tempo codes

            // TALB Album/Movie/Show title
            // TBPM BPM (beats per minute)
            // TCOM Composer
            // TCON Content type
            // TCOP Copyright message
            // TDAT Date (replaced by TDRC in v2.4)
            // TDLY Playlist delay
            // TENC Encoded by
            // TEXT Lyricist/Text writer
            // TFLT File type
            // TIME Time (replaced by TDRC in v2.4)
            // TIT1 Content group description
            // TIT2 Title/songname/content description
            // TIT3 Subtitle/Description refinement
            // TKEY Initial key
            // TLAN Language(s)
            // TLEN Length
            // TMED Media type
            // TOAL Original album/movie/show title
            // TOFN Original filename
            // TOLY Original lyricist(s)/text writer(s)
            // TOPE Original artist(s)/performer(s)
            // TORY Original release year (replaced by TDOR in v2.4)
            // TOWN File owner/licensee
            // TPE1 Lead performer(s)/Soloist(s)
            // TPE2 Band/orchestra/accompaniment
            // TPE3 Conductor/performer refinement
            // TPE4 Interpreted, remixed, or otherwise modified by
            // TPOS Part of a set
            // TPUB Publisher
            // TRCK Track number/Position in set
            // TRDA Recording dates (replaced by TDRC in v2.4)
            // TRSN Internet radio station name
            // TRSO Internet radio station owner
            // TSIZ Size (deprecated in v2.4)
            // TSRC ISRC (international standard recording code)
            // TSSE Software/Hardware and settings used for encoding
            // TYER Year (replaced by TDRC in v2.4)
            // TXXX User defined text information frame

            // UFID Unique file identifier
            // USER Terms of use
            // USLT Unsynchronized lyric/text transcription

            // WCOM Commercial information
            // WCOP Copyright/Legal information
            // WOAF Official audio file webpage
            // WOAR Official artist/performer webpage
            // WOAS Official audio source webpage
            // WORS Official internet radio station homepage
            // WPAY Payment
            // WPUB Publishers official webpage
            // WXXX User defined URL link frame
            /////////////////////////////////////

            switch (i_FrameID)
            {
                case "AENC":
                {
                    
                    Debugger.Trace("AENC: ");
                }
                break;
                case "APIC":
                {
                    AENC(i_FrameSize, i_Frames, i_Index);
                    
                }
                break;
                case "COMM":
                {
                    Debugger.Trace("COMM: ");
                }
                break;
                case "COMR":
                {
                    Debugger.Trace("COMR: ");
                }
                break;
                case "ENCR":
                {
                    Debugger.Trace("ENCR: ");
                }
                break;
                case "EQUA":
                {
                    Debugger.Trace("EQUA: ");
                }
                break;
                case "ETCO":
                {
                    Debugger.Trace("ETCO: ");
                }
                break;
                case "GEOB":
                {
                    Debugger.Trace("GEOB: ");
                }
                break;
                case "GRID":
                {
                    Debugger.Trace("GRID: ");
                }
                break;
                case "IPLS":
                {
                    Debugger.Trace("IPLS: ");
                }
                break;
                case "LINK":
                {
                    Debugger.Trace("LINK: ");
                }
                break;
                case "MCDI":
                {
                    Debugger.Trace("MCDI: ");
                }
                break;
                case "MLLT":
                {
                    Debugger.Trace("MLLT: ");
                }
                break;
                case "OWNE":
                {
                    Debugger.Trace("OWNE: ");
                }
                break;
                case "PRIV":
                {
                    Debugger.Trace("PRIV: ");
                }
                break;
                case "PCNT":
                {
                    Debugger.Trace("PCNT: ");
                }
                break;
                case "POPM":
                {
                    Debugger.Trace("POPM: ");
                }
                break;
                case "POSS":
                {
                    Debugger.Trace("POSS: ");
                }
                break;
                case "RBUF":
                {
                    Debugger.Trace("RBUF: ");
                }
                break;
                case "RVAD":
                {
                    Debugger.Trace("RVAD: ");
                }
                break;
                case "RVRB":
                {
                    Debugger.Trace("RVRB: ");
                }
                break;
                case "SYLT":
                {
                    Debugger.Trace("SYLT: ");
                }
                break;
                case "SYTC":
                {
                    Debugger.Trace("SYTC: ");
                }
                break;
                case "TALB":
                {
                    // The 'Album/Movie/Show title' frame is intended for the title of 
                    // the recording(/source of sound) which the audio in the file is taken from. 
                    i_Index += 1;
                    char[] Title = new char[i_FrameSize - 1];
                    for (int i = 0; i < Title.Length; i++)
                        Title[i] = (char)i_Frames[i_Index++];
                    Debugger.Trace("Album: " + new string(Title));
                }
                break;
                case "TBPM":
                {
                    Debugger.Trace("TBPM: ");
                }
                break;
                case "TCOM":
                {
                    Debugger.Trace("TCOM: ");
                }
                break;
                case "TCON":
                {
                    Debugger.Trace("TCON: ");
                }
                break;
                case "TCOP":
                {
                    Debugger.Trace("TCOP: ");
                }
                break;
                case "TDAT":
                {
                    // The 'Date' frame is a numeric string in the DDMM format containing
                    // the date for the recording. This field is always four characters long.
                    i_Index += 1;
                    char[] Date = new char[i_FrameSize - 1];
                    for (int i = 0; i < Date.Length; i++)
                        Date[i] = (char)i_Frames[i_Index++];
                    Debugger.Trace("Date: " + new string(Date));
                }
                break;
                case "TDLY":
                {
                    Debugger.Trace("TDLY: ");
                }
                break;
                case "TENC":
                {
                    Debugger.Trace("TENC: ");
                }
                break;
                case "TEXT":
                {
                    Debugger.Trace("TEXT: ");
                }
                break;
                case "TFLT":
                {
                    Debugger.Trace("TFLT: ");
                }
                break;
                case "TIME":
                {
                    // The 'Time' frame is a numeric string in the HHMM format containing the 
                    // time for the recording. This field is always four characters long.
                    i_Index += 1;
                    char[] Title = new char[i_FrameSize-1];
                    for (int i = 0; i < Title.Length; i++)
                        Title[i] = (char)i_Frames[i_Index++];
                    Debugger.Trace("Time: " + new string(Title));
                }
                break;
                case "TIT1":
                {
                    Debugger.Trace("TIT1: ");
                }
                break;
                case "TIT2":
                {
                    // The 'Title/Songname/Content description' frame is the actual 
                    // name of the piece (e.g. "Adagio", "Hurricane Donna").
                    i_Index += 1;
                    char[] Title = new char[i_FrameSize-1];
                    for (int i = 0; i < Title.Length; i++)
                        Title[i] = (char)i_Frames[i_Index++];
                    Debugger.Trace("Title: " + new string(Title));
                }
                break;
                case "TIT3":
                {
                    Debugger.Trace("TIT3: ");
                }
                break;
                case "TKEY":
                {
                    Debugger.Trace("TKEY: ");
                }
                break;
                case "TLAN":
                {
                    Debugger.Trace("TLAN: ");
                }
                break;
                case "TLEN":
                {
                    Debugger.Trace("TLEN: ");
                }
                break;
                case "TMED":
                {
                    Debugger.Trace("TMED: ");
                }
                break;
                case "TOAL":
                {
                    Debugger.Trace("TOAL: ");
                }
                break;
                case "TOFN":
                {
                    Debugger.Trace("TOFN: ");
                }
                break;
                case "TOLY":
                {
                    Debugger.Trace("TOLY: ");
                }
                break;
                case "TOPE":
                {
                    Debugger.Trace("TOPE: ");
                }
                break;
                case "TORY":
                {
                    Debugger.Trace("TORY: ");
                }
                break;
                case "TOWN":
                {
                    Debugger.Trace("TOWN: ");
                }
                break;
                case "TPE1":
                {
                    Debugger.Trace("TPE1: ");
                }
                break;
                case "TPE2":
                {
                    Debugger.Trace("TPE2: ");
                }
                break;
                case "TPE3":
                {
                    Debugger.Trace("TPE3: ");
                }
                break;
                case "TPE4":
                {
                    Debugger.Trace("TPE4: ");
                }
                break;
                case "TPOS":
                {
                    Debugger.Trace("TPOS: ");
                }
                break;
                case "TPUB":
                {
                    Debugger.Trace("TPUB: ");
                }
                break;
                case "TRCK":
                {
                    Debugger.Trace("TRCK: ");
                }
                break;
                case "TRDA":
                {
                    Debugger.Trace("TRDA: ");
                }
                break;
                case "TRSN":
                {
                    Debugger.Trace("TRSN: ");
                }
                break;
                case "TRSO":
                {
                    Debugger.Trace("TRSO: ");
                }
                break;
                case "TSIZ":
                {
                    Debugger.Trace("TSIZ: ");
                }
                break;
                case "TSRC":
                {
                    Debugger.Trace("TSRC: ");
                }
                break;
                case "TSSE":
                {
                    Debugger.Trace("TSSE: ");
                }
                break;
                case "TYER":
                {
                    Debugger.Trace("TYER: ");
                }
                break;
                case "TXXX":
                {
                    Debugger.Trace("TXXX: ");
                }
                break;
                case "UFID":
                {
                    Debugger.Trace("UFID: ");
                }
                break;
                case "USER":
                {
                    Debugger.Trace("USER: ");
                }
                break;
                case "USLT":
                {
                    Debugger.Trace("USLT: ");
                }
                break;
                case "WCOM":
                {
                    Debugger.Trace("WCOM: ");
                }
                break;
                case "WCOP":
                {
                    Debugger.Trace("WCOP: ");
                }
                break;
                case "WOAF":
                {
                    Debugger.Trace("WOAF: ");
                }
                break;
                case "WOAR":
                {
                    Debugger.Trace("WOAR: ");
                }
                break;
                case "WOAS":
                {
                    Debugger.Trace("WOAS: ");
                }
                break;
                case "WORS":
                {
                    Debugger.Trace("WORS: ");
                }
                break;
                case "WPAY":
                {
                    Debugger.Trace("WPAY: ");
                }
                break;
                case "WPUB":
                {
                    Debugger.Trace("WPUB: ");
                }
                break;
                case "WXXX":
                {
                    Debugger.Trace("WXXX: ");
                }
                break;
                default:
                {
                    Debugger.Trace("Unknown ID3v2 TAG: " + i_FrameID);
                }
                break;
            }


            // V2.4 
            switch(i_FrameID)
            {
                case "TDRC":
                {
                    // TDRC replaced TIME
                    i_Index += 1;
                    char[] Year = new char[i_FrameSize - 1];
                    for (int i = 0; i < Year.Length; i++)
                        Year[i] = (char)i_Frames[i_Index++];
                    Debugger.Trace("Year: " + new string(Year));
                }
                break;
                 
            }
        }


        private void AENC(int i_FrameSize, byte[] i_Frames, int i_Index)
        {
            Debugger.Trace("AENC: Found image");

            // This frame contains a picture directly related to the audio file. 
            // Image format is the MIME type and subtype for the image. 
            // In the event that the MIME media type name is omitted, "image/" will be implied. 
            // The "image/png" or "image/jpeg" picture format should be used when interoperability is wanted.
            // Description is a short description of the picture, represented as a terminated textstring.
            // The description has a maximum length of 64 characters, but may be empty.
            // There may be several pictures attached to one file, each in their individual "APIC" frame, 
            // but only one with the same content descriptor. There may only be one picture with the picture
            // type declared as picture type $01 and $02 respectively. There is the possibility to put only 
            // a link to the image file by using the 'MIME type' "-->" and having a complete URL instead of 
            // picture data. The use of linked files should however be used sparingly since there is the risk of separation of files.

            // <Header for 'Attached picture', ID: "APIC">
            // Text encoding	$xx
            // MIME type 	<text string> $00
            // Picture type 	$xx
            // Description 	<text string according to encoding> $00 (00)
            // Picture data 	<binary data>

            int Index = i_Index;

            int TextEncoding = System.Convert.ToInt32(i_Frames[Index++]);

            StringBuilder MIME = new StringBuilder();
            while (i_Frames[Index] != 0)
                MIME.Append((char)i_Frames[Index++]);

            Index++;
            Debugger.Trace(MIME.ToString());

            int PictureType = System.Convert.ToInt32(i_Frames[Index++]);

            StringBuilder Description = new StringBuilder();
            while (i_Frames[Index] != 0)
                Description.Append((char)i_Frames[Index++]);

            Index++;
            Debugger.Trace(Description.ToString());

            

            byte[] Image = new byte[i_FrameSize - (Index - i_Index)];
            for (int i = 0; i < Image.Length; i++)
                Image[i] = i_Frames[Index++];


            m_Image = new MemoryStream(Image);

            // Picture type:	$00	Other
            // $01	32x32 pixels 'file icon' (PNG only)
            // $02	Other file icon
            // $03	Cover (front)
            // $04	Cover (back)
            // $05	Leaflet page
            // $06	Media (e.g. lable side of CD)
            // $07	Lead artist/lead performer/soloist
            // $08	Artist/performer
            // $09	Conductor
            // $0A	Band/Orchestra
            // $0B	Composer
            // $0C	Lyricist/text writer
            // $0D	Recording Location
            // $0E	During recording
            // $0F	During performance
            // $10	Movie/video screen capture
            // $11	A bright coloured fish
            // $12	Illustration
            // $13	Band/artist logotype
            // $14	Publisher/Studio logotype
        }
    }
}
