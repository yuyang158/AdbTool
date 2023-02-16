using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for LogView.xaml
	/// </summary>
	public partial class LogView : UserControl, ISubControlPanel {
		private IDevice m_device;

		public LogView() {
			InitializeComponent();
			cbUsingNetwork.Checked += CbUsingNetwork_Checked;
			cbUsingNetwork.Unchecked += CbUsingNetwork_Checked;
		}

		private void CbUsingNetwork_Checked(object sender, RoutedEventArgs e) {
			ifInput.Visibility = cbUsingNetwork.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
		}

		public void AssignDevice(IDevice device) {
			m_device = device;
		}

		public string GetName() {
			return "Log View";
		}

		private string PullFileAndReadToText(string remoteFilePath) {
			try {
				using var stream = m_device.Pull(remoteFilePath);
				using var reader = new StreamReader(stream);
				return reader.ReadToEnd();
			}
			catch (System.Exception e) {
				MessageBox.Show(e.Message, "ERROR");
			}
			return "";
		}

		private void PullErrorLog_Click(object sender, RoutedEventArgs e) {
			if (cbUsingNetwork.IsChecked == true) {
				txtLog.Text = FetchLogByUDP("error.log");
			}
			else {
				txtLog.Text = PullFileAndReadToText("error.log");
			}
		}

		private string FetchLogByUDP(string type) {
			UdpClient udpClient = new(16688);
			udpClient.Connect(ifInput.IpAddress, 36677);
			var cmdBuffer = Encoding.UTF8.GetBytes(type);
			udpClient.Send(cmdBuffer);

			var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
			var receiveBuffer = udpClient.Receive(ref ipEndPoint);
			var length = BitConverter.ToInt32(receiveBuffer, 0);
			var stream = new MemoryStream(length);
			if (receiveBuffer.Length > 4) {
				stream.Write(receiveBuffer, 4, length - 4);
			}
			while (length > stream.Length) {
				var endPoint = new IPEndPoint(IPAddress.Any, 0);
				receiveBuffer = udpClient.Receive(ref endPoint);
				stream.Write(receiveBuffer, 0, receiveBuffer.Length);
			}
			udpClient.Close();

			return Encoding.UTF8.GetString(stream.GetBuffer());
		}

		private void PullLastErrorLog_Click(object sender, RoutedEventArgs e) {
			if (cbUsingNetwork.IsChecked == true) {
				txtLog.Text = FetchLogByUDP("last-error.log");
			}
			else {
				txtLog.Text = PullFileAndReadToText("last-error.log");
			}
		}

		private void PullStatLog_Click(object sender, RoutedEventArgs e) {
			txtLog.Text = PullFileAndReadToText("stat.log");
		}

		public void Active() {
		}
	}
}
