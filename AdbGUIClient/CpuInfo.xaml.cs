using System.Threading;
using System.Windows.Controls;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for CpuInfo.xaml
	/// </summary>
	public partial class CpuInfo : UserControl, ISubControlPanel {
		public CpuInfo() {
			InitializeComponent();
		}

		public void AssignDevice(IDevice data) {
			txtCpuDetail.Text = data == null ? "" : data.CpuInfo;
		}

		public string GetName() {
			return "Cpu Info";
		}
	}
}
