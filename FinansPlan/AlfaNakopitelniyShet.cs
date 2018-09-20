using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FinansPlan
{
    /*
     открыв и пополнив на любую сумму, владелец получит 7% годовых на минимальный остаток на счете 
     за период с 1-го месяца до 3-го месяца. С 4-го месяца ставка становится ниже, но остается весьма достойной — 5% 
     годовых. Важный момент по поводу минимального остатка: имеется в виду именно минимальный остаток 
     в период с 1-го по 3-й месяц. То есть, если вы сначала внесли 10000 рублей, а потом довнесли еще 20000 рублей, 
     то проценты будут начисляться только на 10000 рублей как на минимальный остаток за этот период. 
     С 4-го месяца минимальный остаток высчитывается помесячно. Это также следует учитывать и при снятии средств со счета.
    */
    public class AlfaNakopitelniyShet : Account
    {        
        public AlfaNakopitelniyShet(DateTime _start,double initSum)//,double _procent= 7)
        : base(_start)
        {   
            Transactions.Add(Start, initSum, 1, TranCat.addCash);
            //Recalc();
        }
       
        public double Limit {get; set;}       

        public IProcenter procenter = new StandartProcenter();
              

        public void Recalc()
        {
            Transactions.ClearTempTrans();
            Claims.Clear();
            var dat = Start;
            double sum = 0;
            double firstThreeMonthsMinSum =double.MaxValue;
            while (Transactions.FirstTranDat(dat, ref dat))
            {
                dat = new DateTime(dat.Year, dat.Month, Start.Day);//TODO if 30/31/29
                var endPerDat = dat.AddMonths(1);
                double minMonthSum = double.MaxValue;

                while (dat < endPerDat)
                {
                    var dayTrans = Transactions.TranPerDat(dat);
                    foreach (var ct in dayTrans)
                    {
                        sum += ct.sum;
                    }
                    if (sum < minMonthSum) minMonthSum = sum;
                    dat = dat.AddDays(1);

                }
                if (minMonthSum != double.MaxValue)
                {
                        int monthSpend = (dat.Month - Start.Month + 12) % 12;
                    if (monthSpend <= 3 )
                        firstThreeMonthsMinSum = Math.Min(firstThreeMonthsMinSum, minMonthSum);
                    if (minMonthSum > 0)
                    {                        
                            double procent = (monthSpend > 3 ? 4 : 7);
                        double procentSum = (monthSpend > 3 ? minMonthSum : firstThreeMonthsMinSum)
                            * procenter.GetProcentSum(endPerDat.AddMonths(-1),
                            endPerDat, procent);
                        var t = Transactions.Add(endPerDat, procentSum, 0, TranCat.addCash);
                    }
                }
                if (dat >= App.PlanHorizont) break;
            }
        }


                public Tran AddTran(DateTime dat, double sum, int type, TranCat cat)
                {
                    return Transactions.Add(dat, sum, type,cat);
                }

        public double GetLimitOst(DateTime dat, bool noSdvig)
        {
            return GetTotal(dat);
        }
    }
}
