using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class PayCommand : IActionCommand
    {
        public DateTime D { get; set; }
        OperationRequest Request;

        public PayCommand(OperationRequest request)
        {
            Request = request;
            D = request.Dat;
        }

        public static CanRashodResponse CanExecute(OperationRequest request)
        {
            var source = App.Dogovors[request.SourceDogovorId] as IAccount;

            return source.CanRashod(new RashodRequest { Dat = request.Dat, OpType = OperationType.Pay, sum = request.sum });
        }


        public ActionResult Execute()
        {
            var errors = new List<Error>();


            //validate SourceDogovorId != TargetDogovorId
            var source = App.Dogovors[Request.SourceDogovorId] as IAccount;

            var resp = source.OnRashod(new RashodRequest { Dat = D, OpType = OperationType.Pay, sum = Request.sum });
            if (resp.Any()) errors.AddRange(resp);


            return new ActionResult(errors);
            /*
                        d1.Ticker==CardTicker.TinkoffBlackCard;
                        d2.Ticker == CardTicker.HalvaCard;
                        OperationType.Move;

                        //List<ATMPlace> atms = null;//all new ATMPlace.Other

                        var resp = dd.OnRashod(D, new Tran(D, Sum, 0, TranCat.getCash) { atmPlace = ATMPlace.Other });
                        if (resp.Any()) errors.AddRange(resp);

                        var vallet = App.Dogovors.Single(x => x.Value is CashVallet).Value as CashVallet;
                        vallet.OnPrihod(D, new Tran(D, Sum, 0, TranCat.addCash));

                        return new ActionResult(errors);*/
        }
    }
}
