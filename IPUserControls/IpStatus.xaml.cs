using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace IPUserControls
{
    /// <summary>
    /// Interaction logic for IpStatus.xaml
    /// </summary>
    public partial class IpStatus : INotifyPropertyChanged
    {
        public IpStatus()
        {
            InitializeComponent();
        }

        // Status Image Sources
        private const string ImageConnected = "/IPUserControls;component/Images/ip_connected.png";
        private const string ImageDisconnected = "/IPUserControls;component/Images/ip_disconnected.png";
        private const string ImageConnecting = "/IPUserControls;component/Images/ip_connecting.png";
        private const string ImageError = "/IPUserControls;component/Images/ip_error.png";

        ///// <summary>
        ///// This can have the following statuses:
        ///// Connected, Disconnected, Connecting, Error.
        ///// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get => (ConnectionStatus)GetValue(ConnectionStatusProperty);
            set
            {
                SetValue(ConnectionStatusProperty, value);
                OnPropertyChanged();
                UpdateConnectionImage();
            }
        }

        public static readonly DependencyProperty ConnectionStatusProperty = DependencyProperty.Register
        (
            "ConnectionStatus", 
            typeof(ConnectionStatus),
            typeof(IpStatus), 
            new FrameworkPropertyMetadata(
                ConnectionStatus.Disconnected, 
                new PropertyChangedCallback(ConnectionStatusChangedCallback)
            ) {BindsTwoWayByDefault = true} 
        );

        private static void ConnectionStatusChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine($"ConnectionStatusChangedCallback: {e.NewValue}");
            var uc = (IpStatus)d;
            uc.ConnectionStatus = (ConnectionStatus) e.NewValue;
        }

        private string _connectionImageSource = ImageDisconnected;

        /// <summary>
        /// Contains the image sources to use for the different connection statuses.
        /// </summary>
        public string ConnectionImageSource
        {
            get => _connectionImageSource;
            set => SetProperty(ref _connectionImageSource, value);
        }

        private void UpdateConnectionImage()
        {
            switch (ConnectionStatus)
            {
                case ConnectionStatus.Connected:
                    ConnectionImageSource = ImageConnected;
                    break;

                case ConnectionStatus.Disconnected:
                    ConnectionImageSource = ImageDisconnected;
                    break;

                case ConnectionStatus.Connecting:
                    ConnectionImageSource = ImageConnecting;
                    break;

                case ConnectionStatus.Error:
                    ConnectionImageSource = ImageError;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StatusIcon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PopupStatusInfo.Visibility = Visibility.Visible;
            PopupStatusInfo.IsOpen = true;
        }

        private void StatusIcon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PopupStatusInfo.Visibility = Visibility.Collapsed;
            PopupStatusInfo.IsOpen = false;
        }

        #region Property Notifications

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion Property Notifications
    }
}