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

				CancellationToken token = new CancellationToken();
				m_data.CurrentClient.ExecuteRemoteCommandAsync("wm size", device.Data, new InfoReceiver(txtDeviceDetail, "Screen"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("wm density", device.Data, new InfoReceiver(txtDeviceDetail, "Density"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("getprop ro.build.version.release", device.Data, new InfoReceiver(txtDeviceDetail, "OS"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("ifconfig | grep Mask", device.Data, new InfoReceiver(txtDeviceDetail, "Network"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("getprop dalvik.vm.heapsize", device.Data, new InfoReceiver(txtDeviceDetail, "Heap Size"), token);


				txtDeviceDetail.Text = m_deviceDetailBuilder.ToString();
			}
		}

		public string GetName() {
			return "基础信息";
		}
	}
}
