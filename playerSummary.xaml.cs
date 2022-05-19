using System;
using System.Collections.Generic;
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

namespace SW_BFM
{
    /// <summary>
    /// Interaction logic for playerSummary.xaml
    /// </summary>
    public partial class playerSummary : Window
    {
        public string userName;
        public PlayersDataSet playerDataSet = new PlayersDataSet();
        public PlayersDataSetTableAdapters.PlayersTableAdapter adapter = new PlayersDataSetTableAdapters.PlayersTableAdapter();
        public playerSummary()
        {
            InitializeComponent();
            userName = "anonymous";
            adapter.Fill(playerDataSet.Players);
            this.DataContext = playerDataSet.Players.DefaultView;

        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(userNameTextBox.Text))
            {
                PlayersDataSet.PlayersRow newPlayersRow = playerDataSet.Players.NewPlayersRow();
                newPlayersRow.Username = userNameTextBox.Text;
                newPlayersRow.Score = MainWindow.score;
                playerDataSet.Players.AddPlayersRow(newPlayersRow);
                adapter.Update(playerDataSet);

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
