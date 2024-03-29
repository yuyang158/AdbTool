﻿using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AdbGUIClient {
	public class IOSDevice : IDevice {
		public string DeviceInfo {
			get {
				var builder = new StringBuilder(1024);
				builder.AppendLine($"Name : {m_name}");
				builder.AppendLine($"UDID : {m_deviceUdid}");

				return builder.ToString();
			}
		}

		public string CpuInfo => "IOS 无";

		private static string RunCmd(string cmdParam) {
			ProcessStartInfo start = new ProcessStartInfo {
				FileName = "tidevice",
				Arguments = cmdParam,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true
			};

			try {
				using (Process process = Process.Start(start))
				using (StreamReader reader = process.StandardOutput) {
					process.WaitForExit();
					if (process.ExitCode != 0) {
						MessageBox.Show("处理失败，请确认手机连接正确。打包时打开沙盒数据访问权限。");
						return "";
					}

					string result = reader.ReadToEnd();
					return result;
				}
			}
			catch (Exception) {
				return string.Empty;
			}
		}

		private static async Task RunCmdAsync(string cmdParam, Action<string> callback, bool showCmdWin = false) {
			ProcessStartInfo start = new ProcessStartInfo {
				FileName = "tidevice",
				Arguments = cmdParam,
				CreateNoWindow = !showCmdWin,
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			using (Process process = Process.Start(start))
			using (StreamReader reader = process.StandardOutput) {
				while (reader.EndOfStream == false) {
					var line = await reader.ReadLineAsync();
					callback(line + "\n");
				}
			}
		}


		public static IDevice[] CollectIOSDevices() {
			var json = RunCmd("list --json");
			if (string.IsNullOrEmpty(json)) {
				return Array.Empty<IDevice>();
			}
			try {
				var deviceDataArray = JArray.Parse(json);
				IDevice[] devices = new IDevice[deviceDataArray.Count];
				for (int i = 0; i < deviceDataArray.Count; i++) {
					var deviceData = deviceDataArray[i] as JObject;
					var udid = deviceData["udid"].ToString();
					var name = deviceData["name"].ToString();
					devices[i] = new IOSDevice(udid, name);
				}

				return devices;
			}
			catch (Exception) {
				return new IDevice[0];
			}
			
		}

		private readonly string m_deviceUdid;
		private readonly string m_name;

		public IOSDevice(string udid, string name) {
			m_deviceUdid = udid;
			m_name = name;
		}

		public void Delete(string remotePath) {
			if(string.IsNullOrEmpty(GlobalData.Instance.IOSBundleID)) {
				MessageBox.Show("App Bundle ID Is Empty");
				return;
			}
			RunCmd($"--udid {m_deviceUdid} fsync -B \"{GlobalData.Instance.IOSBundleID}\" rmtree \"/Documents/{remotePath}\"");
		}

		public async void InstallPackage(string localFilePath, Action<string> callback) {
			if (string.IsNullOrEmpty(GlobalData.Instance.IOSBundleID)) {
				MessageBox.Show("App Bundle ID Is Empty");
				return;
			}
			await RunCmdAsync($"--udid {m_deviceUdid} install \"{localFilePath}\"", callback, true);
			RunCmd($"--udid {m_deviceUdid} launch {GlobalData.Instance.IOSBundleID}");
		}

		public FileStream Pull(string remotePath) {
			if (string.IsNullOrEmpty(GlobalData.Instance.IOSBundleID)) {
				MessageBox.Show("App Bundle ID Is Empty");
				return null;
			}
			var result = RunCmd($"--udid {m_deviceUdid} fsync -B \"{GlobalData.Instance.IOSBundleID}\" pull \"/Documents/{remotePath}\"");
			if (string.IsNullOrEmpty(result)) {
				return null;
			}
			return new FileStream(remotePath, FileMode.Open);
		}

		public void Push(string localSourceFile, string remotePath) {
			if (string.IsNullOrEmpty(GlobalData.Instance.IOSBundleID)) {
				MessageBox.Show("App Bundle ID Is Empty");
				return;
			}
			localSourceFile = localSourceFile.Replace('\\', '/');
			remotePath = remotePath.Replace('\\', '/');

			var directories = remotePath.Split('/');
			const string baseDir = "/Documents";
			var path = baseDir;
			for (int i = 0; i < directories.Length - 1; i++) {
				var dirName = directories[i];
				path += $"/{dirName}";
				var stat = RunCmd($"--udid {m_deviceUdid} fsync -B \"{GlobalData.Instance.IOSBundleID}\" stat \"{path}\"");
				if (string.IsNullOrEmpty(stat)) {
					RunCmd($"--udid {m_deviceUdid} fsync -B \"{GlobalData.Instance.IOSBundleID}\" mkdir \"{path}\"");
				}
			}

			RunCmd($"--udid {m_deviceUdid} fsync -B \"{GlobalData.Instance.IOSBundleID}\" push \"{localSourceFile}\" \"/Documents/{remotePath}\"");
		}

		public string TakeScreenShot() {
			RunCmd($"--udid {m_deviceUdid} screenshot \"screenshot.png\"");
			return Path.GetFullPath("screenshot.png");
		}

		public override string ToString() {
			return m_name;
		}

		public void Dispose() {
		}

		public void PushPackage(string localFilePath) {
			throw new NotImplementedException();
		}
	}
}
