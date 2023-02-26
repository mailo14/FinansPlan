using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.Products.TinkoffBlack
{
     public class TinkoffBlack : IAccount, IDogovor
    {
        public Bank Bank => Bank.Tinkoff;
        public CardTicker Ticker => CardTicker.TinkoffBlackCard;

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

        public TinkoffBlackState InitState;

        public TinkoffBlackState CurrentState { get; set; }
        public string Name;

        public int PeriodStartDayOfMonth=20;

        public decimal LimitGetCash = 500000;
        public decimal LimitGetCashOtherATM = 100000;
        public decimal LimitSendOtherBankCard = 20000;

        public List<ISumActionCommand> OnDayStart(DateTime d)
        {
            if (CurrentState == null)
            {
                if (InitState != null && InitState.Dat != d) throw new Exception("InitState.Dat != d");

                CurrentState = InitState.DeepClone() ?? new TinkoffBlackState { Dat = d }; //TODO limits reinit
                CurrentState.Dat = d;
            }
            else
            {
                if (CurrentState.Dat.AddDays(1) != d) throw new Exception("CurrentState.Dat.AddDays(1) != d");
                CurrentState.Dat = d;
            }

            if (d.Day == PeriodStartDayOfMonth)
            {
                CurrentState.LimitGetCash_Ost = LimitGetCash;
                CurrentState.LimitGetCashOtherATM_Ost = LimitGetCashOtherATM;
                CurrentState.LimitSendOtherBankCard_Ost = LimitSendOtherBankCard;
            }

            return new List<ISumActionCommand>();
        }

        public List<Error> OnDayEnd(DateTime d)
        {
            return new List<Error>();
        }

        public void OnPrihod(RashodRequest request)
        {//TODO налом у партнеров Тинькофф до 150 000 руб. за расчетный период
            CurrentState.Amount += request.sum;
            Transactions.trans.Add(new Tran(request));
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
                    max = ((int)CurrentState.LimitGetCash_Ost/100)*100;
                }
                else if (request.Place == ATMPlace.Other)
                {
                    min = 3000;
                    max = ((int)CurrentState.LimitGetCashOtherATM_Ost/100)*100;
                }
                else throw new NotImplementedException();
            }
            else if (request.OpType == OperationType.SendCashless)
            {
                if (!request.SameBank && request.Place == ATMPlace.Own && !request.SendedFastByPhoneNumber)
                {
                    max = CurrentState.LimitSendOtherBankCard_Ost;
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
                    max = CurrentState.LimitGetCash_Ost;
                }
                else throw new NotImplementedException();
            }

            if (max > CurrentState.Amount)
                max = CurrentState.Amount;

            if (min > CurrentState.Amount)
                result = false;

            if (request.sum < min || request.sum > max || min>max)
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
                    //TODO налом у партнеров Тинькофф до 150 000 руб. за расчетный период
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
                if (request.Place == ATMPlace.Own)
                {
                    if (CurrentState.LimitGetCash_Ost < request.sum)
                        errors.Add(new Error($"LimitGetCash {CurrentState.LimitGetCash_Ost} is less than {request.sum}", ErrorType.Warning));

                    CurrentState.LimitGetCash_Ost -= request.sum;
                }
                else if (request.Place == ATMPlace.Other)
                {
                    if (CurrentState.LimitGetCashOtherATM_Ost < request.sum)
                        errors.Add(new Error($"LimitGetCashOtherATM {CurrentState.LimitGetCashOtherATM_Ost} is less than {request.sum}", ErrorType.Warning));

                    CurrentState.LimitGetCashOtherATM_Ost -= request.sum;
                }
                else throw new NotImplementedException();
            }
            else if (request.OpType == OperationType.SendCashless)
            {
                if (!request.SameBank && request.Place == ATMPlace.Own && !request.SendedFastByPhoneNumber)
                {
                    if (CurrentState.LimitSendOtherBankCard_Ost< request.sum)
                        errors.Add(new Error($"LimitSendOtherBankCard_Ost {CurrentState.LimitSendOtherBankCard_Ost} is less than {request.sum}", ErrorType.Warning));

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
                    if (CurrentState.LimitGetCash_Ost < request.sum)
                        errors.Add(new Error($"LimitGetCash {CurrentState.LimitGetCash_Ost} is less than {request.sum}", ErrorType.Warning));

                    CurrentState.LimitGetCash_Ost -= request.sum;
                }
                else throw new NotImplementedException();
            }

            CurrentState.Amount -= request.sum;

            if (CurrentState.Amount < 0)
                errors.Add(new Error($"Cash {CurrentState.Amount} is less 0"));

            Transactions.trans.Add(new Tran(request));

            return errors;
        }
    }

    public class TinkoffBlackState
    {
        [JsonIgnore]
        public DateTime Dat;

        public decimal Amount;

        public decimal LimitGetCash_Ost ;
        public decimal LimitGetCashOtherATM_Ost;
        public decimal LimitSendOtherBankCard_Ost;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
