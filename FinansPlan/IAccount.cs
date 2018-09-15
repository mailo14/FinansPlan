using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan
{
    public interface IAccount
    {
        /// <summary>
        /// Лимит максимальной сумма для снятия
        /// </summary>
        double Limit { get; set; }
        /// <summary>
        /// Отстаток лимита на снятие на дату
        /// </summary>
        double GetLimitOst(DateTime dat, bool noSdvig);

        double GetTotal(DateTime dat, bool onDayStart=false);

        TranList Transactions { get; set; }

        IList<Tran> GetMaxCash(DateTime dat, bool noSdvig);
        List<Claim> Claims { get; set; }
    }
}
