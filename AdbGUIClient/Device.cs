using SharpAdbClient;

namespace AdbGUIClient {
	public class Device {
		private readonly DeviceData m_deviceData;

		public string DisplayName => m_deviceData.Model;

		public string Serial => m_deviceData.Serial;

		public DeviceState State => m_deviceData.State;

		public DeviceData Data => m_deviceData;

		public Device(DeviceData deviceData) {
			m_deviceData = deviceData;
		}

		public override string ToString() {
			return $"{DisplayName}-{Serial}";
		}
	}
}
