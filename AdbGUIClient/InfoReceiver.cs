using SharpAdbClient;
using System.Text;
using System.Windows.Controls;

namespace AdbGUIClient {
	public class InfoReceiver : IShellOutputReceiver {
		public bool ParsesErrors => false;
		public StringBuilder Output = new StringBuilder();
		private TextBox m_content;
		private string m_title;

		public InfoReceiver(TextBox content, string title) {
			m_content = content;
			m_title = title;
		}

		public void AddOutput(string line) {
			Output.AppendLine(line);
		}

		public void Flush() {
			m_content.Dispatcher.Invoke(() => {
				m_content.Text += $"{m_title} : \n{Output}";
			});
		}
	}
}
