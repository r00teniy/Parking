using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using StageWorkScripts.Functions;

namespace StageWorkScripts.Forms;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Variables _variables;
    private DataImport _import;
    private DataExport _export;
    ObservableCollection<string> currentCurbTypes;
    ObservableCollection<string> currentPavementTypes;
    ObservableCollection<string> currentGreeneryTypes;
    ObservableCollection<Dictionary<string, string>> pavementHatchPatterns;
    ObservableCollection<Dictionary<string, string>> greeneryHatchPatterns;
    public MainWindow(Variables variables)
    {
        InitializeComponent();
        _variables = variables;
        _import = new DataImport();
        _export = new DataExport();
        //setting up curbTypeComboBox
        currentCurbTypes = new(_import.GetAllLayersContainingString(_variables.curbLayerStart));
        CurbTypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = currentCurbTypes });
        if (currentCurbTypes.Count != 0)
        {
            CurbTypeComboBox.SelectedIndex = 0;
        }
        currentPavementTypes = new(_import.GetAllLayersContainingString(_variables.pavementLayerStart));
        PavementTypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = currentPavementTypes });
        if (currentPavementTypes.Count != 0)
        {
            PavementTypeComboBox.SelectedIndex = 0;
        }
        currentGreeneryTypes = new(_import.GetAllLayersContainingString(_variables.greeneryLayerStart));
        GreeneryTypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = currentGreeneryTypes });
        if (currentGreeneryTypes.Count != 0)
        {
            GreeneryTypeComboBox.SelectedIndex = 0;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CreateCrurbPolylineButton_Click(object sender, RoutedEventArgs e)
    {
        _export.CreateCurb(currentCurbTypes[CurbTypeComboBox.SelectedIndex]);
    }
}
