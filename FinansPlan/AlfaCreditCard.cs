using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FinansPlan
{
    public class AlfaCreditCard : IAccount
    {
        public AlfaCreditCard(DateTime _start, int _yearcommis, int _limit = 200000)
        {
            Limit = _limit;
            start = _start;
            end = start.AddYears(5).AddDays(-1);
            yearcommis = _yearcommis;
            Transactions = new TranList();
            Claims = new List<Claim>();

            Recalc();
        }
        public DateTime start, end;
       public int yearcommis;
        public double Limit {get; set;}
        public TranList Transactions { get; set; }
        public List<Claim> Claims { get; set; }

        public double GetLimitOst(DateTime dat, bool noSdvig)
        {
            int i = 0;
            var trans = this.Transactions.trans;
            double sum=GetTotal(dat);
            var min = sum;
            if (noSdvig==true)
                while (Transactions.firstTranDat(dat, ref dat))                    
                {
                    var dayTrans = Transactions.tranPerDat(dat);
                    foreach (var ct in dayTrans)
                        sum += ct.sum;
                    if (sum < min) min = sum;
                    dat = dat.AddDays(1);
                }
            return Limit+min;
        }

        public IList<Tran> GetMaxCash(DateTime dat, bool noSdvig)
        {
            //рсчет остатка месячного лимита на снятие без комиссии:
            var startDat = dat.Date.AddDays(-dat.Day + 1);
            var endDat =noSdvig?startDat.AddMonths(1):dat.Date.AddDays(1);
            var gets = -(from t in Transactions.trans
                        where t.dat >= startDat && t.dat < endDat
                        && t.sum<0
                        && t.cat == TranCat.getCash
                        select t.sum).Sum();
            double noKomisLimitOst =Math.Max(0, 50000-gets);

            var limitOst = GetLimitOst(dat, noSdvig);
            var ret = new List<Tran>();

            double maxCash;
            //if (gets < limitOst)
            {
               // if (noSdvig)
                {
                    maxCash =Math.Min(noKomisLimitOst,limitOst);
                    ret.Add(new Tran(dat, -maxCash, 0, TranCat.getCash));
                }
            }
            return ret;
        }

        public void Recalc()
        {
            Transactions.ClearTempTrans();
            Claims.Clear();
            var dat = start;
            if (Transactions.firstTranDat(start, ref dat))
            while (dat < end)
            {
                Transactions.Add(dat, -yearcommis, 0,TranCat.payCard);
                dat = dat.AddYears(1);
            }
            dat = start;
            double sum = 0;
            while (dat < end)
            {
                if (Transactions.firstTranDat(dat, ref dat))//есть транзакция начала периода
                {
                    var startPerDat = dat;
                    var endPerDat = dat.AddDays(100 );
                    if (endPerDat > end) endPerDat = end;

                    while (dat <= endPerDat)
                    {
                        var dayTrans = Transactions.tranPerDat(dat);
                        foreach (var ct in dayTrans)
                        {
                            sum += ct.sum;
                            if (-sum > Limit) ct.error = "more than limit on " + (sum + Limit);
                        }
                        if (dat > startPerDat && dat.Day == start.Day && sum<0 && dat!=endPerDat)
                        {
                            var endDatEzemec = dat.AddDays(20);
                            if (endDatEzemec < endPerDat)
                            {
                                double ezemecNeedSum = Math.Max(320, Math.Round(-sum* 5 / 100, 2));
                                var ezemecPerPrihod = (from t in Transactions.trans
                                                       where t.sum > 0
                                                       && t.dat >= dat && t.dat < endDatEzemec.AddDays(1)
                                                       select t.sum).Sum();
                                if (ezemecPerPrihod < ezemecNeedSum)
                                {
                                    ezemecNeedSum -= ezemecPerPrihod;
                                    var t = Transactions.Add(endDatEzemec, ezemecNeedSum, 0, TranCat.addCash);
                                    var c = new Claim(ezemecNeedSum, endDatEzemec, dat);
                                    Claims.Add(c);
                                    c.trans.Add(t);
                                    //sum += ezemecNeedSum;
                                    // if (-sum > Limit) t.error = "more than limit on " + (sum + Limit);
                                }
                            }
                        }
                        dat = dat.AddDays(1);// endPerDat.AddDays(1);
                        if (sum >= 0) break;
                    }
                    if (sum < 0)
                    {
                        var t =Transactions.Add(endPerDat, -sum, 0,TranCat.addCash);
                        var c = new Claim(-sum, endPerDat);
                        Claims.Add(c);
                        c.trans.Add(t);
                        sum = 0;//+= -sum;
                                //if (-sum > limit) t.error = "more than limit on " + (sum + limit);
                        dat = endPerDat.AddDays(1);
                    }

                }
                else break;
            }
            if (sum != 0) MessageBox.Show(sum.ToString());
        }

        public double GetTotal(DateTime dat, bool onDayStart = false)
        {
            var endDat = dat.Date;
            if (!onDayStart) endDat = endDat.AddDays(1);
            var sum=(from t in Transactions.trans where t.dat < endDat                         
                         select t.sum).Sum();
            return sum;
        }

                public Tran AddTran(DateTime dat, double sum, int type, TranCat cat)
                {
                    return Transactions.Add(dat, sum, type,cat);
                }
    }
}
