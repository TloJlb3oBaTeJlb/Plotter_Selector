namespace PlotterDbLib
{
    [Flags]
    public enum PlotterType
    {
        Printer = 1,
        Cutter = 2,
        PrinterCutter = 4
    }

    public enum Positioning
    {
        Drum = 1,
        //RollToToll = 2,
        Flatbed = 2,
    }

    [Flags]
    public enum PrintingType
    {
        Monochrome = 1,
        Colorful = 2,
    }
    
    [Flags]
    public enum DrawingMethod
    {
        Pen = 1,
        Inkjet = 2,
        ElectroStatic = 4,
        Laser = 8,
        Thermal = 16,
    }


    /// <summary>
    /// Обладает свойствами, отражающими атрибуты сущности Plotter в даталогической модели.
    /// </summary>
    public class Plotter
    {
        public int PlotterId { set; get; }

        
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
