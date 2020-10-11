using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class ObligState//Position
    {
        public BrockerAccState BrokerState;
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
                Dat = Dat,
                BrokerState = BrokerState,
                BuyFactPrice = BuyFactPrice,
                Gashenie = Gashenie,
                SellFactPrices = SellFactPrices,
                FactKupons = FactKupons,
            };
        }

        public void OnDayStart(DateTime d)
        {
            NKD = Instr.GetNKD(d);
        }
        public PriceInfo BuyFactPrice;
        public PriceInfo Gashenie;
        public List<PriceInfo> SellFactPrices;
        public decimal FactKupons;
        //public List<PriceInfo> FactKupons
        public void OnDayEnd(DateTime d)
        {
            var taxer = BrokerState.Birja.TaxStrategies.GetValue(d);

            var kupon = Instr.PlanKupons.GetValueExactOnDate(d);
            if (kupon != null)
            {
                var priceInfo = new PriceInfo() { Type = EventType.KuponPay, Dat = d, Count = Count };
                priceInfo.Price = kupon.v;

                priceInfo.Tax = taxer.GetTaxOnKuponPay(d, kupon.v, Instr);
                //return (priceInfo, newState);
                //newState.RubSum -= response.priceInfo.TotalPrice * ev.Count;
                //newState.States.Add(response.state);

                //BrockerAccStates.Add(newState);
                FactKupons += priceInfo.TotalPrice * Count;
                BrokerState.RubSum += priceInfo.TotalPrice * Count;
            }
            if (d == Instr.EndDat)
            {
                var priceInfo = new PriceInfo() { Type = EventType.Gashenie, Dat = d, Count = Count };
                var nominal = 1000;//TODO amortization
                priceInfo.Price = nominal;
                priceInfo.Tax = taxer.GetTaxOnObligEnd(d, BuyFactPrice, nominal);
                Gashenie = priceInfo;

                BrokerState.RubSum += priceInfo.TotalPrice * Count;
                IsActive = false;
            }
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
            var newState = new ObligState() { BrokerState=this };
            newState.EntityId = States.Any()?States.Max(pp => pp.EntityId) + 1:1;
            newState.Dat = dat.Date;
            newState.Instr = Birja.GetOblig(instrCode);
            if (!price.HasValue)
            {
                price = newState.Instr.Prices.GetValue(dat);
            }
            //newState.NKD = newState.Instr.GetNKD(dat);
            newState.Count = count;
            newState.IsActive = true;

            var priceInfo = new PriceInfo() { Type = EventType.Buy, Dat = dat, Count = count };
            priceInfo.Price = price.Value;
            priceInfo.NKD = newState.Instr.GetNKD(dat);//newState.NKD;
            priceInfo.Commission = Math.Round((priceInfo.Price+ priceInfo.NKD) * Birja.Commission / 100, 2);//TODO brocker comm rate

            newState.BuyFactPrice = priceInfo;
            return (priceInfo, newState);
        }
        public (PriceInfo priceInfo, ObligState state) SellInstr(int entityId, int count, decimal? price, DateTime dat)
        {
            var prevState = (from s in States where s.EntityId == entityId orderby s.Dat descending select s).FirstOrDefault();
            if (prevState == null) throw new Exception("no entity to sell");

            var newState = prevState.Clone(); //new ObligState() { BrokerState=this };
            //newState.EntityId = entityId;
            newState.Dat = dat.Date;
            //newState.Instr = prevState..Instr;
            if (!price.HasValue)
            {
                price = newState.Instr.Prices.GetValue(dat);
            }
            //newState.NKD = newState.Instr.GetNKD(dat);
            var countOst = prevState.Count - count; if (countOst<0) throw new Exception("not enough count to sell");
            newState.Count = countOst;
            if (countOst==0) newState.IsActive = false;

            var priceInfo = new PriceInfo() { Type=EventType.Sell, Dat = dat, Count = count };
            //priceInfo.Count = price.Value;
            priceInfo.Price = price.Value;
            priceInfo.NKD = newState.Instr.GetNKD(dat);// newState.NKD;
            priceInfo.Commission = Math.Round((priceInfo.Price+ priceInfo.NKD) * Birja.Commission / 100, 2);//TODO brocker comm rate
            var taxer = Birja.TaxStrategies.GetValue(dat);
            priceInfo.Tax = taxer.GetTaxOnObligSell(dat, newState.BuyFactPrice, priceInfo);
            newState.SellFactPrices.Add(priceInfo);
            return (priceInfo, newState);
        }

        public List<RevenueDiap> GetRevenueDiaps(int entityId)
        {
            var prevState = (from s in States where s.EntityId == entityId orderby s.Dat descending select s).FirstOrDefault();
            if (prevState == null) throw new Exception("no entity to sell");

            var ret = new List<RevenueDiap>();//ret[].InputSum
            var startDat = prevState.BuyFactPrice.Dat;
            var startSum = prevState.BuyFactPrice.TotalPrice;

            foreach (var s in prevState.SellFactPrices)
            {
                decimal kupons = 0;
                var kd = prevState.Instr.StartDat;
                while ((kd = kd.AddDays(prevState.Instr.Period)) <= s.Dat)
                    kupons += prevState.Instr.PlanKupons.GetValue(kd);

                var newDiap = new RevenueDiap() { StartDat = startDat, EndDat = s.Dat, InputSum = 0,
                    OutputSum = (s.TotalPrice - startSum+kupons)*s.Count };
                ret.Add(newDiap);
            }
            if (prevState.Gashenie!=null)
            {
                decimal kupons = 0;
                var kd = prevState.Instr.StartDat;
                while ((kd = kd.AddDays(prevState.Instr.Period)) <= prevState.Gashenie.Dat)
                    kupons += prevState.Instr.PlanKupons.GetValue(kd);

                var newDiap = new RevenueDiap() { StartDat = startDat, EndDat = prevState.Gashenie.Dat, InputSum = 0,
                    OutputSum = (prevState.Gashenie.TotalPrice - startSum + kupons) * prevState.Gashenie.Count
                    };
                ret.Add(newDiap);
            }
            return ret;
        }
    }

    public class PriceInfo
    {
        public EventType Type;
        public decimal Price;
        public decimal Commission;
        public decimal Tax;
        public decimal TotalPrice
        {
            get
            {
                switch (Type)
                {
                    case EventType.Buy: return Price + NKD + Commission + Tax;

                    case EventType.Gashenie:
                    case EventType.KuponPay:
                        return Price - Tax;                    
                    case EventType.Sell: return Price + NKD - Commission - Tax;

                    default: throw new Exception("unknown type for TotalPrice");
                }
            }
        }

        public decimal NKD;
        public int Count;
        public DateTime Dat;
    }
    public class MOEX
    {
        public List<Oblig> Obligs=new List<Oblig>();

        public decimal Commission = 0.06m;

        public DatedValueCollection<ITaxStrategy> TaxStrategies = new DatedValueCollection<ITaxStrategy>(new List<DatedValue<ITaxStrategy>> {
            new DatedValue<ITaxStrategy>("01.01.2000", new NewTaxStrategy())
        });

        public MOEX()
        {
          //  TaxStrategies.
        }

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
        //public int? InstrId;
        public DateTime Dat;
        public int Count;
        public decimal? Price;
        public int EntityId; //position id
        //public decimal Comission;
        //public decimal Nalog;
    }
    public enum EventType
    {
        Buy,Sell,
        KuponPay, Gashenie
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
            else if (ev.Type == EventType.Sell)
            {
                var response = newState.SellInstr(ev.EntityId, ev.Count, ev.Price, ev.Dat);
                newState.RubSum += response.priceInfo.TotalPrice* ev.Count;
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