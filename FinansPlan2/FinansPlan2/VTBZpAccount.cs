using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class VTBZpAccount:  IAccount,IDogovor
    {
        public Bank Bank => Bank.VTB;
        public CardTicker Ticker => CardTicker.VTBDebitCard;

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

        public VTBZpAccountState InitState;

        public VTBZpAccountState CurrentState { get; set; }
        public string Name;

        public List<ISumActionCommand> OnDayStart(DateTime d)
        {
            if (CurrentState == null)
            {
                if (InitState != null && InitState.Dat != d) throw new Exception("InitState.Dat != d");

                CurrentState = InitState.DeepClone() ?? new VTBZpAccountState { Dat = d }; 
                CurrentState.Dat = d;
            }
            else
            {
                if (CurrentState.Dat.AddDays(1) != d) throw new Exception("CurrentState.Dat.AddDays(1) != d");
                CurrentState.Dat = d;
            }

            return new List<ISumActionCommand>();
        }

        public List<Error> OnDayEnd(DateTime d)
        {
            return new List<Error>();
        }

        public void OnPrihod(RashodRequest request)
        {
            CurrentState.Amount += request.sum;
            Transactions.trans.Add(new Tran(request));
        }

        public List<Error> OnRashod(RashodRequest request)
        {
            var errors = new List<Error>();

            CurrentState.Amount -= request.sum;

            if (CurrentState.Amount < 0)
                errors.Add(new Error($"Cash {CurrentState.Amount} is less 0"));

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
                    //TODO max = ((int)CurrentState.LimitGetCash_Ost / 100) * 100;
                }
                else throw new NotImplementedException();
            }
            else if (request.OpType == OperationType.SendCashless)
            {
                if (!request.SameBank && request.Place == ATMPlace.Own && !request.SendedFastByPhoneNumber)
                {
                    //TODO max = CurrentState.LimitSendOtherBankCard_Ost;
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
                    //TODO max = CurrentState.LimitGetCash_Ost;
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

    public class VTBZpAccountState
    {
        [JsonIgnore]
        public DateTime Dat;

        public decimal Amount;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    }
