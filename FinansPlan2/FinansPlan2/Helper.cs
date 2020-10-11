using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class Helper
    {
        /// <summary>
        /// Decrease osts and return list of buy-price - count to sell (FIFO for position)
        /// </summary>
        /// <param name="ostsTable"></param>
        /// <param name="sellCount"></param>
        /// <returns></returns>
        public List<CountPricePair> GetFactSells(List<CountPricePair> ostsTable, int sellCount)//,double price)
        {
            var ret = new List<CountPricePair>();

            var ost = sellCount;
            while (ost > 0)
            {
                if (ostsTable.Count == 0) throw new Exception("not enough in ost table");

                var sum = Math.Min(ost, ostsTable[0].Count);
                ret.Add(new CountPricePair(sum, ostsTable[0].Price));

                ostsTable[0].Count -= sum;
                if (ostsTable[0].Count == 0) ostsTable.RemoveAt(0);

                ost -= sum;
            }

            return ret;
        }
    }

    public class CountPricePair
    {
        public int Count;
        public decimal Price;

        public CountPricePair(int count, decimal price)
        {
            Count = count;
            Price = price;
        }
    }
}
