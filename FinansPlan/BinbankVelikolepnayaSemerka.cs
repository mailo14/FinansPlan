using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan
{
    public class BinbankDepVelikolepnayaSemerka : Account
    {
        public BinbankDepVelikolepnayaSemerka(DateTime _start,int _srok, double _procent,double initSum,int nesnizOst)//,double _procent= 7)
            : base(_start)
        {
            srok = _srok;
            End = Start.AddDays(srok);
            procent = _procent;
            Transactions.Add(Start, initSum, 1, TranCat.addCash);
            //Recalc();
        }
        public DateTime? closeDat;
        public int srok;
        double procent;

        public void CloseDep(DateTime dat)
        {
            End = dat;
            Recalc();
        }

        public IProcenter procenter = new StandartProcenter();
      
        public void Recalc()
        {
            Transactions.ClearTempTrans();
            Claims.Clear();
            double sum = 0;
            var dat = Start;
            while (Transactions.FirstTranDat(dat,ref dat))
            {
                var dayTrans = Transactions.TranPerDat(dat);
                foreach (var ct in dayTrans)
                {
                    sum += ct.sum;
                }
                if (sum > 0 && dat < End)
                {
                    double procentSum = sum * procenter.GetProcentSum(dat, dat.AddDays(1), procent);
                    var t = Transactions.Add(dat.AddDays(1), procentSum, 0, TranCat.addCash);
                }
                dat = dat.AddDays(1);
                if (dat >End || dat >= App.PlanHorizont) break;
            }
            if (End<=App.PlanHorizont && sum>0)
            {
                var t = Transactions.Add(End.Value, -GetTotal(End.Value), 0, TranCat.getCash);
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
