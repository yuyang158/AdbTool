using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using static AdbGUIClient.IOSWindow;

namespace AdbGUIClient {
	public partial class RemoteIPAInstall : UserControl {
		private const int DOWNLOAD_BUFFER_SIZE = 10240;
		public RemoteIPAInstall() {
			InitializeComponent();
		}

		private SaveData m_data;
		public void AssignAppData(SaveData data) {
			m_data = data;
		}

		public string GetName() {
			return "Install IPA";
		}

		private void LogInstall(Process proc) {
			txtLog.Dispatcher.Invoke(async () => {
				while (!proc.StandardOutput.EndOfStream) {
					string line = await proc.StandardOutput.ReadLineAsync();
					txtLog.Text += line + "\n";
				}

				if (!proc.HasExited) {
					LogInstall(proc);
				}
			});
		}

		private void InstallFile(string filename) {
			if (m_data.SelectedDevice == null)
				return;

			txtLog.Text += ($"Install IPA : {Path.GetFileName(filename)} To {m_data.SelectedDevice}\n");
			var proc = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "tidevice",
					Arguments = $"--udid {m_data.SelectedDevice.UUID} install {filename}",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			proc.Start();
			LogInstall(proc);
			proc.Exited += Proc_Exited;
		}

		private void Proc_Exited(object sender, EventArgs e) {
			var proc = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "tidevice",
					Arguments = $"--udid {m_data.SelectedDevice.UUID} launch {m_data.BundleID}",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};
			proc.Start();
		}

		private async void btnInstall_Click(object sender, RoutedEventArgs e) {
			string targetFile;
			if (m_data.DownloadUrl.StartsWith("http") || m_data.DownloadUrl.StartsWith("https")) {
				var uri = new Uri(m_data.DownloadUrl);
				if (Path.GetExtension(uri.LocalPath) != ".ipa") {
					return;
				}
				var filename = Path.GetFileName(uri.LocalPath);
				targetFile = $"./IPADownload/{filename}";
				if (File.Exists(targetFile)) {
					InstallFile(targetFile);
					return;
				}

				var req = WebRequest.Create(uri);
				var response = await req.GetResponseAsync();
				var fileSize = response.ContentLength;

				if (!Directory.Exists("./IPADownload")) {
					Directory.CreateDirectory("./IPADownload");
				}
				txtLog.Text += $"Downloading IPA : {Path.GetFileName(filename)}\n";

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
