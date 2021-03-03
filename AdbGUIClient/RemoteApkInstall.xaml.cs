using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.IO;
using System.Net;
using System.Threading;
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
			Drop += RemoteApkInstall_Drop;
		}

		private void RemoteApkInstall_Drop(object sender, DragEventArgs e) {
			if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
				return;
			}

			var drop = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (drop.Length == 0) {
				return;
			}

			if (Path.GetExtension(drop[0]) != ".apk") {
				return;
			}
			m_data.DownloadUrl = drop[0];
		}

		private AppData m_data;
		public void AssignAppData(AppData data) {
			m_data = data;
			DataContext = data;
		}

		public string GetName() {
			return "Apk Install";
		}

		private class ShellResponseToLog : IShellOutputReceiver {
			public bool ParsesErrors => false;

			private TextBlock m_txtOutput;
			public ShellResponseToLog(TextBlock output) {
				m_txtOutput = output;
			}

			public void AddOutput(string line) {
				m_txtOutput.Dispatcher.Invoke(() => {
					m_txtOutput.Text += line;
				});
			}

			public void Flush() {
			}
		}

		private void InstallFile(string filename) {
			if (m_data.SelectedDevice == null)
				return;

			txtLog.Text += ($"Install Apk : {Path.GetFileName(filename)} To {m_data.SelectedDevice}\n");

			try {
				PackageManager manager = new PackageManager(m_data.CurrentClient, m_data.SelectedDevice.Data);
				manager.InstallPackage(filename, true);
				m_data.CurrentClient.ExecuteRemoteCommandAsync($"am start -n {m_data.PackageName}/com.unity3d.player.UnityPlayerActivity", m_data.SelectedDeviceData,
					new ShellResponseToLog(txtLog), CancellationToken.None);
			}
			catch (Exception ex) {
				txtLog.Text += $"Install fail : {ex.Message}";
			}
		}

		private async void btnInstall_ClickAsync(object sender, RoutedEventArgs e) {
			string targetFile;
			if (m_data.DownloadUrl.StartsWith("http") || m_data.DownloadUrl.StartsWith("https")) {
				var uri = new Uri(m_data.DownloadUrl);
				if (Path.GetExtension(uri.LocalPath) != ".apk") {
					return;
				}
				var filename = Path.GetFileName(uri.LocalPath);
				targetFile = $"./ApkDownload/{filename}";
				if (File.Exists(targetFile)) {
					InstallFile(targetFile);
					return;
				}

				var req = WebRequest.Create(uri);
				var response = await req.GetResponseAsync();
				var fileSize = response.ContentLength;

				if (!Directory.Exists("./ApkDownload")) {
					Directory.CreateDirectory("./ApkDownload");
				}
				txtLog.Text += $"Downloading Apk : {Path.GetFileName(filename)}\n";

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
			}
			else {
				if (File.Exists(m_data.DownloadUrl)) {
					targetFile = m_data.DownloadUrl;
				}
				else {
					txtLog.Text += $"URL is not a load address or web address : {m_data.DownloadUrl}\n";
					return;
				}
			}

			InstallFile(targetFile);
		}
	}
}
