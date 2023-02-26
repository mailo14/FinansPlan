using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class CashVallet:IAccount, IDogovor
    {
        public Bank Bank => Bank.None;
        public CardTicker Ticker => CardTicker.CashVallet;

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

        public CashValletState InitState;

        public CashValletState CurrentState { get; set; }
        public string Name;

        public List<ISumActionCommand> OnDayStart(DateTime d)
        {
            if (CurrentState == null)
            {
                if (InitState != null && InitState.Dat != d) throw new Exception("InitState.Dat != d");

                CurrentState = InitState.DeepClone() ?? new CashValletState { Dat = d };
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
            var min = 0.01m;
            return new CanRashodResponse() { Success = CurrentState.Amount > min, MinSum = min, MaxSum = CurrentState.Amount };
        }

        public CanRashodResponse CanPrihod(RashodRequest request)
        {
            return new CanRashodResponse() { Success = true, MinSum = 0.01m, MaxSum = decimal.MaxValue };
        }
    }

    public class CashValletState
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
