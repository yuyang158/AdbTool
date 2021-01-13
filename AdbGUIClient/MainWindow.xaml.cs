using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private AppData m_data;
		private readonly Type[] m_subControlTypes = new[] {
			typeof(BasicInfo),
			typeof(CpuInfo),
			typeof(RemoteApkInstall),
			typeof(LuaUpload)
		};

		private const string CONFIG_PATH = "./save.xml";

		public MainWindow() {
			InitializeComponent();
			if (File.Exists(CONFIG_PATH)) {
				using (var reader = new StreamReader(CONFIG_PATH)) {
					XmlSerializer sl = new XmlSerializer(typeof(AppData));
					m_data = sl.Deserialize(reader) as AppData;
				}
			}
			else {
				m_data = new AppData();
			}

			foreach (var type in m_subControlTypes) {
				var panel = Activator.CreateInstance(type) as ISubControlPanel;
				panel.AssignAppData(m_data);

				var tabItem = new TabItem {
					Header = panel.GetName(),
					Content = panel
				};

				tcControlContainer.Items.Add(tabItem);
			}

			DataContext = m_data;
			Closing += MainWindow_Closing;
			m_data.TriggerDeviceChanged();
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			XmlSerializer sl = new XmlSerializer(typeof(AppData));
			using (var stream = new StreamWriter(CONFIG_PATH)) {
				sl.Serialize(stream, m_data);
			}
			m_data.Exit();
		}

		private void SelectAdb_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Android Debug Bridge|*exe";
			if (dlg.ShowDialog() == true) {
				m_data.AdbPath = dlg.FileName;
			}
		}

		private void tcControlContainer_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}
	}
}
