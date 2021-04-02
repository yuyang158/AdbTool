using System;
using System.IO;

namespace AdbGUIClient {
	public interface IDevice {
		string DeviceInfo { get; }

		string CpuInfo { get; }

		FileStream Pull(string remotePath);

		void Push(string localSourceFile, string remotePath);

		void InstallPackage(string localFilePath, Action<string> callback);

		void TakeScreenShot();

		void Delete(string remotePath);
	}
}
