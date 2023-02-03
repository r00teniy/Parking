using System.Windows;

namespace Parking.Forms;
/// <summary>
/// Interaction logic for CreateCityWindow.xaml
/// </summary>
public partial class CreateCityWindow : Window
{
    public CreateCityWindow()
    {
        InitializeComponent();
    }

    private void helpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpWindow help = new HelpWindow();
        help.Show();
    }

    private void closeButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CreateCityButton_Click(object sender, RoutedEventArgs e)
    {
        //TODO: add checks
        Variables.cityList.Add(new Models.CityModel(LongParkingReq.Text, GuestParkingReq.Text, OfficeParkingReq.Text, StoreParkingReq.Text, CityName.Text, SqMPerPerson.Text));
        MessageBox.Show("Параметры города созданы", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
