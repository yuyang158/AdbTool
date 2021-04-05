using System.Windows.Controls;

namespace AdbGUIClient {
	public partial class BasicInfo : UserControl, ISubControlPanel {
		public BasicInfo() {
			InitializeComponent();
		}

		public void AssignDevice(IDevice device) {
			txtDeviceDetail.Text = device != null ? device.DeviceInfo : "";
		}

		public string GetName() {
			return "基础信息";
		}
	}
}
