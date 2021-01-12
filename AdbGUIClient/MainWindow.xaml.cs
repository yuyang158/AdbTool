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
		private Type[] m_subControlTypes = new[] {
			typeof(BasicInfo)
		};

		public MainWindow() {
			InitializeComponent();
			foreach (var type in m_subControlTypes) {
				var panel = Activator.CreateInstance(type) as ISubControlPanel;
				panel.AssignAppData(m_data);

				var tabItem = new TabItem {
					Header = panel.GetName(),
					Content = panel
				};

				tcControlContainer.Items.Add(tabItem);
			}

			DataContext = m_data;
			Closing += MainWindow_Closing;
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			m_data.Exit();
		}

		private void SelectAdb_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Android Debug Bridge|*exe";
			if (dlg.ShowDialog() == true) {
				m_data.AdbPath = dlg.FileName;	
			}
		}

		private void tcControlContainer_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			
		}
	}
}
