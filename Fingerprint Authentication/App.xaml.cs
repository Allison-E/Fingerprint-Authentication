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
        Dictionary<string, string> arguments;

        private void Application_start(object sender, StartupEventArgs e)
        {
            arguments = new Dictionary<string, string>();
            // Puts the arguments into the Dictionary as key/value pair.
            for (int i = 0; i < e.Args.Length; i++)
            {
                if (isEven(i))
                {
                    arguments.Add(e.Args[i], e.Args[i + 1]);
                    i++;
                }
            }
            MainWindow mainWindow = new MainWindow(arguments);
            mainWindow.Show();
        }

        private bool isEven(int number)
        {
            int remainder;
            Math.DivRem(number, 2, out remainder);

            if (remainder == 0)
                return true;
            else
                return false;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}
