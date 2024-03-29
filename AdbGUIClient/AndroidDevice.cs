﻿using MediaDevices;
using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

			if (!GlobalData.Instance.UsingMTP) {
				remotePath = $"/sdcard/Android/data/{GlobalData.Instance.AndroidPackageName}/files/{remotePath}";
				var service = new SyncService(m_client, m_deviceData);
				var stream = new FileStream("./pull.log", FileMode.Create);
				service.Pull(remotePath, stream, null, CancellationToken.None);
				service.Dispose();
				stream.Seek(0, SeekOrigin.Begin);
				service.Dispose();
				return stream;
			}
			else {
				if (string.IsNullOrEmpty(GlobalData.Instance.AndroidMTPId)) {
					return null;
				}

				MediaDevice selectDevice = SelectMTPDevice();
				if (selectDevice == null) {
					return null;
				}

				var stream = File.Open("./pull.txt", FileMode.OpenOrCreate);
				var deviceRoot = selectDevice.GetDrives()[0].RootDirectory.FullName;
				remotePath = $"{deviceRoot}/Android/data/{GlobalData.Instance.AndroidPackageName}/files/{remotePath}";
				selectDevice.DownloadFile(remotePath, stream);
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
		}

		private static MediaDevice SelectMTPDevice() {
			var devices = MediaDevice.GetDevices();
			foreach (var device in devices) {
				if (device.DeviceId == GlobalData.Instance.AndroidMTPId) {
					return device;
				}
			}

			return null;
		}

		private void MTPDirectoryCreate(MediaDevice device, string directory) {
			if(device.DirectoryExists(directory)) {
				return;
			}

			MTPDirectoryCreate(device, Path.GetDirectoryName(directory));
			device.CreateDirectory(directory);
		}

		public void Push(string localSourceFile, string remotePath) {
			if (!GlobalData.Instance.UsingMTP) {
				remotePath = $"/sdcard/Android/data/{GlobalData.Instance.AndroidPackageName}/files/{remotePath}";
				using var stream = new FileStream(localSourceFile, FileMode.Open);
				var m_syncService = new SyncService(m_client, m_deviceData);
				m_syncService.Push(stream, remotePath, 666, DateTime.Now, null, CancellationToken.None);
				m_syncService.Dispose();
			}
			else {
				MediaDevice selectDevice = SelectMTPDevice();
				if (selectDevice == null) {
					return;
				}
				var deviceRoot = selectDevice.GetDrives()[0].RootDirectory.FullName;
				remotePath = $"{deviceRoot}/Android/data/{GlobalData.Instance.AndroidPackageName}/files/{remotePath}";

				MTPDirectoryCreate(selectDevice, Path.GetDirectoryName(remotePath));
				if(selectDevice.FileExists(remotePath)) {
					selectDevice.DeleteFile(remotePath);
				}
				selectDevice.UploadFile(localSourceFile, remotePath);
			}
		}

		public void Delete(string remotePath) {
			m_client.ExecuteRemoteCommand($"rm -rf /sdcard/Android/data/{GlobalData.Instance.AndroidPackageName}/files/{remotePath}", m_deviceData, null);
		}

		public string TakeScreenShot() {
			var task = m_client.GetFrameBufferAsync(m_deviceData, CancellationToken.None);
			task.Wait();
			var image = task.Result;
			using (var stream = new FileStream("screenshot.png", FileMode.OpenOrCreate)) {
				image.Save(stream, ImageFormat.Png);
			}
			return Path.GetFullPath("screenshot.png");
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

		public void Dispose() {
		}

		public void PushPackage(string localSourceFile) {
			var filename = Path.GetFileName(localSourceFile);
			var remotePath = $"/sdcard/{filename}";
			using var stream = new FileStream(localSourceFile, FileMode.Open);
			var m_syncService = new SyncService(m_client, m_deviceData);
			m_syncService.Push(stream, remotePath, 666, DateTime.Now, null, CancellationToken.None);
			m_syncService.Dispose();
		}
	}
}
