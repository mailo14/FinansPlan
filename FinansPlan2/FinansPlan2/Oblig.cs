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
        public DatedValueCollection<decimal> PlanKupons = new DatedValueCollection<decimal>(new List<DatedValue<decimal>> {
            new DatedValue<decimal>("10.10.2017", 55.10M),
            new DatedValue<decimal>("09.04.2019", 55.60M),
            new DatedValue<decimal>("06.10.2020", 51.61M),
        });
        public DatedValueCollection<decimal> Prices = new DatedValueCollection<decimal>(new List<DatedValue<decimal>> {
            new DatedValue<decimal>("10.10.2017", 1000M),
            new DatedValue<decimal>("06.10.2020", 1024M),
        });
        public decimal GetNKD(DateTime dat)
        {
            dat = dat.Date;
            if (dat<StartDat || dat>EndDat) throw new Exception("not in bound");

            decimal d= PlanKupons.GetValue(dat);
            var days = (int)(dat - StartDat).TotalDays;
            days =days % Period;
return Math.Round( d/Period*days,2);
        }
    }
    
}
