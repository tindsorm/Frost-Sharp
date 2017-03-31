using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Frost_Sharp.Utils {
	public class ZonedDateTimeConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			ZonedDateTime? zdt = value as ZonedDateTime?;
			if (zdt.HasValue) {
				return Utils.FormatDate(zdt.Value);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
