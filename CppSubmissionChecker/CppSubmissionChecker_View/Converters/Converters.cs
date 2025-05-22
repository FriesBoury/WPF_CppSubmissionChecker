using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;

namespace CppSubmissionChecker_View.Converters
{
	class BoolToVisibilityConverter_Collapsed : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch ((Visibility)value)
			{
				case Visibility.Visible:
					return true;
				default:
					return false;
			}
		}
	}
	class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				return Visibility.Visible;
			}
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch ((Visibility)value)
			{
				case Visibility.Visible:
					return true;
				default:
					return false;
			}
		}
	}
	class BoolToInvisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(bool)value)
			{
				return Visibility.Visible;
			}
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch ((Visibility)value)
			{
				case Visibility.Visible:
					return false;
				default:
					return true;
			}
		}
	}
	class ScoreToColorBrushConverter : IValueConverter
	{
		static readonly Color _red = Color.FromScRgb(.5f, 1f, .2f, .2f);
		static readonly Color _green = Color.FromScRgb(.5f, 0.5f, 1f, 0.1f);
		static readonly Color _orange = Color.FromScRgb(.5f, 1f, .45f,0f);

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			if (value is float score)
			{
				if (score >= 1f)
				{
					return new SolidColorBrush(_green);
				}
				if(score > 0.25f)
				{
					return new SolidColorBrush(_orange);
				}
				return new SolidColorBrush(_red);
			}
			else if (value is double scoreDouble)
			{
				if (scoreDouble >= 1d)
				{
					return new SolidColorBrush(_green);
				}
				if (scoreDouble > 0.25d)
				{
					return new SolidColorBrush(_orange);
				}
				return new SolidColorBrush(_red);
			}
			return new SolidColorBrush(Color.FromScRgb(0f, 0f, 0f, 0f));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
