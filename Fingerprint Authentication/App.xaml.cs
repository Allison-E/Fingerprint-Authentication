using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Fingerprint_Authentication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_start(object sender, StartupEventArgs e)
        {
            QuestionWhatever question = null;

            if (e.Args.Count() == 1)
                question = new QuestionWhatever(e.Args[0]);
            question.Show();
        }
    }
}
