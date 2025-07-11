using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

namespace Project_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ObservableCollection<FilterOption> _typeOptions;
        private ObservableCollection<FilterOption> _printingTypeOptions;
        private ObservableCollection<FilterOption> _manufacturerOptions;

        public MainWindow()
        {
            InitializeComponent();

            _typeOptions = CreateFilterOptions("печатающий", "режущий", "гибридный");
            FilterTypeListBox.ItemsSource = _typeOptions;
            InitializeFilterType();

            _printingTypeOptions = CreateFilterOptions("перьевой", "струйный", "электрический", "лазерный (светодиодный)", "с термоподдачей");
            FilterPrintingTypeListBox.ItemsSource = _printingTypeOptions;
            InitializeFilterPrintingType();

            _manufacturerOptions = CreateFilterOptions("Производитель 1","Производитель 2","Производитель 3","Производитель 4");
            FilterManufacturerListBox.ItemsSource = _manufacturerOptions;
            InitializeFilterManufacturer();
        }

        private void InitializeFilterType()
        {
            foreach (var option in _typeOptions)
            {
                option.PropertyChanged += FilterOption_IsSelectedChanged;
            }

            UpdateFilterHeaderText(FilterTypeListBox, FilterTypeText, "Тип плоттера");
        }
        private void InitializeFilterPrintingType()
        {
            foreach (var option in _printingTypeOptions)
            {
                option.PropertyChanged += FilterOption_IsSelectedChanged;
            }

            UpdateFilterHeaderText(FilterPrintingTypeListBox, FilterPrintingTypeText, "Способ печати");
        }
        private void InitializeFilterManufacturer()
        {
            
            foreach(var option in _manufacturerOptions)
            {
                option.PropertyChanged += FilterOption_IsSelectedChanged;
            }
            
            UpdateFilterHeaderText(FilterManufacturerListBox, FilterManufacturerText, "Производитель");
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
            this.Close();
        }

        /// <summary>
        /// Кнопка поиска
        /// </summary>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Универсальная функция для создания коллекции FilterOption
        /// </summary>
        /// <param name="optionNames">Массив строк с названиями опций</param>
        /// <returns>ObservableCollection<FilterOption> с созданными опциями</returns>
        private ObservableCollection<FilterOption> CreateFilterOptions(params string[] optionNames)
        {
            var options = new ObservableCollection<FilterOption>();
            foreach (var name in optionNames)
            {
                options.Add(new FilterOption { Name = name, IsSelected = false });
            }
            return options;
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

        private void FilterOption_IsSelectedChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterOption.IsSelected))
            {
                FilterOption? changedOption = sender as FilterOption;
                if (changedOption == null) return;

                if (_typeOptions.Contains(changedOption))
                {
                    UpdateFilterHeaderText(FilterTypeListBox, FilterTypeText, "Тип плоттера");
                }
                else if (_printingTypeOptions.Contains(changedOption))
                {
                    UpdateFilterHeaderText(FilterPrintingTypeListBox, FilterPrintingTypeText, "Способ печати");
                }
                else if (_manufacturerOptions.Contains(changedOption))
                {
                    UpdateFilterHeaderText(FilterManufacturerListBox, FilterManufacturerText, "Производитель");
                }
            }
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
    }
}