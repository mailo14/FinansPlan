using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.New
{
    public class HalvaDogovor : Dogovor
    {
        public HalvaDogovor()
        {
            Name = "Halva";
            Typee = DogovorType.Karta;
            Bank = Banks.Sovcom;

            AvailableActions = new List<IActionn> {
                new OpenHalvaActionn(this),

                new DayStartHalvaActionn(this),
               new  DummyDayEndActionn(this),

                new GetBeznalHalvaActionn(this),
                new PutBeznalHalvaActionn(this),

                new SnyatCashCommonActionn(this),
                new PutCashCommonActionn(this),
                new BuyCommonActionn(this),
                
                //new PutBeznalCommonActionn(this),
            };
        }
    }

    public class HalvaDogovorLineState : DogovorLineStateWithSum
    {
        public decimal ProcentsNakopl { get; set; }
        public decimal LimitDaySendOtherBankCard_Ost { get; set; }
        public decimal LimitMonthSendOtherBankCard_Ost { get; set; }
        public decimal LimitDaySendSbp_Ost { get; set; }
        //public decimal AvailableToGet { get; set; }

        /*public DateTime? NextVipiskaDate { get; set; }
        //public decimal? GracePeriodOst { get; set; }
        public DateTime? GracePeriodEndDate { get; set; }
        public decimal? MinMonthPayOst { get; set; }
        public DateTime? MinMonthPayEndDate { get; set; }
        public decimal MonthSnyat50Ost { get; set; }*/
    }
    public class OpenHalvaActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public OpenHalvaActionn(Dogovor dogovor)
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
            var dogovor = Dogovor as HalvaDogovor;
            //LineName=paramss
            var startDate = request.eventtt.Dat;
            var line = new HalvaDogovorLine
            {
                Dogovorr = dogovor,
                StartDate = startDate,
                LineName = StandardDogLineName.Halva,
                PeriodStartDay = 27,//TODO из request.paramss
                ProcentOnOst = new DatedDecimalCollection(new List<DatedValue<decimal>> { new DatedValue<decimal>(startDate.ToString(), paramss.ProcentOnOst.Value) }),
                //Limit = new DatedDecimalCollection(new List<DatedValue<decimal>> { new DatedValue<decimal>(startDate.ToString(), paramss.Limit.Value) }),
                //IsActive = true,
            };
            var initState = new HalvaDogovorLineState { };//InitState=paramss
            initState.Dat = startDate; initState.InitialEvent = request.eventtt;
            initState.Sum = paramss.Sum ?? 0m;

            request.strategyBranch.DogovorLines.Add(line.LineName, line);
            request.DogovorLinesStates.Add(line.LineName, initState);
        }
    }

    public class HalvaDogovorLine : DogovorLine
    {
        public int PeriodStartDay { get; set; }
        public decimal LimitDaySendOtherBankCard { get; set; } = 150000;
        public decimal LimitDaySendSbp { get; set; } = 150000;
        public decimal LimitMonthSendOtherBankCard { get; set; } = 300000;

        public DatedValueCollection<decimal> ProcentOnOst;

        //public DatedDecimalCollection Limit = new DatedDecimalCollection(new List<DatedValue<decimal>> { });
    }


    public class DayStartHalvaActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public DayStartHalvaActionn(Dogovor dogovor)
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

        public void Execute0(ExecuteRequest request)
        {
            var line = request.strategyBranch.DogovorLines[request.itemDogovorLineName] as HalvaDogovorLine;
            var dogovor = (HalvaDogovor)line.Dogovorr;

            var dat = request.eventtt.Dat;

            var oldState = request.DogovorLinesStates[line.LineName] as HalvaDogovorLineState;
            var newState = oldState.Clone() as HalvaDogovorLineState;

            newState.LimitDaySendOtherBankCard_Ost = line.LimitDaySendOtherBankCard;
            newState.LimitDaySendSbp_Ost = line.LimitDaySendSbp;
            if (dat.Day == 1) newState.LimitMonthSendOtherBankCard_Ost = line.LimitMonthSendOtherBankCard;

            if (newState.Sum > 0) newState.ProcentsNakopl += newState.Sum * line.ProcentOnOst.GetValue(dat) / 100 / (DateTime.IsLeapYear(dat.Year) ? 366 : 365);

            if (dat.Day == line.PeriodStartDay)
            {
                var correction = request.strategyBranch.Corrections.SingleOrDefault(x => x.Dat == dat && x.DogovorLineName == line.LineName && x.OpType == OpType.PayProcents);

                if (newState.ProcentsNakopl > 0 || correction != null)
                {
                    decimal sum = newState.ProcentsNakopl;
                    decimal? sumBeforeCorrection = null;
                    if (correction != null)
                    {
                        sumBeforeCorrection = sum;
                        sum = correction.Sum;
                    }

                    /*var payProcentsOnOstEventt = new Eventt { Name = "Выплата процента на остаток", Dat = dat };
                    var payProcentsOnOstEventtState = new EventtState { Eventtt = payProcentsOnOstEventt};
                    request.NewAutoEventStates.Add(payProcentsOnOstEventtState);

                    //var dayEventExecuteRequest = new EventExecuteRequest { Context = request.context};
                    payProcentsOnOstEventt.ActionItems.Add(new ActionnItem
                    {
                        Eventtt = payProcentsOnOstEventt,
                        DogovorName = line.Dogovorr.Name,
                        DogovorLineName = line.LineName,
                        ItemAction = ActionnType.PutBeznal,
                        Sum = sum,
                        SumBeforeCorrection = sumBeforeCorrection
                    });*/
                    
                    var payProcentsOnOstEventt = Operation.BuidEvent(new BuidEventFromOpRequest { OpTyp = OpType.PayProcents, Dat = dat, DogLine1Id = line.LineName, Summ = sum, SumBeforeCorrection = sumBeforeCorrection });
                    request.NewAutoEventStates.Add(new EventtState { Eventtt = payProcentsOnOstEventt });

                    newState.ProcentsNakopl = 0;
                }
            }

            if (!ObjectCloner.DeepEquals(oldState, newState))
            {
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = oldState;
                request.DogovorLinesStates[line.LineName] = newState;
            }
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
            var line = request.strategyBranch.DogovorLines[request.itemDogovorLineName] as HalvaDogovorLine;
            var dogovor = (HalvaDogovor)line.Dogovorr;
            var newState = inputNewState as HalvaDogovorLineState;
            var dat = request.eventtt.Dat;

            newState.LimitDaySendOtherBankCard_Ost = line.LimitDaySendOtherBankCard;
            newState.LimitDaySendSbp_Ost = line.LimitDaySendSbp;
            if (dat.Day == 1) newState.LimitMonthSendOtherBankCard_Ost = line.LimitMonthSendOtherBankCard;

            if (newState.Sum > 0) newState.ProcentsNakopl += newState.Sum * line.ProcentOnOst.GetValue(dat) / 100 / (DateTime.IsLeapYear(dat.Year) ? 366 : 365);

            if (dat.Day == line.PeriodStartDay)
            {
                var correction = request.strategyBranch.Corrections.SingleOrDefault(x => x.Dat == dat && x.DogovorLineName == line.LineName && x.OpType == OpType.PayProcents);

                if (newState.ProcentsNakopl > 0 || correction != null)
                {
                    decimal sum =Math.Round( newState.ProcentsNakopl,2);
                    decimal? sumBeforeCorrection = null;
                    if (correction != null)
                    {
                        sumBeforeCorrection = sum;
                        sum = correction.Sum;
                    }

                    /*var payProcentsOnOstEventt = new Eventt { Name = "Выплата процента на остаток", Dat = dat };
                    var payProcentsOnOstEventtState = new EventtState { Eventtt = payProcentsOnOstEventt};
                    request.NewAutoEventStates.Add(payProcentsOnOstEventtState);

                    //var dayEventExecuteRequest = new EventExecuteRequest { Context = request.context};
                    payProcentsOnOstEventt.ActionItems.Add(new ActionnItem
                    {
                        Eventtt = payProcentsOnOstEventt,
                        DogovorName = line.Dogovorr.Name,
                        DogovorLineName = line.LineName,
                        ItemAction = ActionnType.PutBeznal,
                        Sum = sum,
                        SumBeforeCorrection = sumBeforeCorrection
                    });*/
                    var payProcentsOnOstEventt = Operation.BuidEvent(new BuidEventFromOpRequest { OpTyp = OpType.PayProcents, Dat = dat, DogLine1Id = line.LineName, Summ = sum, SumBeforeCorrection = sumBeforeCorrection });
                    request.NewAutoEventStates.Add(new EventtState { Eventtt = payProcentsOnOstEventt });

                    newState.ProcentsNakopl = 0;
                }
            }
        }

        public CanResponse CanExecute(ExecuteRequest request)
        {
            throw new NotImplementedException();
        }
    }
    public class GetBeznalHalvaActionn : IActionn 
    {
        Dogovor Dogovor;
        public GetBeznalHalvaActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.GetBeznal;

        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response = new CanResponse { Success = true, MaxSum = decimal.MaxValue };

            var state = request.DogovorLinesStates[request.itemDogovorLineName] as HalvaDogovorLineState;

            var maxAvailable = Math.Min(100000/*лимит на операцию - для стягивания будет свой*/, state.LimitDaySendSbp_Ost);

            if (maxAvailable > 0)
            {
                if (state.Sum > 0) response.CanSums.Add(Math.Min(maxAvailable, state.Sum));
                if (state.Sum > 400000) response.CanSums.Add(Math.Min(maxAvailable, state.Sum - 400000));
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
            var line = request.strategyBranch.DogovorLines[lineName] as HalvaDogovorLine;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[lineName] as HalvaDogovorLineState;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as HalvaDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;

                if (request.Sum > 100000m) throw new Exception($"LimitOpSendOtherBankCard ");// {LimitOpSendOtherBankCard} is less than {request.sum}", ErrorType.Warning));

                newState.Sum -= request.Sum;

                newState.LimitDaySendOtherBankCard_Ost -= request.Sum;
                if (newState.LimitDaySendOtherBankCard_Ost < 0) throw new Exception($"LimitDaySendOtherBankCard_Ost");// {CurrentState.LimitDaySendOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));

                newState.LimitMonthSendOtherBankCard_Ost -= request.Sum;
                if (newState.LimitMonthSendOtherBankCard_Ost < 0) throw new Exception($"LimitMonthSendOtherBankCard_Ost");// {CurrentState.LimitMonthSendOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));

                request.DogovorLinesStates[lineName] = newState;
                //TODO if <0
            }
        }
    }

    public class SendSbpHalvaActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public SendSbpHalvaActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.SendSbp;

        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response = new CanResponse { Success = true, MaxSum = decimal.MaxValue };

            var state = request.DogovorLinesStates[request.itemDogovorLineName] as HalvaDogovorLineState;

            var maxAvailable = Math.Min(state.LimitDaySendSbp_Ost, state.LimitDaySendSbp_Ost);

            if (maxAvailable > 0)
            {
                if (state.Sum > 0) response.CanSums.Add(Math.Min(maxAvailable, state.Sum));
                if (state.Sum > 400000) response.CanSums.Add(Math.Min(maxAvailable, state.Sum - 400000));
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
            var line = request.strategyBranch.DogovorLines[lineName] as HalvaDogovorLine;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[lineName] as HalvaDogovorLineState;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as HalvaDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;

                newState.Sum -= request.Sum;

                newState.LimitDaySendSbp_Ost -= request.Sum;
                if (newState.LimitDaySendSbp_Ost < 0) throw new Exception($"LimitDaySendSbpOtherBankCard_Ost");// {CurrentState.LimitDaySendSbpOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));

                newState.LimitMonthSendOtherBankCard_Ost -= request.Sum;
                if (newState.LimitMonthSendOtherBankCard_Ost < 0) throw new Exception($"LimitMonthSendOtherBankCard_Ost");// {CurrentState.LimitMonthSendOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));

                request.DogovorLinesStates[lineName] = newState;
                //TODO if <0
            }
        }
    }
    public class PutBeznalHalvaActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public PutBeznalHalvaActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.PutBeznal;

        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response = new CanResponse { Success = true, MaxSum = decimal.MaxValue };

            var state = request.DogovorLinesStates[request.itemDogovorLineName] as HalvaDogovorLineState;
                        
            if (state.Sum<400000) response.CanSums.Add(400000- state.Sum);
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
            var lineName = request.itemDogovorLineName;
            var line = request.strategyBranch.DogovorLines[lineName] as HalvaDogovorLine;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[lineName] as HalvaDogovorLineState;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as HalvaDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;

                newState.Sum += request.Sum;

                request.DogovorLinesStates[lineName] = newState;
            }
        }
    }
}