using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AdbGUIClient {
	public partial class IOSWindow : Window {
		private const string CONFIG_PATH = "./save_ios.xml";

		[Serializable]
		public class SaveData : DependencyObject {
			public class Device {
				public string UUID;
				public string Name;

				public override string ToString() {
					return Name;
				}
			}

			public string DownloadUrl {
				get { return (string)GetValue(DownloadUrlProperty); }
				set { SetValue(DownloadUrlProperty, value); }
			}

			// Using a DependencyProperty as the backing store for DownloadUrl.  This enables animation, styling, binding, etc...
			public static readonly DependencyProperty DownloadUrlProperty =
				DependencyProperty.Register("DownloadUrl", typeof(string), typeof(SaveData), new PropertyMetadata(""));

			public string BundleID {
				get { return (string)GetValue(BundleIDProperty); }
				set { SetValue(BundleIDProperty, value); }
			}

			// Using a DependencyProperty as the backing store for BundleID.  This enables animation, styling, binding, etc...
			public static readonly DependencyProperty BundleIDProperty =
				DependencyProperty.Register("BundleID", typeof(string), typeof(SaveData), new PropertyMetadata(""));


			[XmlIgnore]
			public Device[] Devices {
				get { return (Device[])GetValue(DevicesProperty); }
				set { SetValue(DevicesProperty, value); }
			}

			// Using a DependencyProperty as the backing store for Devices.  This enables animation, styling, binding, etc...
			public static readonly DependencyProperty DevicesProperty =
				DependencyProperty.Register("Devices", typeof(Device[]), typeof(SaveData), new PropertyMetadata(null));

			[XmlIgnore]
			public Device SelectedDevice {
				get { return (Device)GetValue(SelectedDeviceProperty); }
				set { SetValue(SelectedDeviceProperty, value); }
			}

			// Using a DependencyProperty as the backing store for SelectedDevice.  This enables animation, styling, binding, etc...
			public static readonly DependencyProperty SelectedDeviceProperty =
				DependencyProperty.Register("SelectedDevice", typeof(Device), typeof(SaveData), new PropertyMetadata(null));
		}

		private readonly SaveData m_data;

		public IOSWindow() {
			InitializeComponent();
			Closing += IOSWindow_Closing;
			if (File.Exists(CONFIG_PATH)) {
				using var reader = new StreamReader(CONFIG_PATH);
				XmlSerializer sl = new XmlSerializer(typeof(SaveData));
				m_data = sl.Deserialize(reader) as SaveData;
			}
			else {
				m_data = new SaveData();
			}

			var panel = new RemoteIPAInstall();
			var tabItem = new TabItem {
				Header = panel.GetName(),
				Content = panel
			};
			panel.AssignAppData(m_data);
			tcContainer.Items.Add(tabItem);
			RefreshDevices();
		}

		private void RefreshDevices() {
			var proc = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "tidevice",
					Arguments = "list --json",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			proc.Start();
			var json = string.Empty;
			while (!proc.StandardOutput.EndOfStream) {
				string line = proc.StandardOutput.ReadLine();
				json += line;
			}
			JArray devices = JArray.Parse(json);
			m_data.Devices = new SaveData.Device[devices.Count];
			for (int i = 0; i < devices.Count; i++) {
				var deviceData = devices[i];
				m_data.Devices[i] = new SaveData.Device() {
					Name = deviceData["name"].ToString(),
					UUID = deviceData["udid"].ToString()
				};
			}
			if (devices.Count > 0) {
				m_data.SelectedDevice = m_data.Devices[0];
			}
			DataContext = m_data;
		}

		private void IOSWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			XmlSerializer sl = new XmlSerializer(typeof(SaveData));
			using (var stream = new StreamWriter(CONFIG_PATH)) {
				sl.Serialize(stream, m_data);
			}
		}

		private void btnRefresh_Click(object sender, RoutedEventArgs e) {
			RefreshDevices();
		}
	}
}
