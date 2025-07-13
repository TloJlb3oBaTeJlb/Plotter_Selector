using Project_UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Project_UI.Models
{
    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Да" : "Нет";
            }
            return value; // Возвращаем исходное значение, если это не bool
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертирует значения Enum (включая Flags Enum) в их описания из DescriptionAttribute.
    /// Если DescriptionAttribute отсутствует, используется строковое представление имени элемента.
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty; // Возвращаем пустую строку для null значения
            }

            Type enumType = value.GetType();

            if (!enumType.IsEnum)
            {
                return value.ToString()!;
            }

            // Обработка Flags Enum
            if (enumType.IsDefined(typeof(FlagsAttribute), false))
            {
                var activeFlags = Enum.GetValues(enumType)
                                       .Cast<Enum>()
                                       .Where(flag => System.Convert.ToInt32(flag) != 0 && ((Enum)value).HasFlag(flag))
                                       .ToList();

                if (!activeFlags.Any())
                {
                    FieldInfo? noneField = enumType.GetField("None");
                    if (noneField != null)
                    {
                        return GetDescriptionFromEnumField(noneField);
                    }
                    return System.Convert.ToInt32(value) == 0 ? string.Empty : value.ToString()!;
                }

                return string.Join(", ", activeFlags.Select(flag =>
                {
                    FieldInfo? field = flag.GetType().GetField(flag.ToString());
                    return field != null ? GetDescriptionFromEnumField(field) : flag.ToString()!;
                }));
            }
            else
            {
                FieldInfo? field = enumType.GetField(value.ToString() ?? string.Empty);
                return field != null ? GetDescriptionFromEnumField(field) : value.ToString()!;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("EnumDescriptionConverter.ConvertBack не реализован.");
        }

        /// <summary>
        /// Вспомогательный метод для получения описания из DescriptionAttribute.
        /// </summary>
        private string GetDescriptionFromEnumField(FieldInfo field)
        {
            DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? field.Name; // Возвращаем описание или имя поля, если описания нет
        }
    }

    /// <summary>
    /// Конвертер, который возвращает Visibility.Collapsed, если строка пуста/null/пробелы, иначе Visibility.Visible.
    /// </summary>
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, является ли значение строкой
            if (value is string str)
            {
                // Если строка пустая, null или состоит из одних пробелов, скрываем элемент
                if (string.IsNullOrWhiteSpace(str))
                {
                    return Visibility.Collapsed;
                }
            }
            // Если значение не строка, или строка не пуста, показываем элемент
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("EmptyStringToVisibilityConverter.ConvertBack не реализован.");
        }
    }

    public class EnumNoneToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, является ли значение Enum и равно ли его целое представление 0
            if (value is Enum enumValue && System.Convert.ToInt32(enumValue) == 0)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
