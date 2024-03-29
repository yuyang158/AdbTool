﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace AdbGUIClient.Converter {
	[ValueConversion(typeof(object), typeof(bool))]
	public class NullToBoolValueConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return value;
		}
	}
}
