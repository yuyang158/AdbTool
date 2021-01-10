using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private readonly AppData m_data = new AppData();
		public MainWindow() {
			InitializeComponent();
			DataContext = m_data;
		}

		private void SelectAdb_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Android Debug Bridge|*exe";
			if (dlg.ShowDialog() == true) {
				m_data.AdbPath = dlg.FileName;	
			}
		}
	}
}
