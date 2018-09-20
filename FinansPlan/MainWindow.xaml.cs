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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinansPlan
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var ac = new AlfaCreditCard(DateTime.Parse("15.08.2018"), 1490);
            ac.AddTran(DateTime.Parse("17.08.2018"), -149000, 1,TranCat.payCard);            
            ac.AddTran(DateTime.Parse("19.08.2018"), -16661.43, 1,TranCat.payCard);
            ac.AddTran(DateTime.Parse("19.08.2018"), -13700, 1,TranCat.getCash);
            //ac.AddTran(DateTime.Parse("15.09.2018"), 12000, 1,TranCat.payCard);
            ac.Recalc();
            var dat = DateTime.Parse("20.08.2018");//DateTime.Today
            var total = ac.GetTotal(dat);
            var gett = ac.GetMaxCash(dat, true);
            textBlock1.Text = $"Total: {total}  Available:{gett} / {ac.GetMaxCash(dat, false)} ";
            listBox1.Items.Clear();
            foreach (var t in ac.Transactions.trans) listBox1.Items.Add(t);

            var uni = new UnicreditAutoCredit();//DateTime.Parse("9.08.2018"), 233640.32, 24,11.3) 
            uni.Recalc();
            listBox1.Items.Clear();            
            foreach (var t in uni.Transactions.trans) listBox1.Items.Add(t);
            textBlock1.Text = $"Total: {uni.Transactions.trans.Sum(pp=>pp.sum)}";
        }
    }
}
