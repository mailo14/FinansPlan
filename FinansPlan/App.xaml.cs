using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FinansPlan
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            /* double s = 10000.0 - 10000.01;
            s = Math.Round(s, 2);
            decimal d = 10000.0m - 10000.01m;

            if (d == -0.01m)
                 MessageBox.Show("");
            if (s == -0.01)
                 MessageBox.Show("");
             */

            base.OnStartup(e);

            PlanHorizont = DateTime.Today.AddYears(1);
        }
    public static DateTime PlanHorizont;
    }
}
