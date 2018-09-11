using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan
{
    public class TranList
    {
       public List<Tran> trans = new List<Tran>();
        public Tran Add(DateTime _dat, double _sum, int type,TranCat cat)
        {
            int i = 0;
            while (i < trans.Count && trans[i].dat < _dat) i++;
            Tran t = new Tran(_dat, _sum, type,cat);
            trans.Insert(i, t);

            return t;
        }

        public void ClearTempTrans()
        {
            int i = 0;
            while (i < trans.Count)
                if (trans[i].type == 0) trans.RemoveAt(i);
                else i++;
        }
        public bool firstTranDat(DateTime startDat, ref DateTime dat)
        {
            var t = trans.FirstOrDefault(pp => pp.dat >= startDat.Date);
            if (t == null) return false;
            dat = t.dat.Date;
            return true;
        }
        public List<Tran> tranPerDat(DateTime dat)
        {
            return trans.Where(pp => pp.dat >= dat.Date && pp.dat < dat.Date.AddDays(1)).ToList();
        }
    }
    public class Tran
    {
        public int type;
        public DateTime dat;
        public double sum;
        public double komis=0;
        public string error = null;
        public TranCat cat;

        public Tran(DateTime _dat, double _sum, int _type, TranCat _cat) { dat = _dat; sum = _sum; type = _type; cat = _cat; }
        public override string ToString()
        {
            return dat.ToShortDateString() + " " + (sum > 0 ? "+" : "") + sum.ToString("C") + (error != null ? (" ---" + error) : "");
        }
    }
    public enum TranCat
    {
        getCash=0,
        payCard,
        addCash
    }

    public class Claim
    {
        DateTime? startDat;
        public DateTime dat;
        public double sum;
        public Claim(double sum, DateTime endDat, DateTime? startDat=null)
        {
            this.dat = endDat;
            this.sum = sum;
        }
        public List<Tran> trans = new List<Tran>();
    }
}
