using PlotterDbLib;
using Project_UI.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

namespace Project_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<FilterOption> _typeOptions;
        private ObservableCollection<FilterOption> _printingTypeOptions;
        private ObservableCollection<FilterOption> _manufacturerOptions;
        private ObservableCollection<FilterOption> _printingFormatOptions;
        private ObservableCollection<FilterOption> _priningColorOptions;
        private ObservableCollection<FilterOption> _positioningOptions;
        private ObservableCollection<FilterOption> _materialOptions;
        private ObservableCollection<Plotter> _plotters;
        public ObservableCollection<Plotter> Plotters
        {
            get => _plotters;
            set
            {
                _plotters = value;
                OnPropertyChanged(nameof(Plotters));
            }
        }

        public ICommand SelectPlotterCommand { get; private set; }
        public ICommand ApplyFiltersCommand { get; private set; }
        public ICommand ResetFiltersCommand { get; private set; }

        private void Filter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = (TextBox)sender;
                // Получаем BindingExpression для свойства Text
                BindingExpression binding = tb.GetBindingExpression(TextBox.TextProperty);
                if (binding != null)
                {
                    // Принудительно обновляем источник привязки (т.е. свойство MinPriceText, MaxPriceText и т.д.)
                    binding.UpdateSource();
                }
                // Опционально: предотвращаем дальнейшую обработку события Enter,
                // чтобы избежать, например, перехода на следующий элемент, если это нежелательно.
                e.Handled = true;
            }
        }

        private PlotterDbAdminClient _dbAdmin;
        private PlotterDbClient _dbCilent;
        private PlotterDbServer _server;

        public string ModelSearchText { get; set; } = string.Empty;

        private string _minPriceText = string.Empty;
        public string MinPriceText
        {
            get => _minPriceText;
            set
            {
                if (_minPriceText != value)
                {
                    _minPriceText = value;
                    OnPropertyChanged(nameof(MinPriceText)); // Уведомляем UI об изменении
                                                             // Важно: Вызываем ApplyFiltersAsync() здесь,
                                                             // после того как свойство обновилось
                                                             // Task.Run(() => ApplyFiltersAsync()); // Или без Task.Run, если ApplyFiltersAsync достаточно быстрое
                    ApplyFiltersAsync(); // await здесь не нужен, так как сеттер не может быть async void
                                         // Если ApplyFiltersAsync действительно асинхронный и долгий,
                                         // лучше вызывать его через Task.Run, чтобы не блокировать UI.
                                         // Однако, для фильтрации данных это редко нужно.
                }
            }
        }
        private string _maxPriceText = string.Empty;
        public string MaxPriceText
        {
            get => _maxPriceText;
            set
            {
                if (_maxPriceText != value)
                {
                    _maxPriceText = value;
                    OnPropertyChanged(nameof(MaxPriceText));
                    ApplyFiltersAsync();
                }
            }
        }
        private string _minWidthText = string.Empty;
        public string MinWidthText
        {
            get => _minWidthText;
            set
            {
                if (_minWidthText != value)
                {
                    _minWidthText = value;
                    OnPropertyChanged(nameof(MinWidthText));
                    ApplyFiltersAsync();
                }
            }
        }
        private string _maxWidthText = string.Empty;
        public string MaxWidthText
        {
            get => _maxWidthText;
            set
            {
                if (_maxWidthText != value)
                {
                    _maxWidthText = value;
                    OnPropertyChanged(nameof(MaxWidthText));
                    ApplyFiltersAsync();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _server = new();
            var task = _server.StartAsync();

            _dbAdmin = new();
            _dbCilent = new();
            _plotters = new ObservableCollection<Plotter>();

            LoadAllPlotters();

            _typeOptions = CreateFilterOptions(typeof(PlotterType));
            FilterTypeListBox.ItemsSource = _typeOptions;
            InitializeFilterOptions(_typeOptions, FilterTypeListBox, FilterTypeText, "Тип плоттера");

            _printingTypeOptions = CreateFilterOptions(typeof(DrawingMethod));
            FilterPrintingTypeListBox.ItemsSource = _printingTypeOptions;
            InitializeFilterOptions(_printingTypeOptions, FilterPrintingTypeListBox, FilterPrintingTypeText, "Способ нанесения");

            _manufacturerOptions = CreateManufacturerFilterOptions();
            FilterManufacturerListBox.ItemsSource = _manufacturerOptions;

            _printingFormatOptions = CreateFilterOptions(typeof(PaperFormat));
            FilterPrintingFormatListBox.ItemsSource = _printingFormatOptions;
            InitializeFilterOptions(_printingFormatOptions, FilterPrintingFormatListBox, FilterPrintingFormatText, "Формат печати");

            _priningColorOptions = CreateFilterOptions(typeof(PrintingType));
            FilterPrintingColorListBox.ItemsSource = _priningColorOptions;
            InitializeFilterOptions(_priningColorOptions, FilterPrintingColorListBox, FilterPrintingColorText, "Цвет печати");

            _positioningOptions = CreateFilterOptions(typeof(Positioning));
            FilterPositioningListBox.ItemsSource = _positioningOptions;
            InitializeFilterOptions(_positioningOptions, FilterPositioningListBox, FilterPositioningText, "Тип подачи материала");

            _materialOptions = CreateFilterOptions(typeof(Material));
            FilterMaterialListBox.ItemsSource = _materialOptions;
            InitializeFilterOptions(_materialOptions, FilterMaterialListBox, FilterMaterialText, "Тип материала");

            Plotters = new ObservableCollection<Plotter>(); // Инициализация
            SelectPlotterCommand = new RelayCommand(OnSelectPlotter);
            ApplyFiltersCommand = new RelayCommand(async _ => await ApplyFiltersAsync()); // Асинхронная команда
            ResetFiltersCommand = new RelayCommand(ResetFilters); // Команда сброса

            this.DataContext = this;
        }

        private void OnSelectPlotter(object? parameter)
    {
        if (parameter is Plotter selectedPlotter)
        {
            //MessageBox.Show($"Выбран плоттер: {selectedPlotter.Model} (ID: {selectedPlotter.PlotterId})");
            
            InfoWindow infoWindow = new InfoWindow(selectedPlotter);
                infoWindow.ShowDialog();
        }
    }

        private async void LoadAllPlotters()
        {
            try
            {
                List<Plotter> allPlottersFromDb = await _dbCilent.GetFilteredPlottersAsync(new Filter());

                _manufacturerOptions.Clear();

                var uniqueManufacturers = allPlottersFromDb
                                            .Where(p => !string.IsNullOrWhiteSpace(p.Manufacturer))
                                            .Select(p => p.Manufacturer)
                                            .Distinct()
                                            .OrderBy(m => m)
                                            .ToList();
                                
                foreach (string manufacturer in uniqueManufacturers)
                {
                    _manufacturerOptions.Add(new FilterOption { Name = manufacturer, Value = manufacturer, IsSelected = false });
                }

                InitializeFilterOptions(_manufacturerOptions, FilterManufacturerListBox, FilterManufacturerText, "Производитель");
                await ApplyFiltersAsync(new Filter());
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка запроса к серверу при загрузке всех плоттеров: {ex.Message}\nПроверьте, запущен ли сервер на localhost:1111.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка при загрузке всех плоттеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeFilterOptions(ObservableCollection<FilterOption> options, ListBox listBox, TextBlock headerText, string categoryName)
        {
            foreach (var option in options)
            {
                option.PropertyChanged += FilterOption_IsSelectedChanged;
            }
            UpdateFilterHeaderText(listBox, headerText, categoryName);
        }

        private async void FilterOption_IsSelectedChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterOption.IsSelected))
            {
                // При изменении выбора опции фильтра, сразу применяем фильтр
                await ApplyFiltersAsync();
            }

            // Обновление заголовка ListBox
            FilterOption? changedOption = sender as FilterOption;
            if (changedOption == null) return;

            // Определяем, к какому ListBox относится опция
            if (_typeOptions.Contains(changedOption)) UpdateFilterHeaderText(FilterTypeListBox, FilterTypeText, "Тип плоттера");
            else if (_printingTypeOptions.Contains(changedOption)) UpdateFilterHeaderText(FilterPrintingTypeListBox, FilterPrintingTypeText, "Способ нанесения");
            else if (_manufacturerOptions.Contains(changedOption)) UpdateFilterHeaderText(FilterManufacturerListBox, FilterManufacturerText, "Производитель");
            else if (_printingFormatOptions.Contains(changedOption)) UpdateFilterHeaderText(FilterPrintingFormatListBox, FilterPrintingFormatText, "Формат печати");
            else if (_priningColorOptions.Contains(changedOption)) UpdateFilterHeaderText(FilterPrintingColorListBox, FilterPrintingColorText, "Цвет печати");
            else if (_positioningOptions.Contains(changedOption)) UpdateFilterHeaderText(FilterPositioningListBox, FilterPositioningText, "Тип подачи материала");
            else if (_materialOptions.Contains(changedOption)) UpdateFilterHeaderText(FilterMaterialListBox, FilterMaterialText, "Тип материала");
        }

        /// <summary>
        /// Создает новый объект Filter, заполняет его на основе UI и применяет фильтрацию.
        /// </summary>
        private async Task ApplyFiltersAsync(Filter? initialFilter = null)
        {
            Filter currentFilter = initialFilter ?? new Filter();

            currentFilter.Model = ModelSearchText;

            currentFilter.PriceRange = ParseRange<int>(MinPriceText, MaxPriceText);
            currentFilter.WidthRange = ParseRange<double>(MinWidthText, MaxWidthText);

            currentFilter.PlotterType = GetSelectedFlags<PlotterType>(_typeOptions);
            currentFilter.DrawingMethod = GetSelectedFlags<DrawingMethod>(_printingTypeOptions);
            currentFilter.PrintingType = GetSelectedFlags<PrintingType>(_priningColorOptions);
            currentFilter.PaperFormat = GetSelectedFlags<PaperFormat>(_printingFormatOptions);
            currentFilter.Material = GetSelectedFlags<Material>(_materialOptions);

            currentFilter.Positioning = GetSelectedNonFlagsEnum<Positioning>(_positioningOptions);

            currentFilter.Manufacturers = _manufacturerOptions
                                    .Where(o => o.IsSelected)
                                    .Select(o => (string)o.Value!)
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToList();

            try
            {
                List<Plotter> fetchedPlotters = await _dbCilent.GetFilteredPlottersAsync(currentFilter);

                Plotters.Clear();
                foreach (var plotter in fetchedPlotters)
                {
                    Plotters.Add(plotter);
                }
                /*if (Plotters.Count == 0)
                {
                    MessageBox.Show("По вашему запросу ничего не найдено.", "Результат поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                }*/
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка запроса к серверу: {ex.Message}\nПроверьте, запущен ли сервер на localhost:1111.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Сбрасывает все фильтры в UI и применяет их.
        /// </summary>
        private async void ResetFilters(object? parameter)
        {
            ModelSearchText = string.Empty;
            MinPriceText = string.Empty;
            MaxPriceText = string.Empty;
            MinWidthText = string.Empty;
            MaxWidthText = string.Empty;

            OnPropertyChanged(nameof(ModelSearchText));
            OnPropertyChanged(nameof(MinPriceText));
            OnPropertyChanged(nameof(MaxPriceText));
            OnPropertyChanged(nameof(MinWidthText));
            OnPropertyChanged(nameof(MaxWidthText));

            // Сбрасываем все FilterOption (снимаем все галочки)
            foreach (var option in _typeOptions) option.IsSelected = false;
            foreach (var option in _printingTypeOptions) option.IsSelected = false;
            foreach (var option in _manufacturerOptions) option.IsSelected = false;
            foreach (var option in _printingFormatOptions) option.IsSelected = false;
            foreach (var option in _priningColorOptions) option.IsSelected = false;
            foreach (var option in _positioningOptions) option.IsSelected = false;
            foreach (var option in _materialOptions) option.IsSelected = false;

            await ApplyFiltersAsync(new Filter());
        }

        /// <summary>
        /// Вспомогательный метод для парсинга диапазонных значений из строк.
        /// Возвращает ненулевые типы. При неудачном парсинге возвращает 0 или 0.0.
        /// </summary>
        private (T Min, T Max) ParseRange<T>(string minText, string maxText) where T : struct, IComparable<T>
        {
            T defaultMin = (T)Convert.ChangeType(0, typeof(T)); 
            T defaultMax = (T)Convert.ChangeType(0, typeof(T)); 

            T min = defaultMin;
            T max = defaultMax;

            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(minText, out int iMin))
                {
                    min = (T)(object)iMin;
                }
                if (int.TryParse(maxText, out int iMax))
                {
                    max = (T)(object)iMax;
                }
                else
                {
                    max = (T)(object)int.MaxValue;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                if (double.TryParse(minText.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double dMin))
                {
                    min = (T)(object)dMin;
                }

                if (double.TryParse(maxText.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double dMax))
                {
                    max = (T)(object)dMax;
                }
                else
                {
                    max = (T)(object)double.MaxValue;
                }
            }
            return (min, max);
        }

        /// <summary>
        /// Извлекает выбранные значения из ObservableCollection<FilterOption> и преобразует их во Flag Enum.
        /// </summary>
        /// <typeparam name="TEnum">Тип Enum (должен быть Flags Enum).</typeparam>
        /// <param name="options">Коллекция FilterOption.</param>
        /// <returns>Комбинированное значение Enum.</returns>
        private TEnum GetSelectedFlags<TEnum>(ObservableCollection<FilterOption> options) where TEnum : Enum
        {
            long combinedValue = 0; // Используем long для безопасного комбинирования флагов

            foreach (var option in options.Where(o => o.IsSelected))
            {
                if (option.Value is TEnum enumValue)
                {
                    combinedValue |= Convert.ToInt64(enumValue);
                }
            }
            return (TEnum)Enum.ToObject(typeof(TEnum), combinedValue);
        }

        /// <summary>
        /// Извлекает выбранное значение из ObservableCollection<FilterOption> для обычного (не Flags) Enum.
        /// </summary>
        /// <typeparam name="TEnum">Тип Enum.</typeparam>
        /// <param name="options">Коллекция FilterOption.</param>
        /// <returns>Выбранное значение Enum или значение по умолчанию (0/None).</returns>
        private TEnum GetSelectedNonFlagsEnum<TEnum>(ObservableCollection<FilterOption> options) where TEnum : Enum
        {
            var selectedOption = options.FirstOrDefault(o => o.IsSelected);
            if (selectedOption?.Value is TEnum enumValue)
            {
                return enumValue;
            }
            // Если ничего не выбрано или выбрано неверно, возвращаем значение по умолчанию (0)
            return (TEnum)Enum.ToObject(typeof(TEnum), 0);
        }

        /// <summary>
        /// Универсальная функция для создания коллекции FilterOption из Enum.
        /// Использует DescriptionAttribute, если доступен.
        /// </summary>
        /// <param name="enumType">Тип Enum.</param>
        /// <returns>ObservableCollection<FilterOption> с созданными опциями.</returns>
        private ObservableCollection<FilterOption> CreateFilterOptions(Type enumType)
        {
            var options = new ObservableCollection<FilterOption>();
            foreach (Enum enumValue in Enum.GetValues(enumType))
            {
                // Для флаговых Enum'ов, если значение равно 0 (None) и Enum помечен как Flags,
                // мы обычно не хотим показывать его как отдельную опцию выбора.
                // Оно подразумевается, когда ничего не выбрано.
                if (enumType.IsDefined(typeof(FlagsAttribute), false) && Convert.ToInt32(enumValue) == 0)
                {
                    continue;
                }

                string name = enumValue.ToString();
                // Пытаемся получить DescriptionAttribute для более читабельного имени
                FieldInfo? field = enumType.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
                    name = attribute?.Description ?? name; // Используем описание или имя, если описания нет
                }

                // Теперь мы устанавливаем и Name (для отображения), и Value (для логики фильтрации)
                options.Add(new FilterOption { Name = name, Value = enumValue, IsSelected = false });
            }
            return options;
        }

        private ObservableCollection<FilterOption> CreateManufacturerFilterOptions()
        {
            return new ObservableCollection<FilterOption>();
        }

        /// <summary>
        /// Изменяет вид кнопок, подсказки и отступы при разных режимах окна
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                TopPanel.Padding = new Thickness(5, 5, 5, 0);
                SearchBar.Padding = new Thickness(10, 15, 10 ,15);
                MainContent.Padding = new Thickness(5, 0, 5, 0);
                BottomPanel.Padding = new Thickness(0, 0, 0, 6);
                MaximizeRestoreButtonIcon.Data = (Geometry)this.FindResource("RestoreButton");
                MaximizeRestoreButton.ToolTip = "Свернуть в окно";
            }
            else if (this.WindowState == WindowState.Normal)
            {
                TopPanel.Padding = new Thickness(0);
                SearchBar.Padding = new Thickness(5, 15, 5, 15);
                MainContent.Padding = new Thickness(0);
                BottomPanel.Padding = new Thickness(0);
                MaximizeRestoreButtonIcon.Data = (Geometry)this.FindResource("MaximizeButton");
                MaximizeRestoreButton.ToolTip = "Развернуть";
            }
        }

        /// <summary>
        /// Изменяет окно при двойном нажатии
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    if (this.WindowState == WindowState.Maximized)
                    {
                        this.WindowState = WindowState.Normal;//Полный экран -> Окно
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;//Не полный экран -> Полный экран
                    }
                }

                // Позволяет перетаскивать окно
                this.DragMove();
            }
        }

        /// <summary>
        /// Кнопка Свернуть
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Кнопка Развернуть/Свернуть в окно
        /// </summary>
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Кнопка Закрыть
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _server.Stop();
            this.Close();
        }

        private void UpdateFilterHeaderText(ListBox filterListBox, TextBlock headerTextBlock, string filterCategoryName)
        {
            var allOptions = filterListBox.ItemsSource?.OfType<FilterOption>().ToList();

            if (allOptions == null || !allOptions.Any())
            {
                headerTextBlock.Text = $"{filterCategoryName}: любой";
                return;
            }

            int selectedCount = allOptions.Count(o => o.IsSelected);
            int totalCount = allOptions.Count;

            if (selectedCount == totalCount || selectedCount == 0)
            {
                headerTextBlock.Text = $"{filterCategoryName}: любой";
            }
            else
            {
                var selectedNames = allOptions.Where(o => o.IsSelected)
                                               .Select(o => o.Name)
                                               .ToList();
                headerTextBlock.Text = $"{filterCategoryName}: {string.Join(", ", selectedNames)}";
            }
        }

        private void ToggleFilterVisibility(ListBox filterListBox, Path iconPath)
        {
            bool isCurrentlyCollapsed = (filterListBox.Visibility == Visibility.Collapsed);

            filterListBox.Visibility = isCurrentlyCollapsed ? Visibility.Visible : Visibility.Collapsed;

            iconPath.Data = isCurrentlyCollapsed ? (Geometry)this.FindResource("AngleUp") : (Geometry)this.FindResource("AngleDown");
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem? item = sender as ListBoxItem;
            if (item == null) return;

            FilterOption? dataItem = item.Content as FilterOption;
            if (dataItem == null) return;

            dataItem.IsSelected = !dataItem.IsSelected;

            e.Handled = true;
        }
        //
        private void FilterTypeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterTypeListBox, FilterTypeButtonIcon);
        }

        private void FilterPrintingTypeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterPrintingTypeListBox, FilterPrintingTypeButtonIcon);
        }

        private void FilterManufacturerButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterManufacturerListBox, FilterManufacturerButtonIcon);
        }

        private void FilterPrintingFormatButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterPrintingFormatListBox, FilterPrintingFormatButtonIcon);
        }

        private void FilterPrintingColorButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterPrintingColorListBox, FilterPrintingColorButtonIcon);
        }

        private void FilterPositioningButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterPositioningListBox, FilterPositioningButtonIcon);
        }

        private void FilterMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterMaterialListBox, FilterMaterialButtonIcon);
        }

        private async void ModelSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ModelSearchText = ((TextBox)sender).Text;
            await ApplyFiltersAsync();
        }
    }
}