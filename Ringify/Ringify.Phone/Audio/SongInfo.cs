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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.IO;

namespace Ringify
{
    public enum SongState
    {
        Online,
        Downloading,
        Local
    }

    public class SongInfo : INotifyPropertyChanged
    {
        private WebClient m_Client;

        public void Download()
        {
            if (State == SongState.Online)
            {
                State = SongState.Downloading;
                m_Client.OpenReadAsync(new Uri(m_Url));
            }
        }

        public bool IsLocal
        {
            get
            {
                if (State == SongState.Local)
                    return true;
                else
                    return false;
            }
        }

        public Visibility HideProgressBar
        {
            get
            {
                if (State != SongState.Local)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public Visibility ShowProgressBar
        {
            get
            {
                if (State == SongState.Local)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        private SongState m_State;
        public SongState State
        {
            get
            {
                return m_State;
            }
            private set
            {
                if (value != m_State)
                {
                    m_State = value;
                    NotifyPropertyChanged("State");
                    NotifyPropertyChanged("IsLocal");
                    NotifyPropertyChanged("ShowProgressBar");
                    NotifyPropertyChanged("HideProgressBar");
                }
            }
        }

        private string m_SongTitle;
        private string m_Url;
        private string m_Artist;
        private string m_Album;
        private string m_Length;
        private string m_Position;
        public string SongTitle 
        { 
            get
            {
                if (m_SongTitle == null)
                {
                    return "";
                }
                else
                {
                    return m_SongTitle;
                }
            } 
            set
            {
                if (value != m_SongTitle)
                {
                    m_SongTitle = value;
                    NotifyPropertyChanged("SongTitle");
                }
            }
        }

        public string Url
        {
            get
            {
                if (m_Url == null)
                {
                    return "";
                }
                else
                {
                    return m_Url;
                }
            }
            set
            {
                if (value != m_Url)
                {
                    m_Url = value;
                    NotifyPropertyChanged("Url");
                }
            }
        }

        public String Artist {
            get
            {
                if (m_Artist == null)
                {
                    return "";
                }
                else
                {
                    return m_Artist;
                }
            } 
            set{
                if (value != m_Artist)
                {
                    m_Artist = value;
                    NotifyPropertyChanged("Artist");
                }
            }
        }
        public String Album {
            get
            {
                if (m_Album == null)
                {
                    return "";
                }
                else
                {
                    return m_Album;
                }
            }
            set{
                if (value != m_Album)
                {
                    m_Album = value;
                    NotifyPropertyChanged("Album");
                }
            }
        }

        public string Length
        {
            get
            {
                if (m_Length == null)
                {
                    return "0:00";
                }
                else
                {
                    return m_Length;
                }
            }
            set
            {
                if (value != m_Length)
                {
                    m_Length = value;
                    NotifyPropertyChanged("Length");
                }
            }
        }

        public string Position
        {
            get
            {
                if (m_Position == null)
                {
                    return "0:00";
                }
                else
                {
                    return m_Position;
                }
            }
            private set
            {
                if (value != m_Position)
                {
                    m_Position = value;
                    NotifyPropertyChanged("Position");
                }
            }
        }
        
        public string SongTitleTrimmed
        {
            get
            {
                if (m_SongTitle == null)
                {
                    return "";
                }
                else
                {
                    int IndexPeriod = m_SongTitle.IndexOf('.');
                    if (IndexPeriod != -1)
                        return m_SongTitle.Substring(0, IndexPeriod);
                    else
                        return m_SongTitle;
                }
            }
        }

        private int m_DownloadProgress;
        public int DownloadProgress
        {
            get
            {
                return m_DownloadProgress;
            }
            private set
            {
                if (m_DownloadProgress != value)
                {
                    m_DownloadProgress = value;
                    NotifyPropertyChanged("DownloadProgress");
                }
            }
        }

    public SongInfo(String songTitle, String url, String album)
    {
        this.SongTitle = songTitle;
        this.Url = url;
        this.Artist = string.Empty;
        this.Album = album;
        this.Length = "0:00";
        this.Position = "0:00";
        this.State = SongState.Online;
        this.DownloadProgress = 0;

        m_Client = new WebClient();
        m_Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(m_Client_DownloadProgressChanged);
        //m_Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(m_Client_DownloadStringCompleted);
        m_Client.OpenReadCompleted += new OpenReadCompletedEventHandler(m_Client_OpenReadCompleted);
    }

    public SongInfo()
    {
        this.SongTitle = string.Empty;
        this.Url = string.Empty;
        this.Artist = string.Empty;
        this.Album = string.Empty;
        this.Length = "0:00";
        this.Position = "0:00";
        this.State = SongState.Online;
        this.DownloadProgress = 0;

        m_Client = new WebClient();
        m_Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(m_Client_DownloadProgressChanged);
        m_Client.OpenReadCompleted += new OpenReadCompletedEventHandler(m_Client_OpenReadCompleted);
    }

    void m_Client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
    {
        try
        {
            if (e.Cancelled)
            {
                // The song is online
                State = SongState.Online;
                Debugger.Trace("Canceled");
            }
            else if (e.Error != null)
            {
                // The song is online
                State = SongState.Online;
                Debugger.Trace("Error: " + e.Error.Message);
            }
            else
            {
                IsolatedStorageFile Store = IsolatedStorageFile.GetUserStoreForApplication();
                using (IsolatedStorageFileStream stream = Store.CreateFile(Strings.Directory_Songs + "/" + SongTitle))
                {
                    e.Result.CopyTo(stream);
                    // The song is local
                    State = SongState.Local;
                }
            }
        }
        catch (Exception ex)
        {
            // The song is online
            State = SongState.Online;
            Debugger.Trace(ex);
        }
    }

    void m_Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        DownloadProgress = e.ProgressPercentage;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String PropertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (null != handler)
        {
            handler(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}

    public class SongCollection : ObservableCollection<SongInfo>
    {
        public SongCollection()
        {

        }

    }


}
