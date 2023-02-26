using NUnit.Framework;
using FinansPlan2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinansPlan2.Tests
{
    [TestFixture()]
    public class HalvaCardTests
    {
        [Test()]
        public void CalcProcentsOnOperationsTest()
        {
            FinansPlan2.App.Dogovors.Clear();
            var start = DateTime.Parse("27.04.20");
            var halva = new HalvaCard()
            {
                Name = "halva",
                Start = start,
                InitState = new HalvaCardState() { Amount = 177710.92m, SobstvAmount = 177710.92m, Dat = start, Procents = 0 }
            };

            var cash = new CashVallet()
            {
                Name = "cash",
                Start = start,
                InitState = new CashValletState() { Amount = 1000000m, Dat = start }
            };

            FinansPlan2.App.Dogovors.Add(halva.Name, halva);

            FinansPlan2.App.Dogovors.Add(cash.Name, cash);

            var actions = new List<IActionCommand>()
            {
//new Operation(new OperationRequest{Dat=DateTime.Parse("27.04.2020"),SourceDogovorId="cash",TargetDogovorId="halva",Type=OperationType.PutCash,sum=177710.92m, atmPlace=ATMPlace.Own}),//init

new Operation(new OperationRequest{Dat=DateTime.Parse("27.04.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=94682.89m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("28.04.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=1845.23m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("29.04.2020"),SourceDogovorId="cash",TargetDogovorId="halva",Type=OperationType.PutCash,sum=14869.58m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("30.04.2020"),SourceDogovorId="cash",TargetDogovorId="halva",Type=OperationType.PutCash,sum=74900.89m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("01.05.2020"),SourceDogovorId="cash",TargetDogovorId="halva",Type=OperationType.PutCash,sum=136819.63m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("02.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=100m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("03.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=1050m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("04.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=976.72m, atmPlace=ATMPlace.Own}),

new Operation(new OperationRequest{Dat=DateTime.Parse("11.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=549m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("12.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=425.41m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("13.05.2020"),SourceDogovorId="cash",TargetDogovorId="halva",Type=OperationType.PutCash,sum=51990m, atmPlace=ATMPlace.Own}),

new Operation(new OperationRequest{Dat=DateTime.Parse("16.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=1502.3m, atmPlace=ATMPlace.Own}),

new Operation(new OperationRequest{Dat=DateTime.Parse("19.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=520.71m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("20.05.2020"),SourceDogovorId="cash",TargetDogovorId="halva",Type=OperationType.PutCash,sum=8847.31m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("21.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=23177.91m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("22.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=1000m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("23.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=1720.81m, atmPlace=ATMPlace.Own}),

new Operation(new OperationRequest{Dat=DateTime.Parse("26.05.2020"),SourceDogovorId="halva",TargetDogovorId="cash",Type=OperationType.GetCash,sum=6508.77m, atmPlace=ATMPlace.Own}),

            };/*{ new CreateDogovorCommand(DateTime.Parse("10.08.20"),
                    "alfa", new AlfaCreditCardState { Dat = DateTime.Parse("10.08.20"), Amount = 0, FreeMonthCashOst = 50000 })  };*/

            var endDat = DateTime.Parse("27.05.20");
            var d = start;
            AlfaCreditCardState prev = null;
            while (d <= endDat)
            {
                foreach (var dog in FinansPlan2.App.Dogovors.Values)
                {
                    if (dog.IsActive(d))
                    {
                        var aa = dog.OnDayStart(d);
                        actions.AddRange(aa);
                    }
                }

                foreach (var action in actions.Where(pp => pp.D.Date == d))
                    action.Execute();

                foreach (var dog in FinansPlan2.App.Dogovors.Values)
                {
                    if (dog.IsActive(d))
                        dog.OnDayEnd(d);
                }

                d = d.AddDays(1);
            }

            var actual = halva.CurrentState.SobstvAmount;
            //Assert.AreEqual(DateTime.Parse(expected), actual);
            //var actual = zp.CalcZpDate(DateTime.Parse(dat));
            Assert.AreEqual(332673.32m, actual);
        }
    }
}