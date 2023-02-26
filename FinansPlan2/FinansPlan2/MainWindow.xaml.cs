using AngouriMath.Extensions;
using FinansPlan2.SampleViewData;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    [ValueConversion(typeof(int), typeof(double))]
    public class doubleToScaledConverter : IValueConverter
    {
        public object Convert(object value, Type typeTarget,
                              object param, CultureInfo culture)
        {
            return (int)value * 100;
        }
        public object ConvertBack(object value, Type typeTarget,
                                      object param, CultureInfo culture)
        {
            return null;
        }

    }
    public class CreateDogovorCommand : IActionCommand
    {
        public DateTime D { get; set; }
        string Id; AlfaCreditCardState InitState;
        public CreateDogovorCommand(DateTime d, string id, AlfaCreditCardState initState)
        {
            D = d;
            Id = id;
            InitState = initState;
        }
        public ActionResult Execute()
        {
          /*  var alfa = new AlfaCreditCard() { Name = Id, InitState = InitState };//new AlfaCreditCardState { Dat = DateTime.Parse("10.08.20"), Amount = 0, FreeMonthCashOst = 50000 } };

            App.Dogovors.Add(alfa.Name, alfa);
            var d = alfa.InitState?.Dat??alfa.Start.Value;
            //TODO throw if less
            while (d < D)
            {
                alfa.OnDayStart(d);
                var dayEndErrors=alfa.OnDayEnd(d);
                if (dayEndErrors.Any())
                    throw new Exception("CreateDogovorCommand error dayEndErrors");


                d = d.AddDays(1);
            }
alfa.OnDayStart(D);
*/
            return new ActionResult();
        }
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       

        public void SnyatAlfaCash(DateTime d, string alfaId,string cashValletId,decimal sum)
        {
            /*AlfaCreditCard alfa = App.Dogovors[alfaId] as AlfaCreditCard;
            CashVallet cashVallet = App.Dogovors[cashValletId] as CashVallet;
            alfa.OnRashod(d, new Tran(d, sum, 0, TranCat.getCash));
            cashVallet.OnPrihod(d, new Tran(d, sum, 0, TranCat.addCash));*/
        }
        public class IgnoreQuasiLettersStringComparer : StringComparer
        {
            private readonly StringComparer BaseComparer;
            public IgnoreQuasiLettersStringComparer(StringComparer baseComparer)
            {
                BaseComparer = baseComparer;
            }
            string repl(string s)
            {
                return s.Replace("ё", "е").Replace("Ё", "Е");
            }
            public override int Compare(string x, string y)
            {
               return BaseComparer.Compare(repl(x), repl(y));
            }

            public override bool Equals(string x, string y)
            {
                return BaseComparer.Equals(repl(x), repl(y));
            }

            public override int GetHashCode(string obj)
            {
                return BaseComparer.GetHashCode(repl(obj));
            }
        }

        public class Calker
        {
            public static string Get(decimal x=5000000)//, DateTime start,DateTime end)
            {
                string r = x.ToString();
                int T = 12*10;
                string inflatKoeffNakopl = "1";
                for (int i = 1; i <= T;i++) {
                    var investKoeff = 1+ 15.0 / 100/ 12;
                    r = $"({r})*{investKoeff}-a*{inflatKoeffNakopl}";
                    var inflatKoeff =1  + 10.0 / 100/12; //1;// 1.0 + 7.5 / 100 / 12;
                    inflatKoeffNakopl += "*" + inflatKoeff.ToString();
                }
                r += " = 0";
                return r.Replace(",",".");
            }
        }

        public MainWindow()
        {
            var re=Calker.Get();
           // var simpl = re.Simplify();
            var rerer=(decimal)re.Solve("a").DirectChildren[0].EvalNumerical();

            var r = ("x + 2x = -2".Solve("x").DirectChildren[0].EvalNumerical());
            var rrrrr = "УДАРЕ́НИЕ".Length;

            string s1 ="ё", s2="е"; var comp = new IgnoreQuasiLettersStringComparer(StringComparer.CurrentCultureIgnoreCase);
             var rr = comp.Equals("ё", "е");
            rr = comp.Equals("Её", "еЕ");
            rr = comp.Equals("ёЁ", "еЁ");
            rr = comp.Equals("ёЁ", "еЕ");
            rr = comp.Equals("ёЁ", "еЕ");
            rr = comp.Equals("ёЁё", "еЕ");
            //string.Equals("a","A",StringComparison.)
            //var alfa = new AlfaCreditCard() {Name="alfa",  InitState = new AlfaCreditCardState { Dat = DateTime.Parse("10.08.20"), Amount = 0,FreeMonthCashOst=50000 } };
            var cash = new CashVallet() { Name = "cash", };
            //Dogovors.Add(alfa.Name, alfa);
            App.Dogovors.Add(cash.Name, cash);
                List<IActionCommand> actions = new List<IActionCommand> { new CreateDogovorCommand(DateTime.Parse("10.08.20"),
                    "alfa", new AlfaCreditCardState { Dat = DateTime.Parse("10.08.20"), Amount = 0, FreeMonthCashOst = 50000 })  };
            var start = DateTime.Parse("10.08.20");
            var end = DateTime.Today;
            var d = start;
            AlfaCreditCardState prev = null;
            List<object> log = new List<object>();
            log.Add($"---START {d:g}");
            while (d <= end)
            {
                foreach (var dog in App.Dogovors.Values)
                {
                    if (dog.IsActive(d))
                        try
                        {
                            dog.OnDayStart(d);
                            /*if (!prev.DeepEquals(dog.CurrentState))
                            {
                                log.Add($"--- after daystart {d:g} -> {dog.CurrentState}");
                                prev = dog.CurrentState.DeepClone();
                            }*/
                        }
                        catch (Exception e) { log.Add($"--- throw on daystart {d:g} -> {e.Message}"); break; }
                }

                foreach (var action in actions.Where(pp => pp.D == d))
                {
                    var opResult=action.Execute();
                    opResult.ThrowIfHasErrors();
                }

                    try
                    {
                    //dog.OnRashod(d, new Tran(d, 2200, 0, TranCat.getCash));
                    SnyatAlfaCash(d, "alfa", "cash", 2200);
                 /*       if (!prev.DeepEquals(dog.CurrentState))
                        {
                            log.Add($"--- during day {d:g} -> {dog.CurrentState}");
                            prev = dog.CurrentState.DeepClone();
                        }*/
                    }
                    catch (Exception e) { log.Add($"--- throw during day {d:g} -> {e.Message}"); break; }

                foreach (var dog in App.Dogovors.Values)
                {
                    if (dog.IsActive(d))
                        try
                        {
                            dog.OnDayEnd(d);
                           /* if (!prev.DeepEquals(dog.CurrentState))
                            {
                                log.Add($"--- after dayend {d:g} -> {dog.CurrentState}");
                                prev = dog.CurrentState.DeepClone();
                            }*/
                        }
                        catch (Exception e) { log.Add($"--- throw on dayend {d:g} -> {e.Message}"); break; }
                }

                d = d.AddDays(1);
            }
            log.Add($"---FINISH {d:g}");
           /*  return;
           var a1 = new AlfaCreditCard() { CreditLimit =200 };
            var b = a1.DeepClone();
            var aa = a1 == b;
            aa = a1.DeepEquals(b);
            b.CreditLimit = 300;
                aa = a1.DeepEquals(b);


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
            }*/

            InitializeComponent();
            DataContext = new SampleViewModel();
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
