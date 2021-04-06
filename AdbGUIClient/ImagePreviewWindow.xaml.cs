using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for ImagePreviewWindow.xaml
	/// </summary>
	public partial class ImagePreviewWindow : Window {
		public ImagePreviewWindow(string path) {
			InitializeComponent();
			var bitmap = new BitmapImage(new Uri(Path.GetFullPath(path)));
			Width = bitmap.Width;
			Height = bitmap.Height;
			img.Source = bitmap;
		}
	}
}
