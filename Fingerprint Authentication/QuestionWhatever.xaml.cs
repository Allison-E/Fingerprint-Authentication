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

namespace Fingerprint_Authentication
{
    /// <summary>
    /// Interaction logic for QuestionWhatever.xaml
    /// </summary>
    public partial class QuestionWhatever : Window
    {
        MainWindow mainWindow;
        string ID;

        public QuestionWhatever(string ID)
        {
            InitializeComponent();
            this.ID = ID;
        }

        private void enrollButton_Click(object sender, RoutedEventArgs e)
        {
            startMainWindow("enroll");
        }

        private void verifyButton_Click(object sender, RoutedEventArgs e)
        {
            startMainWindow("verify");
        }

        private void startMainWindow(string functionToExecute)
        {
            mainWindow = new MainWindow(functionToExecute, ID);
            Close();
            mainWindow.Show();
        }
    }
}
