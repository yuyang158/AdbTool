namespace AdbGUIClient {
	public interface ISubControlPanel {
		void AssignDevice(IDevice device);
		string GetName();

		void Active();
	}
}
