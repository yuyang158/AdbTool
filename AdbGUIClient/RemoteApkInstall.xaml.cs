using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdbGUIClient {
	public partial class RemoteApkInstall : UserControl, ISubControlPanel {
		private const int DOWNLOAD_BUFFER_SIZE = 20240;
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

			var extension = Path.GetExtension(drop[0]);
			if (extension != ".apk" && extension != ".ipa") {
				return;
			}
			GlobalData.Instance.DownloadUrl = drop[0];
		}

		public string GetName() {
			return "Install";
		}

		private IDevice m_device;
		public void AssignDevice(IDevice device) {
			m_device = device;
		}

		private void InstallFile(string filename) {
			txtLog.Text += ($"Install : {Path.GetFileName(filename)} To {m_device}\n");
			m_device.InstallPackage(filename, line => {
				txtLog.Dispatcher.Invoke(() => {
					txtLog.Text += line;
				});
			});
		}

		private const string DownloadDirectoryName = "PackageDownload";

		private async void btnInstall_ClickAsync(object sender, RoutedEventArgs e) {
			string targetFile;
			if (GlobalData.Instance.DownloadUrl.StartsWith("http") || GlobalData.Instance.DownloadUrl.StartsWith("https")) {
				var uri = new Uri(GlobalData.Instance.DownloadUrl);
				var filename = Path.GetFileName(uri.LocalPath);
				targetFile = $"./${DownloadDirectoryName}/{filename}";
				if (File.Exists(targetFile)) {
					InstallFile(targetFile);
					return;
				}

				var req = WebRequest.Create(uri);
				var response = await req.GetResponseAsync();
				var fileSize = response.ContentLength;

				if (!Directory.Exists(DownloadDirectoryName)) {
					Directory.CreateDirectory(DownloadDirectoryName);
				}
				txtLog.Text += $"Downloading : {Path.GetFileName(filename)}\n";

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
					await Task.Delay(10);
				}

				spProgress.Visibility = Visibility.Collapsed;
			}
			else {
				if (File.Exists(GlobalData.Instance.DownloadUrl)) {
					targetFile = GlobalData.Instance.DownloadUrl;
				}
				else {
					txtLog.Text += $"URL is not a load address or web address : {GlobalData.Instance.DownloadUrl}\n";
					return;
				}
			}

			InstallFile(targetFile);
		}
	}
}
