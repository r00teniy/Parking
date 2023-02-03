using System.Windows;

using Parking.Models;

namespace Parking.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            parkingBlockSearchTypeBox.ItemsSource = Variables.whereToFind;
            zonesBlockSearchTypeBox.ItemsSource = Variables.whereToFind;
            plotBlockSearchTypeBox.ItemsSource = Variables.whereToFind;
            parkingBlockSearchTypeBox.SelectedIndex = 0;
            zonesBlockSearchTypeBox.SelectedIndex = 0;
            plotBlockSearchTypeBox.SelectedIndex = 0;

            if (Variables.cityList.Count != 0)
            {
                cityBox.ItemsSource = Variables.cityList;
                cityBox.DisplayMemberPath = "Name";
                cityBox.SelectedIndex = 0;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void createCityButton_Click(object sender, RoutedEventArgs e)
        {
            CreateCityWindow createCityWindow = new CreateCityWindow();
            createCityWindow.ShowDialog();
            cityBox.ItemsSource = Variables.cityList;
            cityBox.SelectedIndex = Variables.cityList.Count - 1;
        }

        private void deleteCityButton_Click(object sender, RoutedEventArgs e)
        {
            Variables.cityList.Remove((CityModel)cityBox.SelectedItem);
            cityBox.DataContext = Variables.cityList;
            if (Variables.cityList.Count != 0)
            {
                cityBox.SelectedIndex = Variables.cityList.Count - 1;
            }
            MessageBox.Show("Параметры удалены", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void parkingBlockSearchTypeBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (parkingBlockSearchTypeBox.SelectedIndex == 1)
            {
                selectedParkingBlocksXrefBox.IsEnabled = true;
                var xRefs = Functions.DataImport.GetXRefList();
                if (xRefs != null)
                {
                    selectedParkingBlocksXrefBox.ItemsSource = xRefs;
                    selectedParkingBlocksXrefBox.DisplayMemberPath = "Name";
                    selectedParkingBlocksXrefBox.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("В данном файле нет внешних ссылок", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                selectedParkingBlocksXrefBox.IsEnabled = false;
            }
        }
    }
}
