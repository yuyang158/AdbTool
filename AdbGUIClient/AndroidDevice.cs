using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace AdbGUIClient {
	public class AndroidDevice : IDevice {
		private readonly AdbClient m_client;
		private readonly DeviceData m_deviceData;

		public static IDevice[] CollectAndroidDevices() {
			var client = new AdbClient();
			var devices = client.GetDevices();
			var deviceProxies = new IDevice[devices.Count];
			for (int i = 0; i < devices.Count; i++) {
				deviceProxies[i] = new AndroidDevice(client, devices[i]);
			}

			return deviceProxies;
		}

		public AndroidDevice(AdbClient client, DeviceData deviceData) {
			m_client = client;
			m_deviceData = deviceData;
		}

		private class ShellExcuteReceiver : IShellOutputReceiver {
			private readonly string m_title;
			private readonly StringBuilder m_builder;
			public ShellExcuteReceiver(string title, StringBuilder builder) {
				m_title = title;
				m_builder = builder;
				if (m_title.Length > 0) {
					m_builder.Append(m_title + " : ");
				}
			}

			public bool ParsesErrors => throw new NotImplementedException();

			public void AddOutput(string line) {
				m_builder.AppendLine(line);
			}

			public void Flush() {
			}
		}

		public string DeviceInfo {
			get {
				var builder = new StringBuilder(1024);
				builder.AppendLine($"Modal : {m_deviceData.Model}");
				builder.AppendLine($"Name : {m_deviceData.Name}");
				builder.AppendLine($"Features : {m_deviceData.Features}");
				builder.AppendLine($"Product : {m_deviceData.Product}");

				m_client.ExecuteRemoteCommand("wm size", m_deviceData, new ShellExcuteReceiver("Screen", builder));
				m_client.ExecuteRemoteCommand("wm density", m_deviceData, new ShellExcuteReceiver("Density", builder));
				m_client.ExecuteRemoteCommand("getprop ro.build.version.release", m_deviceData, new ShellExcuteReceiver("OS", builder));
				m_client.ExecuteRemoteCommand("ifconfig | grep Mask", m_deviceData, new ShellExcuteReceiver("Network", builder));
				m_client.ExecuteRemoteCommand("getprop dalvik.vm.heapsize", m_deviceData, new ShellExcuteReceiver("Heap Size", builder));

				return builder.ToString();
			}
		}

		public string CpuInfo {
			get {
				var builder = new StringBuilder();
				m_client.ExecuteRemoteCommand("cat /proc/cpuinfo", m_deviceData, new ShellExcuteReceiver("", builder));
				return builder.ToString();
			}
		}

		private class ShellResponseToLog : IShellOutputReceiver {
			public bool ParsesErrors => false;

			private readonly Action<string> m_callback;
			public ShellResponseToLog(Action<string> callback) {
				m_callback = callback;
			}

			public void AddOutput(string line) {
				m_callback.Invoke(line);
			}

			public void Flush() {
			}
		}

		public void InstallPackage(string localFilePath, Action<string> callback) {
			try {
				PackageManager manager = new PackageManager(m_client, m_deviceData);
				manager.InstallPackage(localFilePath, true);
				m_client.ExecuteRemoteCommandAsync($"am start -n {GlobalData.Instance.AndroidPackageName}/com.unity3d.player.UnityPlayerActivity",
					m_deviceData, new ShellResponseToLog(callback), CancellationToken.None);
			}
			catch (Exception ex) {
				callback.Invoke($"Install fail : {ex.Message}");
			}
		}

		public FileStream Pull(string remotePath) {
			File.Delete("./pull.log");
			remotePath = $"/sdcard/Android/data/{GlobalData.Instance.AndroidPackageName}/files/{remotePath}";
			var service = new SyncService(m_client, m_deviceData);
			var stream = new FileStream("./pull.log", FileMode.Create);
			service.Pull(remotePath, stream, null, CancellationToken.None);
			service.Dispose();
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public void Push(string localSourceFile, string remotePath) {
			remotePath = $"/sdcard/Android/data/{GlobalData.Instance.AndroidPackageName}/files/Lua/{remotePath}";
			using var stream = new FileStream(localSourceFile, FileMode.Open);
			var m_syncService = new SyncService(m_client, m_deviceData);
			m_syncService.Push(stream, remotePath, 666, DateTime.Now, null, CancellationToken.None);
			m_syncService.Dispose();
		}

		public void Delete(string remotePath) {
			m_client.ExecuteRemoteCommand($"rm -rf /sdcard/Android/data/{GlobalData.Instance.AndroidPackageName}/files/{remotePath}", m_deviceData, null);
		}

		public void TakeScreenShot() {
		}

		public void Forward(int port) {
			var localSpec = new ForwardSpec {
				Protocol = ForwardProtocol.Tcp,
				Port = port
			};

			var remoteSpec = new ForwardSpec {
				Protocol = ForwardProtocol.LocalAbstract,
				SocketName = "Unity-" + GlobalData.Instance.AndroidPackageName
			};

			port = m_client.CreateForward(m_deviceData, localSpec, remoteSpec, true);
			if (port == 0) {
				return;
			}
			MessageBox.Show($"Forward : {port}");
		}

		public override string ToString() {
			return m_deviceData.Name;
		}
	}
}
