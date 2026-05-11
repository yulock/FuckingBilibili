using System;
using System.Globalization;
using System.Windows.Data;
using FuckingBilibili.Models;

namespace FuckingBilibili.Converters
{
    /// <summary>
    /// 服务器类型转布尔值（用于单选按钮）
    /// </summary>
    public class ServerTypeToBoolConverter : IValueConverter
    {
        public ServerType TargetType { get; set; } = ServerType.Official;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServerType serverType)
            {
                return serverType == TargetType;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                return TargetType;
            }
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// 服务器类型转显示文字
    /// </summary>
    public class ServerTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServerType serverType)
            {
                return serverType == ServerType.Official ? "官服" : "B服";
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值反转
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }
    }

    /// <summary>
    /// 字符串非空转布尔值
    /// </summary>
    public class StringNotEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return !string.IsNullOrWhiteSpace(strValue);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
