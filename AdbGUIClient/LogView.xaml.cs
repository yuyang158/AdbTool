using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for LogView.xaml
	/// </summary>
	public partial class LogView : UserControl, ISubControlPanel {
		private IDevice m_device;

		public LogView() {
			InitializeComponent();
		}

		public void AssignDevice(IDevice device) {
			m_device = device;
		}

		public string GetName() {
			return "Log View";
		}

		private void PullFileAndReadToText(string remoteFilePath) {
			try {
				using var stream = m_device.Pull(remoteFilePath);
				using var reader = new StreamReader(stream);
				txtLog.Text = reader.ReadToEnd();
			}
			catch (System.Exception) {

			}
		}

		private void PullErrorLog_Click(object sender, RoutedEventArgs e) {
			PullFileAndReadToText("error.log");
		}

		private void PullLastErrorLog_Click(object sender, RoutedEventArgs e) {
			PullFileAndReadToText("last-error.log");
		}

		private void PullStatLog_Click(object sender, RoutedEventArgs e) {
			PullFileAndReadToText("stat.log");
		}

		public void Active() {
		}
	}
}
