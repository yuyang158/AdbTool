﻿using System.Threading;
using System.Windows.Controls;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for CpuInfo.xaml
	/// </summary>
	public partial class CpuInfo : UserControl, ISubControlPanel {
		private AppData m_data;

		public CpuInfo() {
			InitializeComponent();
		}

		public void AssignAppData(AppData data) {
			m_data = data;
			m_data.SelectionChanged += Device_SelectionChanged;
		}

		private void Device_SelectionChanged(Device device) {
			if (device == null)
				return;
			CancellationToken token = new CancellationToken();
			var receiver = new InfoReceiver();
			receiver.Callback += text => {
				Dispatcher.Invoke(() => {
					txtCpuDetail.Text = text;
				});
			};
			m_data.CurrentClient.ExecuteRemoteCommandAsync("cat /proc/cpuinfo", device.Data, receiver, token);
		}

		public string GetName() {
			return "Cpu Info";
		}
	}
}
