using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace SW_BFM
{
    /// <summary>
    /// Interaction logic for playerSummary.xaml
    /// </summary>
    public partial class playerSummary : Window
    {
        public string userName;

        public playerSummary()
        {
            InitializeComponent();
            userName = "anonymous";

            try
            {
                StreamReader srrr = new StreamReader("wyniki.txt");
                string[] str = srrr.ReadToEnd().Split('\n');
                playerListView.ItemsSource = str;

                srrr.Close();
            }
            catch (FileNotFoundException)
            {
                StreamWriter swww = new StreamWriter("wyniki.txt");
            }
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(userNameTextBox.Text))
            {
                userName = userNameTextBox.Text;
                StreamWriter sw = new StreamWriter("wyniki.txt", true);
                sw.WriteLine(userName + ": " + MainWindow.score);
                sw.Close();
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            else
            {
                MessageBox.Show("Niepoprawna akcja! Proszę uzupełnić puste pola", "Błąd");
            }
        }
    }
}