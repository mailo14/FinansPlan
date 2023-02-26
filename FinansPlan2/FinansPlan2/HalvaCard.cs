using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class HalvaCard : IAccount, IDogovor
    {
        public Bank Bank => Bank.Sovkom;
        public CardTicker Ticker => CardTicker.HalvaCard;

        public TranList Transactions { get; set; } = new TranList();

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; } = null;

        public bool IsActive(DateTime d)
        {
            if (InitState != null && d < InitState.Dat
                || Start.HasValue && d < Start
                || End.HasValue && d > End) return false;
            return true;
        }

        public HalvaCardState InitState;

        public HalvaCardState CurrentState { get; set; }
        public string Name;

        public DatedValueCollection<decimal> ProcentOnOst = new DatedValueCollection<decimal>(new List<DatedValue<decimal>> {
            new DatedValue<decimal>("01.01.2000", 7m),
            new DatedValue<decimal>("13.11.2019", 6.5m),
            new DatedValue<decimal>("1.06.2020", 5.5m),
            new DatedValue<decimal>("18.08.2020", 5m) });


        public int PeriodStartDayOfMonth = 27;
        public int LimitsPeriodStartDayOfMonth = 1;

        public decimal LimitGetCash = 500000;

        public decimal LimitSendOtherBankCard = 300000;
        public decimal LimitDaySendOtherBankCard = 100000;
        public decimal LimitOpSendOtherBankCard = 40000;

        public decimal LimitOpPull = 15000;

        public decimal LimitDaySendFastByPhoneNumber = 30000;

        public List<ISumActionCommand> OnDayStart(DateTime d)
        {
            var ret = new List<ISumActionCommand>();

            if (CurrentState == null)
            {
                if (InitState != null && InitState.Dat != d) throw new Exception("InitState.Dat != d");

                CurrentState = InitState.DeepClone() ?? new HalvaCardState();
                CurrentState.Dat = d ; //TODO limits reinit
            }
            else
            {
                if (CurrentState.Dat.AddDays(1) != d) throw new Exception("CurrentState.Dat.AddDays(1) != d");
                CurrentState.Dat = d;
            }

            if (d.Day == LimitsPeriodStartDayOfMonth)
            {
                CurrentState.LimitGetCash_Ost = LimitGetCash;
                CurrentState.LimitSendOtherBankCard_Ost = LimitSendOtherBankCard;
            }
            CurrentState.LimitDaySendOtherBankCard_Ost = LimitDaySendOtherBankCard;
            CurrentState.LimitDaySendFastByPhoneNumber_Ost = LimitDaySendFastByPhoneNumber;

            if (d!= InitState?.Dat)
            {
                var procents = CurrentState.SobstvAmount * ProcentOnOst.GetValue(d) / 100 / (DateTime.IsLeapYear(d.Year) ? 366 : 365);
                CurrentState.Procents += procents;

                if (d.Day == PeriodStartDayOfMonth)
                {
                    if (CurrentState.Procents > 0)
                    {
                        ret.Add(new PutSumCommand(d.AddHours(15), Math.Round(CurrentState.Procents, 2), this.Name,CommandTicker.HalvaPayProcents));
                    }
                }
            }

            return ret;
        }

        public List<Error> OnDayEnd(DateTime d)
        {
            return new List<Error>();
        }

        public void OnPrihod(RashodRequest request)
        {
            CurrentState.Amount += request.sum;
            CurrentState.SobstvAmount += request.sum;

            Transactions.trans.Add(new Tran(request));
        }

        public CanRashodResponse CanRashod(RashodRequest request)
        {
            bool result = true;
            decimal max = decimal.MaxValue, min = 0.01m,opMax=decimal.MaxValue;
            if (request.OpType == OperationType.GetCash)
            {
                min = 100;

                    max = ((int)CurrentState.LimitGetCash_Ost / 100) * 100;              
            }
            else if (request.OpType == OperationType.SendCashless)
            {
                if (!request.SameBank && request.Place == ATMPlace.Own && !request.SendedFastByPhoneNumber)
                {
                    max =Math.Min(CurrentState.LimitSendOtherBankCard_Ost ,CurrentState.LimitDaySendOtherBankCard_Ost);
                    opMax = LimitOpSendOtherBankCard;
                }
            }
            else if (request.OpType == OperationType.PullCashless)
            {
                if (request.Place == ATMPlace.Own)
                {
                    //ok
                }
                else
                if (request.Place == ATMPlace.Other)
                {
                    max = Math.Min(CurrentState.LimitSendOtherBankCard_Ost,CurrentState.LimitDaySendOtherBankCard_Ost);

                    opMax = LimitOpPull;
                }
                else throw new NotImplementedException();
            }

            if (max > CurrentState.Amount)
                max = CurrentState.Amount;

            if (min > CurrentState.Amount)
                result = false;

            if (request.sum < min || request.sum > max || min > max)
                result = false;

            return new CanRashodResponse { Success = result, MinSum = min, MaxSum = max, OpMaxSum=opMax };
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
                else result = false;
            }
            else if (request.OpType == OperationType.AddCashless)
            {

            }

            if (request.sum < min || request.sum > max)
                result = false;

            return new CanRashodResponse { Success = result, MinSum = min, MaxSum = max };
        }

        public List<Error> OnRashod(RashodRequest request)
        {
            var errors = new List<Error>();

            if (request.OpType == OperationType.GetCash)
            {
                    if (CurrentState.LimitGetCash_Ost < request.sum)
                        errors.Add(new Error($"LimitGetCash {CurrentState.LimitGetCash_Ost} is less than {request.sum}", ErrorType.Warning));

                    CurrentState.LimitGetCash_Ost -= request.sum;
            }
            else if (request.OpType == OperationType.SendCashless)
            {
                if (!request.SameBank && request.Place == ATMPlace.Own && !request.SendedFastByPhoneNumber)
                {
                    if (CurrentState.LimitDaySendOtherBankCard_Ost < request.sum)
                        errors.Add(new Error($"LimitDaySendOtherBankCard_Ost {CurrentState.LimitDaySendOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));
                    if (CurrentState.LimitSendOtherBankCard_Ost < request.sum)
                        errors.Add(new Error($"LimitSendOtherBankCard_Ost {CurrentState.LimitSendOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));
                    if (LimitOpSendOtherBankCard < request.sum)
                        errors.Add(new Error($"LimitOpSendOtherBankCard {LimitOpSendOtherBankCard} is less than {request.sum}", ErrorType.Warning));

                    CurrentState.LimitDaySendOtherBankCard_Ost -= request.sum;
                    CurrentState.LimitSendOtherBankCard_Ost -= request.sum;
                }
                //else throw new NotImplementedException();
            }
            else if (request.OpType == OperationType.PullCashless)
            {
                if (request.Place == ATMPlace.Own)
                {
                    //ok
                }
                else
                if (request.Place == ATMPlace.Other)
                {
                    if (CurrentState.LimitDaySendOtherBankCard_Ost < request.sum)
                        errors.Add(new Error($"LimitDaySendOtherBankCard_Ost {CurrentState.LimitDaySendOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));
                    if (CurrentState.LimitSendOtherBankCard_Ost < request.sum)
                        errors.Add(new Error($"LimitSendOtherBankCard_Ost {CurrentState.LimitSendOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));
                    if (LimitOpPull < request.sum)
                        errors.Add(new Error($"LimitOpPull {LimitOpPull} is less than {request.sum}", ErrorType.Warning));

                    CurrentState.LimitDaySendOtherBankCard_Ost -= request.sum;
                    CurrentState.LimitSendOtherBankCard_Ost -= request.sum;
                }
                else throw new NotImplementedException();
            }
            
            CurrentState.Amount -= request.sum;

            //TODO own money
            if (request.IsSobstvAmount)
                CurrentState.SobstvAmount -= request.sum;

            if (CurrentState.SobstvAmount < 0)
                errors.Add(new Error($"SobstvAmount {CurrentState.SobstvAmount} is less 0"));

            Transactions.trans.Add(new Tran(request));

            return errors;
        }
    }

    public class HalvaCardState
    {
        [JsonIgnore]
        public DateTime Dat;

        public decimal Amount;

        public decimal SobstvAmount;

        public decimal LimitGetCash_Ost;

        public decimal LimitSendOtherBankCard_Ost;
        public decimal LimitDaySendOtherBankCard_Ost;
        //public decimal LimitOpSendOtherBankCard_Ost;

        //public decimal LimitOpPull_Ost;

        public decimal LimitDaySendFastByPhoneNumber_Ost;
        public decimal Procents;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}

        /*public class Halva : Dogovor
        {
            public Halva(DateTime dat, double sum = 0) : base(dat, sum) { }

            public double proc = 6;
            public override void AnywayInEndOfDay(DateTime dat)
            {
                var sum = sumOn(dat, false);
                if (sum > 0)
                {
                    var procents = sum * proc / 100 / (DateTime.IsLeapYear(dat.Year) ? 366 : 365);
                    addOp(dat, procents);
                    sum += procents;
                }
            }

        }*/

