using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.New
{
    public class ZpDogovor : Dogovor
    {

        private readonly IWorkDayService _workDayService;

        public string ZpAccountDogovorLineName;

        public DatedValueCollection<int> AvansGainDayOfMonths = new DatedValueCollection<int>(new List<DatedValue<int>> {
            new DatedValue<int>("1.01.2000", 20) });
        public DatedValueCollection<int> ZpOstGainDayOfMonths = new DatedValueCollection<int>(new List<DatedValue<int>> {
            new DatedValue<int>("1.01.2000", 5) });

        public DatedDecimalCollection AvansSum = new DatedDecimalCollection(new List<DatedValue<decimal>> {
            new DatedValue<decimal>("1.01.2000", 48000), new DatedValue<decimal>("10.11.2020", 60000)         ,
            new DatedValue<decimal>("10.10.2022", 88000) });
        public DatedDecimalCollection ZpOstSum = new DatedDecimalCollection(new List<DatedValue<decimal>> {
            new DatedValue<decimal>("1.01.2000", 82500), new DatedValue<decimal>("10.11.2020", 70500),
            new DatedValue<decimal>("10.10.2022", 103400) });


        public ZpDogovor(IWorkDayService workDayService)
        {
            Name = "Zp";
            Typee = DogovorType.Zp;
            //Bank = Banks.Sovcom;

            AvailableActions = new List<IActionn> {
                new DummyDayEndActionn(this),

                new OpenZpActionn(this),
                new DayStartZpActionn(this)
            };


            _workDayService = workDayService;
        }

        public DateTime CalcAvansDate(DateTime d)
        {
            DateTime ret;
            ret = _workDayService.GetWorkDayOrBefore(d.SetDay(AvansGainDayOfMonths.GetValue(d)));
            if (ret >= d) return ret;
            else return CalcAvansDate(d.AddMonths(1).SetDay(1));
        }

        public DateTime CalcZpDate(DateTime d)
        {
            DateTime ret;
            if (d.Month == 12 && d > _workDayService.GetWorkDayOrBefore(d.SetDay(ZpOstGainDayOfMonths.GetValue(d))))
            {
                ret = _workDayService.GetWorkDayOrBefore(d.SetDay(29));
            }
            else if (d.Month == 1) ret = CalcZpDate(d.AddMonths(1).SetDay(1));
            else ret = _workDayService.GetWorkDayOrBefore(d.SetDay(ZpOstGainDayOfMonths.GetValue(d)));

            if (ret >= d) return ret;
            else return CalcZpDate(d.AddMonths(1).SetDay(1));
        }

    }

    public class ZpDogovorLineState : DogovorLineState
    {
        public DateTime? NextAvansDate { get; set; }
        public DateTime? NextZpOstDate { get; set; }
    }
    public class OpenZpActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public OpenZpActionn(Dogovor dogovor)
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
            var dogovor = Dogovor as ZpDogovor;
            //LineName=paramss
            var startDate = request.eventtt.Dat;
            var line = new DogovorLine
            {
                LineName = StandardDogLineName.Zp,
                Dogovorr = dogovor,
                StartDate = startDate,
                //IsActive = true,
            };

            var initState = new ZpDogovorLineState {};//InitState=paramss
            initState.Dat = startDate; initState.InitialEvent = request.eventtt;

            if (initState.NextAvansDate == null)
            {
                initState.NextAvansDate = dogovor.CalcAvansDate(startDate);
            }
            if (initState.NextZpOstDate == null)
            {
                initState.NextZpOstDate = dogovor.CalcZpDate(startDate);
            }
            if (initState.NextAvansDate == startDate || initState.NextZpOstDate == startDate)
                throw new Exception("initDate == zpDate");

            //line.DogovorLineStates.Add(initState);

            request.strategyBranch.DogovorLines.Add(line.LineName, line);
            request.DogovorLinesStates.Add(line.LineName, initState);
        }
    }


    public class DayStartZpActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public DayStartZpActionn(Dogovor dogovor)
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
            var line =request.strategyBranch.DogovorLines[request.itemDogovorLineName];
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[line.LineName] as ZpDogovorLineState;

            var dogovor = (ZpDogovor)line.Dogovorr;
            decimal sum = 0;
            if (dat == state.NextAvansDate)
            {
                sum = dogovor.AvansSum.GetValue(dat);

                var newState = state.Clone() as ZpDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;
                newState.NextAvansDate = dogovor.CalcAvansDate(dat.AddDays(1));
                request.DogovorLinesStates[line.LineName] = newState;
            }
            else if (dat == state.NextZpOstDate)
            {
                sum = dogovor.ZpOstSum.GetValue(dat);

                var newState = state.Clone() as ZpDogovorLineState;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;
                newState.NextZpOstDate = dogovor.CalcZpDate(dat.AddDays(1));
                request.DogovorLinesStates[line.LineName] = newState;
            }

            if (sum > 0)
            {
                decimal? sumBeforeCorrection = null;
                var correction = request.strategyBranch.Corrections.SingleOrDefault(x => x.Dat == dat && x.DogovorLineName == line.LineName && x.OpType == OpType.PayZp);
                if (correction != null)
                {
                    sumBeforeCorrection = sum;
                    sum = correction.Sum;
                }
                var zpLine = request.strategyBranch.DogovorLines[dogovor.ZpAccountDogovorLineName];
                var zpAccState = request.DogovorLinesStates[zpLine.LineName] as ZpDogovorLineState;


                var payZpEventt = Operation.BuidEvent(new BuidEventFromOpRequest { OpTyp = OpType.PayZp, Dat = dat, DogLine1Id = zpLine.LineName, Summ = sum, SumBeforeCorrection = sumBeforeCorrection });
               request.NewAutoEventStates.Add(new EventtState { Eventtt = payZpEventt });

                /*var payZpEventt = new Eventt { Name = "Выплата зарплаты", Dat = dat };
                var payZpEventtState = new EventtState { Eventtt = payZpEventt};
                request.NewAutoEventStates.Add(payZpEventtState);

                payZpEventt.ActionItems.Add(new ActionnItem
                {
                    Eventtt = payZpEventt,
                    DogovorName = zpLine.Dogovorr.Name,
                    DogovorLineName = zpLine.LineName,
                    ItemAction = ActionnType.PutBeznal,
                    Sum = sum,
                    SumBeforeCorrection = sumBeforeCorrection
                });*/

                //payZpEventtState.Execute(dayEventExecuteRequest);
            }
        }
    }
}
