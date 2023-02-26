using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinansPlan2.New
{

    public class Processor {
        public static void ProcessPeriod(Contextt context)
        {
            context.InitDatedScope = CalcDat(context, context.PeriodStart, null);

            var curDatedScope = context.InitDatedScope;
            var curDat = context.PeriodStart;
            while (curDat < context.PeriodEnd)
            {
                curDat = curDat.AddDays(1);
                curDatedScope = CalcDat(context, curDat, curDatedScope);
            }
        }

        public static void RecalcPeriod(Contextt context, DateTime fromDat)
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
        }

        public static DatedEventtStateScope CalcDat(Contextt context, DateTime dat, DatedEventtStateScope prevDatedScope)
        {
            var newDayEvents = new List<EventtState>();
            var datedScope = new DatedEventtStateScope { Dat = dat, DayEventStates = newDayEvents, Prev = prevDatedScope };
            if (prevDatedScope != null) prevDatedScope.Next = datedScope;

            var dayEvents = context.Events.Where(x => x.Dat == dat).OrderBy(x => x.OrderId).ToList();

            var dayStartEventt = new Eventt { Dat = dat };
            var dayStartEventtState = new EventtState { Eventtt = dayStartEventt, Prev = prevDatedScope?.DayEventStates.Last() };
            newDayEvents.Add(dayStartEventtState);

            var newAutoEvents = new List<EventtState>();
            var dayEventExecuteRequest = new EventExecuteRequest { Context = context, DayEvents = dayEvents, NewAutoEventStates = newAutoEvents };

            foreach (var l in context.DogovorLines.Values)
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
                newAutoEvent.Execute(new EventExecuteRequest { Context = context });
                prev = newAutoEvent;
            }

            foreach (var de in dayEvents)
            {
                var manualDayEventtState = new EventtState { Eventtt = de, Prev = prev };
                newDayEvents.Add(manualDayEventtState);
                manualDayEventtState.Execute(dayEventExecuteRequest);

                prev = manualDayEventtState;
            }


            var dayEndEventt = new Eventt { Dat = dat };
            var dayEndEventtState = new EventtState { Eventtt = dayEndEventt, Prev = prev };
            newDayEvents.Add(dayEndEventtState);
            foreach (var l in context.DogovorLines.Values)
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

            return datedScope;
        }

        public static void Print(Contextt context)
        {
            StringBuilder result = new StringBuilder();
            var actual = context.InitDatedScope;
            while (actual != null)
            {
                result.AppendLine("-" + actual.Dat.ToShortDateString());

                foreach (var es in actual.DayEventStates)
                {
                    result.AppendLine("---"+es.Eventtt.Name);
                    foreach (var ls in es.DogovorLineStates)
                    {
                        if (ls.Value.InitialEvent != es.Eventtt) continue;

                        if (ls.Value.prev != null)
                        {
                            var diff = ObjectCloner.GetDiff(ls.Value.prev, ls.Value);
                            result.AppendLine("-----" + ls.Key+": "+string.Join("; ", diff));
                        }
                    }
                }

                result.AppendLine("----------------");
                actual = actual.Next;
            }
            var s = result.ToString();
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