using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using TreeViewFileExplorer.ShellClasses;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for LuaUpload.xaml
	/// </summary>
	public partial class LuaUpload : UserControl, ISubControlPanel {
		private AppData m_data;
		public LuaUpload() {
			InitializeComponent();
		}

		public void AssignAppData(AppData data) {
			m_data = data;
			if (string.IsNullOrEmpty(m_data.LuaRootPath))
				return;
			txtLuaRoot.Text = m_data.LuaRootPath;
			BuildRootPath();
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
			treeView.Items.Clear();
			var fileSystemObject = new FileSystemObjectInfo(new DirectoryInfo(txtLuaRoot.Text));
			fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
			fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
			treeView.Items.Add(fileSystemObject);
			fileSystemObject.IsExpanded = true;
		}

		private void ApplyLuaRoot_Click(object sender, System.Windows.RoutedEventArgs e) {
			BuildRootPath();
			m_data.LuaRootPath = txtLuaRoot.Text;
		}
	}
}
