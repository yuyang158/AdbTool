using SharpAdbClient.DeviceCommands;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for RemoteApkInstall.xaml
	/// </summary>
	public partial class RemoteApkInstall : UserControl, ISubControlPanel {
		private const int DOWNLOAD_BUFFER_SIZE = 10240;
		public RemoteApkInstall() {
			InitializeComponent();
		}

		private AppData m_data;
		public void AssignAppData(AppData data) {
			m_data = data;
		}

		public string GetName() {
			return "Apk Install";
		}

		private void InstallFile(string filename) {
			if (m_data.SelectedDevice == null)
				return;

			txtLog.AppendText($"Install Apk : {Path.GetFileName(filename)} To {m_data.SelectedDevice}\n");

			try {
				PackageManager manager = new PackageManager(m_data.CurrentClient, m_data.SelectedDevice.Data);
				manager.InstallPackage(filename, true);
			}
			catch (Exception ex) {
				txtLog.AppendText($"Install fail : {ex.Message}");
			}
		}

		private async void btnInstall_ClickAsync(object sender, RoutedEventArgs e) {
			var uri = new Uri(txtUrl.Text);
			if (Path.GetExtension(uri.LocalPath) != ".apk") {
				return;
			}
			var filename = Path.GetFileName(uri.LocalPath);
			var targetFile = $"./ApkDownload/{filename}";
			if (File.Exists(targetFile)) {
				InstallFile(targetFile);
				return;
			}

			var req = WebRequest.Create(uri);
			var response = await req.GetResponseAsync();
			foreach (var item in response.Headers) {
				Console.WriteLine(item);
			}
			var fileSize = response.ContentLength;

			if (!Directory.Exists("./ApkDownload")) {
				Directory.CreateDirectory("./ApkDownload");
			}
			txtLog.AppendText($"Downloading Apk : {Path.GetFileName(filename)}\n");

			spProgress.Visibility = Visibility.Visible;
			using (var stream = new FileStream(targetFile, FileMode.CreateNew)) {
				var netStream = response.GetResponseStream();
				var readSize = 0;
				var buffer = new byte[DOWNLOAD_BUFFER_SIZE];
				while (readSize < fileSize) {
					var size = await netStream.ReadAsync(buffer, 0, DOWNLOAD_BUFFER_SIZE);
					readSize += size;
					stream.Write(buffer, 0, size);
					tbProgress.Text = $"Downloading : {readSize} / {fileSize}";
					pbDownload.Value = (double)readSize / fileSize;
				}
			}

			spProgress.Visibility = Visibility.Collapsed;
			InstallFile(targetFile);
		}
	}
}
