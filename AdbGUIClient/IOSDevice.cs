using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

		public string CpuInfo => "";

		private static string RunCmd(string cmdParam) {
			ProcessStartInfo start = new ProcessStartInfo {
				FileName = "tidevice",
				Arguments = cmdParam,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			using (Process process = Process.Start(start))
			using (StreamReader reader = process.StandardOutput) {
				string result = reader.ReadToEnd();
				return result;
			}
		}

		private static async Task RunCmdAsync(string cmdParam, Action<string> callback) {
			ProcessStartInfo start = new ProcessStartInfo {
				FileName = "tidevice",
				Arguments = cmdParam,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			using (Process process = Process.Start(start))
			using (StreamReader reader = process.StandardOutput) {
				while (reader.EndOfStream == false) {
					var line = await reader.ReadLineAsync();
					callback(line);
				}
			}
		}


		public static IDevice[] CollectIOSDevices() {
			var json = RunCmd("list --json");
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

		private readonly string m_deviceUdid;
		private readonly string m_name;

		public IOSDevice(string udid, string name) {
			m_deviceUdid = udid;
			m_name = name;
		}

		public void Delete(string remotePath) {
			throw new NotImplementedException();
		}

		public async void InstallPackage(string localFilePath, Action<string> callback) {
			await RunCmdAsync($"--udid {m_deviceUdid} install {localFilePath}", callback);
			RunCmd($"--udid {m_deviceUdid} launch {GlobalData.Instance.IOSBundleID}");
		}

		public FileStream Pull(string remotePath) {
			RunCmd("fsync");
		}

		public void Push(string localSourceFile, string remotePath) {
			throw new NotImplementedException();
		}

		public void TakeScreenShot() {
			throw new NotImplementedException();
		}

		public override string ToString() {
			return m_name;
		}

		public void Dispose() {
		}
	}
}
