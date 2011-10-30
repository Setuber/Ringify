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
using System.IO.IsolatedStorage;
using System.IO;
using RangeSlider;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace Ringify
{
    public partial class edit_ringtone : PhoneApplicationPage
    {
        SaveRingtoneTask saveRingtoneChooser;
        TimeSpan StartRing;
        TimeSpan EndRing;
        ClientGetAsync SongLoader;
        SongFile Song;

        public edit_ringtone()
        {
            InitializeComponent();

            // Initialize the SaveRingtoneTask and assign the Completed handler in the page constructor.
            saveRingtoneChooser = new SaveRingtoneTask();
            saveRingtoneChooser.Completed += new EventHandler<TaskEventArgs>(MakeRingtone);

            StartRing = new TimeSpan(0, 0, 0);
            EndRing = new TimeSpan(0, 0, 39);

            LoadSong(App.ViewModel.SelectedSong);
            TextBox_SongTitle.Text = App.ViewModel.SelectedSong.SongTitleTrimmed;
            TextBlock_SongLength.Text = "0:00";
            TextBox_EndPosition.Text = "0:00";
            TextBox_StartPosition.Text = "0:00";
            Slider.MinimumRangeSpan = 1;
            //Slider.MaximumRangeSpan = 39;
            //media.Source = new Uri("isostore:/" + App.ViewModel.SelectedSong.SongTitle);
        }

        private void LoadSong(SongInfo i_Song)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                progressBar1.Visibility = System.Windows.Visibility.Collapsed;
                TextBox_SongTitle.IsEnabled = true;
                Slider.IsEnabled = true;
                //slider2.IsEnabled = true;
                Button_PlayPause.IsEnabled = true;
                Button_Ringify.IsEnabled = true;
                Button_Cancel.IsEnabled = true;

                //Load Song
                WavFile WAV = new WavFile();
                MP3File MP3 = new MP3File();

                if (WAV.Initialize(Strings.Directory_Songs + "/" + App.ViewModel.SelectedSong.SongTitle))
                {
                    Song = WAV;
                }
                else if (MP3.Initialize(Strings.Directory_Songs + "/" + App.ViewModel.SelectedSong.SongTitle))
                {
                    Song = MP3;
                    if (MP3.m_MetaData != null && MP3.m_MetaData.m_Image != null)
                    {
                        BitmapImage Test = new BitmapImage();
                        Test.SetSource(MP3.m_MetaData.m_Image);
                        image1.Source = Test;
                        //image1.Visibility = System.Windows.Visibility.Visible;
                    }
                }
                else
                {
                    MessageBox.Show("The selected song is not supported as a Ringtone at the current time.");
                    Debugger.Trace("Unknown file format");
                }

                if (Song != null)
                {
                    string SongLength = Song.Length.Minutes + ":" + Song.Length.Seconds;
                    TextBlock_SongLength.Text = SongLength;
                    TextBox_EndPosition.Text = SongLength;
                    Slider.Maximum =  Song.Length.TotalSeconds;
                    Slider.RangeEnd = 39;
                }
                else
                {
                    NavigationService.GoBack();
                }
            });
        }

        private void ControlMedia(object sender, RoutedEventArgs e)
        {
            Button B = (Button)sender;
            if ((string)B.Content == "Preview")
            {
                IsolatedStorageFile Store = IsolatedStorageFile.GetUserStoreForApplication();
                int trimStart = (int)Slider.RangeStart;
                int trimEnd = (int)Slider.RangeEnd;
                if (Song != null &&
                    Song.Trim(new TimeSpan(trimStart * 1000 * 1000 * 10), new TimeSpan(trimEnd * 1000 * 1000 * 10), "RINGIFY_RINGTONE.mp3"))
                {
                    // Call the Save Reingtone Chooser
                    if (Store.FileExists("RINGIFY_RINGTONE.mp3"))
                    {
                        MP3File Test = new MP3File();
                        Test.Initialize("RINGIFY_RINGTONE.mp3");

                        using (IsolatedStorageFileStream stream = Store.OpenFile("RINGIFY_RINGTONE.mp3", FileMode.Open))
                        {
                            //media.Source = new Uri("isostore:/" + App.ViewModel.SelectedSong.SongTitle);
                            
                            media.SetSource(stream);
                        }
                    }
                }
            }
            else
            {
                StopSong();
            }
        }

        private void MakeRingtone(object sender, TaskEventArgs e)
        {
            MP3File Test = new MP3File();
            Test.Initialize("RINGIFY_RINGTONE.mp3");

            if (e.TaskResult == TaskResult.OK)
            {
                MessageBox.Show("Your Ringtone was created!\n\nThe Ringtone is now listed in\n[Settings] -> [Ringtones + Sounds]\n under the Rintones selector.");
                Debugger.Trace("Save Completed");
            }
            else if (e.TaskResult == TaskResult.Cancel)
            {
                Debugger.Trace("Save Cancelled");
                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message);
                    //TextBlock_Error.Text = e.Error.Message;
                    
                    Debugger.Trace(e.Error);
                    Debugger.Trace("FILE_SIZE [" + Test.Size + "]");
                }
            }
            else if (e.TaskResult == TaskResult.None)
            {
                TextBlock_Error.Text = "None!";
                Debugger.Trace("None");
                if (e.Error != null)
                {
                    TextBlock_Error.Text = e.Error.Message;
                    Debugger.Trace(e.Error);
                }
            }

            TextBlock_Error.Text += "FILE_SIZE [" + Test.Size + "]";
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if(NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void Button_Ringify_Click2(object sender, RoutedEventArgs e)
        {
            string strMsg = string.Empty;
            int trimStart = (int)Slider.RangeStart;
            int trimEnd = (int)Slider.RangeEnd;
     
            try
            {
                if (Song != null &&
                    Song.Trim(new TimeSpan(trimStart * 1000 * 1000 * 10 + 1000), new TimeSpan(trimEnd * 1000 * 1000 * 10), "RINGIFY_RINGTONE.mp3"))
                {
                    // Call the Save Reingtone Chooser
                    IsolatedStorageFile Store = IsolatedStorageFile.GetUserStoreForApplication();
                    if (Store.FileExists("RINGIFY_RINGTONE.mp3"))
                    {
                        MP3File Test = new MP3File();
                        Test.Initialize("RINGIFY_RINGTONE.mp3");

                        if (Test.Size < 1048576)
                        {
                            saveRingtoneChooser.Source = new Uri("isostore:/" + "RINGIFY_RINGTONE.mp3");
                            // Hard-coded name of the test sound file in the project
                            saveRingtoneChooser.DisplayName = TextBox_SongTitle.Text;
                            saveRingtoneChooser.Show();
                        }
                        else
                        {
                            MessageBox.Show("File Too Large [" + Test.Size + "].\nPlease shorten the Ringtone clip and try again.");
                            TextBlock_Error.Text = "File Too Large! [" + Test.Size + "]";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.Trace(ex);
            }

            Debugger.Trace(strMsg);
        }


        private void Slider_RangeChanged(object sender, EventArgs e)
        {
            RangeSlider.RangeSlider S = (RangeSlider.RangeSlider)sender;
            TimeSpan start = new TimeSpan(0, 0, (int)S.RangeStart);
            TimeSpan end = new TimeSpan(0, 0, (int)S.RangeEnd);
            TextBox_StartPosition.Text = start.Minutes + ":" + start.Seconds;
            TextBox_EndPosition.Text = end.Minutes + ":" + end.Seconds;
        }

        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            PlaySong();
        }

        private void PlaySong()
        {
            Button_PlayPause.Content = "Stop";
            TextBox_SongTitle.IsEnabled = true;
            Slider.IsEnabled            = false;
            Button_PlayPause.IsEnabled  = true;
            Button_Ringify.IsEnabled    = false;
            Button_Cancel.IsEnabled     = true;

            //media.Position = TimeSpan.FromSeconds(Slider.RangeStart);
            TextBlock_Error.Text = "I Think length is " + media.NaturalDuration.ToString();
            media.Play();
            RunTimerLoop();
        }

        private void StopSong()
        {
            Button_PlayPause.Content = "Preview";
            TextBox_SongTitle.IsEnabled = true;
            Slider.IsEnabled            = true;
            Button_PlayPause.IsEnabled  = true;
            Button_Ringify.IsEnabled    = true;
            Button_Cancel.IsEnabled     = true;

            textBlock4.Text = media.Position.Minutes + ":" + media.Position.Seconds;
            media.Stop();
            media.Source = new Uri("isostore:/");
            loopTimer.Stop();
        }

        private DispatcherTimer loopTimer;
        private void RunTimerLoop()
        {
    	    loopTimer = new DispatcherTimer();
    	    loopTimer.Interval =
    		    TimeSpan.FromMilliseconds(100);
    	    loopTimer.Tick += UpdateTime;
    	    loopTimer.Start();

        }

        // This method is called approx.
        // every 100 milliseconds
        // until the timer is stopped
        void UpdateTime(object sender, EventArgs e)
        {
            TimeSpan CurTime = media.Position + TimeSpan.FromSeconds(Slider.RangeStart);
            textBlock4.Text = CurTime.Minutes + ":" + CurTime.Seconds;
        }


        private void media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debugger.Trace("The Media element failed to load the stream");
            if (e.ErrorException != null)
                Debugger.Trace(e.ErrorException);
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopSong();
        }
    }
}