using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EdicaoFotos;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SelectImageButton_Click(object sender, RoutedEventArgs e)
    {
        // Create an OpenFileDialog to select an image
        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*";

        // Show the dialog and check if the user selected a file
        if (openFileDialog.ShowDialog() == true)
        {
            var editorWindow = new EditorWindow(openFileDialog.FileName);
            editorWindow.Show();
            this.Close(); // Close the main window after opening the editor window'
        }
    }
}