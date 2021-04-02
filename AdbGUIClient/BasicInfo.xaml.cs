using System.Text;
using System.Windows.Controls;

namespace AdbGUIClient {
	public partial class BasicInfo : UserControl, ISubControlPanel {
		public BasicInfo() {
			InitializeComponent();
		}

		private IDevice m_data;
		private StringBuilder m_deviceDetailBuilder;

		public void AssignDevice(IDevice data) {
			txtDeviceDetail.Text = data != null ? data.DeviceInfo : "";
		}

		public string GetName() {
			return "基础信息";
		}
	}
}
