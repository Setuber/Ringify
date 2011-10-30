using System;
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
	public partial class RingifyLogo : UserControl
	{
		public RingifyLogo()
		{
			// Required to initialize variables
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
            
			//this.Animate.
		}

        public void Play(int i_Count)
        {
            this.Animate.Storyboard.RepeatBehavior = new RepeatBehavior(i_Count);
            this.Animate.Storyboard.Begin();
        }

        public void Stop()
        {
            this.Animate.Storyboard.Stop();
        }

        public void PlayForever()
        {
            this.Animate.Storyboard.RepeatBehavior = RepeatBehavior.Forever;
            this.Animate.Storyboard.Begin();
        }
	}
}