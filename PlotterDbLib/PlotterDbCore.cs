using System.ComponentModel;

namespace PlotterDbLib
{
    [Flags]
    public enum PlotterType
    {
        [Description("Печатающий")]
        Printer = 1,
        [Description("Режущий")]
        Cutter = 2,
        [Description("Гибридный")]
        PrinterCutter = 4
    }

    [Flags]
    public enum Positioning
    {
        [Description("Барабанный (Рулонный)")]
        Drum = 1,
        // RollToToll = 2, 
        [Description("Планшетный")]
        Flatbed = 2
    }

    [Flags]
    public enum PrintingType
    {
        [Description("Монохромная")]
        Monochrome = 1,
        [Description("Цветная")]
        Colorful = 2,
    }

    [Flags]
    public enum DrawingMethod
    {
        [Description("Перьевой")]
        Pen = 1,
        [Description("Струйный")]
        Inkjet = 2,
        [Description("Электростатический")]
        ElectroStatic = 4,
        [Description("Лазерный")]
        Laser = 8,
        [Description("Термальный")]
        Thermal = 16,
    }

    /// <summary>
    /// Обладает свойствами, отражающими атрибуты сущности Plotter в даталогической модели.
    /// </summary>
    public class Plotter
    {
        public int PlotterId { set; get; }

        /// <summary>
        /// Модель плоттера
        /// </summary>
        public string Model { set; get; } = null!;
        /// <summary>
        /// Производитель
        /// </summary>
        public string Manufacturer { set; get; } = string.Empty;//= null!;
        /// <summary>
        /// Формат печати
        /// </summary>
        public string Format { set; get; } = string.Empty; // maybe
        /// <summary>
        /// Тип материала (Назначение)
        /// </summary>
        public string Material { set; get; } = string.Empty; // maybe
        /// <summary>
        /// Габариты
        /// </summary>
        public string Dimensions { set; get; } = string.Empty; // maybe
        /// <summary>
        /// Дополнительная информация
        /// </summary>
        public string Addendum { set; get; } = string.Empty;

        /// <summary>
        /// Цена
        /// </summary>
        public int Price { set; get; }
        /// <summary>
        /// Ширина печати
        /// </summary>
        public double Width { set; get; }
        /// <summary>
        /// Вес
        /// </summary>
        public double Weight { set; get; }
        /// <summary>
        /// Наличие жёсткого диска
        /// </summary>
        public bool HasHardDrive { set; get; }

        /// <summary>
        /// Класс
        /// </summary>
        public PlotterType PlotterType { set; get; }
        /// <summary>
        /// Способ нанесения изображения
        /// </summary>
        public DrawingMethod DrawingMethod { set; get; }
        /// <summary>
        /// Тип подачи материала
        /// </summary>
        public Positioning Positioning { set; get; }
        /// <summary>
        /// Тип печати
        /// </summary>
        public PrintingType PrintingType { set; get; }

        /// <summary>
        /// Путь к изображению
        /// </summary>
        public string PathToImage { set; get; } = string.Empty;

        internal string[] StringProps
        {
            get => [Model, Manufacturer, Format, Material];
        }
        internal Enum[] EnumProps
        {
            get => [PlotterType, DrawingMethod, Positioning, PrintingType];
        }

        /// <summary>
        /// Метод для вывода объекта в консоль. Нужен для отладки.
        /// </summary>
        public override string ToString()
        {
            string str = @$"[{PlotterId}] Model: {Model}
Price: {Price},
Width: {Width},
Weight: {Weight},
";

            foreach (var property in EnumProps) 
                str += $"{property.GetType().Name}: {property},\n";
            foreach (var property in StringProps)
                str += $"{property.GetType().Name}: {property},\n";
            str += $"HasHardDrive: {HasHardDrive},\n";
            str += $"Addendum: {Addendum}";

            return str;
        }
    }


