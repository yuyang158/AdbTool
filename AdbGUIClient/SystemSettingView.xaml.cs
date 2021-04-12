using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for SystemSettingView.xaml
	/// </summary>
	public partial class SystemSettingView : UserControl, ISubControlPanel {
		public SystemSettingView() {
			InitializeComponent();
		}

		private IniFile m_iniFile;
		private class IniValue {
			private IniFile m_owner;
			public IniValue(IniFile owner) {
				m_owner = owner;
			}

			public string Key { get; set; }

			private string m_value;
			public string Value {
				get => m_value;
				set {
					m_value = value;
					m_owner.TryGetSection(Section, out var section);
					section[Key] = Value;
				}
			}

			public string RawValue { set { m_value = value; } }

			public string Section { get; set; }
		}

		private readonly List<IniValue> m_dataBindingValues = new List<IniValue>();

		private void SystemSettingView_GotFocus(object sender, RoutedEventArgs e) {
			m_iniFile = new IniFile();
			m_iniFile.Load(m_device.Pull("SystemSetting.ini"));
			m_dataBindingValues.Clear();
			foreach (var section in m_iniFile) {
				foreach (var keyValue in section.Value) {
					m_dataBindingValues.Add(new IniValue(m_iniFile) {
						Key = keyValue.Key,
						RawValue = keyValue.Value.GetString(),
						Section = section.Key
					});
				}
			}

			lvIniValues.ItemsSource = m_dataBindingValues;
		}

		private IDevice m_device;

		public void AssignDevice(IDevice device) {
			m_device = device;
			SystemSettingView_GotFocus(null, (RoutedEventArgs)null);
		}

		public string GetName() {
			return "System Settings";
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e) {
			m_iniFile.Save("./temp_setting.ini");
			m_device.Push("./temp_setting.ini", "SystemSetting.ini");
		}
	}
}
