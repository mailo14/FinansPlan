using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class ZpVSK : IDogovor//:IAccount
    {
        public CardTicker Ticker => CardTicker.None;
        public Bank Bank => Bank.None;

        private readonly IWorkDayService _workDayService;

        public ZpVSK(IWorkDayService workDayService)
        {
            _workDayService = workDayService;
        }

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

        public ZpVSKState InitState;

        public ZpVSKState CurrentState { get; set; }
        public string Name;

        public string ZpAccId;

        public DatedValueCollection<int> AvansGainDayOfMonths = new DatedValueCollection<int>(new List<DatedValue<int>> { new DatedValue<int>("1.01.2000", 20) });
        public DatedValueCollection<int> ZpOstGainDayOfMonths = new DatedValueCollection<int>(new List<DatedValue<int>> { new DatedValue<int>("1.01.2000", 5) });

        public DatedDecimalCollection AvansSum = new DatedDecimalCollection(new List<DatedValue<decimal>> { new DatedValue<decimal>("1.01.2000", 48000), new DatedValue<decimal>("20.11.2020", 60000) });
        public DatedDecimalCollection ZpOstSum = new DatedDecimalCollection(new List<DatedValue<decimal>> { new DatedValue<decimal>("1.01.2000", 82500), new DatedValue<decimal>("20.11.2020", 70500) });

        public List<ISumActionCommand> OnDayStart(DateTime d)
        {
            /*if (CurrentState == null)
            {
                if (InitState != null && InitState.Dat != d) throw new Exception("InitState.Dat != d");

                CurrentState = InitState ?? new ZpVSKState { Dat = d };
            }
            else
            {
                if (CurrentState.Dat.AddDays(1) != d) throw new Exception("CurrentState.Dat.AddDays(1) != d");
                CurrentState.Dat = d;
            }*/

            var ret = new List<ISumActionCommand>();
            if (d == CalcAvansDate(d))
            {
                ret.Add(new PutSumCommand(d.AddHours(15), AvansSum.GetValue(d), ZpAccId,CommandTicker.PayZpVsk));
            }
            else if (d == CalcZpDate(d))
            {
                ret.Add(new PutSumCommand(d.AddHours(15), ZpOstSum.GetValue(d), ZpAccId, CommandTicker.PayZpVsk));
            }

            foreach (var fix in CommandHotFixes.Where(x => x.Dat == d))
            {
                var existCommand = ret.FirstOrDefault(x => x.Ticker == fix.CommandTicker);
                if (existCommand != null)
                {
                    if (fix.NewSum == 0)
                        ret.Remove(existCommand);
                    else
                        existCommand.Sum = fix.NewSum;
                }
            }

            return ret;
        }

        public List<ISumActionCommand> OuterOnDayStart(DateTime d) //TODO to base class
        {
            var ret=OnDayStart(d); //TODO inner? abstract

            foreach (var fix in CommandHotFixes.Where(x => x.Dat == d))
                ;//...

            return ret;
        }

        public List<CommandHotFix> CommandHotFixes = new List<CommandHotFix>();



        public DateTime CalcAvansDate(DateTime d)
        {
            DateTime ret;
            ret = _workDayService.GetWorkDayOrBefore(d.SetDay(AvansGainDayOfMonths.GetValue(d)));
            if (ret >= d) return ret;
            else return CalcAvansDate(d.AddMonths(1).SetDay(1));
        }

        public DateTime CalcZpDate(DateTime d)
        {
            DateTime ret;
            if (d.Month == 12 && d > _workDayService.GetWorkDayOrBefore(d.SetDay(ZpOstGainDayOfMonths.GetValue(d))))
            {
                ret = _workDayService.GetWorkDayOrBefore(d.SetDay(29));
            }
            else if (d.Month == 1) ret = CalcZpDate(d.AddMonths(1).SetDay(1));
            else ret = _workDayService.GetWorkDayOrBefore(d.SetDay(ZpOstGainDayOfMonths.GetValue(d)));

            if (ret >= d) return ret;
            else return CalcZpDate(d.AddMonths(1).SetDay(1));
        }

        public DateTime SetDateDay(DateTime d, int day) => new DateTime(d.Year, d.Month, day);

        public List<Error> OnDayEnd(DateTime d)
        {
            return new List<Error>();
        }

        /*public void OnPrihod(DateTime d, Tran tran)
        {
            CurrentState.Amount += tran.sum;
            Transactions.trans.Add(tran);
        }

        public void OnRashod(DateTime d, Tran tran)
        {
            CurrentState.Amount -= tran.sum;

            if (CurrentState.Amount < 0)
                throw new Exception($"Cash {CurrentState.Amount} is less 0");

            Transactions.trans.Add(tran);
        }*/
    }
    public static class DateExtension
    {
        public static DateTime SetDay(this DateTime d, int day)
        {
            return new DateTime(d.Year, d.Month, day);
        }
        /*public static DateTime WorkDayOrBefore(this DateTime d)
        {            
            return d; //TODO
        }*/
    }

    public class ZpVSKState
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
