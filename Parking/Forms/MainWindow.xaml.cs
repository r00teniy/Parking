using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Autodesk.AutoCAD.DatabaseServices;

using Parking.Models;

namespace Parking.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BindingList<XrefGraphNode> xRefs = new BindingList<XrefGraphNode>(Functions.DataImport.GetXRefList());
        public MainWindow()
        {

            InitializeComponent();

            selectedParkingBlocksXrefBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = xRefs });
            selectedParkingBlocksXrefBox.DisplayMemberPath = "Name";
            selectedParkingBlocksXrefBox.SelectedIndex = 0;
            selectedPlotsXrefBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = xRefs });
            selectedPlotsXrefBox.DisplayMemberPath = "Name";
            selectedPlotsXrefBox.SelectedIndex = 0;
            selectedZonesXrefBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = xRefs });
            selectedZonesXrefBox.DisplayMemberPath = "Name";
            selectedZonesXrefBox.SelectedIndex = 0;

            parkingBlockSearchTypeBox.ItemsSource = Variables.whereToFind;
            zonesBlockSearchTypeBox.ItemsSource = Variables.whereToFind;
            plotBlockSearchTypeBox.ItemsSource = Variables.whereToFind;
            parkingBlockSearchTypeBox.SelectedIndex = 0;
            zonesBlockSearchTypeBox.SelectedIndex = 0;
            plotBlockSearchTypeBox.SelectedIndex = 0;

            cityBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = Variables.cityList });
            cityBox.DisplayMemberPath = "Name";
            if (Variables.cityList.Count != 0)
            {
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
            cityBox.SelectedIndex = Variables.cityList.Count - 1;
        }

        private void deleteCityButton_Click(object sender, RoutedEventArgs e)
        {
            Variables.cityList.Remove((CityModel)cityBox.SelectedItem);
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
                if (xRefs.Count != 0)
                {
                    selectedParkingBlocksXrefBox.IsEnabled = true;
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

        private void zonesBlockSearchTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (zonesBlockSearchTypeBox.SelectedIndex == 1)
            {
                if (xRefs.Count != 0)
                {
                    selectedZonesXrefBox.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("В данном файле нет внешних ссылок", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                selectedZonesXrefBox.IsEnabled = false;
            }
        }

        private void plotBlockSearchTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (plotBlockSearchTypeBox.SelectedIndex == 1)
            {
                if (xRefs.Count != 0)
                {
                    selectedPlotsXrefBox.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("В данном файле нет внешних ссылок", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                selectedPlotsXrefBox.IsEnabled = false;
            }
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            Functions.DataProcessing.CreateParkingTableWithData((CityModel)cityBox.SelectedItem);
            Show();
        }

        private void infoButton_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWindow = new();
            infoWindow.Show();
        }
    }
}
