using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.New
{
    public class RaifDogovor : Dogovor
    {
        public RaifDogovor()
        {
            Name = "Raif";

            Typee = DogovorType.Karta;
            Bank = Banks.Raif;

            AvailableActions = new List<IActionn> {
                new OpenRaifActionn(this),

                new DayStartRaifActionn(this),
               new  DayEndRaifActionn(this),

                new GetBeznalRaifActionn(this),
                new PutBeznalRaifActionn(this),

                new BuyCommonActionn(this),
            };
        }
    }

    public class RaifDogovorLineState : DogovorLineStateWithSum
    {
        public decimal AvailableToGet { get; set; }

        public DateTime? NextVipiskaDate { get; set; }
        //public decimal? GracePeriodOst { get; set; }
        public DateTime? GracePeriodEndDate { get; set; }
        public decimal? MinMonthPayOst { get; set; }
        public DateTime? MinMonthPayEndDate { get; set; }
        public decimal MonthSnyat50Ost { get;  set; }
    }
    public class OpenRaifActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public OpenRaifActionn(Dogovor dogovor)
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
            var dogovor = Dogovor as RaifDogovor;
            //LineName=paramss
            var startDate = request.eventtt.Dat;
            var line = new RaifDogovorLine
            {
                Dogovorr = dogovor,
                StartDate = startDate,
                LineName = paramss.LineName,
                RaschetDateDay= 6,//TODO из request.paramss или (если пустое) от startDate Отчетный период начинается в расчетную дату (это может быть 1, 6, 11, 21 и 26-е число, в зависимости от даты оформления карты -следующее)
Limit = new DatedDecimalCollection(new List<DatedValue<decimal>> { new DatedValue<decimal>(startDate.ToString(), paramss.Limit.Value) }),
                //IsActive = true,
            };
            var initState = new RaifDogovorLineState { };//InitState=paramss
            initState.Dat = startDate; initState.InitialEvent = request.eventtt;
            initState.Sum = paramss.Sum ?? 0m;
            initState.MonthSnyat50Ost = 50000m; //TODO from paramss ??50000m

            if (!initState.NextVipiskaDate.HasValue)
            {
                initState.NextVipiskaDate = line.CalcNextVipiskaDate(startDate);
            }
            initState.AvailableToGet = line.Limit.GetValue(startDate)+initState.Sum;

            request.context.DogovorLines.Add(line.LineName, line);
            request.DogovorLinesStates.Add(line.LineName, initState);
        }
    }

    public class RaifDogovorLine: DogovorLine
    {
        public int RaschetDateDay { get; set; }//TODO to dated

        public DatedDecimalCollection Limit = new DatedDecimalCollection(new List<DatedValue<decimal>> { });

        public DateTime CalcNextVipiskaDate(DateTime startDate)
        {
            var vipDat = startDate.SetDay(this.RaschetDateDay);
            if (vipDat.DayOfWeek == DayOfWeek.Sunday) vipDat = vipDat.AddDays(1);
            if (vipDat < startDate)
            {
                vipDat = startDate.AddMonths(1).SetDay(this.RaschetDateDay);
                if (vipDat.DayOfWeek == DayOfWeek.Sunday) vipDat = vipDat.AddDays(1);
            }

            return vipDat.AddDays(1);
        }
    }


    public class DayStartRaifActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public DayStartRaifActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.DayStart;

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
            var line = request.context.DogovorLines[request.itemDogovorLineName] as RaifDogovorLine;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[line.LineName] as RaifDogovorLineState;

            var dogovor = (RaifDogovor)line.Dogovorr;
            decimal sum = 0;
            if (dat == state.NextVipiskaDate)
            {  
                var newState = state.Clone() as RaifDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;
                
                if (state.GracePeriodEndDate.HasValue)
                {
                    newState.MinMonthPayOst = newState.Sum * 0.03m;
                    newState.MinMonthPayEndDate = dat.AddDays(20);
                }

                    request.DogovorLinesStates[line.LineName] = newState;
            }
            else if (dat.Day == 1)
            {

                var newState = state.Clone() as RaifDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;

                newState.MonthSnyat50Ost = 50000m;
                

                request.DogovorLinesStates[line.LineName] = newState;
            }
        }
    }
    
    public class DayEndRaifActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public DayEndRaifActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.DayEnd;

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
            var line = request.context.DogovorLines[request.itemDogovorLineName] as RaifDogovorLine;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[line.LineName] as RaifDogovorLineState;

            var dogovor = (RaifDogovor)line.Dogovorr;
            decimal sum = 0;
            if (dat == state.NextVipiskaDate)
            {
                var newState = state.Clone() as RaifDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;

                if (!state.GracePeriodEndDate.HasValue && state.Sum<0)
                {
                    newState.GracePeriodEndDate= line.CalcNextVipiskaDate(dat.AddMonths(1).AddDays(1)).AddDays(20);                    
                }
                newState.NextVipiskaDate = line.CalcNextVipiskaDate(dat.AddDays(1));

                request.DogovorLinesStates[line.LineName] = newState;
            }
            else
            if (dat == state.MinMonthPayEndDate && state.MinMonthPayOst>0)
            {
                throw new Exception($"Не уплачен мин.платеж");
            }
            else
            if (dat == state.GracePeriodEndDate)
            {
                throw new Exception($"Не уплачен весь долг");
            }
        }
    }


    public class GetBeznalRaifActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public GetBeznalRaifActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.GetBeznal;

        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response = new CanResponse { Success = true, MaxSum = decimal.MaxValue };
           
            var lineName = request.itemDogovorLineName;
            var line = request.context.DogovorLines[lineName] as RaifDogovorLine;
            var dat = request.eventtt.Dat;
           
            var state = request.DogovorLinesStates[request.itemDogovorLineName] as RaifDogovorLineState;

            var maxAvailable = line.Limit.GetValue(dat) + state.Sum;

            if (maxAvailable > 0 && state.MonthSnyat50Ost > 0)
            {
                var canSum = Math.Min(maxAvailable, state.MonthSnyat50Ost);
                response.CanSums.Add(canSum);

                if (state.Sum > 0)
                {
                    var canSum2 = Math.Min(canSum, state.Sum);
                    if (canSum2 != canSum) response.CanSums.Add(state.Sum);
                }
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
            var line =request.context.DogovorLines[lineName] as RaifDogovorLine;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[lineName] as RaifDogovorLineState;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as RaifDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;
                
                newState.Sum -= request.Sum;
                
                if (request.Sum > state.MonthSnyat50Ost) throw new Exception($"Нельзя снять больше остатка 50тыс в мес");
                newState.MonthSnyat50Ost -= request.Sum;

                newState.AvailableToGet= line.Limit.GetValue(dat)+newState.Sum;
                if (newState.AvailableToGet < 0) throw new Exception($"Выход суммы за лимит");

                request.DogovorLinesStates[lineName] = newState;
            }
        }
    }

    public class PutBeznalRaifActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public PutBeznalRaifActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.PutBeznal;

        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response= new CanResponse { Success = true, MaxSum = decimal.MaxValue };

            var state = request.DogovorLinesStates[request.itemDogovorLineName] as RaifDogovorLineState;
            
            if (state.MinMonthPayOst>0)  response.CanSums.Add(state.MinMonthPayOst.Value);
            if (state.Sum < 0) response.CanSums.Add(-state.Sum);

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
            var line = request.context.DogovorLines[lineName] as RaifDogovorLine; 
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[lineName] as RaifDogovorLineState;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as RaifDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;

                newState.Sum += request.Sum;
                
                if (newState.MinMonthPayOst.HasValue)
                {
                    newState.MinMonthPayOst -= Math.Min(newState.MinMonthPayOst.Value, request.Sum);
                }
                if (newState.GracePeriodEndDate.HasValue && state.Sum<0 && newState.Sum >= 0)
                {
                    newState.GracePeriodEndDate = null;
                    newState.MinMonthPayEndDate= null;
                }
                newState.AvailableToGet = line.Limit.GetValue(dat) + newState.Sum;

                request.DogovorLinesStates[lineName] = newState;
            }
        }
    }
}
