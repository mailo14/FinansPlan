using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.New
{

    public class Contextt
    {
        public Dictionary<string, Dogovor> Dogovors=new Dictionary<string, Dogovor>();//глобальная бд договоров
        public List<StrategyBranch> StrategyBranches { get; set; } = new List<StrategyBranch>();

        //public Dictionary<string, DogovorLine> DogovorLines { get; set; } = new Dictionary<string, DogovorLine>();
        //public Dictionary<DogovorLine, DogovorLineState> States { get; set; } = new Dictionary<DogovorLine, DogovorLineState>(); //Срез состояний
        //public List<Eventt> Events { get; set; } = new List<Eventt>();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        //public DatedEventtStateScope InitDatedScope { get; set; }
        //public EventtState InitialEventtState { get; set; }
    }

    public class StrategyBranch
    {
        public EventtState LastEventState { get; set; }
        public Dictionary<string, DogovorLine> DogovorLines { get; set; } = new Dictionary<string, DogovorLine>();
        public List<Eventt> Events { get; set; } = new List<Eventt>();
        public List<Correction> Corrections = new List<Correction>();

        public string Error { get; set; }

        public StrategyBranch Clone()
        {
            throw new NotImplementedException();
        }
    }

    public class Correction
    {
        public DateTime Dat;
        public string DogovorLineName;
        public OpType OpType;
        public decimal Sum;
    }

    public class main
    {
        


        public void MMain()
        {
            var context = new Contextt();
            /*var halva = new HalvaDogovor(); context.Dogovors.Add(halva);

            var zp = new ZpDogovor()
            {
                ZpAccountDogovorLineName = "VTB zp karta"
            };
            context.Dogovors.Add(zp);

            var vtbDebit = new VTB_DebitDogovor();
            context.Dogovors.Add(vtbDebit);

            var stringEvents = new List<string>() { };

            var se = stringEvents[0];
            //foreach(var se in stringEvents)
            {
                var eventtt = ParseEvent(context.Dogovors, se);
                context.Events.Add(eventtt);
            }*/

            /*context.PeriodStart = context.Events.Min(x => x.Dat);
            context.PeriodEnd = context.Events.Max(x => x.Dat).AddDays(5);

            //ProcessPeriod(context);

            Eventt neweventt = null, beforeEventt = null;
            InsertEvent(context, neweventt, beforeEventt);*/

            /*  var eventt = new Eventt() { Name = a.Name };

              eventt.Actions = new List<ActionnItem>();
              eventt.Actions.Add(new ActionnItem { ItemDogovor = halva, ItemAction = halva.AvailableActions.Single(x=>x.Type==ActionnType.Buy), Sum = 100 });
              //eventt.Actions.Add(new ActionnItem { ItemDogovor = d2, ItemAction = a2, Sum = maxSum });
  */
        }

       /* private void InsertEvent(Contextt context, Eventt eventt, Eventt beforeEventt)
        {
            var dayEvents = context.Events.Where(x => x.Dat == beforeEventt.Dat).OrderBy(x => x.OrderId).ToList();
            eventt.OrderId = beforeEventt.OrderId;
            for (int i = dayEvents.IndexOf(beforeEventt) + 1; i < dayEvents.Count; i++)
                dayEvents[i].OrderId++;

            context.Events.Insert(context.Events.IndexOf(beforeEventt), eventt);
            //TODO add to db + submit OrderId change 
        }*/

       

        private Eventt ParseEvent(List<Dogovor> dd, string se)
        {
            var arr = se.Split();
            var dat = DateTime.Parse(arr[0]);
            if (arr[1] == "Open") // "dat Open dogovorName dogovorLineName(uniq) start initstatejson"
            {
                var eventt = new Eventt() { Name = se, Dat = dat };
                Dogovor d = dd.Single(x => nameof(x).StartsWith(arr[2]));
                eventt.ActionItems = new List<ActionnItem>();
                //eventt.ActionItems.Add(new ActionnItem { ItemDogovor = null, ItemAction =  ActionnType.Open, Eventtt = eventt, Params = se });
                return eventt;
            }
            /*  else
              if (arr[0] == "Buy") // "dat Buy tmth fromDogovorLineName price"
              {
                  var eventt = new Eventt() { Name = se };

                  eventt.Actions = new List<ActionnItem>();
                  Dogovor d = dd.Single(x => nameof(x).StartsWith(arr[1]));
                  //TODO getLine
                  //eventt.Actions.Add(new ActionnItem { ItemDogovor = halva, ItemAction = halva.AvailableActions.Single(x => x.Type == ActionnType.Buy), Sum = 100 });

              }*/

            throw new Exception("ParseEvent");
        }
    }

    public class DatedEventtStateScope
    {
        public DatedEventtStateScope()
        {
        }

        public DateTime Dat { get; set; }
        public List<EventtState> DayEventStates { get; set; }
        public DatedEventtStateScope Prev { get; set; }
        public DatedEventtStateScope Next { get; set; }
    }

    public class EventtState
    {
        public Eventt Eventtt { get; set; }

        private EventtState _prev;
        public EventtState Prev
        {
            get => _prev;
            set
            {
                if (value != null)
                {
                    value.Nexts.Add(this);
                }

                _prev = value;
            }
        }

        public List<EventtState> Nexts { get; set; } = new List<EventtState>();

        public Dictionary<string, IDogovorLineState> DogovorLineStates; //Срез состояний после отработки события

        public CanResponse CanExecute(EventExecuteRequest request)
        {
            DogovorLineStates = Prev?.DogovorLineStates?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, IDogovorLineState>();

            var canResponses = Eventtt.ActionItems.Select(x => x.CanExecute(request.Context, request.DayEvents, request.NewAutoEventStates, DogovorLineStates,request.strategyBranch)).ToList();

            if (canResponses.Count == 1) return canResponses[0];
            else
            {
                if (canResponses[0].CanSums.Any() && canResponses[1].CanSums.Any())
                {
                    var minSums = new HashSet<decimal>();

                    foreach (var cs1 in canResponses[0].CanSums)
                        foreach (var cs2 in canResponses[1].CanSums)
                        {
                            minSums.Add(Math.Min(cs1, cs2));
                        }

                    return new CanResponse { Success = true, CanSums = minSums, MaxSum = minSums.Max() };
                }
                else return new CanResponse { Success = false };
            }
        }


        public void Execute(EventExecuteRequest request)
        {
            DogovorLineStates = Prev?.DogovorLineStates?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, IDogovorLineState>();

            foreach (var ai in Eventtt.ActionItems)
            {
                ai.Execute(request.Context, request.DayEvents, request.NewAutoEventStates, DogovorLineStates,request.strategyBranch);
            }
        }
    }

    public class EventExecuteRequest
    {
        public Contextt Context { get; set; }
public StrategyBranch strategyBranch { get; set; }
        public List<Eventt> DayEvents { get; set; }
        public List<EventtState> NewAutoEventStates { get; set; }
    }

    public class DogovorLine
    {
        public Dogovor Dogovorr;
        public string LineName;
        public DateTime StartDate;

        //public List<DogovorLineState> DogovorLineStates;

        public bool IsActive(DateTime dat)
        {
            //TODO
            return true;
        }
    }

    public class DogovorLineState: IDogovorLineState
    {
        [JsonIgnore]
        public DateTime Dat { get; set; }
        //public int OrderNum;
        //public bool IsActive;

        [JsonIgnore]
        public IDogovorLineState prev { get; set; }

        [JsonIgnore]
        public Eventt InitialEvent { get; set; }

        public IDogovorLineState Clone()
        {
            return this.MemberwiseClone() as IDogovorLineState;
        }
    }
    public class DogovorLineStateWithSum: DogovorLineState
    {
        public decimal Sum { get; set; }
    }


    public interface IDogovorLineState
    {
        DateTime Dat { get; set; }
        IDogovorLineState prev { get; set; }
        Eventt InitialEvent { get; set; }

        IDogovorLineState Clone();
    }

    public enum Banks
    {
        Sovcom,
        Ubrir,
        Alfa,
        Tinkof,
        Vtb,
        Raif
    }
    public class ExecuteRequest
    {
        public Contextt context;
        public StrategyBranch strategyBranch;
        public string itemDogovorLineName;
        public IActionnItemParams paramss;
        public Eventt eventtt;
        public List<EventtState> NewAutoEventStates ;
        public decimal Sum;
        public string DogovorName;

        public Dictionary<string, IDogovorLineState> DogovorLinesStates { get; set; } = new Dictionary<string, IDogovorLineState>(); //Срез состояний
    }

   /* public class PutBeznalCommonActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public PutBeznalCommonActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.PutBeznal;

        public CanResponse CanExecute(Dogovor d1, Dogovor d2, decimal sum)
        {
            return new CanResponse { Success = true, MaxSum = decimal.MaxValue };
        }

        public DogovorState Execute(Dogovor itemDogovor, Eventt eventt, decimal sum)
        {
            throw new NotImplementedException();
            //Dogovor.Summ += sum;
        }
    }*/

    public class Dogovor
    {
        public string Name;
        public Banks? Bank;
        public DogovorType Typee;

        public List<IActionn> AvailableActions = new List<IActionn>();

        //public decimal Summ { get; set; }

       /*   public void RunEventHistory()
        {
            //с start запуск daystart, events и dayend с построением истории
        }

           public void ExecEvent(Eventt eventt,decimal sum)//TODO date, eventHistory, previous?
             {
                 //TODO gen orderId, link to previous event
                 foreach(var actionItem in eventt.Actions)
                 {
                    var newState= actionItem.ItemAction.Execute(actionItem.ItemDogovor, eventt,  sum);
                 }
             }

        public void InitDogs()
        {
        }*/

       public static List<Eventt> GetAvailableEvents(DogovorLine d1, List<DogovorLine> dd, bool? forManualAddOnly , bool? ForAutoGenAddOnly)
        {
            //Dogovor dog1 = new Dogovor(); Dogovor dog2 = new Dogovor();
            //List<Dogovor> dd = new List<Dogovor> { dog1, dog2 };
            var ret = new List<Eventt>();

            var availableOps = Operation.Operations
                .Where(x => x.AvailableD1Types.Contains(d1.Dogovorr.Typee) 
                            && (!forManualAddOnly .HasValue || forManualAddOnly ==x.CanAddForManual)
                            && (!ForAutoGenAddOnly.HasValue || ForAutoGenAddOnly == x.CanAddForAutoGen)
            );

            foreach (var a in availableOps)
            {
                var a1 = d1.Dogovorr.AvailableActions.Single(x => x.Type == a.ActionForD1);

                var needOtherDogovor = a.ActionForD2 != null;
                if (needOtherDogovor)
                {
                    foreach (var d2 in dd.Where(x => x != d1 && a.AvailableD2Types.Contains(x.Dogovorr.Typee)))
                    {
                        decimal sum = decimal.MaxValue;

                        CanResponse canCommon =  new CanResponse(); //a.CanExecute(d1, d2, sum); //TODO

                        var a2 = d2.Dogovorr.AvailableActions.Single(x => x.Type == a.ActionForD2);

                        CanResponse can1 = new CanResponse(); //a1.CanExecute(d1, d2, sum);
                        if (can1.Success)
                        {
                            CanResponse can2 = new CanResponse(); // = a2.CanExecute(d2, d1, sum);
                            if (can2.Success)
                            {
                                var maxSum = Math.Min(can1.MaxSum, can2.MaxSum);
                                //TODO try execute
                                var eventt = new Eventt() { Name = a.Name,OpType=a.Typ };
                                eventt.ActionItems = new List<ActionnItem>();
                                eventt.ActionItems.Add(new ActionnItem { DogovorLineName = d1.LineName, ItemAction = a.ActionForD1.Value, Sum = maxSum });
                                eventt.ActionItems.Add(new ActionnItem { DogovorLineName = d2.LineName, ItemAction = a.ActionForD2.Value, Sum = maxSum });

                                ret.Add(eventt);
                            }
                        }
                    }
                }
                else
                {
                    //add singleAction ex. closing
                    decimal sum = decimal.MaxValue;
                    CanResponse can1 =   new CanResponse(); //a1.CanExecute(d1, null, sum);
                    if (can1.Success)
                    {
                        var eventt = new Eventt() { Name = a.Name,OpType=a.Typ };
                        eventt.ActionItems = new List<ActionnItem>();
                        eventt.ActionItems.Add(new ActionnItem { DogovorLineName = d1.LineName, ItemAction = a.ActionForD1.Value, Sum = sum });
                        ret.Add(eventt);
                    }
                }
            }

            return ret;
        }/**/
    }

    public class CanResponse
    {
        public bool Success { get; set; }
        public decimal MaxSum;
        public HashSet<decimal> CanSums { get; set; } = new HashSet<decimal>();
    }

    public interface IActionn
    {
        ActionnType Type { get; set; }

        //CanResponse CanExecute(Dogovor d1, Dogovor d2, decimal sum);
        CanResponse CanExecute(ExecuteRequest request);

        DogovorState Execute(Dogovor itemDogovor, Eventt eventt, decimal sum);
        void Execute(ExecuteRequest request);
    }

    public class DogovorState
    {
    }

    /*  public class MacroActionn
      {
          public ActionnType InitialActionnType;
          public ActionnType SecondActionnType;

          public static List<MacroActionn> MacroActionns = new List<MacroActionn>
          {
              new  MacroActionn{InitialActionnType=ActionnType.PutBeznal,SecondActionnType=ActionnType.GetBeznal}
          };
          public string Name;
          }*/

 
    public enum OpType
    {
        PayZp,
        PayProcents,
        PayCashback,

        Perevod,

        SnyatCash,
        PopolnitFromCash
    }
    public enum DogovorType
    {
        Karta,
        CashWallet,
        Vklad,
        Zp
    }
    public enum ActionnType
    {
        GetBeznal,
        PutBeznal,
        Close,
        Open,
        Popolnit,
        Buy,
        DayStart,
        DayEnd,
 //       SnyatCash,
        GetCash,
        PutCash,
        SendSbp
    }

    public class StandardDogLineName
    {
        public const string CashWallet = "CashWallet";
        public const string ZpVtbKarta= "VTB zp karta";//TODO del - only for tests
        public const string Zp = "Zp";
        public const string Halva = "Halva";
    }

    /*public class StandardDogName
    {
        public const string Zp = "Zp";
    }*/

    public class ActionnItem
    {
        public string DogovorName;
        public string DogovorLineName;
        public decimal Sum;
        public ActionnType ItemAction;
        public decimal? SumBeforeCorrection;

        public IActionnItemParams Params { get; set; }
        public Eventt Eventtt { get; set; }

        public CanResponse CanExecute(Contextt context, List<Eventt> dayEvents, List<EventtState> newAutoEventStates, Dictionary<string, IDogovorLineState> dogovorLineStates, StrategyBranch strategyBranch)
        {
            var dogovor = DogovorName != null ? context.Dogovors[DogovorName] : strategyBranch.DogovorLines[DogovorLineName].Dogovorr;

            return
                  dogovor.AvailableActions.Single(x => x.Type == ItemAction)
                  .CanExecute(new ExecuteRequest
                  {
                      context = context,
                      DogovorName = DogovorName,
                      itemDogovorLineName = DogovorLineName,
                      eventtt = Eventtt,
                      paramss = Params,
                      DogovorLinesStates = dogovorLineStates,
                      NewAutoEventStates = newAutoEventStates,
                      Sum = Sum
                  });
        }

        public void Execute(Contextt context, List<Eventt> dayEvents, List<EventtState> newAutoEventStates, Dictionary<string, IDogovorLineState> dogovorLineStates, StrategyBranch strategyBranch)
        {
            var dogovor = DogovorName != null ? context.Dogovors[DogovorName] : strategyBranch.DogovorLines[DogovorLineName].Dogovorr;
            
            dogovor.AvailableActions.Single(x => x.Type == ItemAction)
            .Execute(new ExecuteRequest { context = context, strategyBranch=strategyBranch,
                DogovorName = DogovorName,
                itemDogovorLineName = DogovorLineName,
                eventtt = Eventtt, paramss = Params, DogovorLinesStates = dogovorLineStates,
                NewAutoEventStates =newAutoEventStates,
                Sum=Sum
            });
        }
    }

    public class OpenDogovorParams:IActionnItemParams
    {
        public string LineName { get; set; }
        public decimal? Limit { get;  set; }
        public decimal? Sum { get;  set; }
        public decimal? ProcentOnOst { get;  set; }
    }

    public interface IActionnItemParams
    {
    }

    public class Eventt
    {
        public List<ActionnItem> ActionItems=new List<ActionnItem>();
        public string Name;

        public Eventt()
        {
        }

        public DateTime Dat { get; set; }
        public int OrderId { get; set; }

        public bool IsPlaced { get; set; }
        public OpType? OpType { get; internal set; }
    }

    public class DummyDayStartActionn : IActionn
    {
        private Dogovor dogovor;

        public DummyDayStartActionn(Dogovor vDogovor)
        {
            this.dogovor = vDogovor;
        }

        public ActionnType Type { get; set; } = ActionnType.DayStart;

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

        }
    }

    public class DummyDayEndActionn : IActionn
    {
        private Dogovor dogovor;

        public DummyDayEndActionn(Dogovor vDogovor)
        {
            this.dogovor = vDogovor;
        }

        public ActionnType Type { get; set; } = ActionnType.DayEnd;

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

        }
    }


    public class SnyatCashCommonActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public SnyatCashCommonActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.GetCash;
      
        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response = new CanResponse { Success = true, MaxSum = decimal.MaxValue };

            var line = request.itemDogovorLineName;
            var state = request.DogovorLinesStates[line] as DogovorLineStateWithSum;
            if (state.Sum > 0) response.CanSums.Add(state.Sum);
         
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

            var state = request.DogovorLinesStates[line] as DogovorLineStateWithSum;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as DogovorLineStateWithSum;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;
                newState.Sum -= request.Sum;
                request.DogovorLinesStates[line] = newState;

                //TODO if <0
            }
        }
    }
    public class PutCashCommonActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public PutCashCommonActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.PutCash;

        public CanResponse CanExecute(ExecuteRequest request)
        {
            var response = new CanResponse { Success = true, MaxSum = decimal.MaxValue };

          response.CanSums.Add(decimal.MaxValue);

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

            var state = request.DogovorLinesStates[line] as DogovorLineStateWithSum;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as DogovorLineStateWithSum;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;
                newState.Sum += request.Sum;
                request.DogovorLinesStates[line] = newState;
            }
        }

    }

    public class BuyCommonActionn : IActionn //vs Operation.CanExecute
    {
        Dogovor Dogovor;
        public BuyCommonActionn(Dogovor dogovor)
        {
            Dogovor = dogovor;
        }
        public ActionnType Type { get; set; } = ActionnType.Buy;

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
            var line = request.itemDogovorLineName;
            var dat = request.eventtt.Dat;

            var state = request.DogovorLinesStates[line] as DogovorLineStateWithSum;

            if (request.Sum != 0)
            {
                var newState = state.Clone() as DogovorLineStateWithSum;
                newState.Dat = dat; newState.InitialEvent = request.eventtt;
                newState.prev = state;
                newState.Sum -= request.Sum;
                request.DogovorLinesStates[line] = newState;

                //TODO if <0
            }
        }
    }
}

