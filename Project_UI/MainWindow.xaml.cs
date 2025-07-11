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

        bool _isUpdatingFilterTypeMajorCheckBox = false;

        public MainWindow()
        {
            InitializeComponent();
            UpdateContent();
        }

        private void UpdateContent()
        {
            UpdateFilterHeaderText(FilterPrintingTypeCheckBoxes, FilterPrintingTypeText, "Тип печати");
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
        /// Обновляет текст заголовка фильтра в зависимости от выбранных CheckBox
        /// </summary>
        /// <param name="checkBoxContainer">StackPanel, содержащий CheckBox'ы фильтра.</param>
        /// <param name="headerTextBlock">TextBlock, в котором отображается заголовок фильтра</param>
        /// <param name="filterCategoryName">Имя категории фильтра</param>
        private void UpdateFilterHeaderText(StackPanel checkBoxContainer, TextBlock headerTextBlock, string filterCategoryName)
        {
            var checkBoxes = checkBoxContainer.Children.OfType<CheckBox>().ToList();

            int checkedCount = checkBoxes.Count(cb => cb.IsChecked == true);

            if (checkedCount == checkBoxes.Count || checkedCount == 0)
            {
                headerTextBlock.Text = $"{filterCategoryName}: Любой";
            }
            else
            {
                var selectedNames = checkBoxes.Where(cb => cb.IsChecked == true)
                                              .Select(cb => cb.Content?.ToString())
                                              .Where(name => !string.IsNullOrWhiteSpace(name))
                                              .ToList();

                if (selectedNames.Any())
                {
                    headerTextBlock.Text = $"{filterCategoryName}: {string.Join(", ", selectedNames)}";
                }
                else
                {
                    headerTextBlock.Text = $"{filterCategoryName}: Любой";
                }
            }
        }

        /// <summary>
        /// Переключает видимость CheckBox'ов внутри указанного контейнера и меняет иконку кнопки
        /// </summary>
        /// <param name="checkBoxContainer">StackPanel, содержащий CheckBox'ы фильтра</param>
        /// <param name="iconPath">Элемент Path, представляющий иконку кнопки</param>
        private void ToggleFilterVisibility(StackPanel checkBoxContainer, Path iconPath)
        {
            bool isCurrentlyCollapsed = true;
            if (checkBoxContainer.Children.OfType<CheckBox>().FirstOrDefault() is CheckBox firstCb)
            {
                isCurrentlyCollapsed = (firstCb.Visibility == Visibility.Collapsed);
            }

            Visibility newVisibility = isCurrentlyCollapsed ? Visibility.Visible : Visibility.Collapsed;
            Geometry newIconData = isCurrentlyCollapsed ? (Geometry)this.FindResource("AngleUp") : (Geometry)this.FindResource("AngleDown");

            foreach (CheckBox cb in checkBoxContainer.Children.OfType<CheckBox>())
            {
                cb.Visibility = newVisibility;
            }

            iconPath.Data = newIconData;
        }

        //========================================================================
        private void FilterTypeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateFilterHeaderText(FilterTypeCheckBoxes, FilterTypeText, "Тип плоттера");
        }

        private void FilterTypeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterTypeCheckBoxes, FilterTypeButtonIcon);
        }

        private void FilterPrintingTypeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateFilterHeaderText(FilterPrintingTypeCheckBoxes, FilterPrintingTypeText, "Тип печати");
        }

        private void FilterPrintingTypeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFilterVisibility(FilterPrintingTypeCheckBoxes, FilterPrintingTypeButtonIcon);
        }
    }
}