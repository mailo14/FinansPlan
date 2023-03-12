using AutoFixture;
using AutoFixture.AutoMoq;
using FinansPlan2.New;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinansPlan2.Tests
{
    [TestFixture()]
    public class ZpVTB_DebitTests
    {
        private IFixture Fixture { get; set; }
        public ZpVTB_DebitTests()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
        }

        private Contextt GetContext(string[] nonWorkingDays)
        {
            var workDayProviderMock = Fixture.Freeze<Mock<IWorkDayProvider>>();
            workDayProviderMock.Setup(s => s.IsWorkDay(It.IsAny<DateTime>()))
                .Returns<DateTime>(
                x => nonWorkingDays != null
                ? !nonWorkingDays.Contains(x.ToShortDateString())
                : true);

            Fixture.Inject<IWorkDayService>(new WorkDayService(workDayProviderMock.Object));


            var context = new Contextt();                        
            context.StrategyBranches.Add(new StrategyBranch());

            var zp = Fixture.Freeze<ZpDogovor>(c => c.OmitAutoProperties());
            zp.ZpAccountDogovorLineName = StandardDogLineName.ZpVtbKarta;
            context.Dogovors.Add(zp.Name, zp);

            var vtbDebit = new VTB_DebitDogovor();
            context.Dogovors.Add(vtbDebit.Name, vtbDebit);

            var event1 = new Eventt { Dat = DateTime.Parse("1.09.2022"), Name = "Добавить договор зарплаты" };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = zp.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams { LineName = StandardDogLineName.ZpVtbKarta }
            });
            context.StrategyBranches.Single().Events.Add(event1);

            event1 = new Eventt { Dat = DateTime.Parse("1.09.2022"), Name = "Добавить карту ВТБ зарплатная" };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = vtbDebit.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams { LineName = StandardDogLineName.ZpVtbKarta }
            });
            context.StrategyBranches.Single().Events.Add(event1);

            context.PeriodStart = context.StrategyBranches.Single().Events.Min(x => x.Dat);
            context.PeriodEnd = context.StrategyBranches.Single().Events.Max(x => x.Dat).AddDays(5);

            return context;
        }

        [Test()]
        public void CalcZpSum()
        {
            var context = GetContext(null);

            Processor.ProcessPeriod(context);

            var actual = context.StrategyBranches.Single().LastEventState;

            Assert.AreEqual(2, context.StrategyBranches.Single().DogovorLines.Count);
            var state = actual.DogovorLineStates[StandardDogLineName.ZpVtbKarta] as VTB_DebitDogovorLineState; 
            Assert.AreEqual(70500, state.Sum);

            //var actual = context.InitDatedScope;
        }

        [Test()]
        [TestCase(0)]
        [TestCase(100000)]
        public void CalcZpSumWithZpCorrection(decimal corrSum)
        {
            var context = GetContext(null);
            context.StrategyBranches.Single().Corrections.Add(new Correction { Dat = DateTime.Parse("5.09.2022"), DogovorLineName = "Zp", OpType = OpType.PayZp, Sum = corrSum });

            Processor.ProcessPeriod(context);

            var actual = context.StrategyBranches.Single().LastEventState;

            Assert.AreEqual(2, context.StrategyBranches.Single().DogovorLines.Count);
            var state = actual.DogovorLineStates[StandardDogLineName.ZpVtbKarta] as VTB_DebitDogovorLineState; 
            Assert.AreEqual(corrSum, state.Sum);
        }

        [Test()]
        [TestCase(0)]
        [TestCase(100000)]
        public void CalcZpAndAvansSumWithZpCorrection(decimal corrSum)
        {
            var context = GetContext(null);
            context.StrategyBranches.Single().Corrections.Add(new Correction { Dat = DateTime.Parse("5.09.2022"), DogovorLineName = "Zp", OpType = OpType.PayZp, Sum = corrSum });

            context.PeriodEnd = DateTime.Parse("30.09.2022");
            Processor.ProcessPeriod(context);

            var actual = context.StrategyBranches.Single().LastEventState;

            var avans = 60000;
            Assert.AreEqual(2, context.StrategyBranches.Single().DogovorLines.Count);
            var state = actual.DogovorLineStates[StandardDogLineName.ZpVtbKarta] as VTB_DebitDogovorLineState; 
            Assert.AreEqual(corrSum+ avans, state.Sum);
        }

        [Test()]
        public void PrintTmp()
        {
            var context = GetContext(null);
            context.StrategyBranches.Single().Corrections.Add(new Correction { Dat = DateTime.Parse("5.09.2022"), DogovorLineName = "Zp", OpType = OpType.PayZp, Sum = 100000 });

            context.PeriodEnd = DateTime.Parse("30.09.2022");
            Processor.ProcessPeriod(context);

            //var actual = context.StrategyBranches.Single().LastEventState;

            Processor.Print(context);
        }
        [Test()]
        public void Tmp()
        {
            Assert.AreEqual(0, AtmPlaceInfo.GetDist(Place.Dom, Place.Dom));
            Assert.AreEqual(14, AtmPlaceInfo.GetDist(Place.Dom, Place.TcAmsterdam));
            Assert.AreEqual(14, AtmPlaceInfo.GetDist(Place.TcAmsterdam, Place.Dom));
        }
        [Test()]
        public void Tmp3()
        {
            var rr=AtmPlaceInfo.GetPlacesToOperateCash(Banks.Vtb, true, 30000);
            var rr2=AtmPlaceInfo.GetPlacesToOperateCash(Banks.Vtb, false, 30000);
            var rr3=AtmPlaceInfo.GetPlacesToOperateCash(Banks.Sovcom, true, 30000);
            var rr4=AtmPlaceInfo.GetPlacesToOperateCash(Banks.Sovcom, false, 30000);
            var rr5=AtmPlaceInfo.GetPlacesToOperateCash(Banks.Alfa, true, 30000);
            var rr6=AtmPlaceInfo.GetPlacesToOperateCash(Banks.Alfa, false, 30000);
            
        }
        [Test()]
        public void Tmp35()
        {
            IDogovorLineState s1 = new HalvaDogovorLineState { LimitDaySendOtherBankCard_Ost = 300 };
            IDogovorLineState s2 = new HalvaDogovorLineState { LimitDaySendOtherBankCard_Ost = 500};
            
            Assert.IsTrue(!ObjectCloner.DeepEquals(s1,s2));

            IDogovorLineState s12 = s1.Clone();
            Assert.IsTrue(ObjectCloner.DeepEquals(s1, s12));

            (s12 as HalvaDogovorLineState).Sum += 10;
            Assert.IsTrue(!ObjectCloner.DeepEquals(s1, s12));

        }
        [Test()]
        public void PerevodTest()
        {            
            var context = new Contextt();
            context.StrategyBranches.Add(new StrategyBranch());

            var vtbDebit = new VTB_DebitDogovor();
            context.Dogovors.Add(vtbDebit.Name, vtbDebit);
            var raif = new RaifDogovor();
            context.Dogovors.Add(raif.Name, raif);

            var cashWallet = new CashWalletDogovor();
            context.Dogovors.Add(cashWallet.Name, cashWallet);

            var event1 = new Eventt { Dat = DateTime.Parse("1.09.2022"), Name = "Добавить карту ВТБ зарплатная" };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = vtbDebit.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams { LineName = StandardDogLineName.ZpVtbKarta }
            });
            context.StrategyBranches.Single().Events.Add(event1);

             event1 = new Eventt { Dat = DateTime.Parse("1.09.2022"), Name = "Добавить карту raif " };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = raif.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams { LineName = "raif" ,Limit=150000}
            });
            context.StrategyBranches.Single().Events.Add(event1);

            event1 = new Eventt { Dat = DateTime.Parse("1.09.2022"), Name = "Добавить кошелек" };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = cashWallet.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams {Sum=1000 }
            });
            context.StrategyBranches.Single().Events.Add(event1);

            var sum = 300;

            event1 = New.Operation.BuidEvent(new BuidEventFromOpRequest { OpTyp = OpType.Perevod, Dat = DateTime.Parse("1.09.2022"), 
                DogLine1Id ="raif",
                DogLine2Id=  StandardDogLineName.ZpVtbKarta,
                Summ = sum });            
                        
            context.StrategyBranches.Single().Events.Add(event1);

            context.PeriodStart = context.StrategyBranches.Single().Events.Min(x => x.Dat);
            context.PeriodEnd = context.StrategyBranches.Single().Events.Max(x => x.Dat);//.AddDays(1);

            Processor.ProcessPeriod(context);

            //context.InitialEventtState.DogovorLineStates
        }

        [Test()]
        public void SnyatCashPopolnitFromCashTest()
        {
            var context = new Contextt();
            context.StrategyBranches.Add(new StrategyBranch());

            var vtbDebit = new VTB_DebitDogovor();
            context.Dogovors.Add(vtbDebit.Name, vtbDebit);

            var cashWallet = new CashWalletDogovor();
            context.Dogovors.Add(cashWallet.Name, cashWallet);

            var event1 = new Eventt { Dat = DateTime.Parse("1.09.2022"), Name = "Добавить карту ВТБ зарплатная" };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = vtbDebit.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams { LineName = StandardDogLineName.ZpVtbKarta, Sum = 300 }
            });
            context.StrategyBranches.Single().Events.Add(event1);

            event1 = new Eventt { Dat = DateTime.Parse("1.09.2022"), Name = "Добавить кошелек" };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = cashWallet.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams { Sum = 100 }
            });
            context.StrategyBranches.Single().Events.Add(event1);


            event1 = New.Operation.BuidEvent(new BuidEventFromOpRequest
            {
                OpTyp = OpType.SnyatCash,
                Dat = DateTime.Parse("1.09.2022"),
                DogLine1Id = StandardDogLineName.ZpVtbKarta,
                Summ = 200
            });
            context.StrategyBranches.Single().Events.Add(event1);

            event1 = New.Operation.BuidEvent(new BuidEventFromOpRequest
            {
                OpTyp = OpType.PopolnitFromCash,
                Dat = DateTime.Parse("1.09.2022"),
                DogLine1Id = StandardDogLineName.ZpVtbKarta,
                Summ = 250
            });
            context.StrategyBranches.Single().Events.Add(event1);

            context.PeriodStart = context.StrategyBranches.Single().Events.Min(x => x.Dat);
            context.PeriodEnd = context.StrategyBranches.Single().Events.Max(x => x.Dat);//.AddDays(1);

            Processor.ProcessPeriod(context);

            //context.InitialEventtState.DogovorLineStates
        }

        [Test()]
        public void HalvaProcentTest()
        {
            var context = new Contextt();
            context.StrategyBranches.Add(new StrategyBranch());

            var halva = new HalvaDogovor();
            context.Dogovors.Add(halva.Name, halva);

            var cashWallet = new CashWalletDogovor();
            context.Dogovors.Add(cashWallet.Name, cashWallet);

            var event1 = new Eventt { Dat = DateTime.Parse("27.04.2020"), Name = "Добавить халву" };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = halva.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams{  Sum = 177710.92m,ProcentOnOst=6.5m}
            });
            context.StrategyBranches.Single().Events.Add(event1);

            event1 = new Eventt { Dat = DateTime.Parse("27.04.2020"), Name = "Добавить кошелек" };
            event1.ActionItems.Add(new ActionnItem
            {
                Eventtt = event1,
                DogovorName = cashWallet.Name,
                ItemAction = ActionnType.Open,
                Params = new OpenDogovorParams { Sum = 1000000 }
            });
            context.StrategyBranches.Single().Events.Add(event1);

            var arr = new[] {
                ("27.04.2020", -94682.89m),
                ("28.04.2020", -1845.23m),
                ("29.04.2020", 14869.58m),
                ("30.04.2020", 74900.89m),
                ("01.05.2020", 136819.63m),
                ("02.05.2020", -100m),
                ("03.05.2020", -1050m),
                ("04.05.2020", -976.72m),
                ("11.05.2020", -549m),
                ("12.05.2020", -425.41m),
                ("13.05.2020", 51990m),
                ("16.05.2020", -1502.3m),
                ("19.05.2020", -520.71m),
                ("20.05.2020", 8847.31m),
                ("21.05.2020", -23177.91m),
                ("22.05.2020", -1000m),
                ("23.05.2020", -1720.81m),
                ("26.05.2020", -6508.77m),
            };
            foreach (var op in arr)
            {
                event1 = New.Operation.BuidEvent(new BuidEventFromOpRequest
                {
                    OpTyp = op.Item2 > 0 ? OpType.PopolnitFromCash : OpType.SnyatCash,
                    Dat = DateTime.Parse(op.Item1),
                    DogLine1Id = StandardDogLineName.Halva,
                    Summ = Math.Abs(op.Item2)
                });
                context.StrategyBranches.Single().Events.Add(event1);
            }

            context.PeriodStart = context.StrategyBranches.Single().Events.Min(x => x.Dat);
            context.PeriodEnd = context.StrategyBranches.Single().Events.Max(x => x.Dat)
                .AddDays(1);

            Processor.ProcessPeriod(context);

            var halvaState= context.StrategyBranches.Single().LastEventState.DogovorLineStates[StandardDogLineName.Halva] as HalvaDogovorLineState;
            Assert.AreEqual(332673.32m, halvaState.Sum);
        }

    }
}