    /// <summary>
    /// Фильтр, передаваемый в метод 
    /// <c>PlotterDataBase.GetFilteredPlotters(Filter)</c>.
    /// <para>Поля энумераторы являются битовыми полями, т.е.
    /// несколько значений могут быть скомбинированы через "побитовое или" 
    /// ( <c>|</c> ).</para>
    /// </summary>
    public class Filter
    {   
        
        /// <summary>
        /// Будут найдены плоттеры, модель которых содержит данную строку. 
        /// Регистр не учитывается.
        /// </summary>
        public string Model { set; get; } = string.Empty;
        /// <summary>
        /// Фильтр по производителю
        /// </summary>
        public string Manufacturer { set; get; } = string.Empty;//= null!;
        /// <summary>
        /// Фильтр по формату печати
        /// </summary>
        public string Format { set; get; } = string.Empty; // maybe
        /// <summary>
        /// Фильтр по типу материала
        /// </summary>
        public string Material { set; get; } = string.Empty; // maybe
        /// <summary>
        /// Диапазон цен. Кидает исключение если хотя бы одно из чисел меньше 
        /// нуля или максимум меньше минимума.
        /// </summary>

        public (int min, int max) PriceRange
        {
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value.min, 0);
                ArgumentOutOfRangeException.ThrowIfLessThan(value.max, 0);
                ArgumentOutOfRangeException.ThrowIfLessThan(value.max, value.min);

                priceRange = value;
            }
            get => priceRange;
        }
        /// <summary>
        /// Диапазон ширины печати. Кидает исключение если хотя бы одно из чисел 
        /// меньше нуля или максимум меньше минимума.
        /// </summary>
        public (double min, double max) WidthRange
        {
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value.min, 0);
                ArgumentOutOfRangeException.ThrowIfLessThan(value.max, 0);
                ArgumentOutOfRangeException.ThrowIfLessThan(value.max, value.min);

                widthRange = value;
            }
            get => widthRange;
        }

        /// <summary>
        /// Фильтр по классу
        /// </summary>
        public PlotterType PlotterType { set; get; }
        /// <summary>
        /// Фильтр по способу нанесения изображения
        /// </summary>
        public DrawingMethod DrawingMethod { set; get; }
        /// <summary>
        /// Фильтр по типу подачи материала
        /// </summary>
        public Positioning Positioning { set; get; }
        /// <summary>
        /// Фильтр по типу печати
        /// </summary>
        public PrintingType PrintingType { set; get; }


        internal bool IsSuitable(Plotter plotter)
        {
            return DoesFitStringFilters(plotter) &&
                IsInPriceRange(plotter) &&
                IsInWidthRange(plotter) &&
                AreEnumsMatching(plotter);
        }


        private bool IsInPriceRange(Plotter plotter) =>
            PriceRange == (min: 0, max: 0) ||
            (plotter.Price >= PriceRange.min && plotter.Price <= PriceRange.max);


        private bool IsInWidthRange(Plotter plotter) =>
            WidthRange == (min: 0, max: 0) ||
            (plotter.Width >= WidthRange.min && plotter.Width <= WidthRange.max);


        private bool AreEnumsMatching(Plotter plotter)
        {
            foreach(var pair in EnumProps.Zip(plotter.EnumProps))
            {
                if (Convert.ToBoolean(pair.First) &&
                    !pair.First.HasFlag(pair.Second)) return false;
            }
            return true;
        }


        private bool DoesFitStringFilters(Plotter plotter)
        {
            foreach (var pair in StringProps.Zip(plotter.StringProps))
            {
                if (!pair.Second.Contains(pair.First)) return false;
            }
            return true;
        }


        private (int min, int max) priceRange;
        private (double min, double max) widthRange;

        private string[] StringProps 
        {
            get => [Model, Manufacturer, Format, Material];
        }
        private Enum[] EnumProps
        {
            get => [PlotterType, DrawingMethod, Positioning, PrintingType];
        }

    }
}
