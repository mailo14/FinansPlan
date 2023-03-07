using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinansPlan2.New
{

    public class Processor {
        public static void ProcessPeriod(Contextt context)//, StrategyBranch strategyBranch = null)
        {
            //if (strategyBranch == null) strategyBranch = context.StrategyBranches.Single();
            CalcDat(context, context.PeriodStart);//, strategyBranch);
            
            var curDat = context.PeriodStart;
            while (curDat < context.PeriodEnd)
            {
                curDat = curDat.AddDays(1);
                CalcDat(context, curDat);//, strategyBranch);
            }
        }

        /*public static void RecalcPeriod(Contextt context, DateTime fromDat)
        {
            if (context.InitDatedScope.Dat == fromDat)
            {
                ProcessPeriod(context);
                return;
            }

            var curDatedScope = context.InitDatedScope;
            while (curDatedScope.Next.Dat != fromDat) curDatedScope = curDatedScope.Next;

            var curDat = fromDat.AddDays(-1);
            while (curDat < context.PeriodEnd)
            {
                curDat = curDat.AddDays(1);
                curDatedScope = CalcDat(context, curDat, curDatedScope);
            }
        }*/
        public static void DoDayStart(Contextt context, DateTime dat, StrategyBranch strategyBranch)
        {
            var newDayEvents = new List<EventtState>();

            var dayEvents = strategyBranch.Events.Where(x => x.Dat == dat).OrderBy(x => x.OrderId).ToList();

            var dayStartEventt = new Eventt { Dat = dat };
            var dayStartEventtState = new EventtState { Eventtt = dayStartEventt, Prev = strategyBranch.LastEventState };
            newDayEvents.Add(dayStartEventtState);

            var newAutoEvents = new List<EventtState>();
            var dayEventExecuteRequest = new EventExecuteRequest { Context = context, DayEvents = dayEvents, NewAutoEventStates = newAutoEvents, strategyBranch = strategyBranch };

            foreach (var l in strategyBranch.DogovorLines.Values)
            {
                if (!l.IsActive(dat)) continue;

                dayStartEventt.ActionItems.Add(new ActionnItem
                {
                    Eventtt = dayStartEventt,
                    DogovorName = l.Dogovorr.Name,
                    DogovorLineName = l.LineName,
                    ItemAction = ActionnType.DayStart
                });
            }
            dayStartEventtState.Execute(dayEventExecuteRequest);

            var prev = dayStartEventtState;

            foreach (var newAutoEvent in newAutoEvents)
            {
                newAutoEvent.Prev = prev;
                newDayEvents.Add(newAutoEvent);
                newAutoEvent.Execute(new EventExecuteRequest { Context = context, strategyBranch = strategyBranch });
                prev = newAutoEvent;
            }

            if (newDayEvents.Any()) strategyBranch.LastEventState = newDayEvents.Last();
        }

        public static void DoManualDayEvents(Contextt context, DateTime dat, StrategyBranch strategyBranch)
        {        
            //var newDayEvents = new List<EventtState>();

            var dayEvents = strategyBranch.Events.Where(x => x.Dat == dat).OrderBy(x => x.OrderId).ToList();

            //var dayEventExecuteRequest = new EventExecuteRequest { Context = context, DayEvents = null, NewAutoEventStates = null, strategyBranch = strategyBranch };

            foreach (var de in dayEvents)
            {
                DoEvent(context, dat, strategyBranch, de);
            }
            /*var prev = strategyBranch.LastEventState;

            foreach (var de in dayEvents)
            {
                var manualDayEventtState = new EventtState { Eventtt = de, Prev = prev };
                newDayEvents.Add(manualDayEventtState);
                manualDayEventtState.Execute(dayEventExecuteRequest);

                prev = manualDayEventtState;
            }

           if (newDayEvents.Any()) strategyBranch.LastEventState = newDayEvents.Last();*/
        }

        public static void DoEvent(Contextt context, DateTime dat, StrategyBranch strategyBranch, Eventt eventt)
        {
            var dayEventExecuteRequest = new EventExecuteRequest { Context = context, DayEvents = null, NewAutoEventStates = null, strategyBranch = strategyBranch };

            var manualDayEventtState = new EventtState { Eventtt = eventt, Prev = strategyBranch.LastEventState };
            manualDayEventtState.Execute(dayEventExecuteRequest);

            strategyBranch.LastEventState = manualDayEventtState;
        }

        public static void DoDayEnd(Contextt context, DateTime dat, StrategyBranch strategyBranch)
        {
            var newDayEvents = new List<EventtState>();
           
            var dayEventExecuteRequest = new EventExecuteRequest { Context = context, DayEvents = null, NewAutoEventStates = null,strategyBranch= strategyBranch };

            var prev = strategyBranch.LastEventState;

            var dayEndEventt = new Eventt { Dat = dat };
            var dayEndEventtState = new EventtState { Eventtt = dayEndEventt, Prev = prev };
            newDayEvents.Add(dayEndEventtState);
            foreach (var l in strategyBranch.DogovorLines.Values)
            {
                if (!l.IsActive(dat)) continue;

                dayEndEventt.ActionItems.Add(new ActionnItem
                {
                    Eventtt = dayEndEventt,
                    DogovorName = l.Dogovorr.Name,
                    DogovorLineName = l.LineName,
                    ItemAction = ActionnType.DayEnd
                });
            }
            dayEndEventtState.Execute(dayEventExecuteRequest);

            if (newDayEvents.Any()) strategyBranch.LastEventState = newDayEvents.Last();
        }

        public static void CalcDat(Contextt context, DateTime dat, StrategyBranch concreteStrategyBranch=null)
        {
            var strategyBranches = concreteStrategyBranch == null ? context.StrategyBranches : new List<StrategyBranch> { concreteStrategyBranch };

            foreach (var strategyBranch in strategyBranches)
            {
                DoDayStart(context, dat, strategyBranch);
                DoManualDayEvents(context, dat, strategyBranch);
                DoDayEnd(context, dat, strategyBranch);
            }
        }

        public static void GenDat(Contextt context, DateTime dat, int? maxEventCount=null)
        {
            foreach (var strategyBranch in context.StrategyBranches)
            {
                DoDayStart(context, dat, strategyBranch);
                //DoManualDayEvents(context, dat, strategyBranch); //on start
            }

            var prevStrategyBranchesCount = 0;
            var generationId = 0;

            while (true)
            {
                generationId++;

                var strategyBranchesToProcess = context.StrategyBranches.Skip(prevStrategyBranchesCount);
                prevStrategyBranchesCount = context.StrategyBranches.Count;

                foreach (var strategyBranch in strategyBranchesToProcess)
                {
                    if (strategyBranch.Error != null) continue;

                    var availableEvents = GetAvailableEvents(strategyBranch); 
                    foreach (var ev in availableEvents)
                    {
                        var newstrategyBranch = strategyBranch.Clone();
                        context.StrategyBranches.Add(newstrategyBranch);

                        DoEvent(context, dat, newstrategyBranch, ev);
                    }
                }

                if (context.StrategyBranches.Count == prevStrategyBranchesCount || generationId>(maxEventCount??int.MaxValue)) break;
            }

            foreach (var strategyBranch in context.StrategyBranches)
            {
                DoManualDayEvents(context, dat, strategyBranch); //on end
                DoDayEnd(context, dat, strategyBranch);
            }
        }

        private static List<Eventt> GetAvailableEvents(StrategyBranch strategyBranch)
        {
            var result = new List<Eventt>();

            var curentDayEvents = GetStrategyBranchStates(strategyBranch, true);//TODO .SkipWhile(x=>x.Eventtt.auto;

            var dd = strategyBranch.DogovorLines.Values.ToList();

            var events = new List<Eventt>();
            foreach (var d1 in dd)
            {
                var d1Events = Dogovor.GetAvailableEvents(d1, dd,null,true);
                events.AddRange(d1Events);
            }

            foreach(var ev in events)
            {
                if (CanAddAutoGenEvent(ev, curentDayEvents))
                {
                    result.Add(ev);
                }
            }

            return result;
        }

        private static bool CanAddAutoGenEvent(Eventt ev, List<EventtState> curentDayEvents)
        {
            var existDubl = curentDayEvents.FirstOrDefault(x => x.Eventtt.OpType == ev.OpType.Value
             && x.Eventtt.ActionItems[0].DogovorLineName == ev.ActionItems[0].DogovorLineName && x.Eventtt.ActionItems[0].DogovorName == ev.ActionItems[0].DogovorName
             && (x.Eventtt.ActionItems.Count == 1
                 || x.Eventtt.ActionItems[1].DogovorLineName == ev.ActionItems[1].DogovorLineName && x.Eventtt.ActionItems[1].DogovorName == ev.ActionItems[1].DogovorName)
            );
            if (existDubl != null) return false;//TODO кейс ограничения на одну операцию

            return true;
        }

        public static void Print(Contextt context, StrategyBranch strategyBranch = null)
        {
            if (strategyBranch == null) strategyBranch = context.StrategyBranches.Single();

            StringBuilder result = new StringBuilder();
            var eventStates = GetStrategyBranchStates(strategyBranch);
            DateTime? prevDate = null;
            foreach (var es in eventStates)
            {
                if (es.Eventtt.Dat != prevDate)
                {
                    result.AppendLine("-" + es.Eventtt.Dat.ToShortDateString());
                    prevDate = es.Eventtt.Dat;
                }


                result.AppendLine("---" + es.Eventtt.Name);
                foreach (var ls in es.DogovorLineStates)
                {
                    if (ls.Value.InitialEvent != es.Eventtt) continue;

                    if (ls.Value.prev != null)
                    {
                        var diff = ObjectCloner.GetDiff(ls.Value.prev, ls.Value);
                        result.AppendLine("-----" + ls.Key + ": " + string.Join("; ", diff));
                    }

                }

                result.AppendLine("----------------");
            }
            var s = result.ToString();
        }

        private static List<EventtState> GetStrategyBranchStates(StrategyBranch strategyBranch,bool onlyForLastDay=false)
        {
            var state = strategyBranch.LastEventState;
            var eventStates = new List<EventtState>();
            DateTime? dat = state.Eventtt.Dat;
            while (state != null)
            {
                eventStates.Insert(0, state);
                state = state.Prev;

                if (onlyForLastDay && state?.Eventtt?.Dat != dat)
                {
                    break;
                }

                dat=state?.Eventtt?.Dat;
            }

            return eventStates;
        }

        public static void CalcRoutes(DatedEventtStateScope eventScope)
        {
            var routes = new List<List<PlaceWeighted>>();
            //TODO
            var placesByOp = eventScope.DayEventStates.Where(x => x.Eventtt.IsPlaced).Select(x => AtmPlaceInfo.GetPlacesToOperateCash(Banks.Alfa, true, 400)).ToList();
            //x.Eventtt.GetPlaces()).ToList();
            if (placesByOp.Any())
            {
                var homePlace = new PlaceWeighted { Place = Place.Dom };
                routes = new List<List<PlaceWeighted>> { new List<PlaceWeighted> { homePlace } }; //placesByOp[0].Select(x => new List<PlaceWeighted> { homePlace).ToList();

                foreach (var opPlaces in placesByOp)
                {
                    var startCount = routes.Count;
                    foreach (var r in routes)
                    {
                        foreach (var p in opPlaces)
                        {
                            var newRoute = r.ToList();
                            newRoute.Add(p);
                            routes.Add(newRoute);
                        }
                    }
                    routes.RemoveRange(0, startCount);
                }

                foreach (var r in routes) r.Add(homePlace);

                var estimates = routes.Select(x => new RouteEstimate { Route = x, Dist = 0m, OpTime = 0m }).ToList();//new List<(List<PlaceWeighted> route, decimal weight)> (){;
                foreach (var e in estimates)
                {
                    var route = e.Route;
                    for (int i = 0; i < route.Count - 1; i++)
                    {
                        var curr = route[i]; var next = route[i + 1];
                        if (next.Place != curr.Place)
                        {
                            e.Dist += AtmPlaceInfo.GetDist(next.Place, curr.Place);
                        }
                        e.OpTime += next.OpTime;
                    }
                }
                //estimates.Sort()
            }
        }
        /* private static void ProcessLineDayStart(Eventt dayStartEventt, DogovorLine l, List<Eventt> dayEvents, List<Eventt> newDayEvents)
{
    var corrections = dayEvents.Where(x => x.IsCorrection).ToList();
    //TODO corrections with remove from dayEvents
   var ai= new ActionnItem
    {
        Eventtt = dayStartEventt,
        ItemDogovor = l,
        ItemAction = l.Dogovorr.AvailableActions.Single(x => x.Type == ActionnType.DayStart)
    };

    dayStartEventt.Actions.Add(ai);
}*/
    }

    internal class RouteEstimate
    {
        public List<PlaceWeighted> Route { get; set; }
        public decimal Dist { get; set; }
        public decimal  OpTime { get; set; }
    }
}