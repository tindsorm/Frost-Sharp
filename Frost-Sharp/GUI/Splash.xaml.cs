using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Frost_Sharp.GUI {
	/// <summary>
	/// Interaction logic for Splash.xaml
	/// </summary>
	public partial class Splash : Window {
		public bool splashFinished = false;
		private static Splash _instance = null;

		public static Splash GetInstance() {
			if (_instance == null || PresentationSource.FromVisual(_instance).IsDisposed) {
				return null;
			}
			return _instance;
		}
		public Splash() {
			_instance = this;
			InitializeComponent();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (!splashFinished) {
				e.Cancel = true;
				return;
			}
			Closing -= Window_Closing;
			e.Cancel = true;
			var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(500));
			anim.Completed += (s, _) => { this.Close(); MainWindow.GetInstance().Show(); };
			this.BeginAnimation(OpacityProperty, anim);
		}
	}
}
