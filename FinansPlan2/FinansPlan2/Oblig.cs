using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class Oblig
    {
        public string InstrCode;
        public DateTime StartDat=DateTime.Parse("11.04.2017");
        public DateTime EndDat=DateTime.Parse("05.04.2022");
        public int Period=182;
        public DatedValueCollection Kupons = new DatedValueCollection(new List<DatedValue> {
            new DatedValue("10.10.2017", 55.10M),
            new DatedValue("09.04.2019", 55.60M),
            new DatedValue("06.10.2020", 51.61M),
        });
        public DatedValueCollection Prices = new DatedValueCollection(new List<DatedValue> {
            new DatedValue("10.10.2017", 1000M),
            new DatedValue("06.10.2020", 1024M),
        });
        public decimal GetNKD(DateTime dat)
        {
            dat = dat.Date;
            if (dat<StartDat || dat>EndDat) throw new Exception("not in bound");

            decimal d= Kupons.GetValue(dat);
            var days = (int)(dat - StartDat).TotalDays;
            days =days % Period;
return Math.Round( d/Period*days,2);
        }
    }

    public class DatedValueCollection
    {
        private List<DatedValue> list;

        public DatedValueCollection(List<DatedValue> list)
        {
            this.list = list;
        }

        public decimal GetValue(DateTime dat)
        {
            var q = (from l in list where l.d >= dat orderby l.d select l).FirstOrDefault();
            if (q == null) //throw new Exception("no val on dat");
                q = list.Last();
            return q.v;
        }
    }
    public class DatedValue
    {
        public DateTime d;
        public decimal v;

        public DatedValue(string d, decimal v)
        {
            this.d = DateTime.Parse(d);
            this.v = v;
        }
    }
}
