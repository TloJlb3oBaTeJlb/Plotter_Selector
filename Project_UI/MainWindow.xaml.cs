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
            UpdateMasterCheckBoxState();
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

        private void FilterTypeMajorCheckBox_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            bool newIsCheckedState = (FilterTypeMajorCheckBox.IsChecked == true);

            _isUpdatingFilterTypeMajorCheckBox = true;
            try
            {
                FilterTypeMajorCheckBox.IsChecked = newIsCheckedState;
                SetAllChildCheckBoxes(newIsCheckedState);
            }
            finally
            {
                _isUpdatingFilterTypeMajorCheckBox = false;
            }
        }

        private void SetAllChildCheckBoxes (bool isChecked)
        {
            _isUpdatingFilterTypeMajorCheckBox = true;
            foreach (CheckBox cb in FilterTypeChildCheckBoxes.Children.OfType<CheckBox>())
            {
                cb.IsChecked = isChecked;
            }
            _isUpdatingFilterTypeMajorCheckBox = false;
        }

        private void ChildCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateMasterCheckBoxState();
        }

        private void ChildCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateMasterCheckBoxState();
        }

        private void UpdateMasterCheckBoxState()
        {
            _isUpdatingFilterTypeMajorCheckBox = true;

            var childCheckBoxes = FilterTypeChildCheckBoxes.Children.OfType<CheckBox>().ToList();

            if (!childCheckBoxes.Any())
            {
                FilterTypeMajorCheckBox.IsChecked = false;
                FilterTypeMajorCheckBox.Content = "Все фильтры";
            }
            else
            {
                int checkedCount = childCheckBoxes.Count(cb => cb.IsChecked == true);

                if (checkedCount == childCheckBoxes.Count)
                {
                    FilterTypeMajorCheckBox.IsChecked = true;
                    FilterTypeMajorCheckBox.Content = "Все фильтры";
                }
                else if (checkedCount == 0)
                {
                    FilterTypeMajorCheckBox.IsChecked = false;
                    FilterTypeMajorCheckBox.Content = "Все фильтры";
                }
                else
                {
                    FilterTypeMajorCheckBox.IsChecked = null;
                    FilterTypeMajorCheckBox.Content = string.Empty;
                    foreach (CheckBox cb in FilterTypeChildCheckBoxes.Children.OfType<CheckBox>())
                    {
                        if (cb.IsChecked == true)
                        {
                            if (FilterTypeMajorCheckBox.Content.ToString() == string.Empty)
                            {
                                FilterTypeMajorCheckBox.Content = cb.Content;
                            }
                            else
                            {
                                FilterTypeMajorCheckBox.Content = FilterTypeMajorCheckBox.Content.ToString() + ", "+ cb.Content;
                            }
                        }
                    }
                }
            }
            _isUpdatingFilterTypeMajorCheckBox = false;
        }

        private void FilterTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if(FilterTypeChild1.Visibility == Visibility.Collapsed)
            {
                FilterTypeChild1.Visibility = Visibility.Visible;
                FilterTypeChild2.Visibility = Visibility.Visible;
                FilterTypeChild3.Visibility = Visibility.Visible;
                FilterTypeButtonIcon.Data = (Geometry)this.FindResource("AngleUp");
            }
            else if(FilterTypeChild1.Visibility == Visibility.Visible)
            {
                FilterTypeChild1.Visibility = Visibility.Collapsed;
                FilterTypeChild2.Visibility = Visibility.Collapsed;
                FilterTypeChild3.Visibility = Visibility.Collapsed;
                FilterTypeButtonIcon.Data = (Geometry)this.FindResource("AngleDown");
            }
        }
    }
}