using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    /*
 После полного погашения займа можно заново запустить 100-дневный льготный период, но только на следующий день. 
 Если вы совершите расходную операцию в тот же день, в который полностью погасили долг, то она будет считаться 
 в рамках текущего льготного периода.
 Также важно знать, что льготный срок отсчитывается со следующего дня после первой покупки. 
 А ежемесячные взносы нужно вносить в определенный платежный период: после 20 дней со дня заключения договора.
*/
    public class AlfaCreditCard: IAccount, IDogovor
    {             private readonly IWorkDayService _workDayService;

        public AlfaCreditCard(IWorkDayService workDayService)
        {
            _workDayService = workDayService;
        }

        public Bank Bank => Bank.Alfa;
        public CardTicker Ticker => CardTicker.AlfaCreditCard;

        public TranList Transactions { get; set; } = new TranList();



        // public List<Claim> Claims { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; } = null;

        public decimal CreditLimit = 200000;//DatedValueCollection<decimal> CreditLimit;
        private decimal Debt
        {
            get {                //var d=
                return CurrentState.Amount < CreditLimit ? CreditLimit - CurrentState.Amount : 0;
            }
        }

        public decimal MinPayProcent = 3; //DatedValueCollection<decimal> MinPayProcent;

        public bool IsActive(DateTime d)
        {
            if (InitState != null && d < InitState.Dat 
                || Start.HasValue && d < Start
                || End.HasValue && d > End) return false;
            return true;
        }

        public int MinPayCalcDay=15;// Start.Day

        public decimal FreeMonthCashAmount=50000;
        /// <summary>
        /// FirstBuyDate? To calc date to get year commission 
        /// </summary>
        public DateTime? YearCommDate=new DateTime(2020,08,16);
        public decimal YearComm=1490;//DatedValueCollection<decimal> 

        public AlfaCreditCardState InitState;

        public AlfaCreditCardState CurrentState { get; set; }
        public string Name;

        public List<ISumActionCommand> OnDayStart(DateTime d)
        {
            var ret = new List<ISumActionCommand>();

            if (CurrentState == null)
            {
                if (InitState != null && InitState.Dat != d) throw new Exception("InitState.Dat != d");

                if (InitState != null)
                {
                    CurrentState = InitState.DeepClone();
                    CurrentState.Dat = d;
                }
                else
                {
                    CurrentState = new AlfaCreditCardState { Dat = d };
                    CurrentState.Amount = CreditLimit;
                }
            }
            else
            {
                if (CurrentState.Dat.AddDays(1) != d) throw new Exception("CurrentState.Dat.AddDays(1) != d");
                CurrentState.Dat = d;
            }

            if (d.Day == 1) CurrentState.FreeMonthCashOst = FreeMonthCashAmount;

            if (Debt== 0 && CurrentState.PeriodEndDate.HasValue)
            {
                CurrentState.PeriodEndDate = null;
            }

            if (Debt>0 && d.Day == MinPayCalcDay)
            {
                CurrentState.MinPayEndDate = _workDayService.GetWorkDayOrAfter(d.AddDays(20));

                CurrentState.MinPayOst = Math.Max(320, (int)(Debt * MinPayProcent / 100)/100*100);
            }

            if (d.Day == YearCommDate.Value.Day && d.Month == YearCommDate.Value.Month)
            {
                ret.Add(new GetSumCommand(d,YearComm,this.Name,CommandTicker.AlfaYearComm));
            }

            return ret;
        }

        public List<Error> OnDayEnd(DateTime d)
        {
            var errors = new List<Error>();

            /* if (CurrentState.MinPayOst > 0 && d == CurrentState.MinPayEndDate)
                 errors.Add(new Error($"min pay {CurrentState.MinPayOst} not paid"));

             if (CurrentState.Amount < 0 && d == CurrentState.PeriodEndDate)
                 errors.Add(new Error($"debt {CurrentState.Amount} not paid"));*/
            foreach (var claim in GetClaims())
                errors.Add(new Error(claim.Message));

            return errors;
        }

        public List<Claim> GetClaims()
        {
            var claims = new List<Claim>();

            var d = CurrentState.Dat;
            if (CurrentState.MinPayOst > 0 && d == CurrentState.MinPayEndDate)
                claims.Add(new Claim() { EndDat = d, Sum = CurrentState.MinPayOst, Message = $"min pay {CurrentState.MinPayOst} not paid" });

            if (Debt> 0 && d == CurrentState.PeriodEndDate)
                claims.Add(new Claim() { EndDat = d, Sum = Debt, Message = $"debt {Debt} not paid" });

            return claims;
        }

        public void OnPrihod(RashodRequest request)
        {
            if (CurrentState.MinPayOst > 0)
            {
                if (request.sum < CurrentState.MinPayOst)
                    CurrentState.MinPayOst -= request.sum;
                else
                {
                    CurrentState.MinPayOst = 0;
                    CurrentState.MinPayEndDate = null;
                }
            }

            CurrentState.Amount += request.sum;
            Transactions.trans.Add(new Tran(request));
        }

        public List<Error> OnRashod(RashodRequest request)
        {
            var errors = new List<Error>();

            if (YearCommDate == null) YearCommDate = request.Dat;

                CurrentState.Amount -= request.sum;

            if (Debt>0 && Debt > CreditLimit)
                errors.Add(new Error($"CreditLimit {Debt} is exceeded"));

            if (Debt>0 && CurrentState.PeriodEndDate == null)
            {
                CurrentState.PeriodEndDate = request.Dat.AddDays(100+1);
            }
                            
            if (request.OpType==OperationType.GetCash)
            {
                if (CurrentState.FreeMonthCashOst < request.sum)
                    errors.Add(new Error($"FreeMonthCashOst {CurrentState.FreeMonthCashOst} is less than {request.sum}",ErrorType.Warning));

                CurrentState.FreeMonthCashOst -=request.sum;
            }
            Transactions.trans.Add(new Tran(request));

            return errors;
        }

        public CanRashodResponse CanRashod(RashodRequest request)
        {           
            bool result = true;
            decimal max = decimal.MaxValue, min = 0.01m;


            if (request.OpType == OperationType.GetCash)
            {
                min = 100;

                if (request.Place == ATMPlace.Own)
                {
                    max = ((int)( CurrentState.FreeMonthCashOst) / 100) * 100;
                }
                else throw new NotImplementedException();
            }

            if (max > CurrentState.Amount)
                max = CurrentState.Amount;

            if (min > CurrentState.Amount)
                result = false;

            if (request.sum < min || request.sum > max || min > max)
                result = false;

            return new CanRashodResponse { Success = result, MinSum = min, MaxSum = max };
        }

        public CanRashodResponse CanPrihod(RashodRequest request)
        {
            bool result = true;
            decimal max = decimal.MaxValue, min = 0.01m;
            if (request.OpType == OperationType.PutCash)
            {
                min = 100;

                if (request.Place == ATMPlace.Own)
                {

                }
                else if (request.Place == ATMPlace.Partner)
                {
                    //TODO налом у партнеров альфы
                }
                else result = false;
            }
            else if (request.OpType == OperationType.AddCashless)
            {

            }

            if (request.sum < min || request.sum > max)
                result = false;

            return new CanRashodResponse { Success = result, MinSum = min, MaxSum = max };
        }
    }

    public class AlfaCreditCardState
    {
        [JsonIgnore]
        public DateTime Dat;

        public decimal FreeMonthCashOst;

        public decimal Amount;//доступное для трат
        //public decimal Debt;

        public DateTime? PeriodEndDate;

        public decimal MinPayOst;
        public DateTime? MinPayEndDate;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /*public BrockerAccState EndDayAndStartNew(DateTime d)
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
            var newState = new ObligState() { BrokerState = this };
            newState.EntityId = States.Any() ? States.Max(pp => pp.EntityId) + 1 : 1;
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
            priceInfo.Commission = Math.Round((priceInfo.Price + priceInfo.NKD) * Birja.Commission / 100, 2);//TODO brocker comm rate

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
            var countOst = prevState.Count - count; if (countOst < 0) throw new Exception("not enough count to sell");
            newState.Count = countOst;
            if (countOst == 0) newState.IsActive = false;

            var priceInfo = new PriceInfo() { Type = EventType.Sell, Dat = dat, Count = count };
            //priceInfo.Count = price.Value;
            priceInfo.Price = price.Value;
            priceInfo.NKD = newState.Instr.GetNKD(dat);// newState.NKD;
            priceInfo.Commission = Math.Round((priceInfo.Price + priceInfo.NKD) * Birja.Commission / 100, 2);//TODO brocker comm rate
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

                var newDiap = new RevenueDiap()
                {
                    StartDat = startDat,
                    EndDat = s.Dat,
                    InputSum = 0,
                    OutputSum = (s.TotalPrice - startSum + kupons) * s.Count
                };
                ret.Add(newDiap);
            }
            if (prevState.Gashenie != null)
            {
                decimal kupons = 0;
                var kd = prevState.Instr.StartDat;
                while ((kd = kd.AddDays(prevState.Instr.Period)) <= prevState.Gashenie.Dat)
                    kupons += prevState.Instr.PlanKupons.GetValue(kd);

                var newDiap = new RevenueDiap()
                {
                    StartDat = startDat,
                    EndDat = prevState.Gashenie.Dat,
                    InputSum = 0,
                    OutputSum = (prevState.Gashenie.TotalPrice - startSum + kupons) * prevState.Gashenie.Count
                };
                ret.Add(newDiap);
            }
            return ret;
        }*/
    }



    
}
