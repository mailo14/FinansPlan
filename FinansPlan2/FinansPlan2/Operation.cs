using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2
{
    public class Operation:IActionCommand //перевод
    {
        public DateTime D { get ; set ; }
        OperationRequest Request;

        public Operation(OperationRequest request)
        {
            Request = request;
            D = request.Dat;
        }

        private static (bool Success, RashodRequest rashodRequest, RashodRequest prihodRequest) BuildRashodPrihodRequests(OperationRequest request)
        {
            var source = App.Dogovors[request.SourceDogovorId] as IAccount;
            var target = App.Dogovors[request.TargetDogovorId] as IAccount;

            var ticker1 = source.Ticker;
            var ticker2 = target.Ticker;

            var configs = new[] {
                //new {sourceTickers = GetOnly(CardTicker.HalvaCard),opType = OperationType.Pay, targetTickers = GetAllExcept(CardTicker.CashVallet) },
                new {sourceTickers = GetOnly(CardTicker.HalvaCard),opType = OperationType.SendCashless, targetTickers = GetAllExcept(CardTicker.CashVallet) },
                new {sourceTickers = GetAllExcept(CardTicker.CashVallet),opType = OperationType.PullCashless,targetTickers = GetAllExcept(CardTicker.CashVallet) },
                new {sourceTickers = GetOnly(CardTicker.HalvaCard),opType = OperationType.SendFastByPhoneNumber, targetTickers = GetAllExcept(CardTicker.AlfaCreditCard, CardTicker.CashVallet) },
                new {sourceTickers= GetAllExcept(CardTicker.CashVallet), opType = OperationType.GetCash, targetTickers = GetOnly(CardTicker.CashVallet) },
                new {sourceTickers= GetOnly(CardTicker.CashVallet), opType = OperationType.PutCash, targetTickers = GetAllExcept(CardTicker.CashVallet) }
            };//throw if more than 2 matches

            if (!configs.Any(x => x.opType == request.Type && x.sourceTickers.Contains(ticker1) && x.targetTickers.Contains(ticker2)))
                return (false, null, null);

            bool sameBank = source.Bank != Bank.None && source.Bank == target.Bank;
            if (request.Type != OperationType.GetCash && request.Type != OperationType.PutCash)
            {
                if (sameBank) request.atmPlace = ATMPlace.Own;
                else
                    //TODO if partners - ATMPlace.Partner
                    request.atmPlace = ATMPlace.Other;

            }

            var moneyType = //new[] { OperationType.MoveCashless, OperationType.PullCashless, OperationType.SendFastByPhoneNumber }.Contains(request.Type)
                new[] { OperationType.GetCash, OperationType.PutCash }.Contains(request.Type)
                ? MoneyType.Cash : MoneyType.Сashless;

            var rashodRequest = new RashodRequest
            {
                Dat = request.Dat,
                OpType = request.Type,
                //TranCat = TranCat2.get,
                MoneyType = moneyType,
                Place = request.atmPlace,
                SameBank = sameBank,
                sum = request.sum,
                SendedFastByPhoneNumber = request.Type == OperationType.SendFastByPhoneNumber
            };

            var prihodOpType = moneyType == MoneyType.Cash ? OperationType.PutCash : OperationType.AddCashless;

            var prihodRequest = new RashodRequest
            {
                Dat = request.Dat,
                OpType = prihodOpType,
                //TranCat = TranCat2.add,
                MoneyType = moneyType,
                Place = request.atmPlace,
                SameBank = sameBank,
                sum = request.sum
            };

            return (true, rashodRequest, prihodRequest);
        }

        public static CanRashodResponse CanExecute(OperationRequest request)
        {
            var source = App.Dogovors[request.SourceDogovorId] as IAccount;
            var target = App.Dogovors[request.TargetDogovorId] as IAccount;

            if (source != target)
            {
                var (Success, rashodRequest, prihodRequest) = BuildRashodPrihodRequests(request);
                if (Success)
                {
                    var sourceCan = source.CanRashod(rashodRequest);
                    var targetCan = target.CanRashod(prihodRequest);

                    if (sourceCan.Success && targetCan.Success)
                    {
                        var diaps = new List<CanRashodResponse> { sourceCan, targetCan }.OrderBy(x => x.MinSum).ThenBy(x => x.MaxSum).ToArray();
                        if (diaps[1].MinSum <= diaps[0].MaxSum)
                        {
                            var f = Math.Max(diaps[0].MinSum, diaps[1].MinSum);
                            var t = Math.Min(diaps[0].MaxSum, diaps[1].MaxSum);
                            return new CanRashodResponse() { Success = true, MinSum = f, MaxSum = t };
                        }
                    }
                }
            }

            return new CanRashodResponse() { Success = false };
        }


        public ActionResult Execute()
        {
            var errors = new List<Error>();


            //validate SourceDogovorId != TargetDogovorId
            var source = App.Dogovors[Request.SourceDogovorId] as IAccount;
            var target = App.Dogovors[Request.TargetDogovorId] as IAccount;

            var requests = BuildRashodPrihodRequests(Request);

            if (!requests.Success)
                throw new Exception("no op config");

            var resp = source.OnRashod(requests.rashodRequest);
            if (resp.Any()) errors.AddRange(resp);

            target.OnPrihod(requests.prihodRequest);


            return new ActionResult(errors);
            /*
                        d1.Ticker==CardTicker.TinkoffBlackCard;
                        d2.Ticker == CardTicker.HalvaCard;
                        OperationType.Move;

                        //List<ATMPlace> atms = null;//all new ATMPlace.Other

                        var resp = dd.OnRashod(D, new Tran(D, Sum, 0, TranCat.getCash) { atmPlace = ATMPlace.Other });
                        if (resp.Any()) errors.AddRange(resp);

                        var vallet = App.Dogovors.Single(x => x.Value is CashVallet).Value as CashVallet;
                        vallet.OnPrihod(D, new Tran(D, Sum, 0, TranCat.addCash));

                        return new ActionResult(errors);*/
        }

        public static List<T> GetAll<T>()
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");

            var items = Enum.GetValues(typeof(T)).OfType<T>()
                .Where(x => x.ToString() != "None");

            return items.ToList();
        }

        public static List<T> GetOnly<T>(params T[] items)
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");

            return items.ToList();
        }

        public static List<T> GetAllExcept<T>(params T[] exceptItems)
        {

            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");

            //Enum.GetValues(typeof(T))
            //if (item.ToString().ToLower().Equals(value.Trim().ToLower())) return item;
            var items = Enum.GetValues(typeof(T)).OfType<T>()
                .Where(x => x.ToString() != "None");

            if (exceptItems != null)
                return items.Except(exceptItems).ToList();
            else
                return items.ToList();
        }


        public List<(T1, T2)> Gen<T1, T2>(List<T1> list1, List<T2> list2)
        {
            var res = new List<(T1, T2)>();
            foreach (var i1 in list1)
                foreach (var i2 in list2)
                    res.Add((i1, i2));

            return res;
        }

        public static List<AvailableOperation> GetAvailableOps(GetAvailableOpsRequest request)
        {
            if (request.sources == null) request.sources = App.Dogovors.Keys.ToList();
            if (request.opTypes==null) request.opTypes = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();
            if (request.targets == null) request.targets = App.Dogovors.Keys.ToList();
            if (request.places == null) request.places = new List<ATMPlace>() { ATMPlace.Own };

            //var res = new List<(CardTicker, OperationType, CardTicker, ATMPlace)>();
            var res = new List<AvailableOperation>();

            foreach (var s in request.sources)
                foreach (var op in request.opTypes)
                    foreach (var t in request.targets)
                        foreach (var p in request.places)
                        {
                            var opRequest = new OperationRequest { Dat = request.dat, SourceDogovorId = s, TargetDogovorId = t, Type = op, atmPlace = p };
                            var response = Operation.CanExecute(opRequest);
                            if (response.Success)
                            {
                                res.Add(new AvailableOperation { OpRequest = opRequest, MinSum = response.MinSum, MaxSum = response.MaxSum });
                            }
                        }

            return res;
        }

    }
    public class AvailableOperation
    {
        public OperationRequest OpRequest { get; set; }
        public decimal MinSum { get; set; }
        public decimal MaxSum { get; set; }

        public override string ToString()
        {
            return $"{OpRequest} --- {MinSum} .. {MaxSum}";
        }
    }
    public class GetAvailableOpsRequest
    {
        public DateTime dat;
        public List<string> sources;
        public List<OperationType> opTypes;
        public List<string> targets;
        public List<ATMPlace> places;
    }

    public class OperationRequest
    {
        public string SourceDogovorId;
        public string TargetDogovorId;

        public DateTime Dat { get; set; }
       public decimal sum;

        public OperationType Type;
        //public TranCat Type;
        //public MoneyType MoneyType;
        public ATMPlace? atmPlace;

        public override string ToString()
        {
            return $"{SourceDogovorId} -> '{Enum.GetName(typeof(OperationRequest), Type)}' {(atmPlace.HasValue? Enum.GetName(typeof(ATMPlace), atmPlace.Value) +" ":"")}-> {TargetDogovorId}";
        }
    }

    public enum OperationType
    {
        GetCash,
        PutCash,
        SendCashless,
        PullCashless,
        SendFastByPhoneNumber,
        Pay,
        AddCashless
        //Rashod
    }

    public enum MoneyType
    {
        Cash, Сashless
    }

    public enum Bank
    {None,
        VTB, Tinkoff, Alfa, Sovkom
    }
    public enum CardTicker
    {None,
        CashVallet,
        TinkoffBlackCard,
        AlfaCreditCard,
        VTBDebitCard,
        HalvaCard,
    }
}
