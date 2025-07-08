using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace PlotterDbLib
{
    [Flags]
    public enum Color
    {
        Monochrome = 1,
        Colorful = 2,
    }

    [Flags]
    public enum Positioning
    {
        Flatbed = 1,
        RollToToll = 2,
        Hybrid = 4,
    }

    [Flags]
    public enum DrawingMethod
    {
        Inkjet = 1,
        Laser = 2,
        Led = 4,
        //not finished
    }

    [Flags]
    public enum PlotterType
    {
        Printer = 1,
        Cutter = 2,
        PrinterCutter = 4
    }


    /// <summary>
    /// Обладает свойствами, отражающими атрибуты сущности Plotter в даталогической модели.
    /// </summary>
    public class Plotter
    {
        public int PlotterId { set; get; }
        public string Model { set; get; } = null!;
        public int Price { set; get; }
        public double Width { set; get; }
        public PlotterType PlotterType
        {
            set => enumProperties[typeof(PlotterType).Name] = value;
            get => (PlotterType)enumProperties[typeof(PlotterType).Name];
        }
        public Positioning Positioning 
        {
            set => enumProperties[typeof(Positioning).Name] = value;
            get => (Positioning)enumProperties[typeof(Positioning).Name];
        }
        public Color Color
        {
            set => enumProperties[typeof(Color).Name] = value;
            get => (Color)enumProperties[typeof(Color).Name];
        }
        public DrawingMethod DrawingMethod
        {
            set => enumProperties[typeof(DrawingMethod).Name] = value;
            get => (DrawingMethod)enumProperties[typeof(DrawingMethod).Name];
        }
        [NotMapped]
        public (int x, int y) Resolution
        {
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value.x, 1);
                ArgumentOutOfRangeException.ThrowIfLessThan(value.y, 1);

                ResolutionX = value.x;
                ResolutionY = value.y;
            }
            get => (x: ResolutionX, y: ResolutionY);
        }

        internal int ResolutionX { set; get; } = 1;
        internal int ResolutionY { set; get; } = 1;


        /// <summary>
        /// Метод для вывода объекта в консоль. Нужен для отладки.
        /// </summary>
        public override string ToString()
        {
            string str = @$"[{PlotterId}] Model: {Model}
Price: {Price},
Width: {Width},
";
            foreach (var property in enumProperties) 
                str += $"{property.Key}: {property.Value},\n";

            str += $"Resolution: {Resolution}";

            return str;
        }


        internal Dictionary<string, Enum> enumProperties { get; } = new()
        {
            { typeof(PlotterType).Name, PlotterType.Printer },
            { typeof(Positioning).Name, Positioning.Flatbed },
            { typeof(Color).Name, Color.Colorful },
            { typeof(DrawingMethod).Name, DrawingMethod.Inkjet }, // maybe change default value
        };

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
        public PlotterType PlotterType
        {
            set => enumProperties[typeof(PlotterType).Name] = value;
            get => (PlotterType)enumProperties[typeof(PlotterType).Name];
        }
        public Positioning Positioning
        {
            set => enumProperties[typeof(Positioning).Name] = value;
            get => (Positioning)enumProperties[typeof(Positioning).Name];
        }
        public Color Color
        {
            set => enumProperties[typeof(Color).Name] = value;
            get => (Color)enumProperties[typeof(Color).Name];
        }
        public DrawingMethod DrawingMethod
        {
            set => enumProperties[typeof(DrawingMethod).Name] = value;
            get => (DrawingMethod)enumProperties[typeof(DrawingMethod).Name];
        }


        internal bool IsSuitable(Plotter plotter)
        {
            return IsInPriceRange(plotter) &&
                IsInWidthRange(plotter) &&
                AreEnumsMatch(plotter);
        }

        // query
        /*internal Dictionary<string, string> ToDictionary()
        {
            return new()
            {
                { "Model", Model },
                { "PriceRange", PriceRange.ToString() },
                { "width", WidthRange.ToString() },
                { typeof(PlotterType).Name, PlotterType.ToString() },
                { typeof(Positioning).Name, Positioning.ToString() },
                { typeof(Color).Name, Color.ToString() },
                { typeof(DrawingMethod).Name, DrawingMethod.ToString() },
            };
        }


        internal static Filter FromDictionary(Dictionary<string, string> dict)
        {
            return new()
            {
                Model = dict["Model"],
                //PriceRange = WidthRange
            };
        }//*/


        private bool IsInPriceRange(Plotter plotter) =>
            PriceRange == (min: 0, max: 0) ||
            (plotter.Price >= PriceRange.min && plotter.Price <= PriceRange.max);


        private bool IsInWidthRange(Plotter plotter) =>
            WidthRange == (min: 0, max: 0) ||
            (plotter.Width >= WidthRange.min && plotter.Width <= WidthRange.max);


        private bool AreEnumsMatch(Plotter plotter)
        {
            foreach(var key in enumProperties.Keys) 
                if (Convert.ToBoolean(enumProperties[key]) && 
                    !enumProperties[key].HasFlag(plotter.enumProperties[key])) 
                    return false;
            return true;
        }


        private Dictionary<string, Enum> enumProperties { get; } = new()
        {
            { typeof(PlotterType).Name, (PlotterType)0 },
            { typeof(Positioning).Name, (Positioning)0 },
            { typeof(Color).Name, (Color)0 },
            { typeof(DrawingMethod).Name, (DrawingMethod)0 },
        };
        private (int min, int max) priceRange;
        private (double min, double max) widthRange;
    }
}
