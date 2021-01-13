using SharpAdbClient;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for LogView.xaml
	/// </summary>
	public partial class LogView : UserControl, ISubControlPanel {
		private AppData m_data;
		public LogView() {
			InitializeComponent();
		}

		public void AssignAppData(AppData data) {
			m_data = data;
		}

		public string GetName() {
			return "Log View";
		}

		private void PullErrorLog_Click(object sender, RoutedEventArgs e) {
			PullFile($"/sdcard/Android/data/{m_data.PackageName}/files/error.log");
		}

		private void PullLastErrorLog_Click(object sender, RoutedEventArgs e) {
			PullFile($"/sdcard/Android/data/{m_data.PackageName}/files/last-error.log");
		}

		private void PullFile(string remoteRoot) {
			try {
				var service = new SyncService(m_data.CurrentClient, m_data.SelectedDeviceData);
				using var stream = new FileStream("./pull.log", FileMode.OpenOrCreate);
				service.Pull(remoteRoot, stream, null, CancellationToken.None);
				service.Dispose();
				stream.Seek(0, SeekOrigin.Begin);
				using var reader = new StreamReader(stream);
				txtLog.Text = reader.ReadToEnd();
			}
			catch (Exception e) {
				MessageBox.Show(e.Message, "ERROR");
			}
		}

		private void PullStatLog_Click(object sender, RoutedEventArgs e) {
			PullFile($"/sdcard/Android/data/{m_data.PackageName}/files/stat.log");
		}
	}
}
