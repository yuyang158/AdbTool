using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for DeviceTypeSelection.xaml
	/// </summary>
	public partial class DeviceTypeSelection : Window {
		public DeviceTypeSelection() {
			InitializeComponent();
		}

		private void Android_Click(object sender, RoutedEventArgs e) {
			MainWindow w = new MainWindow();
			w.Show();
			Close();
		}

		private void IOS_Click(object sender, RoutedEventArgs e) {
			IOSWindow w = new IOSWindow();
			w.Show();
			Close();
		}
	}
}
