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

    [Flags]
    public enum PaperFormat
    {
        // Форматы B-серии (обычно больше, чем A той же цифры)
        [Description("B0+")]
        B0Plus = 1 << 0,
        [Description("B0")]
        B0 = 1 << 1,
        [Description("B1+")]
        B1Plus = 1 << 2,
        [Description("B1")]
        B1 = 1 << 3,
        [Description("B2+")]
        B2Plus = 1 << 4,
        [Description("B2")]
        B2 = 1 << 5,
        [Description("B3+")]
        B3Plus = 1 << 6,
        [Description("B3")]
        B3 = 1 << 7,
        [Description("B4+")]
        B4Plus = 1 << 8,
        [Description("B4")]
        B4 = 1 << 9,

        // Форматы RA/SRA (увеличенные форматы для печати с обрезкой)
        [Description("RA1")]
        RA1 = 1 << 10,
        [Description("SRA2+")]
        SRA2Plus = 1 << 11,
        [Description("SRA2")]
        SRA2 = 1 << 12,
        [Description("SRA3+")]
        SRA3Plus = 1 << 13,
        [Description("SRA3")]
        SRA3 = 1 << 14,

        // Форматы A-серии (стандартные)
        [Description("более A0")]
        MoreThanA0 = 1 << 15,
        [Description("A0+")]
        A0Plus = 1 << 16,
        [Description("A0")]
        A0 = 1 << 17,
        [Description("более A1")]
        MoreThanA1 = 1 << 18,
        [Description("A1+")]
        A1Plus = 1 << 19,
        [Description("A1")]
        A1 = 1 << 20,
        [Description("A2+")]
        A2Plus = 1 << 21,
        [Description("A2")]
        A2 = 1 << 22,
        [Description("A2-")]
        A2Minus = 1 << 23,
        [Description("A3+")]
        A3Plus = 1 << 24,
        [Description("A3")]
        A3 = 1 << 25,
        [Description("A4+")]
        A4Plus = 1 << 26,
        [Description("A4")]
        A4 = 1 << 27,
        [Description("A5")]
        A5 = 1 << 28,

        // Комбинированные флаги
        // AllA4Formats = A4 | A4Plus,
        // AllAFormats = A0 | A0Plus | A1 | A1Plus | A2 | A2Plus | A2Minus | A3 | A3Plus | A4 | A4Plus | A5 | MoreThanA0 | MoreThanA1,
        // AllBFormats = B0Plus | B0 | B1Plus | B1 | B2Plus | B2 | B3Plus | B3 | B4Plus | B4
    }

    [Flags]
    public enum Material
    {
        [Description("Бумага")]
        Paper = 1,
        [Description("Пленка")]
        Film = 2,
        [Description("Картон")]
        Cardboard = 4,
        [Description("Текстиль")]
        Textile = 8,
        [Description("Металл")]
        Metal = 16,
        [Description("Стекло")]
        Glass = 32,
        [Description("Кожа")]
        Lether = 64,
        [Description("Керамика")]
        Ceramics = 128,
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
        /// Формат печати
        /// </summary>
        public PaperFormat PaperFormat { set; get; }
        /// <summary>
        /// Тип материала (Назначение)
        /// </summary>
        public Material Material { set; get; }

        /// <summary>
        /// Путь к изображению
        /// </summary>
        public string PathToImage { set; get; } = string.Empty;

        internal string[] StringProps
        {
            get => [Model, Manufacturer];
        }
        internal Enum[] EnumProps
        {
            get => [PlotterType, DrawingMethod, Positioning, PrintingType, PaperFormat, Material];
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
        /// Фильтр по производителю. Если список пуст, то фильтр не активен (показывать всех).
        /// </summary>
        public List<string> Manufacturers { set; get; } = new List<string>();
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
        /// <summary>
        /// Фильтр по формату печати
        /// </summary>
        public PaperFormat PaperFormat { set; get; }
        /// <summary>
        /// Фильтр по типу материала
        /// </summary>
        public Material Material { set; get; }


        internal bool IsSuitable(Plotter plotter)
        {
            return DoesModelFit(plotter) &&
                DoesManufacturerFit(plotter) &&
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
            foreach (var pair in EnumProps.Zip(plotter.EnumProps))
            {
                Enum filterValue = pair.First;
                Enum plotterValue = pair.Second;

                int filterInt = Convert.ToInt32(filterValue);
                int plotterInt = Convert.ToInt32(plotterValue);

                if (filterInt != 0)
                {
                    if ((filterInt & plotterInt) == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /*private bool AreEnumsMatching(Plotter plotter)
        {
            foreach(var pair in EnumProps.Zip(plotter.EnumProps))
            {
                if (Convert.ToBoolean(pair.First) &&
                    !pair.First.HasFlag(pair.Second)) return false;
            }
            return true;
        }*/

        private bool DoesModelFit(Plotter plotter)
        {
            if (string.IsNullOrWhiteSpace(Model))
            {
                return true;
            }
            return plotter.Model.Contains(Model, StringComparison.OrdinalIgnoreCase);
        }

        /*private bool DoesFitStringFilters(Plotter plotter)
        {
            foreach (var pair in StringProps.Zip(plotter.StringProps))
            {
                if (!pair.Second.Contains(pair.First)) return false;
            }
            return true;
        }*/

        private bool DoesManufacturerFit(Plotter plotter)
        {
            if (!Manufacturers.Any())
            {
                return true;
            }
            return Manufacturers.Any(m => plotter.Manufacturer.Equals(m, StringComparison.OrdinalIgnoreCase));
        }

        private (int min, int max) priceRange;
        private (double min, double max) widthRange;

        private string[] StringProps 
        {
            get => [Model];
        }
        private Enum[] EnumProps
        {
            get => [PlotterType, DrawingMethod, Positioning, PrintingType, PaperFormat, Material];
        }

    }
}
