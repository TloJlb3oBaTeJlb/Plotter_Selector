using PlotterDbLib;
using Project_UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Project_UI
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PlotterDbAdminClient _dbAdmin { get; set; }
        public PlotterDbServer _server { get; set; }

        public ICommand CreateNewPlotterCommand { get; private set; }
        public ICommand SavePlotterCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand DeletePlotterCommand { get; private set; }

        // --- Свойства для привязки к UI ---
        private Plotter _selectedPlotter;
        public Plotter SelectedPlotter
        {
            get => _selectedPlotter;
            set
            {
                if (_selectedPlotter != value)
                {
                    // Проверяем наличие несохраненных изменений
                    if (HasUnsavedChanges && !PromptToSaveChanges())
                    {
                        // Если есть несохраненные изменения и пользователь отменил переход
                        return;
                    }

                    _selectedPlotter = value;
                    OnPropertyChanged(nameof(SelectedPlotter));
                    LoadEnumOptionsFromPlotter(); // Обновим Enum-опции при выборе нового плоттера
                    MarkChangesSaved(); // При смене плоттера, считаем, что предыдущие изменения сохранены/отменены
                }
            }
        }

        public double InitialLeft { get; set; }
        public double InitialTop { get; set; }
        public double InitialWidth { get; set; }
        public double InitialHeight { get; set; }

        public ObservableCollection<Plotter> AllPlotters { get; set; } = new ObservableCollection<Plotter>();
        public ObservableCollection<string> AllManufacturers { get; set; } = new ObservableCollection<string>();

        // ObservableCollections для Enum'ов
        public ObservableCollection<FilterOption> PlotterTypeOptions { get; set; }
        public ObservableCollection<FilterOption> DrawingMethodOptions { get; set; }
        public ObservableCollection<FilterOption> PositioningOptions { get; set; }
        public ObservableCollection<FilterOption> PrintingTypeOptions { get; set; }
        public ObservableCollection<FilterOption> PaperFormatOptions { get; set; }
        public ObservableCollection<FilterOption> MaterialOptions { get; set; }

        private bool _hasUnsavedChanges = false;
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    OnPropertyChanged(nameof(HasUnsavedChanges));
                    // Можно также обновить состояние кнопок "Сохранить"/"Отмена"
                    ((RelayCommand)SavePlotterCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public AdminWindow()
        {
            InitializeComponent();
        }

        public AdminWindow(PlotterDbServer server,PlotterDbAdminClient dbAdmin, double left, double top, double width, double height) : this()
        {
            this._dbAdmin = dbAdmin;
            this._server = server;

            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            this.WindowStartupLocation = WindowStartupLocation.Manual;

            CreateNewPlotterCommand = new RelayCommand(_ => CreateNewPlotter());
            SavePlotterCommand = new RelayCommand(async _ => await SavePlotter(), _ => HasUnsavedChanges);
            CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => HasUnsavedChanges);
            DeletePlotterCommand = new RelayCommand(async _ => await DeletePlotter(), _ => SelectedPlotter?.PlotterId > 0);


            PlotterTypeOptions = CreateFilterOptions(typeof(PlotterType));
            DrawingMethodOptions = CreateFilterOptions(typeof(DrawingMethod));
            PositioningOptions = CreateFilterOptions(typeof(Positioning)); // Non-Flags Enum
            PrintingTypeOptions = CreateFilterOptions(typeof(PrintingType));
            PaperFormatOptions = CreateFilterOptions(typeof(PaperFormat));
            MaterialOptions = CreateFilterOptions(typeof(Material));

            this.DataContext = this;

            if (SelectedPlotter != null) // Если SelectedPlotter инициализирован в конструкторе
            {
                SelectedPlotter.PropertyChanged += SelectedPlotter_PropertyChanged;
            }
            // А также на изменения в IsSelected для FilterOption'ов
            SubscribeToFilterOptionChanges(PlotterTypeOptions);
            SubscribeToFilterOptionChanges(DrawingMethodOptions);
            SubscribeToFilterOptionChanges(PositioningOptions);
            SubscribeToFilterOptionChanges(PrintingTypeOptions);
            SubscribeToFilterOptionChanges(PaperFormatOptions);
            SubscribeToFilterOptionChanges(MaterialOptions);

            // Загружаем данные при открытии окна
            LoadDataForAdmin();

            this.Closing += AdminWindow_Closing;
        }

        private void SubscribeToFilterOptionChanges(ObservableCollection<FilterOption> options)
        {
            foreach (var option in options)
            {
                option.PropertyChanged -= FilterOption_IsSelectedChanged; // Избегаем дублирования
                option.PropertyChanged += FilterOption_IsSelectedChanged;
            }
        }

        private void FilterOption_IsSelectedChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterOption.IsSelected))
            {
                MarkChangesMade();
            }
        }

        // Этот метод будет загружать плоттеры для администрирования
        // Загрузка данных для администрирования (всех плоттеров и производителей)
        private async Task LoadDataForAdmin()
        {
            try
            {
                var allPlotters = await _dbAdmin.GetFilteredPlottersAsync(new Filter());
                AllPlotters.Clear();
                foreach (var plotter in allPlotters.OrderBy(p => p.Model))
                {
                    AllPlotters.Add(plotter);
                }

                AllManufacturers.Clear();
                foreach (var m in allPlotters.Where(p => !string.IsNullOrEmpty(p.Manufacturer)).Select(p => p.Manufacturer).Distinct().OrderBy(m => m))
                {
                    AllManufacturers.Add(m);
                }

                // Инициализируем SelectedPlotter после загрузки всех плоттеров
                // Если плоттеры есть, выбираем первый, иначе создаем новый
                SelectedPlotter = AllPlotters.FirstOrDefault() ?? new Plotter();
                // Также нужно подписаться на PropertyChanged у _selectedPlotter,
                // потому что его экземпляр может меняться (SelectedPlotter = new Plotter();)
                _selectedPlotter.PropertyChanged += SelectedPlotter_PropertyChanged;

                MarkChangesSaved(); // Считаем, что после загрузки нет несохраненных изменений
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных для администрирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Создать новый плоттер
        private void CreateNewPlotter()
        {
            if (HasUnsavedChanges && !PromptToSaveChanges())
            {
                return; // Пользователь отменил создание нового плоттера
            }

            if (_selectedPlotter != null)
            {
                _selectedPlotter.PropertyChanged -= SelectedPlotter_PropertyChanged; // Отписываемся от старого
            }
            SelectedPlotter = new Plotter(); // Создаем новый пустой объект
            _selectedPlotter.PropertyChanged += SelectedPlotter_PropertyChanged; // Подписываемся на новый

            MarkChangesSaved(); // Новый плоттер не имеет изменений пока что
        }

        // Сохранить плоттер
        private async Task SavePlotter()
        {
            if (SelectedPlotter == null) return;

            try
            {
                UpdatePlotterEnumsFromOptions(); // Обновляем Enum-значения в SelectedPlotter

                if (SelectedPlotter.PlotterId == 0) // Это новый плоттер
                {
                    await _dbAdmin.AddPlotterAsync(SelectedPlotter);
                    MessageBox.Show("Плоттер успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    // После создания, нужно обновить список и выбрать только что созданный
                    await LoadDataForAdmin(); // Перезагружаем все плоттеры
                    SelectedPlotter = AllPlotters.FirstOrDefault(p => p.Model == SelectedPlotter.Model && p.Manufacturer == SelectedPlotter.Manufacturer); // Ищем его по уникальным полям
                }
                else // Это существующий плоттер
                {
                    await _dbAdmin.UpdatePlotterAsync(SelectedPlotter);
                    MessageBox.Show("Плоттер успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Обновляем отображение в списке AllPlotters
                    var existingPlotter = AllPlotters.FirstOrDefault(p => p.PlotterId == SelectedPlotter.PlotterId);
                    if (existingPlotter != null)
                    {
                        // Просто обновите свойства существующего объекта, чтобы UI обновился
                        // Можно также перезагрузить весь список, если это проще
                        int index = AllPlotters.IndexOf(existingPlotter);
                        AllPlotters[index] = SelectedPlotter; // Это обновит ссылку, если нужна новая
                        // Или просто скопировать свойства, если SelectedPlotter не новый экземпляр
                        // existingPlotter.Model = SelectedPlotter.Model; // и т.д.
                    }
                }
                MarkChangesSaved(); // После сохранения нет несохраненных изменений
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения плоттера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Отменить изменения
        private async void CancelEdit()
        {
            if (!PromptToSaveChanges(isCancel: true))
            {
                return; // Пользователь отменил отмену
            }

            if (SelectedPlotter.PlotterId == 0) // Если это новый плоттер, просто сбросить форму
            {
                SelectedPlotter = new Plotter(); // Очистить форму
            }
            else // Перезагрузить данные из БД для текущего плоттера
            {
                try
                {
                    //SelectedPlotter = await _dbAdmin.GetPlotterByIdAsync(SelectedPlotter.PlotterId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка перезагрузки данных плоттера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            MarkChangesSaved(); // После отмены нет несохраненных изменений
        }

        // Удалить плоттер
        private async Task DeletePlotter()
        {
            if (SelectedPlotter == null || SelectedPlotter.PlotterId == 0)
            {
                MessageBox.Show("Выберите плоттер для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить плоттер '{SelectedPlotter.Model}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _dbAdmin.RemovePlotterAsync(SelectedPlotter);
                    MessageBox.Show("Плоттер успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataForAdmin(); // Перезагружаем список
                    SelectedPlotter = AllPlotters.FirstOrDefault() ?? new Plotter(); // Выбираем первый или новый
                    MarkChangesSaved(); // После удаления нет несохраненных изменений
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления плоттера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Вызывается, когда какое-либо свойство SelectedPlotter меняется
        private void SelectedPlotter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            MarkChangesMade();
        }

        private void MarkChangesMade()
        {
            HasUnsavedChanges = true;
        }

        private void MarkChangesSaved()
        {
            HasUnsavedChanges = false;
        }

        // Запрос пользователю о сохранении изменений
        private bool PromptToSaveChanges(bool isCancel = false)
        {
            /*if (!HasUnsavedChanges) return true; // Изменений нет, можно продолжать

            string message = isCancel ? "У вас есть несохраненные изменения. Вы уверены, что хотите отменить их?" :
                                        "У вас есть несохраненные изменения. Сохранить их?";

            MessageBoxResult result = MessageBox.Show(message, "Несохраненные изменения",
                isCancel ? MessageBoxButton.YesNo : MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (isCancel)
            {
                return result == MessageBoxResult.Yes; // Yes = отменить, No = не отменять
            }
            else
            {
                if (result == MessageBoxResult.Yes)
                {
                    SavePlotter(); // Вызываем сохранение
                    return true; // Продолжаем действие после сохранения
                }
                return result == MessageBoxResult.No; // No = не сохранять, но продолжить
                                                      // Cancel = не продолжать и не сохранять
            }*/
            return true;
        }

        // Обработчик события закрытия окна AdminWindow
        private async void AdminWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (HasUnsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show(
                    "У вас есть несохраненные изменения. Выйти без сохранения?",
                    "Несохраненные изменения",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true; // Отменить закрытие окна
                }
                // Если Yes, то продолжить закрытие (изменения будут потеряны)
            }
        }

        // Метод для создания FilterOption из Enum (скопирован из MainWindow)
        private ObservableCollection<FilterOption> CreateFilterOptions(Type enumType)
        {
            var options = new ObservableCollection<FilterOption>();
            foreach (Enum enumValue in Enum.GetValues(enumType))
            {
                // Исключаем 0 для флаговых Enum'ов, если он не представляет "None"
                if (enumType.IsDefined(typeof(FlagsAttribute), false) && Convert.ToInt32(enumValue) == 0)
                {
                    continue;
                }

                string name = enumValue.ToString();
                FieldInfo? field = enumType.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
                    name = attribute?.Description ?? name;
                }
                options.Add(new FilterOption { Name = name, Value = enumValue, IsSelected = false });
            }
            return options;
        }

        // Метод для загрузки Enum-значений из SelectedPlotter в FilterOption'ы
        private void LoadEnumOptionsFromPlotter()
        {
            if (SelectedPlotter == null) return;

            // Флаговые Enum'ы: устанавливаем IsSelected для соответствующих флагов
            SetFlagsFromEnum(PlotterTypeOptions, SelectedPlotter.PlotterType);
            SetFlagsFromEnum(DrawingMethodOptions, SelectedPlotter.DrawingMethod);
            SetFlagsFromEnum(PrintingTypeOptions, SelectedPlotter.PrintingType);
            SetFlagsFromEnum(PaperFormatOptions, SelectedPlotter.PaperFormat);
            SetFlagsFromEnum(MaterialOptions, SelectedPlotter.Material);

            // Обычные Enum'ы (RadioButtons): выбираем один
            SetNonFlagEnum(PositioningOptions, SelectedPlotter.Positioning);
        }

        public static void SetFlagsFromEnum<TEnum>(ObservableCollection<FilterOption> options, TEnum enumValue) where TEnum : Enum
        {
            long value = Convert.ToInt64(enumValue);
            foreach (var option in options)
            {
                // Объявляем optionEnum ЗДЕСЬ, чтобы оно было доступно в обоих if/else if блоках
                if (option.Value is TEnum optionEnum) // Проверяем и сразу приводим
                {
                    if (Convert.ToInt64(optionEnum) != 0)
                    {
                        option.IsSelected = (value & Convert.ToInt64(optionEnum)) == Convert.ToInt64(optionEnum);
                    }
                    else // Это блок для optionEnum, где Convert.ToInt64(optionEnum) == 0
                    {
                        // Обработка 0-значения для флагового Enum, если оно используется как "None"
                        // В вашем случае, если 0 исключается (как в EnumHelper.CreateFilterOptions),
                        // этот 'else' блок будет редко достигаться или вообще не будет нужен.
                        // Если вы хотите, чтобы "None" или 0-значение представляло собой отсутствие флагов:
                        // option.IsSelected = (value == 0);
                    }
                }
            }
        }

        private void SetNonFlagEnum<TEnum>(ObservableCollection<FilterOption> options, TEnum enumValue) where TEnum : Enum
        {
            foreach (var option in options)
            {
                option.IsSelected = option.Value?.Equals(enumValue) ?? false;
            }
        }

        // Метод для обновления SelectedPlotter из FilterOption'ов (вызывается при сохранении)
        private void UpdatePlotterEnumsFromOptions()
        {
            if (SelectedPlotter == null) return;

            SelectedPlotter.PlotterType = GetSelectedFlags<PlotterType>(PlotterTypeOptions);
            SelectedPlotter.DrawingMethod = GetSelectedFlags<DrawingMethod>(DrawingMethodOptions);
            SelectedPlotter.PrintingType = GetSelectedFlags<PrintingType>(PrintingTypeOptions);
            SelectedPlotter.PaperFormat = GetSelectedFlags<PaperFormat>(PaperFormatOptions);
            SelectedPlotter.Material = GetSelectedFlags<Material>(MaterialOptions);
            SelectedPlotter.Positioning = GetSelectedNonFlagsEnum<Positioning>(PositioningOptions);
        }

        // Эти вспомогательные методы скопированы из MainWindow
        private TEnum GetSelectedFlags<TEnum>(ObservableCollection<FilterOption> options) where TEnum : Enum
        {
            long combinedValue = 0;
            foreach (var option in options.Where(o => o.IsSelected))
            {
                if (option.Value is Enum enumVal && enumVal.GetType() == typeof(TEnum))
                {
                    combinedValue |= Convert.ToInt64(enumVal);
                }
            }
            return (TEnum)Enum.ToObject(typeof(TEnum), combinedValue);
        }

        private TEnum GetSelectedNonFlagsEnum<TEnum>(ObservableCollection<FilterOption> options) where TEnum : Enum
        {
            var selectedOption = options.FirstOrDefault(o => o.IsSelected);
            if (selectedOption?.Value is Enum enumVal && enumVal.GetType() == typeof(TEnum))
            {
                return (TEnum)enumVal;
            }
            // Возвращаем дефолтное значение Enum (обычно 0) если ничего не выбрано
            return (TEnum)Enum.ToObject(typeof(TEnum), 0);
        }

        //Вспомогательные классы
        // IdToVisibilityConverter для кнопок/элементов, зависящих от ID плоттера (например, кнопки Удалить)
        public class IdToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is int id && id > 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            private readonly Func<object?, bool>? _canExecute;

            public event EventHandler? CanExecuteChanged;

            public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

            public void Execute(object? parameter) => _execute(parameter);

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        //Buttons
        /// <summary>
        /// Изменяет вид кнопок, подсказки и отступы при разных режимах окна
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                TopPanel.Padding = new Thickness(5, 5, 5, 0);
                SearchBar.Padding = new Thickness(10, 15, 10, 15);
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
            Application.Current.Shutdown();
        }

        private void UserModeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
