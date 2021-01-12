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

		private class InfoReceiver : IShellOutputReceiver {
			public bool ParsesErrors => false;
			public StringBuilder Output = new StringBuilder();
			private BasicInfo m_panel;
			private string m_title;

			public InfoReceiver(BasicInfo panel, string title) {
				m_panel = panel;
				m_title = title;
			}

			public void AddOutput(string line) {
				Output.AppendLine(line);
			}

			public void Flush() {
				m_panel.Dispatcher.Invoke(() => {
					m_panel.m_deviceDetailBuilder.Append($"{m_title} : {Output}");
					m_panel.txtDeviceDetail.Text = m_panel.m_deviceDetailBuilder.ToString();
				});
			}
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
				m_data.CurrentClient.ExecuteRemoteCommandAsync("wm size", device.Data, new InfoReceiver(this, "Screen"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("wm density", device.Data, new InfoReceiver(this, "Density"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("getprop ro.build.version.release", device.Data, new InfoReceiver(this, "OS"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("ifconfig | grep Mask", device.Data, new InfoReceiver(this, "IP"), token);
				m_data.CurrentClient.ExecuteRemoteCommandAsync("cat /proc/cpuinfo", device.Data, new InfoReceiver(this, "CPU"), token);


				txtDeviceDetail.Text = m_deviceDetailBuilder.ToString();
			}
		}

		public string GetName() {
			return "基础信息";
		}
	}
}
