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
using System.IO.IsolatedStorage;
using SL.Phone.Federation.Utilities;

namespace Ringify.Pages
{
    public partial class Login : PhoneApplicationPage
    {
        public Login()
        {
            InitializeComponent();
        }

        

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs args)
        {
            var phoneState = PhoneApplicationService.Current.State;
            if (App.GetIsolatedStorageSetting<bool>("UserIsRegistered"))
            {
                this.NavigationService.GoBack();
            }
            else
            {

                SignInControl.RequestSecurityTokenResponseCompleted += new EventHandler<SL.Phone.Federation.Controls.RequestSecurityTokenResponseCompletedEventArgs>(SignInControl_RequestSecurityTokenResponseCompleted);
                SignInControl.GetSecurityToken();
            }
        }

        private const string NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string EmailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        void SignInControl_RequestSecurityTokenResponseCompleted(object sender, SL.Phone.Federation.Controls.RequestSecurityTokenResponseCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                RequestSecurityTokenResponseStore Store = (RequestSecurityTokenResponseStore)Application.Current.Resources["rstrStore"];
                WebHeaderCollection items = ParseQueryString(Store.SecurityToken);
                string claimsUserName = items[System.Net.HttpUtility.UrlEncode(NameClaimType)];
                string claimsEmail = items[System.Net.HttpUtility.UrlEncode(EmailClaimType)];
                string UserName = string.IsNullOrEmpty(claimsUserName) ? string.Empty : claimsUserName;

                // Check if the user is registered for ringify



                App.SetIsolatedStorageSetting("UserIsRegistered", true);

                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }
        }

        private static WebHeaderCollection ParseQueryString(string queryString)
        {
            WebHeaderCollection res = new WebHeaderCollection();
            int num = (queryString != null) ? queryString.Length : 0;
            for (int i = 0; i < num; i++)
            {
                int startIndex = i;
                int num4 = -1;
                while (i < num)
                {
                    char ch = queryString[i];
                    if (ch == '=')
                    {
                        if (num4 < 0)
                        {
                            num4 = i;
                        }
                    }
                    else if (ch == '&')
                    {
                        break;
                    }

                    i++;
                }

                var str = string.Empty;
                var str2 = string.Empty;
                if (num4 >= 0)
                {
                    str = queryString.Substring(startIndex, num4 - startIndex);
                    str2 = queryString.Substring(num4 + 1, (i - num4) - 1);
                }
                else
                {
                    str2 = queryString.Substring(startIndex, i - startIndex);
                }

                res[str.Replace("?", string.Empty)] = System.Net.HttpUtility.UrlDecode(str2);
            }

            return res;
        }
    }
}