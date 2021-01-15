using SharpAdbClient;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace AdbGUIClient {
	public partial class BasicInfo : UserControl, ISubControlPanel {
		public BasicInfo() {
			InitializeComponent();
		}

		private AppData m_data;
		private StringBuilder m_deviceDetailBuilder;

		public void AssignAppData(AppData data) {
			data.SelectionChanged += Data_SelectionChanged;
			m_data = data;
		}

		private InfoReceiver GenerateReceiver(string title) {
			var receiver = new InfoReceiver();
			receiver.Callback += text => {
				Dispatcher.Invoke(() => {
					txtDeviceDetail.AppendText($"{title} : {text}");
				});
			};

			return receiver;
		}

		private void Data_SelectionChanged(Device device) {
			if(device == null) {
				txtDeviceDetail.Text = "No device is selected.";
			}
			else {
				m_deviceDetailBuilder = new StringBuilder(1024);
				m_deviceDetailBuilder.AppendLine($"Modal : {device.Data.Model}");
				m_deviceDetailBuilder.AppendLine($"Name : {device.Data.Name}");
				m_deviceDetailBuilder.AppendLine($"Features : {device.Data.Features}");
				m_deviceDetailBuilder.AppendLine($"Product : {device.Data.Product}");
				txtDeviceDetail.Text = m_deviceDetailBuilder.ToString();

				CancellationToken token = new CancellationToken();
				m_data.CurrentClient.ExecuteRemoteCommandAsync("wm size", device.Data, GenerateReceiver("Screen"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("wm density", device.Data, GenerateReceiver("Density"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("getprop ro.build.version.release", device.Data, GenerateReceiver("OS"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("ifconfig | grep Mask", device.Data, GenerateReceiver("Network"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("getprop dalvik.vm.heapsize", device.Data, GenerateReceiver("Heap Size"), token);

			}
		}

		public string GetName() {
			return "基础信息";
		}
	}
}
