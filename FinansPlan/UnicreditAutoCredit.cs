using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan
{
    public class UnicreditAutoCredit : AnnuitetCredit
    {
        public UnicreditAutoCredit()
        {
            start = DateTime.Parse("9.08.2018");
            startSum = 233640.32;
            srok = 24;
            procent = 11.3;
            ezemes = 10897;
        }
        double ezemes;

        public void Recalc()
        {
            Transactions.ClearTempTrans();
            var ann=GetAnnuitet(srok, startSum);

            var dat = start;
            for(int i = 0; i < srok-1; i++)
            {
                dat=dat.AddMonths(1);
                Transactions.Add(dat, ezemes, 0, TranCat.addCash);
            }
            
            Transactions.Add(dat.AddMonths(1), 11640.76, 0, TranCat.addCash);
        }
    }
}