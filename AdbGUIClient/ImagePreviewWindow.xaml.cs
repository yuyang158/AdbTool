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
		public ImagePreviewWindow(Image sourceImg) {
			InitializeComponent();
			Width = sourceImg.Size.Width;
			Height = sourceImg.Size.Height;

			using (var stream = new FileStream("./a.png", FileMode.OpenOrCreate)) {
				sourceImg.Save(stream, ImageFormat.Png);
			}
			var bitmap = new BitmapImage(new Uri(Path.GetFullPath("./a.png")));
			img.Source = bitmap;
		}
	}
}
