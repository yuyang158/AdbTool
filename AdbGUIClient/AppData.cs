using SharpAdbClient;
using System.Linq;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Threading;
using System;
using System.Xml.Serialization;

namespace AdbGUIClient {
	[Serializable]
	public class AppData : DependencyObject {
		private AdbClient m_client;
		public AdbClient CurrentClient => m_client;

		public event Action<Device> SelectionChanged;
		public string AdbPath {
			get { return (string)GetValue(AdbPathProperty); }
			set {
				SetValue(AdbPathProperty, value);
				AdbServer server = new AdbServer();
				if (!server.GetStatus().IsRunning) {
					server.StartServer(value, false);
				}

				m_client = new AdbClient();
				StartMonitor();
				foreach (var deviceData in m_client.GetDevices()) {
					Devices.Add(new Device(deviceData));
				}

				if (Devices.Count > 0) {
					SelectedDevice = Devices[0];
				}
			}
		}

		public string LuaRootPath { get; set; }

		public string PackageName {
			get { return (string)GetValue(PackageNameProperty); }
			set { SetValue(PackageNameProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PackageName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PackageNameProperty =
			DependencyProperty.Register("PackageName", typeof(string), typeof(AppData), new PropertyMetadata(""));



		private Thread m_monitorThread;
		private DeviceMonitor m_monitor;

		private void StartMonitor() {
			m_monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
			m_monitor.DeviceConnected += this.OnDeviceConnected;
			m_monitor.DeviceDisconnected += DeviceDisconnected;
			m_monitorThread = new Thread(m_monitor.Start);
			m_monitorThread.Start();
			// monitor.Start();
		}

		private void DeviceDisconnected(object sender, DeviceDataEventArgs e) {
			Dispatcher.Invoke(() => {
				Devices.Remove(Devices.First(device => {
					return device.Serial == e.Device.Serial;
				}));
			});
		}

		private void OnDeviceConnected(object sender, DeviceDataEventArgs e) {
			Dispatcher.Invoke(() => {
				if (Devices.First(device => device.Serial == e.Device.Serial) != null) {
					return;
				}

				var device = new Device(e.Device);
				Devices.Add(device);
				if (SelectedDevice == null) {
					SelectedDevice = device;
				}
			});
		}

		// Using a DependencyProperty as the backing store for AdbPath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AdbPathProperty =
			DependencyProperty.Register("AdbPath", typeof(string), typeof(AppData), new PropertyMetadata(""));

		[XmlIgnore]
		[field: NonSerialized]
		public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

		[XmlIgnore]
		public DeviceData SelectedDeviceData { get; private set; }

		[XmlIgnore]
		public Device SelectedDevice {
			get { return (Device)GetValue(SelectedDeviceProperty); }
			set { 
				SetValue(SelectedDeviceProperty, value);
				SelectionChanged?.Invoke(SelectedDevice);
				SelectedDeviceData = value.Data;
			}
		}

		// Using a DependencyProperty as the backing store for SelectedDevice.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectedDeviceProperty =
			DependencyProperty.Register("SelectedDevice", typeof(Device), typeof(AppData), new PropertyMetadata(null));

		public void Exit() {
			if (m_monitor == null)
				return;
			m_monitor.Dispose();
		}

		public void TriggerDeviceChanged() {
			SelectionChanged?.Invoke(SelectedDevice);
		}
	}
}
