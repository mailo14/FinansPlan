using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan
{
    //public interface IAccount
    public abstract class Account
    {
        public Account(DateTime _start)
        {
            Start = _start;
            Transactions = new TranList();
            Claims = new List<Claim>();
        }
        /*
        /// <summary>
        /// Лимит максимальной сумма для снятия
        /// </summary>
        public double Limit { get; set; }
        /// <summary>
        /// Отстаток лимита на снятие на дату
        /// </summary>
        public virtual double GetLimitOst(DateTime dat, bool noSdvig)
        {
            return double.MaxValue;
        }
        */
        public double GetTotal(DateTime dat, bool onDayStart = false)
        {
            var endDat = dat.Date;
            if (!onDayStart) endDat = endDat.AddDays(1);
            var sum = (from t in Transactions.trans
                       where t.dat < endDat
                       //orderby t.sum
                       select t.sum).Sum();
            return Math.Round(sum,2);
        }

       public TranList Transactions { get; set; }

        public virtual double GetMaxCash(DateTime dat, bool noSdvig)
        {
            var sum = GetTotal(dat);
            if (noSdvig == true)
            {
                dat = dat.AddDays(1);
                var min = sum;
                while (Transactions.FirstTranDat(dat, ref dat))
                {
                    var dayTrans = Transactions.TranPerDat(dat);
                    foreach (var ct in dayTrans)
                        sum += ct.sum;
                    if (sum < min) min = sum;
                    dat = dat.AddDays(1);
                }
                if (min < 0) throw new Exception("min<0");
                return min;
            }
            else
                return sum;            
        }
        public List<Claim> Claims { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; } = null;

        public virtual double PutCash(double maxSum, DateTime dat)
        {
            if (End.HasValue && dat > End)
                return 0;
            return maxSum;
        }
    }
}
