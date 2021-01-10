using SharpAdbClient;

namespace AdbGUIClient {
	public class Device {
		private readonly DeviceData m_deviceData;

		public string DisplayName { get => m_deviceData.Name; }

		public string Serial { get => m_deviceData.Serial; }

		public DeviceState State { get => m_deviceData.State; }

		public Device(DeviceData deviceData) {
			m_deviceData = deviceData;
		}

		public override string ToString() {
			return $"{DisplayName}-{Serial}";
		}
	}
}
