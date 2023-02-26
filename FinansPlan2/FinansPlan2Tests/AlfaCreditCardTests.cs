using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinansPlan2.Tests
{
    [TestFixture()]
    public class AlfaCreditCardTests
    {
        private IFixture Fixture { get; set; }
        public AlfaCreditCardTests()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
        }
        private AlfaCreditCard GetDogovor(string[] nonWorkingDays)
        {
            var workDayProviderMock = Fixture.Freeze<Mock<IWorkDayProvider>>();
            workDayProviderMock.Setup(s => s.IsWorkDay(It.IsAny<DateTime>()))
                .Returns<DateTime>(
                x => nonWorkingDays != null
                ? !nonWorkingDays.Contains(x.ToShortDateString())
                : true);

            Fixture.Inject<IWorkDayService>(new WorkDayService(workDayProviderMock.Object));

            return Fixture.Freeze<AlfaCreditCard>(c => c.OmitAutoProperties());
        }


        [Test()]
        //[TestCase("20.11.2020", "20.01.2021", 321000, "20.11.2020-60000; 04.12.2020-70500; 18.12.2020-60000; 29.12.2020-70500; 20.01.2021-60000")]
        //[TestCase("20.10.2020", "20.01.2021", 451500, "20.10.2020-48000; 05.11.2020-82500; 20.11.2020-60000; 04.12.2020-70500; 18.12.2020-60000; 29.12.2020-70500; 20.01.2021-60000")]
        public void OldAlfaOperationsTest()//string start, string end, decimal sum, string details)
        {
            FinansPlan2.App.Dogovors.Clear();

            var start = DateTime.Parse("25.11.20");

            var nonWorkingDays = new string[] {
                "01.01.2019", "02.01.2019", "03.01.2019", "04.01.2019", "05.01.2019", "06.01.2019", "07.01.2019", "08.01.2019", "12.01.2019", "13.01.2019", "19.01.2019", "20.01.2019", "26.01.2019", "27.01.2019", "02.02.2019", "03.02.2019", "09.02.2019", "10.02.2019", "16.02.2019", "17.02.2019", "22.02.2019", "23.02.2019", "24.02.2019", "02.03.2019", "03.03.2019", "07.03.2019", "08.03.2019", "09.03.2019", "10.03.2019", "16.03.2019", "17.03.2019", "23.03.2019", "24.03.2019", "30.03.2019", "31.03.2019", "06.04.2019", "07.04.2019", "13.04.2019", "14.04.2019", "20.04.2019", "21.04.2019", "27.04.2019", "28.04.2019", "30.04.2019", "01.05.2019", "02.05.2019", "03.05.2019", "04.05.2019", "05.05.2019", "08.05.2019", "09.05.2019", "10.05.2019", "11.05.2019", "12.05.2019", "18.05.2019", "19.05.2019", "25.05.2019", "26.05.2019", "01.06.2019", "02.06.2019", "08.06.2019", "09.06.2019", "11.06.2019", "12.06.2019", "15.06.2019", "16.06.2019", "22.06.2019", "23.06.2019", "29.06.2019", "30.06.2019", "06.07.2019", "07.07.2019", "13.07.2019", "14.07.2019", "20.07.2019", "21.07.2019", "27.07.2019", "28.07.2019", "03.08.2019", "04.08.2019", "10.08.2019", "11.08.2019", "17.08.2019", "18.08.2019", "24.08.2019", "25.08.2019", "31.08.2019", "01.09.2019", "07.09.2019", "08.09.2019", "14.09.2019", "15.09.2019", "21.09.2019", "22.09.2019", "28.09.2019", "29.09.2019", "05.10.2019", "06.10.2019", "12.10.2019", "13.10.2019", "19.10.2019", "20.10.2019", "26.10.2019", "27.10.2019", "02.11.2019", "03.11.2019", "04.11.2019", "09.11.2019", "10.11.2019", "16.11.2019", "17.11.2019", "23.11.2019", "24.11.2019", "30.11.2019", "01.12.2019", "07.12.2019", "08.12.2019", "14.12.2019", "15.12.2019", "21.12.2019", "22.12.2019", "28.12.2019", "29.12.2019", "31.12.2019",
                "01.01.2020", "02.01.2020", "03.01.2020", "04.01.2020", "05.01.2020", "06.01.2020", "07.01.2020", "08.01.2020", "11.01.2020", "12.01.2020", "18.01.2020", "19.01.2020", "25.01.2020", "26.01.2020", "01.02.2020", "02.02.2020", "08.02.2020", "09.02.2020", "15.02.2020", "16.02.2020", "22.02.2020", "23.02.2020", "24.02.2020", "29.02.2020", "01.03.2020", "07.03.2020", "08.03.2020", "09.03.2020", "14.03.2020", "15.03.2020", "21.03.2020", "22.03.2020", "28.03.2020", "29.03.2020", "30.03.2020", "31.03.2020", "01.04.2020", "02.04.2020", "03.04.2020", "04.04.2020", "05.04.2020", "06.04.2020", "07.04.2020", "08.04.2020", "09.04.2020", "10.04.2020", "11.04.2020", "12.04.2020", "13.04.2020", "14.04.2020", "15.04.2020", "16.04.2020", "17.04.2020", "18.04.2020", "19.04.2020", "20.04.2020", "21.04.2020", "22.04.2020", "23.04.2020", "24.04.2020", "25.04.2020", "26.04.2020", "27.04.2020", "28.04.2020", "29.04.2020", "30.04.2020", "01.05.2020", "02.05.2020", "03.05.2020", "04.05.2020", "05.05.2020", "06.05.2020", "07.05.2020", "08.05.2020", "09.05.2020", "10.05.2020", "11.05.2020", "16.05.2020", "17.05.2020", "23.05.2020", "24.05.2020", "30.05.2020", "31.05.2020", "06.06.2020", "07.06.2020", "11.06.2020", "12.06.2020", "13.06.2020", "14.06.2020", "20.06.2020", "21.06.2020", "24.06.2020", "27.06.2020", "28.06.2020", "01.07.2020", "04.07.2020", "05.07.2020", "11.07.2020", "12.07.2020", "18.07.2020", "19.07.2020", "25.07.2020", "26.07.2020", "01.08.2020", "02.08.2020", "08.08.2020", "09.08.2020", "15.08.2020", "16.08.2020", "22.08.2020", "23.08.2020", "29.08.2020", "30.08.2020", "05.09.2020", "06.09.2020", "12.09.2020", "13.09.2020", "19.09.2020", "20.09.2020", "26.09.2020", "27.09.2020", "03.10.2020", "04.10.2020", "10.10.2020", "11.10.2020", "17.10.2020", "18.10.2020", "24.10.2020", "25.10.2020", "31.10.2020", "01.11.2020", "03.11.2020", "04.11.2020", "07.11.2020", "08.11.2020", "14.11.2020", "15.11.2020", "21.11.2020", "22.11.2020", "28.11.2020", "29.11.2020", "05.12.2020", "06.12.2020", "12.12.2020", "13.12.2020", "19.12.2020", "20.12.2020", "26.12.2020", "27.12.2020", "31.12.2020",
                "01.01.2021","02.01.2021","03.01.2021","04.01.2021","05.01.2021","06.01.2021","07.01.2021","08.01.2021","09.01.2021","10.01.2021","16.01.2021","17.01.2021","23.01.2021","24.01.2021","30.01.2021","31.01.2021","06.02.2021","07.02.2021","13.02.2021","14.02.2021","20.02.2021","21.02.2021","22.02.2021","23.02.2021","27.02.2021","28.02.2021","06.03.2021","07.03.2021","08.03.2021","13.03.2021","14.03.2021","20.03.2021","21.03.2021","27.03.2021","28.03.2021","03.04.2021","04.04.2021","10.04.2021","11.04.2021","17.04.2021","18.04.2021","24.04.2021","25.04.2021","30.04.2021","01.05.2021","02.05.2021","03.05.2021","08.05.2021","09.05.2021","10.05.2021","15.05.2021","16.05.2021","22.05.2021","23.05.2021","29.05.2021","30.05.2021","05.06.2021","06.06.2021","11.06.2021","12.06.2021","13.06.2021","14.06.2021","19.06.2021","20.06.2021","26.06.2021","27.06.2021","03.07.2021","04.07.2021","10.07.2021","11.07.2021","17.07.2021","18.07.2021","24.07.2021","25.07.2021","31.07.2021","01.08.2021","07.08.2021","08.08.2021","14.08.2021","15.08.2021","21.08.2021","22.08.2021","28.08.2021","29.08.2021","04.09.2021","05.09.2021","11.09.2021","12.09.2021","18.09.2021","19.09.2021","25.09.2021","26.09.2021","02.10.2021","03.10.2021","09.10.2021","10.10.2021","16.10.2021","17.10.2021","23.10.2021","24.10.2021","30.10.2021","31.10.2021","03.11.2021","04.11.2021","05.11.2021","06.11.2021","07.11.2021","13.11.2021","14.11.2021","20.11.2021","21.11.2021","27.11.2021","28.11.2021","04.12.2021","05.12.2021","11.12.2021","12.12.2021","18.12.2021","19.12.2021","25.12.2021","26.12.2021","31.12.2021"};

            var alfa = GetDogovor(nonWorkingDays);

            alfa.Name = "Alfa";//старая
            alfa.Start = start;
            alfa.CreditLimit = 420000m;
            alfa.MinPayCalcDay = 15;
            FinansPlan2.App.Dogovors.Add(alfa.Name, alfa);

            var cash = new CashVallet()
            {
                Name = "cash",
                Start = start,
                InitState = new CashValletState() { Amount = 1000000m, Dat = start }
            };

            FinansPlan2.App.Dogovors.Add(cash.Name, cash);

            var actions = new List<IActionCommand>() {
                new Operation(new OperationRequest{Dat=DateTime.Parse("25.11.2020"),SourceDogovorId="cash",TargetDogovorId="Alfa",Type=OperationType.PutCash,sum=25000m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("30.11.2020"),SourceDogovorId="Alfa",TargetDogovorId="cash",Type=OperationType.GetCash,sum=50000m, atmPlace=ATMPlace.Own}),
new PayCommand(new OperationRequest{Dat=DateTime.Parse("02.12.2020"),SourceDogovorId="Alfa",sum=46368m}),
new Operation(new OperationRequest{Dat=DateTime.Parse("02.12.2020"),SourceDogovorId="Alfa",TargetDogovorId="cash",Type=OperationType.GetCash,sum=50000m, atmPlace=ATMPlace.Own}),
//new Operation(new OperationRequest{Dat=DateTime.Parse("29.12.2020"),SourceDogovorId="cash",TargetDogovorId="Alfa",Type=OperationType.PutCash,sum=3600m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("11.01.2021"),SourceDogovorId="cash",TargetDogovorId="Alfa",Type=OperationType.PutCash,sum=3600m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("03.01.2021"),SourceDogovorId="Alfa",TargetDogovorId="cash",Type=OperationType.GetCash,sum=50000m, atmPlace=ATMPlace.Own}),
//new Operation(new OperationRequest{Dat=DateTime.Parse("31.01.2021"),SourceDogovorId="cash",TargetDogovorId="Alfa",Type=OperationType.PutCash,sum=5000m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("04.02.2021"),SourceDogovorId="cash",TargetDogovorId="Alfa",Type=OperationType.PutCash,sum=5000m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("02.02.2021"),SourceDogovorId="Alfa",TargetDogovorId="cash",Type=OperationType.GetCash,sum=50000m, atmPlace=ATMPlace.Own}),
new Operation(new OperationRequest{Dat=DateTime.Parse("09.03.2021"),SourceDogovorId="cash",TargetDogovorId="Alfa",Type=OperationType.PutCash,sum=6300m, atmPlace=ATMPlace.Own}),
 };
            var endDat = DateTime.Parse("11.03.2021");
            var d = start;
            AlfaCreditCardState prev = null;
            var errors = new List<Error>();
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

                if (d == DateTime.Parse("11.01.2021"))
                    Assert.AreEqual(alfa.GetClaims()[0].Sum, 3600m);
                else if (d == DateTime.Parse("04.02.2021"))
                    Assert.AreEqual(alfa.GetClaims()[0].Sum, 5000m);
                else if (d == DateTime.Parse("09.03.2021"))
                    Assert.AreEqual(alfa.GetClaims()[0].Sum, 6300m);
                else if (d == DateTime.Parse("11.03.2021"))
                    Assert.AreEqual(alfa.GetClaims()[0].Sum, 212768m-6300m);

                foreach (var action in actions.Where(pp => pp.D.Date == d))
                    action.Execute();

                foreach (var dog in FinansPlan2.App.Dogovors.Values)
                {
                    if (dog.IsActive(d))
                    {
                        var err = dog.OnDayEnd(d);
                        if (err.Any())
                        {
                            errors.AddRange(err);
                        }
                    }
                }

                d = d.AddDays(1);
            }


            Assert.AreEqual(DateTime.Parse("11.03.2021"), alfa.CurrentState.PeriodEndDate);
            Assert.AreEqual(207232m+ 6300m, alfa.CurrentState.Amount);
            Assert.AreEqual(212768m - 6300m, alfa.CreditLimit - alfa.CurrentState.Amount);
            Assert.AreEqual(1, errors.Count);
        }
    }
}