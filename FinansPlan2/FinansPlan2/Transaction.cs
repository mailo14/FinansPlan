﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class TranList
    {
        public List<Tran> trans = new List<Tran>();
        public Tran Add(Tran t)
        {
            int i = 0;
            while (i < trans.Count && trans[i].dat < t.dat) i++;
            trans.Insert(i, t);
            return t;
        }
        public Tran Add(DateTime _dat, decimal _sum, int type, TranCat cat)
        {
            int i = 0;
            while (i < trans.Count && trans[i].dat < _dat) i++;
            Tran t = new Tran(_dat, Math.Round(_sum, 2), type, cat);
            trans.Insert(i, t);

            return t;
        }
        public Tran this[int i]
        {
            get { return trans[i]; }
        }
        public decimal Sum()
        {
            var sum = trans.Sum(pp => pp.sum);
            return Math.Round(sum, 2);
        }
        public void ClearTempTrans()
        {
            int i = 0;
            while (i < trans.Count)
                if (trans[i].type == 0) trans.RemoveAt(i);
                else i++;
        }
        public bool FirstTranDat(DateTime startDat, ref DateTime dat)
        {
            var t = trans.FirstOrDefault(pp => pp.dat >= startDat.Date);
            if (t == null) return false;
            dat = t.dat.Date;
            return true;
        }
        public List<Tran> TranPerDat(DateTime dat)
        {
            return trans.Where(pp => pp.dat >= dat.Date && pp.dat < dat.Date.AddDays(1)).ToList();
        }
    }
    public class Tran
    {
        public int type;
        public DateTime dat;
        public decimal sum;
        public decimal komis = 0;
        public string error = null;
        public TranCat cat;
       // public TranCat2 cat;
        public ATMPlace? atmPlace;

        public Tran(RashodRequest request)
        { dat = request.Dat;
            sum = request.sum;
            //request.TranCat,
            atmPlace = request.Place;
        }

        //public Account fromAcc, toAcc;

        public Tran(DateTime _dat, decimal _sum, int _type, TranCat _cat, ATMPlace? atmPlace=null) { dat = _dat; sum = _sum; type = _type; cat = _cat; }
        public override string ToString()
        {
            return dat.ToShortDateString() + " " + (sum > 0 ? "+" : "") + sum.ToString("C") + (error != null ? (" ---" + error) : "");
        }
    }
    public enum TranCat
    {
        getCash = 0,
        payCard,
        addCard,
        addCash,
        getCard,
        Fake
    }
    public enum TranCat2
    {
        get,
        add,
        pay,
    }

    public enum ATMPlace
    {
        Own = 0,
        Partner,
        Other
    }

}
