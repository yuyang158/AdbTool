using Microsoft.Win32;
using SharpAdbClient;
using System;
using System.IO;
using System.Threading;
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
			typeof(LuaUpload),
			typeof(LogView)
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
			OpenFileDialog dlg = new OpenFileDialog {
				Filter = "Android Debug Bridge|*exe"
			};
			if (dlg.ShowDialog() == true) {
				m_data.AdbPath = dlg.FileName;
			}
		}

		private void ApplyForward_Click(object sender, RoutedEventArgs e) {
			var localSpec = new ForwardSpec {
				Protocol = ForwardProtocol.Tcp,
				Port = int.Parse(txtPort.Text)
			};

			var remoteSpec = new ForwardSpec {
				Protocol = ForwardProtocol.LocalAbstract,
				SocketName = "Unity-" + m_data.PackageName
			};

			var port = m_data.CurrentClient.CreateForward(m_data.SelectedDeviceData, localSpec, remoteSpec, true);
			if (port == 0) {
				return;
			}
			MessageBox.Show($"Forward : {port}");
		}

		private void Refresh_Click(object sender, RoutedEventArgs e) {
			m_data.Refresh();
		}

		private void Capture_Click(object sender, RoutedEventArgs e) {
			var task = m_data.CurrentClient.GetFrameBufferAsync(m_data.SelectedDevice.Data, CancellationToken.None);
			task.Wait();
			var preview = new ImagePreviewWindow(task.Result, m_data.SelectedDevice.DisplayName);
			preview.Show();
		}
	}
}
