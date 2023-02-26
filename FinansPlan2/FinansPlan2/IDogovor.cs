using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public interface IDogovor
    {
        bool IsActive(DateTime d);
        List<ISumActionCommand> OnDayStart(DateTime d);
        List<Error> OnDayEnd(DateTime d);
    }
}
