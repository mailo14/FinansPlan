using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
   /* public abstract class Dogovor
    {
        public List<Op> claims = new List<Op>();

        public List<Op> ops = new List<Op>();
        public abstract void AnywayInEndOfDay(DateTime dat);
        DateTime startDat { get; set; }
        public Dogovor(DateTime dat, double sum = 0)
        {
            startDat = dat;
            addOp(dat, sum);

        }
        public void addOp(DateTime dat, double opSum)
        {
            ops.Add(new Op(dat, opSum));

            var exist = osts.FirstOrDefault(pp => pp.dat == dat.Date);
            if (exist != null)
                exist.sum += opSum;
            else
            {
                var sum = sumOn(dat, false);
                osts.Add(new Op(dat.Date, sum + opSum));
            }
        }

        public List<Op> osts = new List<Op>();
        public double sumOn(DateTime dat, bool onDayStart = false)
        {
            dat = dat.Date;
            var limitDat = onDayStart ? dat : dat.AddDays(1);
            //if (limitDat < this.startDat) throw new Exception();
            var ret = (from o in osts
                       where o.dat < limitDat
                       orderby o.dat descending
                       select o.sum).FirstOrDefault();
            return ret;
        }
    }


    public class Alfa : Dogovor
    {
        public Alfa(DateTime dat, double sum = 0) : base(dat, sum) { }
        public List<IVariant> variants = new List<IVariant>();

        public List<Op> ops = new List<Op>();
        public override void AnywayInEndOfDay(DateTime dat)
        {
            var sum = sumOn(dat, false);
            if (sum < 0) {
                if (dat == srokEnd(dat)) {
                    claims.Add(new Op(dat, -sum));
                }
            }
        }
        
        DateTime? srokEnd(DateTime dat)
        {
            dat = dat.Date;
            var sum = sumOn(dat);
            if (sum >= 0) return null;
                        
            var prevPositiveOst = (from o in osts where o.sum>=0 && o.dat < dat orderby o.dat descending select o).First().dat;
            var firstNegOstAfter= (from o in osts where o.dat > prevPositiveOst orderby o.dat ascending select o).First().dat;
            if ((dat - firstNegOstAfter).TotalDays > 100) throw new Exception();
            return firstNegOstAfter.AddDays(100);
        }
    }

    public class TinkoffVklad : Dogovor
    {
        DateTime datTo;
        public TinkoffVklad(DateTime dat, DateTime datTo, double sum = 0) : base(dat, sum) {
            this.datTo = datTo;
                }
        // public List<IVariant> variants = new List<IVariant>();

        public double proc = 7.22;
        public List<Op> ops = new List<Op>();
        public override void AnywayInEndOfDay(DateTime dat)
        {
            if (dat <= datTo)
            {
                var sum = sumOn(dat, true);
                if (sum > 0)
                {
                    var procents = sum * proc / 100 / (DateTime.IsLeapYear(dat.Year) ? 366 : 365);
                    addOp(dat, procents);
                    sum += procents;
                }
            }
            if (dat == datTo)
                Close(dat);
        }
        public double GetMax(DateTime dat)//,double sum)
        {
            if (dat < datTo)
            {
                //max snyat без закрытия и убрать проценты

                var sum = sumOn(dat, true);
                if (sum > 50000)
                {
                    //pos
                }
            }
        }
        public void Close(DateTime dat)
        {
            if (dat >= datTo) ;//
            if (dat < datTo)
            {
                
            }
        }
    }

    public class Halva: Dogovor
    {
        public Halva(DateTime dat, double sum = 0) : base(dat, sum) { }        
        
        public double proc = 6;
        public override void AnywayInEndOfDay(DateTime dat) {
            var sum = sumOn(dat, false);
            if (sum > 0)
            {
                var procents = sum * proc / 100 / (DateTime.IsLeapYear(dat.Year) ? 366 : 365);
                addOp(dat, procents);
                sum += procents;
            }
        }
       
    }

    public class VariantSnyatAlfaCashMax
    {
        public Dogovor dogovor;
        public string name;
        public VariantSnyatAlfaCashMax(Dogovor d)
        {
            this.dogovor = d;
        }
        public bool CanDo(DateTime dat)
        { //Func<DateTime, bool> CanDo;

            if (dat!=srokEnd(dat) && dogovor.sum > -200000)
            {
                var uzeSnytoVmecyace = 0;//TODO dogovor.ops
                if (uzeSnytoVmecyace < 50000)
                    return true;
            }
            return false;
        }
        public void Do(DateTime dat,Dogovor outDogovor)
        {
            var opSum = Math.Min(200000 + dogovor.sum, 50000);
            dogovor.ops.Add(new Op(dat, -opSum));
            dogovor.sum += -opSum;

            outDogovor.ops.Add(new Op(dat, +opSum));
            outDogovor.sum += +opSum;
        }
        
    }

    public class Op
    {
        public DateTime dat;
        public double sum;
        public Op(DateTime dat,double sum)
        {
            this.dat = dat;
            this.sum = sum;
        }
    }*/
}
