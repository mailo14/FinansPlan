using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class ObligState//Position
    {
        public Oblig Instr;//TODO Interface
        public int EntityId;//gen for each buy and keep in childs
        public int Count;
        public decimal NKD;
        public DateTime Dat;

        public bool IsActive = true;

        public ObligState Clone()
        {
            return new ObligState()
            {
                Instr = Instr,
                Count = Count,
                EntityId = EntityId,
                IsActive = IsActive,
                NKD = NKD,
                Dat = Dat
            };
        }

        public void OnDayStart(DateTime d)
        {
            throw new NotImplementedException();
        }

        public void OnDayEnd(DateTime d)
        {
            throw new NotImplementedException();
        }
    }

    public class BrockerAccState
    {
        public MOEX Birja;

        public List<ObligState> States = new List<ObligState>();
        public decimal RubSum;
        public DateTime Dat;
        public BrockerAccState EndDayAndStartNew(DateTime d)
        {
            if (d != Dat) throw new Exception("not prev dat");

            foreach (var s in States)
            {
                if (s.IsActive)
                {
                    s.OnDayEnd(d);
                }
            }
            var newDay = d.AddDays(1);
            var ret = new BrockerAccState()
            {
                Birja = Birja,
                RubSum = RubSum,
                Dat = newDay,
                States = new List<ObligState>()

            };
            foreach (var s in States)
            {
                if (!s.IsActive)
                    ret.States.Add(s);
                else
                {//TODO clone only if changed while OnDayStart?
                    var newItem = s.Clone();
                    newItem.Dat = newDay;
                    newItem.OnDayStart(newDay);
                    ret.States.Add(newItem);
                }
            }
            return ret;
        }
        public (PriceInfo priceInfo, ObligState state) BuyInstr(string instrCode, int count, decimal? price, DateTime dat)
        {
            var newState = new ObligState() { };
            newState.EntityId = States.Any()?States.Max(pp => pp.EntityId) + 1:1;
            newState.Dat = dat.Date;
            newState.Instr = Birja.GetOblig(instrCode);
            if (!price.HasValue)
            {
                price = newState.Instr.Prices.GetValue(dat);
            }
            newState.NKD = newState.Instr.GetNKD(dat);
            newState.Count = count;
            newState.IsActive = true;

            var priceInfo = new PriceInfo();
            priceInfo.Price = (price + newState.NKD).Value;
            priceInfo.Commission = Math.Round(priceInfo.Price * Birja.Commission / 100, 2);//TODO brocker comm rate
            return (priceInfo, newState);
        }
    }

    public class PriceInfo
    {
        public decimal Price;
        public decimal Commission;
        public decimal TotalPrice
        {
            get
            {
                return Price + Commission;
            }
        }
    }
    public class MOEX
    {
        public List<Oblig> Obligs=new List<Oblig>();

        public decimal Commission = 0.06m;

        public Oblig GetOblig(string instrCode)
        {
            return Obligs.First(pp => pp.InstrCode == instrCode);
        }
    }

    public class HistEvent
    {
        public EventType Type;
        public Oblig Instr;
        public string InstrCode;
        public int? InstrId;
        public DateTime Dat;
        public int Count;
        public decimal? Price;
        //public decimal Comission;
        //public decimal Nalog;
    }
    public enum EventType
    {
        Buy
    }

    public class Timer
    {
        public List<HistEvent> Events;
        public List<BrockerAccState> BrockerAccStates = new List<BrockerAccState>();

        public Timer()
        {
            Init();
        }
        public void Init()
        {
            //var brockerAccState = new BrockerAccState() { Dat = DateTime.Parse("1.08.19"), RubSum = 100000, States = new List<ObligState>() };
            //BrockerAccStates.Add(brockerAccState);
        }

        //public DateTime Dat;
        public void ProcessEvent(HistEvent ev)
        {
            var cur = BrockerAccStates.Last();
            while (cur.Dat < ev.Dat.Date)
            {
                cur = cur.EndDayAndStartNew(cur.Dat);
            }

            var newState = cur.EndDayAndStartNew(ev.Dat.Date);//TODO prev day
                                                              //TODO clean afters
            if (ev.Type == EventType.Buy)
            {
                var response = newState.BuyInstr(ev.InstrCode, ev.Count, ev.Price, ev.Dat);
                newState.RubSum -= response.priceInfo.TotalPrice* ev.Count;
                newState.States.Add(response.state);

                BrockerAccStates.Add(newState);
                // var inst= newState.States.FirstOrDefault(pp => pp.InstrId == ev.InstrId);
                // inst.Instr.
            }
            //var q=(from e in Events
            //if (BrockerAccStates.FirstOrDefault())
        }

    }
}