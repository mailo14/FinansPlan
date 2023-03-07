using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.New
{
    public class CashWalletDogovor : Dogovor
    {
        public CashWalletDogovor()
        {
            Name = "Cash";
            Typee = DogovorType.CashWallet;
            Bank = null;

            AvailableActions = new List<IActionn> {
                new OpenCashWalletActionn(this),
                new DummyDayStartActionn(this),
                new DummyDayEndActionn(this),

                new SnyatCashCommonActionn(this),
                new PutCashCommonActionn(this),
                new BuyCommonActionn(this),
                //new PutBeznalCommonActionn(this),
            };
        }
    }


    public class CashWalletLineState : DogovorLineStateWithSum { }

    public class OpenCashWalletActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public OpenCashWalletActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.Open;

        public CanResponse CanExecute(ExecuteRequest request)
        {
            throw new NotImplementedException();
        }

        public DogovorState Execute(Dogovor itemDogovor, Eventt eventt, decimal sum)
        {
            throw new NotImplementedException();
        }

        public void Execute(ExecuteRequest request)
        {
            var paramss = request.paramss as OpenDogovorParams;
            var dogovor = Dogovor as CashWalletDogovor;
            //LineName=paramss
            var startDate = request.eventtt.Dat;
            var line = new DogovorLine
            {
                Dogovorr = dogovor,
                StartDate = startDate,
                LineName=StandardDogLineName.CashWallet
                //IsActive = true,
            };

            var  initState = new CashWalletLineState {};//InitState=paramss
            initState.Dat = startDate; initState.InitialEvent = request.eventtt;
            initState.Sum = paramss.Sum ?? 0m;
            //line.DogovorLineStates.Add(initState);

            request.strategyBranch.DogovorLines.Add(line.LineName, line);
            request.DogovorLinesStates.Add(line.LineName, initState);
        }
    }


}
