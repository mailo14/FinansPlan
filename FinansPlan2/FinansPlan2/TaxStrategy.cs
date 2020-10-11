using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public interface ITaxStrategy
    {
        decimal GetTaxOnKuponPay(DateTime d, decimal kupon, Oblig instr);
        decimal GetTaxOnObligEnd(DateTime d, PriceInfo buyFactPrice, decimal nominal);
        decimal GetTaxOnObligSell(DateTime d, PriceInfo buyFactPrice, PriceInfo sellFactPrice);
    }
    public class NewTaxStrategy : ITaxStrategy
    {
        public decimal GetTaxOnKuponPay(DateTime d, decimal kupon, Oblig instr)
        {
            return kupon * 0.13m;
        }

        public decimal GetTaxOnObligEnd(DateTime d, PriceInfo buyFactPrice, decimal nominal)
        {
            if (buyFactPrice.Price < nominal)//TODO to percent?
            {
                var dif = nominal - buyFactPrice.Price;
                return dif * 0.13m;
            }
            return 0;
        }

        public decimal GetTaxOnObligSell(DateTime d, PriceInfo buyFactPrice, PriceInfo sellFactPrice)
        {
            var dif = sellFactPrice.Price+sellFactPrice.NKD - (buyFactPrice.Price+buyFactPrice.NKD);//TODO to percent?
            if (dif>0)
            {                
                return dif * 0.13m;
            }
            return 0;
        }
    }
}
