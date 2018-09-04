using System;

namespace FinansPlan
{
    public class AnnuitetCredit
    {
        public AnnuitetCredit() {
            Transactions = new TranList();
        }
        public AnnuitetCredit(DateTime _start, double _sum, int _srok, double _procent)
            :this()
        {
            start = _start;
            startSum = _sum;
            srok = _srok;
            procent = _procent;
        }
        
        public DateTime start;
       protected double startSum;
        protected int srok;
        protected double procent;
        public TranList Transactions { get; set; }

        public double GetAnnuitet(int months, double sum)
        {
            if (sum == startSum) months--;
            double monProcent = procent / 100 / 12;
            return Math.Round(sum * monProcent * Math.Pow(1 + monProcent, months) / (Math.Pow(1 + monProcent, months) - 1), 2);
        }
    }
}