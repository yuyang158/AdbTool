using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace IPUserControls
{
    /// <summary>
    /// Interaction logic for IpPort.xaml
    /// </summary>
    public partial class IpPort : INotifyPropertyChanged
    {
        public IpPort()
        {
            InitializeComponent();
        }

        // Exposed Properties
        // --------------------------------------
        public ushort PortNumber
        {
            get => (ushort)GetValue(PortNumberProperty);
            set
            {
                SetValue(PortNumberProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty PortNumberProperty = DependencyProperty.Register
        (
            "PortNumber", 
            typeof(ushort),
            typeof(IpPort),
            new FrameworkPropertyMetadata
            (
                ushort.MinValue,
                new PropertyChangedCallback(PortNumberChangedCallback)
            )
            {BindsTwoWayByDefault = true});


        // Event Handlers
        // --------------------------------------
        private static void PortNumberChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var Uc = (IpPort) d;
            Uc.PortNumber = (ushort)e.NewValue;
        }
        
        private void PortNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PortNrTextBox.Text == PortNumber.ToString()) return;
            if (PortNrTextBox.Text == "")
            {
                PortNumber = 0;
                PortNrTextBox.Text = 0.ToString();
                PortNrTextBox.SelectAll();
                return;
            }

            if (!PortNrTextBox.Text.IsUShort())
                PortNrTextBox.Text = PortNumber.ToString();
            else
            {
                PortNumber = ushort.Parse(PortNrTextBox.Text);
                PortNrTextBox.Text = PortNumber.ToString();
            }
        }

        private void PortNrTextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) => PortNrTextBox.SelectAll();

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