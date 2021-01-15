using SharpAdbClient;
using System;
using System.Text;
using System.Windows.Controls;

namespace AdbGUIClient {
	public class InfoReceiver : IShellOutputReceiver {
		public bool ParsesErrors => false;
		private readonly StringBuilder m_builder = new StringBuilder();

		public event Action<string> Callback;

		public InfoReceiver() {
		}

		public void AddOutput(string line) {
			m_builder.AppendLine(line);
		}

		public void Flush() {
			Callback?.Invoke(m_builder.ToString());
		}
	}
}
