using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Project_UI
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
            // value не будет null, так как enum не nullable
            Type enumType = value.GetType();

            // Если по какой-то причине передан не enum (хотя мы это учли), возвращаем строковое представление
            if (!enumType.IsEnum)
            {
                return value.ToString();
            }

            // Обработка Flags Enum
            if (enumType.IsDefined(typeof(FlagsAttribute), false))
            {
                // Получаем все установленные флаги, исключая 'None' (0)
                var activeFlags = Enum.GetValues(enumType)
                                      .Cast<Enum>()
                                      .Where(flag => System.Convert.ToInt32(flag) != 0 && ((Enum)value).HasFlag(flag))
                                      .ToList();

                // Если ни один флаг не установлен (значение равно 0, как у 'None')
                if (!activeFlags.Any())
                {
                    // Ищем элемент 'None' в enum и возвращаем его описание, если есть
                    FieldInfo? noneField = enumType.GetField("None");
                    if (noneField != null)
                    {
                        return GetDescriptionFromEnumField(noneField);
                    }
                    // Если 'None' нет или у него нет описания, возвращаем пустую строку или сам 0
                    return System.Convert.ToInt32(value) == 0 ? string.Empty : value.ToString();
                }

                // Объединяем описания активных флагов через запятую
                return string.Join(", ", activeFlags.Select(flag =>
                {
                    FieldInfo? field = flag.GetType().GetField(flag.ToString());
                    return field != null ? GetDescriptionFromEnumField(field) : flag.ToString();
                }));
            }
            // Обработка обычных Enum (не Flags)
            else
            {
                FieldInfo? field = enumType.GetField(value.ToString() ?? string.Empty);
                return field != null ? GetDescriptionFromEnumField(field) : value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Обратное преобразование не требуется для отображения enum в UI
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
}
