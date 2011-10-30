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
using Microsoft.Phone.Tasks;

namespace Ringify.Pages
{
    public partial class Feedback : PhoneApplicationPage
    {
        MarketplaceReviewTask ReviewTask;

        public Feedback()
        {
            InitializeComponent();

            // Initialize the SaveRingtoneTask and assign the Completed handler in the page constructor.
            ReviewTask = new MarketplaceReviewTask();
        }

        private void Button_Review_Click(object sender, RoutedEventArgs e)
        {
            ReviewTask.Show();

            // Dont let them click it twice
            Button_Review.IsEnabled = false;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}