using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.New
{
    public class Operation
    {
        public string Name { get; set; }
        public OpType Typ { get; set; }
        public List<DogovorType> AvailableD1Types;
        public List<DogovorType> AvailableD2Types;
        public ActionnType? ActionForD1;
        public ActionnType? ActionForD2;
        //public Func<OperationCanExecuteRequest, OperationCanExecuteResponse> CanExecute;
        public virtual OperationCanExecuteResponse CanExecute(OperationCanExecuteRequest req)
        {
            var line = req.strategyBranch.DogovorLines[req.DogLine1Id];
            var line1Action = line.Dogovorr.AvailableActions.SingleOrDefault(d => d.Type == ActionForD1);
            if (line1Action != null)
            {
                //  line1Action.CanExecute(new ExecuteRequest { })
                //  var state1 = req.DogovorLinesStates[req.DogLine1Id];
                //    if (state1.)
                if (ActionForD2.HasValue)
                {
                    var dogLines2 = req.strategyBranch.DogovorLines.Values.Where(l => l != line && l.Dogovorr.AvailableActions.Any(d => d.Type == ActionForD2)).ToList();
                    if (dogLines2.Any())
                    {

                        foreach (var line2 in dogLines2)
                        {

                        }

                        return new OperationCanExecuteResponse { Success = true, DogLines2 = dogLines2.Select(x => x.LineName).ToList() };
                    }
                }
                else return new OperationCanExecuteResponse { Success = true };
            }
            return new OperationCanExecuteResponse { Success = false };
        }
        public static Eventt BuidEvent(BuidEventFromOpRequest request)
        {
            var op = Operations.Single(x => x.Typ == request.OpTyp);
            var event1 = new Eventt { Dat = request.Dat, Name = op.Name };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorLineName = request.DogLine1Id,
                ItemAction = op.ActionForD1.Value,
                Sum = request.Summ,
                SumBeforeCorrection = request.SumBeforeCorrection
                //Params = new OpenDogovorParams { LineName = StandardDogLineName.ZpVtbKarta }
            });

            if (request.OpTyp == OpType.SnyatCash || request.OpTyp == OpType.PopolnitFromCash)
                request.DogLine2Id = StandardDogLineName.CashWallet;

            if (op.ActionForD2.HasValue)
                event1.ActionItems.Add(new ActionnItem
                {
                    Eventtt = event1,
                    DogovorLineName = request.DogLine2Id,
                    ItemAction = op.ActionForD2.Value,
                    Sum = request.Summ
                    //Params = new OpenDogovorParams { LineName = StandardDogLineName.ZpVtbKarta }
                });

            return event1;
        }

        public static List<Operation> Operations = new List<Operation>
        { new PerevodOperation(),
            new SnyatCashOperation(),
            new PopolnitFromCashOperation(),
            new PayZpOperation(),
            new PayProcentsOperation(),
            /*new  Operation{Name="Перевод", 
                AvailableD1Types=new List<DogovorType>{DogovorType.Karta },AvailableD2Types=new List<DogovorType>{DogovorType.Karta }            ,
                ActionForD1=ActionnType.GetBeznal,ActionForD2=ActionnType.PutBeznal},
            new  Operation{Name="Пополнить вклад с карты", AvailableD1Types=new List<DogovorType>{DogovorType.Karta },AvailableD2Types=new List<DogovorType>{DogovorType.Vklad }            ,
                ActionForD1=ActionnType.GetBeznal,ActionForD2=ActionnType.Popolnit},
            new  Operation{Name="Покупка картой", AvailableD1Types=new List<DogovorType>{DogovorType.Karta },AvailableD2Types=new List<DogovorType>{}            ,
                ActionForD1=ActionnType.Buy,ActionForD2=null,CanRunForAutoGen=false},
            new  Operation{Name="Открыть", AvailableD1Types=new List<DogovorType>{DogovorType.Karta,DogovorType.Vklad,DogovorType.Zp,DogovorType.CashWallet },AvailableD2Types=new List<DogovorType>{}            ,
                ActionForD1=ActionnType.Open,ActionForD2=null},
            new  Operation{Name="Снять нал", AvailableD1Types=new List<DogovorType>{DogovorType.Karta },AvailableD2Types=new List<DogovorType>{ DogovorType.CashWallet}            ,
                ActionForD1=ActionnType.GetCash,ActionForD2=ActionnType.PutCash},
            new  Operation{Name="Пополнить налом", AvailableD1Types=new List<DogovorType>{DogovorType.CashWallet },AvailableD2Types=new List<DogovorType>{ DogovorType.Karta }            ,
                ActionForD1=ActionnType.PutCash,ActionForD2=ActionnType.GetCash},*/
        };

        public bool CanAddForManual { get; set; } = true;//отображать в возможных действиях на юи
        public bool CanAddForAutoGen { get; set; } = true;//разрешить оперировать действием при автогенерации вариантов

        /*public CanResponse CanExecute(Dogovor d1, Dogovor d2, decimal sum)
        {
            throw new NotImplementedException();
        }*/
    }

    public class BuidEventFromOpRequest
    {
        public OpType OpTyp { get; set; }
        public DateTime Dat { get; set; }
        public string DogLine1Id { get; set; }
        public string DogLine2Id { get; set; }
        public decimal Summ { get; set; }
        public decimal? SumBeforeCorrection { get; internal set; }
    }

    public class PerevodOperation : Operation
    {
        public PerevodOperation()
        {
            Name = "Перевод";
            Typ = OpType.Perevod;
            ActionForD1 = ActionnType.GetBeznal;
            ActionForD2 = ActionnType.PutBeznal;
        }
    }
    public class SnyatCashOperation : Operation
    {
        public SnyatCashOperation()
        {
            Name = "Снять нал";
            Typ = OpType.SnyatCash;
            ActionForD1 = ActionnType.GetCash;
            ActionForD2 = ActionnType.PutCash;
        }
        public override OperationCanExecuteResponse CanExecute(OperationCanExecuteRequest req)
        {
            if (req.DogLine1Id != StandardDogLineName.CashWallet
            && req.strategyBranch.DogovorLines.ContainsKey(StandardDogLineName.CashWallet))
            {
                return new OperationCanExecuteResponse { Success = true, DogLines2 = new List<string> { StandardDogLineName.CashWallet } };
            }

            return new OperationCanExecuteResponse { Success = false };
        }
    }
    public class PopolnitFromCashOperation : SnyatCashOperation
    {
        public PopolnitFromCashOperation()
        {
            Name = "Пополнить налом";
            Typ = OpType.PopolnitFromCash;
            ActionForD1 = ActionnType.PutCash;
            ActionForD2 = ActionnType.GetCash;
        }
    }
    public class PayZpOperation : Operation
    {
        public PayZpOperation()
        {
            Name = "Выплата зарплаты";
            Typ = OpType.PayZp;
            ActionForD1 = ActionnType.PutBeznal;

            CanAddForAutoGen = false;
            CanAddForManual = false;
        }
        public override OperationCanExecuteResponse CanExecute(OperationCanExecuteRequest req)
        {
            var zpAccLines = req.strategyBranch.DogovorLines.Values.Where(l => l.Dogovorr.Typee == DogovorType.Zp)
                .Select(x => (x.Dogovorr as ZpDogovor).ZpAccountDogovorLineName).ToList();

            return new OperationCanExecuteResponse { Success = zpAccLines.Contains(req.DogLine1Id) };
        }
    }
    public class PayProcentsOperation : Operation
    {
        public PayProcentsOperation()
        {
            Name = "Выплата процентов";//а на остаток";
            Typ = OpType.PayProcents;
            ActionForD1 = ActionnType.PutBeznal;

            CanAddForAutoGen = false;
            CanAddForManual = false;
        }
        public override OperationCanExecuteResponse CanExecute(OperationCanExecuteRequest req)
        {
            var zpAccLines = req.strategyBranch.DogovorLines.Values.Where(l => l.LineName == StandardDogLineName.Halva || l.Dogovorr.Typee == DogovorType.Vklad)
                .Select(x => x.LineName).ToList();

            return new OperationCanExecuteResponse { Success = zpAccLines.Contains(req.DogLine1Id) };
        }
    }

    public class OperationCanExecuteResponse
    {
        public bool Success { get; set; }
        public List<string> DogLines2 { get; set; }
    }

    public class OperationCanExecuteRequest
    {
        public Contextt Context { get; set; }
        public DateTime Dat { get; set; }
        public string DogLine1Id { get; set; }
        public StrategyBranch strategyBranch { get; set; }
    }


}
