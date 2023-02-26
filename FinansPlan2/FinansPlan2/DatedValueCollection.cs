using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class DatedDecimalCollection : DatedValueCollection<decimal>
    {
        public DatedDecimalCollection(List<DatedValue<decimal>> list) : base(list)
        {
        }
    }
    public class DatedValueCollection<T> 
    {
        private List<DatedValue<T>> list;

        public DatedValueCollection(List<DatedValue<T>> list)
        {
            this.list = list;
        }

        public T GetValue(DateTime dat)
        {
            var q = (from l in list where l.d <= dat orderby l.d descending select l).FirstOrDefault();
            if (q == null) //throw new Exception("no val on dat");
                q = list.Last();
            return q.v;
        }

        public DatedValue<T> GetValueExactOnDate(DateTime d)
        {
            var item=list.FirstOrDefault(pp => pp.d == d);
            return item;            
        }
    }
    public class DatedValue<T>
    {
        public DateTime d;
        public T v;

        public DatedValue(string d, T v)
        {
            this.d = DateTime.Parse(d);
            this.v = v;
        }
    }
}
