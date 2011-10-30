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
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;

namespace Ringify
{
    public class SongCollectionViewModel : INotifyPropertyChanged
    {

        BlobStorageClient Client;


        public SongCollection SongList { get; set; }
        public SongInfo SelectedSong { get; set; }

        private bool m_DataLoaded;
        public bool IsDataLoaded
        {
            get
            {
                return m_DataLoaded;
            }
            set
            {
                if (value != m_DataLoaded)
                {
                    m_DataLoaded = value;
                    NotifyPropertyChanged("IsDataloaded");
                }
            }
        }

        public SongCollectionViewModel()
        {
            Client = new BlobStorageClient(
                "ringify", "cFv/A0P1D8IRYgUsnO0poiYx8DzLVNFPuMrKFhaiTV24hhetdH2PF6oisr1dqUg1mWzQ08BasajNkUOg9NuM2g==");

            SongList = new SongCollection();
            SelectedSong = null;
            Client.BlobListUpdated += new BlobListUpdatedEventHandler(UpdateSongList);
        }

        public void CreateBlob(string i_BlobName)
        {
            Client.CreateBlob("ringify", i_BlobName);
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        
        public void LoadData()
        {
            IsDataLoaded = false;
            SongList.Clear();
            Client.ListBlobs("ringify");

            // SongList.Add(new SongInfo("06 Yes.mp3", "Coldplay", "X and Y"));
            // SongList.Add(new SongInfo("Title2", "Artist2", "Album2"));
            // SongList.Add(new SongInfo("Title3", "Artist3", "Album3"));
            // SongList.Add(new SongInfo("Title4", "Artist4", "Album4"));
        }

        private void UpdateSongList(object i_Sender, EventArgs i_EventArgs)
        {
            foreach (BlobStorage Blob in Client.BlobList)
            {
                using (AutoResetEvent Wait = new AutoResetEvent(false))
                {

                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        AddSong(Blob.Name, Blob.Url, Blob.Name);
                        Wait.Set();
                    });

                    Wait.WaitOne();
                }
            }
            IsDataLoaded = true;
        }


        private void AddSong(string i_URL, string i_Name, string i_Artist)
        {
            SongList.Add(new SongInfo(i_URL, i_Name, i_Artist));
        }

        public bool SetSelectedSong(string i_SongName)
        {
            SongInfo Selected = this.GetSong(i_SongName);
            if(Selected != null)
            {
                    this.SelectedSong = Selected;
                    return true;
            }
            else
            {
                return false;
            }
        }

        public SongInfo GetSong(string i_SongName)
        {
            IEnumerator<SongInfo> Enum_SongList = SongList.GetEnumerator();
            while (Enum_SongList.MoveNext())
            {
                if (Enum_SongList.Current.SongTitle == i_SongName)
                    return Enum_SongList.Current;
            }

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
