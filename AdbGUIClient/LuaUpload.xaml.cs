using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TreeViewFileExplorer.ShellClasses;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for LuaUpload.xaml
	/// </summary>
	public partial class LuaUpload : UserControl, ISubControlPanel {
		private AppData m_data;
		private Thread m_uploadThread;
		private Semaphore m_uploadWait = new Semaphore(0, 10000);
		private bool m_closing;

		public LuaUpload() {
			InitializeComponent();
			m_uploadThread = new Thread(Upload);
			m_uploadThread.Start();

			Loaded += LuaUpload_Loaded;
		}

		private void LuaUpload_Loaded(object sender, RoutedEventArgs e) {
			var main = Window.GetWindow(this);
			main.Closing += Main_Closing;
		}

		private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			m_closing = true;
			m_uploadWait.Release();
		}

		private int m_uploadTaskCount;
		private int m_totalTaskCount;

		private void Upload() {
			while (true) {
				m_uploadWait.WaitOne();
				if (m_closing)
					break;

				Tuple<string, string> task = null;
				lock (uploadTasks) {
					task = uploadTasks[0];
					uploadTasks.RemoveAt(0);
				}

				m_uploadTaskCount++;
				Dispatcher.Invoke(() => {
					if(m_uploadTaskCount >= m_totalTaskCount) {
						spProgress.Visibility = Visibility.Collapsed;
						MessageBox.Show("Upload Finished");
					}
					tbUploadItem.Text = $"{Path.GetFileName(task.Item1)} {m_uploadTaskCount} / {m_totalTaskCount}";
					pbUpload.Value = (double)m_uploadTaskCount / m_totalTaskCount;
				});

				using var stream = new FileStream(task.Item1, FileMode.Open);
				m_syncService = new SyncService(m_data.CurrentClient, m_data.SelectedDeviceData);
				m_syncService.Push(stream, task.Item2, 666, DateTime.Now, null, CancellationToken.None);
				m_syncService.Dispose();
			}
		}

		public void AssignAppData(AppData data) {
			m_data = data;
			m_data.SelectionChanged += SelectDevice_SelectionChanged;
			if (string.IsNullOrEmpty(m_data.LuaRootPath))
				return;
			txtLuaRoot.Text = m_data.LuaRootPath;
			BuildRootPath();
		}

		private SyncService m_syncService;

		private void SelectDevice_SelectionChanged(Device device) {

		}

		public string GetName() {
			return "Lua Upload";
		}


		private void FileSystemObject_AfterExplore(object sender, EventArgs e) {
			Cursor = Cursors.Arrow;
		}

		private void FileSystemObject_BeforeExplore(object sender, EventArgs e) {
			Cursor = Cursors.Wait;
		}

		private void BuildRootPath() {
			if (!Directory.Exists(txtLuaRoot.Text)) {
				return;
			}
			tvLua.Items.Clear();
			var fileSystemObject = new FileSystemObjectInfo(new DirectoryInfo(txtLuaRoot.Text));
			fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
			fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
			tvLua.Items.Add(fileSystemObject);
			fileSystemObject.IsExpanded = true;
		}

		private void ApplyLuaRoot_Click(object sender, RoutedEventArgs e) {
			BuildRootPath();
			m_data.LuaRootPath = txtLuaRoot.Text;
		}

		private List<Tuple<string, string>> uploadTasks = new List<Tuple<string, string>>();

		private void UploadFile(string localPath, string remotePath) {
			lock (uploadTasks) {
				uploadTasks.Add(new Tuple<string, string>(localPath, remotePath));
			}
			m_uploadWait.Release();
		}

		private void UploadLua_Click(object sender, RoutedEventArgs e) {
			if (m_data.SelectedDevice == null)
				return;

			if (tvLua.SelectedItem == null)
				return;

			var root = tvLua.Items[0] as FileSystemObjectInfo;
			var rootFileName = root.FileSystemInfo.FullName;
			var info = tvLua.SelectedItem as FileSystemObjectInfo;
			var fullName = info.FileSystemInfo.FullName;
			var socket = new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort));
			var remoteRoot = $"/sdcard/Android/data/{m_data.PackageName}/files/Lua";
			var maskValue = (int)(info.FileSystemInfo.Attributes & FileAttributes.Directory);
			if (maskValue != 0) {
				var files = Directory.GetFiles(fullName, "*.lua", SearchOption.AllDirectories);
				m_totalTaskCount = files.Length;
				foreach (var file in files) {
					var subPath = file[rootFileName.Length..].Replace('\\', '/');
					UploadFile(file, remoteRoot + subPath);
				}
			}
			else {
				m_totalTaskCount = 1;
				var subPath = fullName[rootFileName.Length..].Replace('\\', '/');
				UploadFile(fullName, remoteRoot + subPath);
			}
			m_uploadTaskCount = 0;
			spProgress.Visibility = Visibility.Visible;
		}

		private class CmdRecv : IShellOutputReceiver {
			public bool ParsesErrors => throw new NotImplementedException();

			public void AddOutput(string line) {
				Console.WriteLine(line);
			}

			public void Flush() {
				MessageBox.Show("Clear Success");
			}
		}

		private void ClearRemoteFolder_Click(object sender, RoutedEventArgs e) {
			if (m_data.SelectedDevice == null)
				return;

			m_data.CurrentClient.ExecuteRemoteCommandAsync($"rm -rf /sdcard/Android/data/{m_data.PackageName}/files/Lua",
				m_data.SelectedDeviceData, new CmdRecv(), CancellationToken.None);
		}
	}
}
