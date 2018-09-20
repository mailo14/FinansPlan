using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan
{
    public class Claim
    {
      //  DateTime? startDat;
        public DateTime dat;
        /// <summary>
        /// что должно поступить >0, что надо снять <0
        /// </summary>
        public double sum;
        /// <summary>
        /// что должно поступить >0, что надо снять <0
        /// </summary>
        public Claim(double sum, DateTime endDat, DateTime? startDat = null)
        {
            this.dat = endDat;
            this.sum = sum;
        }
        public TranList trans = new TranList();

        public ClaimState State
        {
            get {
                if (trans.trans.Count == 1)
                {
                    var t = trans[0];
                    if (sum > 0 && t.fromAcc == null
                            || sum < 0 && t.toAcc == null)
                        return ClaimState.mocked;
                }
                var ts = trans.Sum();
                if (ts == sum)
                    return ClaimState.resolved;
                return ClaimState.partial;
            }
        }
        /*public bool isDone()
        {
            if (trans.trans.Count == 1)
            {
                var t = trans[0];
                if (sum > 0 && t.fromAcc == null
                        || sum < 0 && t.toAcc == null)
                    return false;
            }
            var ts = trans.Sum();            
            return ts == sum;
        }*/
        
    }
    public enum ClaimState
        {
            resolved,
            mocked,
            partial
        }
}
