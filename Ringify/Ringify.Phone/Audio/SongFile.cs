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
    public class SongFile
    {
        protected bool m_Initialized;
        public bool IsInitialized { get { return m_Initialized; } }

        protected TimeSpan m_SongLength;
        public TimeSpan Length{ get { return m_SongLength; } }

        protected string m_FileName;
        public string Name { get { return m_FileName; } }

        public virtual bool Initialize(String i_FileName) { return false; }
        public virtual bool Trim(TimeSpan i_Start, TimeSpan i_End, String i_FileDestination) { return false; }

        protected long m_FileSize;
        public long Size { get { return m_FileSize; } }
    }
}
