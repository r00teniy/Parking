using System.Windows;

namespace Parking.Forms;
/// <summary>
/// Interaction logic for HelpWindow.xaml
/// </summary>
public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
    }


    private void closeHelpButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
