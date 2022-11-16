using SharpAdbClient;
using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace AdbGUIClient {
	public class GlobalData : DependencyObject {
		private const string CONFIG_PATH = "./save.xml";
		private static GlobalData m_instance;
		public static GlobalData Instance {
			get {
				if(m_instance == null) {
					if (File.Exists(CONFIG_PATH)) {
						using var reader = new StreamReader(CONFIG_PATH);
						XmlSerializer sl = new XmlSerializer(typeof(GlobalData));
						try {
							m_instance = sl.Deserialize(reader) as GlobalData;
						}
						catch (Exception) {
							m_instance = new GlobalData();
						}
					}
					else {
						m_instance = new GlobalData();
					}
				}
				return m_instance;
			}
		}

		public string AndroidPackageName {
			get { return (string)GetValue(AndroidPackageNameProperty); }
			set { SetValue(AndroidPackageNameProperty, value); }
		}

		// Using a DependencyProperty as the backing store for AndroidPackageName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AndroidPackageNameProperty =
			DependencyProperty.Register("AndroidPackageName", typeof(string), typeof(GlobalData), new PropertyMetadata(""));


		public string IOSBundleID {
			get { return (string)GetValue(IOSBundleIDProperty); }
			set { SetValue(IOSBundleIDProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IOSBundleID.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IOSBundleIDProperty =
			DependencyProperty.Register("IOSBundleID", typeof(string), typeof(GlobalData), new PropertyMetadata(""));


		public string LuaRootPath {
			get { return (string)GetValue(LuaRootPathProperty); }
			set { SetValue(LuaRootPathProperty, value); }
		}

		// Using a DependencyProperty as the backing store for LuaRootPath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LuaRootPathProperty =
			DependencyProperty.Register("LuaRootPath", typeof(string), typeof(GlobalData), new PropertyMetadata(""));



		public string DownloadUrl {
			get { return (string)GetValue(DownloadUrlProperty); }
			set { SetValue(DownloadUrlProperty, value); }
		}

		// Using a DependencyProperty as the backing store for DownloadUrl.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DownloadUrlProperty =
			DependencyProperty.Register("DownloadUrl", typeof(string), typeof(GlobalData), new PropertyMetadata(""));

		public string AdbPath {
			get { return (string)GetValue(AdbPathProperty); }
			set {
				SetValue(AdbPathProperty, value);
				AdbServer server = new AdbServer();
				if (!server.GetStatus().IsRunning && !string.IsNullOrEmpty(AdbPath) && File.Exists(AdbPath)) {
					server.StartServer(AdbPath, false);
				}
			}
		}

		// Using a DependencyProperty as the backing store for AdbPath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AdbPathProperty =
			DependencyProperty.Register("AdbPath", typeof(string), typeof(GlobalData), new PropertyMetadata(""));


		[XmlIgnore]
		public IDevice[] Devices {
			get { return (IDevice[])GetValue(DevicesProperty); }
			set { SetValue(DevicesProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Devices.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty DevicesProperty =
			DependencyProperty.Register("Devices", typeof(IDevice[]), typeof(GlobalData), new PropertyMetadata(null));


		[XmlIgnore]
		public IDevice SelectedDevice {
			get { return (IDevice)GetValue(SelectedDeviceProperty); }
			set { SetValue(SelectedDeviceProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SelectedDevice.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectedDeviceProperty =
			DependencyProperty.Register("SelectedDevice", typeof(IDevice), typeof(GlobalData), new PropertyMetadata(null));

		[XmlIgnore]
		public bool UsingMTP { get; set; } = true;

		[XmlIgnore]
		public string AndroidMTPId { get; set; }

		public void Save() {
			XmlSerializer sl = new XmlSerializer(typeof(GlobalData));
			using (var stream = new StreamWriter(CONFIG_PATH)) {
				sl.Serialize(stream, Instance);
			}
		}
	}
}
