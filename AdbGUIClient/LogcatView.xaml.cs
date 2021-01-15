using SharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace AdbGUIClient {
	/// <summary>
	/// Interaction logic for LogcatView.xaml
	/// </summary>
	public partial class LogcatView : UserControl, ISubControlPanel {
		private AppData m_data;
		private readonly System.Timers.Timer m_timer;
		private int m_watchedProcessId;

		private readonly Dictionary<Priority, SolidColorBrush> m_logBrush = new Dictionary<Priority, SolidColorBrush> {
			{Priority.Verbose, new SolidColorBrush(Colors.DarkGray)},
			{Priority.Debug, new SolidColorBrush(Colors.DarkGray)},
			{Priority.Info, new SolidColorBrush(Colors.White)},
			{Priority.Warn, new SolidColorBrush(Colors.Yellow)},
			{Priority.Error, new SolidColorBrush(Colors.Red)},
			{Priority.Assert, new SolidColorBrush(Colors.DarkRed)}
		};

		public void AppendText(string text, SolidColorBrush color) {
			var p = new Paragraph();
			p.Inlines.Add(text);
			p.Foreground = color;
			fdLog.Blocks.Add(p);
		}

		public LogcatView() {
			InitializeComponent();
			Unloaded += LogcatView_Unloaded;

			m_timer = new System.Timers.Timer(10000);
			m_timer.Elapsed += Timer_Elapsed;
			m_timer.Start();
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			if (m_data.CurrentClient == null || m_data.SelectedDeviceData == null) {
				return;
			}

			if (string.IsNullOrEmpty(m_data.Package))
				return;
			InfoReceiver receiver = new InfoReceiver();
			receiver.Callback += Receiver_CallbackAsync;
			m_data.CurrentClient.ExecuteRemoteCommandAsync($"pidof {m_data.Package}",
				m_data.SelectedDeviceData, receiver, CancellationToken.None);
		}

		private async void Receiver_CallbackAsync(string pid) {
			if(m_watchedProcessId != 0) {
				await m_data.CurrentClient.RunLogServiceAsync(m_data.SelectedDeviceData, OnReceiveLog, CancellationToken.None, LogId.Crash, LogId.Main);
			}

			if (string.IsNullOrEmpty(pid))
				return;
			m_watchedProcessId = int.Parse(pid);
		}

		private void LogcatView_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
			m_timer.Stop();
			if (m_tokenSource == null)
				return;
			m_tokenSource.Cancel();
		}

		private CancellationTokenSource m_tokenSource;

		private void OnReceiveLog(LogEntry log) {
			var androidLog = log as AndroidLogEntry;
			System.Diagnostics.Debug.WriteLine(androidLog.ProcessId);
			if (log.ProcessId != m_watchedProcessId)
				return;

			Dispatcher.Invoke(() => {
				var brush = m_logBrush[androidLog.Priority];
				AppendText(androidLog.Message, brush);
			});
		}

		public void AssignAppData(AppData data) {
			m_data = data;
		}

		public string GetName() {
			return "Logcat";
		}
	}
}
