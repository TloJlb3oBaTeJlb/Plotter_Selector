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
            UpdateFilterTypeTextBlockState();
            UpdateFilterPrintingTypeTextBlockState();
        }

        //Изменяет вид кнопок, подсказки и отступы при разных режимах окна
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

        //Изменяет окно при двойном нажатии
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

        //Кнопка Свернуть
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //Кнопка Развернуть/Свернуть в окно
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

        //Кнопка Закрыть
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FilterTypeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateFilterTypeTextBlockState();
        }

        private void UpdateFilterTypeTextBlockState()
        {
            var checkBoxes = FilterTypeCheckBoxes.Children.OfType<CheckBox>().ToList();

            int checkedCount = checkBoxes.Count(cb => cb.IsChecked == true);

            if (checkedCount == checkBoxes.Count || checkedCount == 0)
            {
                FilterTypeText.Text = "Тип плоттера: Любой";
            }
            else
            {
                FilterTypeText.Text = string.Empty;
                foreach (CheckBox cb in FilterTypeCheckBoxes.Children.OfType<CheckBox>())
                {
                    if (cb.IsChecked == true)
                    {
                        if (FilterTypeText.Text.ToString() == string.Empty)
                        {
                            FilterTypeText.Text = "Тип плоттера: " + cb.Content;
                        }
                        else
                        {
                            FilterTypeText.Text = FilterTypeText.Text.ToString() + ", " + cb.Content;
                        }
                    }
                }
            }
        }

        private void FilterTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterType1.Visibility == Visibility.Collapsed)
            {
                foreach (CheckBox cb in FilterTypeCheckBoxes.Children.OfType<CheckBox>())
                {
                    cb.Visibility = Visibility.Visible;
                }
                FilterTypeButtonIcon.Data = (Geometry)this.FindResource("AngleUp");
            }
            else if(FilterType1.Visibility == Visibility.Visible)
            {
                foreach (CheckBox cb in FilterTypeCheckBoxes.Children.OfType<CheckBox>())
                {
                    cb.Visibility = Visibility.Collapsed;
                }
                FilterTypeButtonIcon.Data = (Geometry)this.FindResource("AngleDown");
            }
        }

        private void FilterPrintingTypeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateFilterPrintingTypeTextBlockState();
        }

        private void UpdateFilterPrintingTypeTextBlockState()
        {
            var checkBoxes = FilterPrintingTypeCheckBoxes.Children.OfType<CheckBox>().ToList();

            int checkedCount = checkBoxes.Count(cb => cb.IsChecked == true);

            if (checkedCount == checkBoxes.Count  || checkedCount == 0)
            {
                FilterPrintingTypeText.Text = "Тип печати: Любая";
            }
            else
            {
                FilterPrintingTypeText.Text = string.Empty;
                foreach (CheckBox cb in FilterPrintingTypeCheckBoxes.Children.OfType<CheckBox>())
                {
                    if (cb.IsChecked == true)
                    {
                        if (FilterPrintingTypeText.Text.ToString() == string.Empty)
                        {
                            FilterPrintingTypeText.Text = "Тип печати: " + cb.Content;
                        }
                        else
                        {
                            FilterPrintingTypeText.Text = FilterPrintingTypeText.Text.ToString() + ", " + cb.Content;
                        }
                    }
                }
            }
        }

        private void FilterPrintingTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterPrintingType1.Visibility == Visibility.Collapsed)
            {
                foreach (CheckBox cb in FilterPrintingTypeCheckBoxes.Children.OfType<CheckBox>())
                {
                    cb.Visibility = Visibility.Visible;
                }
                FilterPrintingTypeButtonIcon.Data = (Geometry)this.FindResource("AngleUp");
            }
            else if (FilterPrintingType1.Visibility == Visibility.Visible)
            {
                foreach (CheckBox cb in FilterPrintingTypeCheckBoxes.Children.OfType<CheckBox>())
                {
                    cb.Visibility = Visibility.Collapsed;
                }
                FilterPrintingTypeButtonIcon.Data = (Geometry)this.FindResource("AngleDown");
            }
        }
    }
}