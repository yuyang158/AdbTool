using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for ImagePreviewWindow.xaml
	/// </summary>
	public partial class ImagePreviewWindow : Window {
		public ImagePreviewWindow(Image sourceImg, string modal) {
			InitializeComponent();
			Width = sourceImg.Size.Width;
			Height = sourceImg.Size.Height;
			if(!Directory.Exists("Capture")) {
				Directory.CreateDirectory("Capture");
			}

			var now = DateTime.Now;
			var filename = $"./Capture/{modal}_{now.Year}-{now.Month}-{now.Day} {now.Hour}-{now.Minute}-{now.Second}.png";
			using (var stream = new FileStream(filename, FileMode.OpenOrCreate)) {
				sourceImg.Save(stream, ImageFormat.Png);
			}
			var bitmap = new BitmapImage(new Uri(Path.GetFullPath(filename)));
			img.Source = bitmap;
		}
	}
}
