using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class RashodRequest
    {
        public DateTime Dat;
        public decimal sum;
        //public TranCat2 TranCat;
        //public ATMPlace? Place;
        public MoneyType MoneyType;
        public ATMPlace? Place;

        public bool SendedFastByPhoneNumber;

        public OperationType OpType;
        public bool SameBank;

        public bool IsSobstvAmount = true;
    }
    public class CanRashodResponse
    {
        public bool Success;
        public decimal MaxSum;
        public decimal MinSum;
        public decimal OpMaxSum;
    }
}
