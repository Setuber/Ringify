using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Ringify.Pages
{
    public partial class SongPicker : PhoneApplicationPage
    {
        WebClient Client = new WebClient();


        public SongPicker()
        {
            InitializeComponent();

            App.ViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ViewModel_PropertyChanged);
        }

        void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Debugger.Trace("Downloaded = " + e.ProgressPercentage);
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateRingifyLogo();
        }

        private void UpdateRingifyLogo()
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (App.ViewModel.IsDataLoaded)
                {
                    RingifyLogo_OnlineSongPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    RingifyLogo_OnlineSongPanel.Visibility = Visibility.Visible;
                }
            });
        }

        // Load data for the ViewModel Items
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            RingifyLogo_OnlineSongPanel.PlayForever();
            RingifyLogo_RocordedPanel.PlayForever();

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            UpdateRingifyLogo();
            DataContext = App.ViewModel;
        }

        private void GoToEdit(object sender, RoutedEventArgs e)
        {
            Button B = (Button)sender;
            string SongTitle = (string)B.Tag;

            SongInfo ClickedSong = App.ViewModel.GetSong(SongTitle);
            if (ClickedSong != null)
            {
                if (ClickedSong.IsLocal)
                {
                    App.ViewModel.SetSelectedSong(SongTitle);
                    NavigationService.Navigate(new Uri("/Pages/EditRingtone.xaml", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    ClickedSong.Download();
                }
            }
        }

        private void ApplicationBarIconButton_Refresh_Click(object sender, EventArgs e)
        {
            App.ViewModel.LoadData();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem Selected = (PivotItem)SongPivot.SelectedItem;
            if (Selected == PivotItem_Songs)
            {
                ApplicationBar.Mode = ApplicationBarMode.Default;
            }
            else
            {
                ApplicationBar.Mode = ApplicationBarMode.Minimized;
            }
        }
    }
}