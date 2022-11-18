using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IPUserControls
{
    /// <summary>
    /// Interaction logic for IpField
    /// </summary>
    public partial class IpField : INotifyPropertyChanged
    {
        public IpField()
        {
            InitializeComponent();
        }

        #region Exposed Properties

        /// <summary>
        /// Gets the IP Address as a string.
        /// </summary>
        public string IpAddress
        {
            get => (string)GetValue(IpAddressProperty);
            set
            {
                if (!value.IsValidIpAddress()) return; 
                    
                SetValue(IpAddressProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty IpAddressProperty = DependencyProperty.Register
        (
            "IpAddress", 
            typeof(string), 
            typeof(IpField),
            new FrameworkPropertyMetadata
            (
                "0.0.0.0",
                new PropertyChangedCallback(IpChangedCallback)
            ) { BindsTwoWayByDefault = true }
        );
        
        private static void IpChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uc = (IpField) d;
            var ipAddress = (string)e.NewValue;

            if (!ipAddress.IsValidIpAddress()) return;

            var ipBytes = ipAddress.ToIpBytes();

            uc.IpFirstByte = ipBytes[0].ToString();
            uc.IpSecondByte = ipBytes[1].ToString();
            uc.IpThirdByte = ipBytes[2].ToString();
            uc.IpFourthByte = ipBytes[3].ToString();
        }


        /// <summary>
        /// Returns the IP address as a byte array.
        /// </summary>
        public byte[] IpAddressBytes
        {
            get => (byte[])GetValue(IpAddressBytesProperty);
            set
            {
                SetValue(IpAddressBytesProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty IpAddressBytesProperty =
            DependencyProperty.Register("IpAddressBytes", typeof(byte[]), typeof(IpField), new FrameworkPropertyMetadata(new byte[4]) { BindsTwoWayByDefault = true });

        #endregion Exposed Properties

        private void UpdateIpAddress()
        {
            IpAddress =
                _ipFirstByte.ToByte() + "." +
                _ipSecondByte.ToByte() + "." +
                _ipThirdByte.ToByte() + "." +
                _ipFourthByte.ToByte();
        }

        private void UpdateIpAddressBytes()
        {
            var ipBytes = new[]
            {
                IpFirstByte.ToByte(),
                IpSecondByte.ToByte(),
                IpThirdByte.ToByte(),
                IpFourthByte.ToByte()
            };

            IpAddressBytes = ipBytes;
        }

        private void SetIpByteProperty(ref string backingField, string value, string property)
        {
            // Remove these chars as they will parse as valid numbers
            // and can show "000", "+2" etc. in the text box.
            value = value
                .RemoveWhitespace()
                .RemoveNumberSigns()
                .RemoveLeadingZerosInByte();

            // When deleting the text box input, an empty char is sent in.
            // Set to Zero in this case.
            if (value == "" && backingField != "0")
                backingField = "0";

            // Handles non-numbers
            else if (!value.IsNumber())
                return;

            // Handles valid byte-input 
            else if (value.IsByte())
                backingField = value;
            
            // Is number but not a byte
            else
                return;

            OnPropertyChanged(property);
        }

        #region Properties

        private string _ipFirstByte = "0";

        /// <summary>
        /// The first number in the IP Address.
        /// </summary>
        public string IpFirstByte
        {
            get => _ipFirstByte;
            set
            {
                SetIpByteProperty(ref _ipFirstByte, value, nameof(IpFirstByte));
                UpdateIpAddress();
                UpdateIpAddressBytes();
            }
        }

        private string _ipSecondByte = "0";

        /// <summary>
        /// The second number in the IP Address.
        /// </summary>
        public string IpSecondByte
        {
            get => _ipSecondByte;
            set
            {
                SetIpByteProperty(ref _ipSecondByte, value, nameof(IpSecondByte));
                UpdateIpAddress();
                UpdateIpAddressBytes();
            }
        }

        private string _ipThirdByte = "0";

        /// <summary>
        /// The third number in the IP Address.
        /// </summary>
        public string IpThirdByte
        {
            get => _ipThirdByte;
            set
            {
                SetIpByteProperty(ref _ipThirdByte, value, nameof(IpThirdByte));
                UpdateIpAddress();
                UpdateIpAddressBytes();
            }
        }

        private string _ipFourthByte = "0";

        /// <summary>
        /// The fourth number in the IP Address.
        /// </summary>
        public string IpFourthByte
        {
            get => _ipFourthByte;
            set
            {
                SetIpByteProperty(ref _ipFourthByte, value, nameof(IpFourthByte));
                OnPropertyChanged();
                UpdateIpAddress();
            }
        }

        #endregion Properties

        #region Events

        // Select All Text On Keyboard Focus
        private void FirstByteTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => FirstByteTextBox.SelectAll();
        private void SecondByteTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => SecondByteTextBox.SelectAll();
        private void ThirdByteTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => ThirdByteTextBox.SelectAll();
        private void FourthByteTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => FourthByteTextBox.SelectAll();

        // Shift text box focus on key down events
        private void FirstByteTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e) => ShiftTextBoxFocus((TextBox)sender, e);
        private void SecondByteTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e) => ShiftTextBoxFocus((TextBox)sender, e);
        private void ThirdByteTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e) => ShiftTextBoxFocus((TextBox)sender, e);
        private void FourthByteTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e) => ShiftTextBoxFocus((TextBox)sender, e);
        
        private static void ShiftTextBoxFocus (TextBox textBox, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.OemPeriod:
                case Key.Enter:
                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    if (textBox.Text == "0")
                        textBox.Text = "";
                    break;
                case Key.Right:
                {
                    if (textBox.CaretIndex == textBox.Text.Length)
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right)); ;
                    break;
                }
                case Key.Back:
                case Key.Left:
                {
                    if (textBox.CaretIndex == 0)
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    break;
                }
            }
        }
        #endregion Events

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