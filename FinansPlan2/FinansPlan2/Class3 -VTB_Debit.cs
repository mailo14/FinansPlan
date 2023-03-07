using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.New
{
    public class VTB_DebitDogovor : Dogovor
    {
        public VTB_DebitDogovor()
        {
            Name = "VTB_Debit";
            Typee = DogovorType.Karta;
            //Bank = Banks.Sovcom;

            AvailableActions = new List<IActionn> {
               new DayStartVTB_DebitActionn(this),
                new DummyDayEndActionn(this),

                new OpenVTB_DebitActionn(this),

                new PutBeznalVTB_DebitActionn(this),
                new SendSbpVTB_DebitActionn(this),

                new SnyatCashCommonActionn(this),
                new PutCashCommonActionn(this),
                new BuyCommonActionn(this),
            };
        }
    }

    public class VTB_DebitDogovorLineState : DogovorLineStateWithSum    {
        public decimal LimitMonthSendSbp_Ost { get; set; }
    }
    public class OpenVTB_DebitActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public OpenVTB_DebitActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.Open;

        public CanResponse CanExecute(Dogovor d1, Dogovor d2, decimal sum)
        {
            //If same bank, limits etc
            return new CanResponse { Success = true, MaxSum = decimal.MaxValue };
        }

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
            var dogovor = Dogovor as VTB_DebitDogovor;
            //LineName=paramss
            var startDate = request.eventtt.Dat;
            var line = new DogovorLine
            {
                Dogovorr = dogovor,
                StartDate = startDate,
                LineName = paramss.LineName
                //IsActive = true,
            };
            var initState = new VTB_DebitDogovorLineState { };//InitState=paramss
            initState.Dat = startDate; initState.InitialEvent = request.eventtt;
            initState.Sum = paramss.Sum ?? 0m;

            request.strategyBranch.DogovorLines.Add(line.LineName, line);
            request.DogovorLinesStates.Add(line.LineName, initState);
        }
    }


    public class DayStartVTB_DebitActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public DayStartVTB_DebitActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.DayStart;

        public CanResponse CanExecute(Dogovor d1, Dogovor d2, decimal sum)
        {
            //If same bank, limits etc
            return new CanResponse { Success = true, MaxSum = decimal.MaxValue };
        }

        public DogovorState Execute(Dogovor itemDogovor, Eventt eventt, decimal sum)
        {
            throw new NotImplementedException();
        }

        public void Execute(ExecuteRequest request)
        {
            var dat = request.eventtt.Dat;
            var lineName = request.itemDogovorLineName;

            var oldState = request.DogovorLinesStates[lineName];
            var newState = oldState.Clone();

            InnerExecute(request, newState);

            if (!ObjectCloner.DeepEquals(oldState, newState))
            {
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = oldState;
                request.DogovorLinesStates[lineName] = newState;
            }
        }

        public /*override*/ void InnerExecute(ExecuteRequest request, IDogovorLineState inputNewState)
        {
            var line = request.strategyBranch.DogovorLines[request.itemDogovorLineName] as DogovorLine;
            var dogovor = (VTB_DebitDogovor)line.Dogovorr;
            var newState = inputNewState as VTB_DebitDogovorLineState;
            var dat = request.eventtt.Dat;


            if (dat.Day == 1) newState.LimitMonthSendSbp_Ost = 100000; //TODO line.LimitMonthSendSbp

        }

        public CanResponse CanExecute(ExecuteRequest request)
        {
            throw new NotImplementedException();
        }
    }

    public class SendSbpVTB_DebitActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public SendSbpVTB_DebitActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.SendSbp;

        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response = new CanResponse { Success = true, MaxSum = decimal.MaxValue };

            var state = request.DogovorLinesStates[request.itemDogovorLineName] as VTB_DebitDogovorLineState;

            var maxAvailable = state.LimitMonthSendSbp_Ost;

            if (maxAvailable > 0)
            {
                if (state.Sum > 0) response.CanSums.Add(Math.Min(maxAvailable, state.Sum));
            }

            //If same bank, limits etc
            return response;
        }

        public DogovorState Execute(Dogovor itemDogovor, Eventt eventt, decimal sum)
        {
            throw new NotImplementedException();
        }

        public void Execute(ExecuteRequest request)
        {
            var lineName = request.itemDogovorLineName;
            var line = request.strategyBranch.DogovorLines[lineName] as DogovorLine;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[lineName] as VTB_DebitDogovorLineState;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as VTB_DebitDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;

                newState.Sum -= request.Sum;

                newState.LimitMonthSendSbp_Ost -= request.Sum;
                if (newState.LimitMonthSendSbp_Ost < 0) throw new Exception($"LimitMonthSendSbp_Ost");// {CurrentState.LimitDaySendSbpOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));

                request.DogovorLinesStates[lineName] = newState;
                //TODO if <0
            }
        }
    }

    public class PutBeznalVTB_DebitActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public PutBeznalVTB_DebitActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.PutBeznal;

        public CanResponse CanExecute(Dogovor d1, Dogovor d2, decimal sum)
        {
            //If same bank, limits etc
            return new CanResponse { Success = true, MaxSum = decimal.MaxValue };
        }

        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response = new CanResponse { Success = true, MaxSum = decimal.MaxValue };

            response.CanSums.Add(decimal.MaxValue);

            //If same bank, limits etc
            return response;
        }

        public DogovorState Execute(Dogovor itemDogovor, Eventt eventt, decimal sum)
        {
            throw new NotImplementedException();
        }

        public void Execute(ExecuteRequest request)
        {
            var line = request.itemDogovorLineName;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[line] as VTB_DebitDogovorLineState;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as VTB_DebitDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;
                newState.Sum += request.Sum;
                request.DogovorLinesStates[line] = newState;
            }
        }
    }

}
