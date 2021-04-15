using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AdbGUIClient {
	public partial class MainWindow : Window {
		private readonly Type[] m_subControlTypes = new[] {
			typeof(BasicInfo),
			typeof(CpuInfo),
			typeof(RemoteApkInstall),
			typeof(LuaUpload),
			typeof(LogView),
			typeof(SystemSettingView)
		};

		private const string CONFIG_PATH = "./save.xml";
		private readonly ISubControlPanel[] m_subPanels;

		private ISubControlPanel CreateSubPanel(Type type) {
			var panel = Activator.CreateInstance(type) as ISubControlPanel;
			var tabItem = new TabItem {
				Header = panel.GetName(),
				Content = panel
			};
			tcControlContainer.Items.Add(tabItem);
			return panel;
		}

		public MainWindow() {
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			InitializeComponent();
			if (File.Exists(CONFIG_PATH)) {
				using var reader = new StreamReader(CONFIG_PATH);
				XmlSerializer sl = new XmlSerializer(typeof(GlobalData));
				try {
					GlobalData.Instance = sl.Deserialize(reader) as GlobalData;
				}
				catch (Exception) {
					GlobalData.Instance = new GlobalData();
				}
			}
			else {
				GlobalData.Instance = new GlobalData();
			}

			m_subPanels = new ISubControlPanel[m_subControlTypes.Length];
			for (int i = 0; i < m_subControlTypes.Length; i++) {
				var type = m_subControlTypes[i];
				m_subPanels[i] = CreateSubPanel(type);
			}

			DataContext = GlobalData.Instance;
			Closing += MainWindow_Closing;
			cbbDevice.SelectionChanged += CbbDevice_SelectionChanged;

			RefreshDevice();
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			MessageBox.Show($"Exception : {e.ExceptionObject}");
		}

		private void CbbDevice_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var device = cbbDevice.SelectedItem as IDevice;
			foreach (var sub in m_subPanels) {
				sub.AssignDevice(device);
			}

			if (device is AndroidDevice) {
				grdAndroid.Visibility = Visibility.Visible;
				grdIOS.Visibility = Visibility.Collapsed;
			}
			else if (device is IOSDevice) {
				grdAndroid.Visibility = Visibility.Collapsed;
				grdIOS.Visibility = Visibility.Visible;
			}
			else {
				grdIOS.Visibility = Visibility.Collapsed;
				grdAndroid.Visibility = Visibility.Collapsed;
			}
		}

		public static T[] Concatenate<T>(T[] first, T[] second) {
			if (first == null) {
				return second;
			}
			if (second == null) {
				return first;
			}

			return first.Concat(second).ToArray();
		}

		private void RefreshDevice() {
			if (string.IsNullOrEmpty(GlobalData.Instance.AdbPath)) {
				return;
			}
			GlobalData.Instance.Devices = AndroidDevice.CollectAndroidDevices();
			GlobalData.Instance.Devices = Concatenate(GlobalData.Instance.Devices, IOSDevice.CollectIOSDevices());

			if (GlobalData.Instance.Devices.Length > 0) {
				GlobalData.Instance.SelectedDevice = GlobalData.Instance.Devices[0];
			}
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			XmlSerializer sl = new XmlSerializer(typeof(GlobalData));
			using (var stream = new StreamWriter(CONFIG_PATH)) {
				sl.Serialize(stream, GlobalData.Instance);
			}
		}

		private void SelectAdb_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog {
				Filter = "Android Debug Bridge|*exe"
			};
			if (dlg.ShowDialog() == true) {
				GlobalData.Instance.AdbPath = dlg.FileName;
			}
		}

		private void ApplyForward_Click(object sender, RoutedEventArgs e) {
			var androidDevice = GlobalData.Instance.SelectedDevice as AndroidDevice;
			androidDevice.Forward(int.Parse(txtPort.Text));
		}

		private void Refresh_Click(object sender, RoutedEventArgs e) {
			RefreshDevice();
		}

		private void Capture_Click(object sender, RoutedEventArgs e) {
			var path = GlobalData.Instance.SelectedDevice.TakeScreenShot();
			var preview = new ImagePreviewWindow(path);
			preview.Show();
		}

		private void tcControlContainer_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var index = tcControlContainer.SelectedIndex;
			var panel = m_subPanels[index];
			panel.Active();
		}
	}
}
