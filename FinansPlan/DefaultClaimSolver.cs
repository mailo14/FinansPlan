using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan
{
    public class DefaultClaimSolver
    {
        public void SolveAll(IEnumerable<Account> accs)
        {
            List<Claim> claims = new List<Claim>();
            foreach (var a in accs)
                claims.AddRange(a.Claims);
            claims.Sort((x, y) =>
            {
                if (x.dat == y.dat) return 0;
                return x.dat > y.dat ? 1 : -1;
            });
            foreach (var c in claims)
                Solve(c,accs);
        }
        public void Solve(Claim c, IEnumerable<Account> accs)
        {
            var state = c.State;
            if (state != ClaimState.resolved)
            {
                accs = from a in accs where a.End == null || c.dat <= a.End select a;
                foreach (var a in accs)
                {
                    double sum;
                    if (c.sum < 0)
                    {
                        sum = a.PutCash(-c.sum, c.dat);
                        if (sum == -c.sum)
                            break;
                    }
                    if (state == ClaimState.mocked)
                        ;// c.trans[0].fromAcc.Add(c.dat, c.sum, 0, c.sum > 0 ? TranCat.addCash : TranCat.getCash);
                }
            }
        }
    }
}
