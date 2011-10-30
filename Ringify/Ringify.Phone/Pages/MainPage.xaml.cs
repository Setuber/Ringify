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
using SL.Phone.Federation.Utilities;

namespace Ringify
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            if (App.GetIsolatedStorageSetting<bool>("UserIsRegistered"))
            {
                RequestSecurityTokenResponseStore _rstrStore = App.Current.Resources["rstrStore"] as RequestSecurityTokenResponseStore;
                _rstrStore.RequestSecurityTokenResponse = null;
                App.SetIsolatedStorageSetting("UserIsRegistered", false);
                UpdateLoginTile();
            }
            else
            {
                NavigationService.Navigate(new Uri("/Pages/Login.xaml", UriKind.Relative));
            }
        }

        private void UpdateLoginTile()
        {
            if (App.GetIsolatedStorageSetting<bool>("UserIsRegistered"))
            {
                Button_Login_Text.Text = "Logout";
                //Button_Login_Image.Source = 
            }
            else
            {
                Button_Login_Text.Text = "Login";
            }
        }

        private void Button_Feedback_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Feedback.xaml", UriKind.Relative));
        }

        private void Button_Settings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings.xaml", UriKind.Relative));
        }

        private void Button_Ringify_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/SongPicker.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Logo.Play(1);
            UpdateLoginTile();
        }
    }
}