using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan
{
    public class BinbankDepVelikolepnayaSemerka : IAccount
    {
        public BinbankDepVelikolepnayaSemerka(DateTime _start,int _srok, double _procent,double initSum,int nesnizOst)//,double _procent= 7)
        {
            start = _start;
            srok = _srok;
            end = start.AddDays(srok);
            procent = _procent;
            Transactions = new TranList();
            Transactions.Add(start, initSum, 1, TranCat.addCash);

            Claims = new List<Claim>();

            //Recalc();
        }
        public DateTime start,end;
        public DateTime? closeDat;
        public int srok;
        double procent;

        public void CloseDep(DateTime dat)
        {
            end = dat;
            Recalc();
        }

        public double Limit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TranList Transactions { get; set; }
        public List<Claim> Claims { get; set; }

        public IProcenter procenter = new StandartProcenter();
        public double GetLimitOst(DateTime dateTime, bool noSdvig)
        {
            throw new NotImplementedException();
        }

        public IList<Tran> GetMaxCash(DateTime dateTime, bool noSdvig)
        {
            throw new NotImplementedException();
        }

        public double GetTotal(DateTime dat, bool onDayStart = false)
        {
            var endDat = dat.Date;
            if (!onDayStart) endDat = endDat.AddDays(1);
            var sum = (from t in Transactions.trans
                       where t.dat < endDat
                       //orderby t.sum
                       select t.sum).Sum();
            return Math.Round(sum);
        }

        public void Recalc()
        {
            Transactions.ClearTempTrans();
            Claims.Clear();
            double sum = 0;
            var dat = start;
            while (Transactions.firstTranDat(dat,ref dat))
            {
                var dayTrans = Transactions.tranPerDat(dat);
                foreach (var ct in dayTrans)
                {
                    sum += ct.sum;
                }
                if (sum > 0 && dat < end)
                {
                    double procentSum = sum * procenter.GetProcentSum(dat, dat.AddDays(1), procent);
                    var t = Transactions.Add(dat.AddDays(1), procentSum, 0, TranCat.addCash);
                }
                dat = dat.AddDays(1);
                if (dat >end || dat >= App.PlanHorizont) break;
            }
            if (end<=App.PlanHorizont && sum>0)
            {
                var t = Transactions.Add(end, -GetTotal(end), 0, TranCat.getCash);
            }
            /*var dat = start;
            for (int i = 0; i < srok - 1; i++)
            {
                dat = dat.AddMonths(1);
                Transactions.Add(dat, ezemes, 0, TranCat.addCash);
            }

            Transactions.Add(dat.AddMonths(1), 11640.76, 0, TranCat.addCash);
            */
        }
    }
}
