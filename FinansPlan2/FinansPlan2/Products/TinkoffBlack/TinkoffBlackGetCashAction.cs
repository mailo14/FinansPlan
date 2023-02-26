using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.Products.TinkoffBlack
{
    /*public class TinkoffBlackGetCashAction : IActionCommand
    {
        public DateTime D { get; set; }
        decimal Sum;
        string DogovorId;
        public TinkoffBlackGetCashAction(DateTime d, decimal sum, string dogovorId)
        {
            D = d;
            DogovorId = dogovorId;
            Sum = sum;
        }
        public ActionResult Execute()
        {
            var errors = new List<Error>();

            var dd = App.Dogovors[DogovorId] as TinkoffBlack;

            var resp=dd.OnRashod(D, new Tran(D, Sum, 0, TranCat.getCash) { atmPlace = ATMPlace.Own });
            if (resp.Any())errors.AddRange(resp);

            var vallet = App.Dogovors.Single(x => x.Value is CashVallet).Value as CashVallet;
            vallet.OnPrihod(D, new Tran(D, Sum, 0, TranCat.addCash));

            return new ActionResult(errors);
        }
    }
    public class TinkoffBlackGetCashOtherAtmAction : IActionCommand
    {
        public DateTime D { get; set; }
        decimal Sum;
        string DogovorId;
        public TinkoffBlackGetCashOtherAtmAction(DateTime d, decimal sum, string dogovorId)
        {
            D = d;
            DogovorId = dogovorId;
            Sum = sum;
        }
        public ActionResult Execute()
        {
            var errors = new List<Error>();

            var dd = App.Dogovors[DogovorId] as TinkoffBlack;
            var resp = dd.OnRashod(D, new Tran(D, Sum, 0, TranCat.getCash) { atmPlace=ATMPlace.Other});
            if (resp.Any()) errors.AddRange(resp);

            var vallet = App.Dogovors.Single(x => x.Value is CashVallet).Value as CashVallet;
            vallet.OnPrihod(D, new Tran(D, Sum, 0, TranCat.addCash));

            return new ActionResult(errors);
        }
    }*/
}
