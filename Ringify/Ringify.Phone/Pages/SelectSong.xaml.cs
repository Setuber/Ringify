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

namespace Ringify
{
    public partial class pick_a_song : PhoneApplicationPage
    {
        public pick_a_song()
        {

            InitializeComponent();

            //DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(pick_a_song_Loaded);
        }

        // Load data for the ViewModel Items
        private void pick_a_song_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            DataContext = App.ViewModel;
        }

        private void GoToEdit(object sender, RoutedEventArgs e)
        {
            Button B = (Button)sender;
            string SongTitle = (string)B.Tag;

            if (App.ViewModel.SetSelectedSong(SongTitle))
                NavigationService.Navigate(new Uri("/Pages/EditRingtone.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ApplicationBarIconButton_Refresh_Click(object sender, EventArgs e)
        {
            App.ViewModel.LoadData();
        }
    }
}