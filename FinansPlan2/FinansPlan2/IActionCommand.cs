using System;
using System.Collections.Generic;
using System.Linq;

namespace FinansPlan2
{
    public interface IActionCommand
    {
        DateTime D { get; set; }

        ActionResult Execute();
    }
    public interface ISumActionCommand: IActionCommand
    {
        CommandTicker Ticker { get; set; }
        decimal Sum { get; set; }
    }

    public class ActionResult
    {
        public List<Error> Errors;
        public ActionResult(List<Error> errors = null)
        {
            Errors = errors ?? new List<Error>();
        }

        public void ThrowIfHasErrors()
        {
            if (Errors.Any())
                throw new Exception(string.Join(Environment.NewLine, Errors.Select(x => x.Message)));
        }
    }

    public abstract class CommandBase : ISumActionCommand
    {
        public DateTime D { get; set; }
        public CommandTicker Ticker { get; set; }
        public decimal Sum { get; set; }

        public CommandBase(DateTime d, decimal sum, CommandTicker ticker)
        {
            D = d;
            Sum = sum;
            Ticker = ticker;
        }

        public abstract ActionResult Execute();
    }

    public class PutSumCommand : CommandBase
    {
        string AccId;
        public PutSumCommand(DateTime d, decimal sum, string accId, CommandTicker ticker)
            : base(d, sum, ticker)
        {
            AccId = accId;
        }

        public override ActionResult Execute()
        {
            // var alfa = new AlfaCreditCard() { Name = zpAccId, InitState = InitState };//new AlfaCreditCardState { Dat = DateTime.Parse("10.08.20"), Amount = 0, FreeMonthCashOst = 50000 } };

            var dd = App.Dogovors[AccId] as IAccount;
            dd.OnPrihod(new RashodRequest
            {
                Dat = D,
                OpType = OperationType.AddCashless,
                MoneyType = MoneyType.Сashless,
                Place = ATMPlace.Own,
                sum = Sum,
            });

            return new ActionResult();
        }
    }
    public class GetSumCommand : CommandBase
    {
        string AccId;
        public GetSumCommand(DateTime d, decimal sum, string accId, CommandTicker ticker)
            : base(d, sum, ticker)
        {
            AccId = accId;
        }

        public override ActionResult Execute()
        {
            // var alfa = new AlfaCreditCard() { Name = zpAccId, InitState = InitState };//new AlfaCreditCardState { Dat = DateTime.Parse("10.08.20"), Amount = 0, FreeMonthCashOst = 50000 } };

            var dd = App.Dogovors[AccId] as VTBZpAccount;
            dd.OnRashod(new RashodRequest
            {
                Dat = D,
                OpType = OperationType.Pay,
                MoneyType = MoneyType.Сashless,
                Place = null,
                sum = Sum,
            });

            return new ActionResult();
        }
    }
}