using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TreeViewFileExplorer.ShellClasses;

namespace AdbGUIClient {
	public partial class LuaUpload : UserControl, ISubControlPanel {
		public LuaUpload() {
			InitializeComponent();
		}

		private IDevice m_device;
		public void AssignDevice(IDevice device) {
			BuildRootPath();
			m_device = device;
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
		}

		private List<Tuple<string, string>> uploadTasks = new List<Tuple<string, string>>();

		private void UploadFile(string localPath, string remotePath) {
			m_device.Push(localPath, remotePath);
		}

		private void UploadLua_Click(object sender, RoutedEventArgs e) {
			if (tvLua.SelectedItem == null)
				return;

			var root = tvLua.Items[0] as FileSystemObjectInfo;
			var rootFileName = root.FileSystemInfo.FullName;
			var rootLength = rootFileName.Length + 1;
			var info = tvLua.SelectedItem as FileSystemObjectInfo;
			var fullName = info.FileSystemInfo.FullName;
			var maskValue = (int)(info.FileSystemInfo.Attributes & FileAttributes.Directory);
			if (maskValue != 0) {
				var files = Directory.GetFiles(fullName, "*.lua", SearchOption.AllDirectories);
				foreach (var file in files) {
					var subPath = file[rootLength..].Replace('\\', '/');
					UploadFile(file, subPath);
				}
			}
			else {
				var subPath = fullName[rootLength..].Replace('\\', '/');
				UploadFile(fullName, subPath);
			}

			MessageBox.Show("Upload Success");
		}

		private void ClearRemoteFolder_Click(object sender, RoutedEventArgs e) {
			m_device.Delete("Lua");
		}
	}
}
