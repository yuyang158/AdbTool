using SharpAdbClient;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;

namespace AdbGUIClient {
	public class AppData : DependencyObject {
		private AdbClient m_client;
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
			}
		}

		private void StartMonitor() {
			var monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
			monitor.DeviceConnected += this.OnDeviceConnected;
			monitor.DeviceDisconnected += DeviceDisconnected;
			// monitor.Start();
		}

		private void DeviceDisconnected(object sender, DeviceDataEventArgs e) {
			Devices.Remove(Devices.First(device => {
				return device.Serial == e.Device.Serial;
			}));
		}

		private void OnDeviceConnected(object sender, DeviceDataEventArgs e) {
			Devices.Add(new Device(e.Device));
		}

		// Using a DependencyProperty as the backing store for AdbPath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AdbPathProperty =
			DependencyProperty.Register("AdbPath", typeof(string), typeof(AppData), new PropertyMetadata(""));

		public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

		private Device m_selectedDevice;
		public Device SelectedDevice {
			get => m_selectedDevice;
			set {
				m_selectedDevice = value;

			}
		}
	}
}
