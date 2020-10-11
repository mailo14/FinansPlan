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

namespace FinansPlan2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DateTime dat = DateTime.Parse("1.08.19");
            var di = (dat.AddDays(1) - dat).TotalDays;
            var a = new AlfaCredit100();
            a.Start(dat);

            List<string> states = new List<string>() { a.state };
            while (true)
            {                
                dat=dat.AddDays(1);
                a.curDat = dat;
                a.DayStart(dat);
                if (dat == DateTime.Parse("16.08.19"))
                    a.AddSum(dat, 80m, true);

                if (dat == DateTime.Parse("17.08.19"))
                    a.AddSum(dat, 1500m, true);
                a.DayEnd(dat);
                states.Add(a.state);
            }

            InitializeComponent();
        }
    }

    public class AlfaCredit100
    {
       public DateTime startDat;
       public DateTime curDat;
        decimal monthCashOst;
        decimal debt;
        decimal limit;
        decimal limitOst;
        decimal monthMinPay;

        decimal balance;

        DateTime? periodStart;
        DateTime? periodEnd;

        public string state { get { return ToString(); } }
        public override string ToString()
        {
            return string.Join(", ",new object[] { curDat,balance,debt,limitOst, monthMinPay, periodStart , periodEnd });
        }
        public void Start(DateTime d)
        {
            startDat = d;
            curDat = d;
            monthCashOst = 50000;
            debt = 0;
           limit =  limitOst = 200000;
            monthMinPay = 0;
            balance = 0;

            GetSum(curDat, 1490, false);
        }

        public bool GetSum(DateTime d, decimal sum, bool isCash)
        {
            if (sum <= limitOst)
            {
                if (isCash && sum > monthCashOst)
                    throw new Exception("sum > monthCashOst");
                balance -= sum;
                if (balance < 0)
                {
                    if (debt == 0)
                    {
                        periodStart = d;
                        periodEnd = d.AddDays(100);
                        debt = -balance;
                    }
                    else
                        debt += sum;
                    limitOst = limit - debt;
                }
                if (isCash)
                    monthCashOst -= sum;
                return true;
            }
            else
                throw new Exception("sum > limitOst");
        }

        public bool AddSum(DateTime d, decimal sum, bool isCash)
        {
            balance += sum;
            if (debt > 0)
            {
                if (sum >= debt)
                {
                    debt = 0;
                    limitOst = limit;
                    monthMinPay = 0;
                }
                else
                {
                    debt -= sum;
                    limitOst += sum;
                    if (monthMinPay > 0)
                    {
                        if (sum >= monthMinPay)
                            monthMinPay = 0;
                        else monthMinPay -= sum;
                    }
                }
            }
            return true;
        }

        public void DayStart(DateTime d)
        {
            if (d.Day == 1)
                monthCashOst = 50000;
            if (d.Day == monthMinPeriodStart)
                monthMinPay = debt * 0.05m;
            if (d.Day==startDat.Day && d.Month==startDat.Month && d.Year!=startDat.Year)
                GetSum(d, 1490, false);
        }
        int monthMinPeriodStart=15, monthMinPeriodEnd=4;
        public void DayEnd(DateTime d)
        {
            if (debt > 0)
            {
                if (d==periodEnd)
                    throw new Exception("d==periodEnd "+debt);
                if (monthMinPay>0 && d.Day== monthMinPeriodEnd)
                    throw new Exception("d.Day== monthMinPeriodEnd " + monthMinPay);
            }
            else
            {
                if (periodStart.HasValue)  
                    periodStart = periodEnd = null;
            }
        }
    }
   
}
