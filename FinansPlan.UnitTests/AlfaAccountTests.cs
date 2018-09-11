using System;
using NUnit.Framework;
using NSubstitute;
using FinansPlan;
using System.Linq;

namespace FinansPlan.UnitTests
{
    [TestFixture]
    public class AlfaAccountTransCalcTests
    {
        AlfaCreditCard alfaAccount;

        [SetUp]
        public void SetUp()
        {
            alfaAccount = new AlfaCreditCard(DateTime.Parse("01.08.2017"), 1490);
            alfaAccount.Transactions.Add(DateTime.Parse("1.08.2017"),0, 1, TranCat.getCash);//чтоб списалась комиссия

            alfaAccount.Transactions.Add(DateTime.Parse("1.02.2018"), -500, 1, TranCat.getCash);

            alfaAccount.Transactions.Add(DateTime.Parse("1.03.2018"), -1200, 1, TranCat.getCash);
            alfaAccount.Transactions.Add(DateTime.Parse("2.03.2018"), -3000, 1, TranCat.payCard);
            alfaAccount.Transactions.Add(DateTime.Parse("2.03.2018"), +3000, 1, TranCat.addCash);
            alfaAccount.Transactions.Add(DateTime.Parse("24.03.2018"), -800, 1, TranCat.getCash);
            alfaAccount.Transactions.Add(DateTime.Parse("26.03.2018"), -2600, 1, TranCat.getCash);

            alfaAccount.Recalc();
        }
        
        [Test]
        public void GetLimitOst_NoSdvig_ReturnCorrect()
        {
            //IAccount account = Substitute.For<IAccount>();
            //account.Limit.Returns(200000);
            //var limitOst = account.GetLimitOst(DateTime.Parse("25.03.2018"), true);
            //Assert.AreEqual(191900, limitOst);
            Assert.That(alfaAccount.GetLimitOst(DateTime.Parse("25.03.2018"), true), Is.EqualTo(200000-500-1200-3000+3000-800-2600));
        }

        [Test]
        public void GetLimitOst_CanSdvig_ReturnCorrect()
        {
            Assert.That(alfaAccount.GetLimitOst(DateTime.Parse("25.03.2018"), false), Is.EqualTo(200000-500-1200-3000+3000-800));
        }

        [Test]
        public void GetMaxCash_NoSdvig_ReturnCorrect()
        {
            var result = alfaAccount.GetMaxCash(DateTime.Parse("1.02.2018"), true);
            Assert.That(-result[0].sum, Is.EqualTo(50000-500));
       
            result = alfaAccount.GetMaxCash(DateTime.Parse("25.03.2018"), true);
            Assert.That(-result[0].sum, Is.EqualTo(50000-1200-800-2600));
        }
        [Test]
        public void GetMaxCash_CanSdvig_ReturnCorrect()
        {
            var result = alfaAccount.GetMaxCash(DateTime.Parse("25.03.2018"), false);
            Assert.That(-result[0].sum, Is.EqualTo(50000-1200-800));
        }
    }

    public class AlfaAccountClaimsTests
    {
        AlfaCreditCard alfaAccount;

        [SetUp]
        public void SetUp()
        {
            alfaAccount = new AlfaCreditCard(DateTime.Parse("01.08.2017"), 1490);
            alfaAccount.Transactions.Add(DateTime.Parse("1.08.2017"), 0, 1, TranCat.getCash);//чтоб списалась комиссия

            alfaAccount.Recalc();
        }

        [Test]
        public void Climes_Default_Created()
        {
            var climesCreated=alfaAccount.Claims.Where(pp => pp.dat < DateTime.Parse("01.01.2018"));
            Assert.That(climesCreated.Count(), Is.EqualTo(3));
            Assert.That(climesCreated.Sum(pp=>pp.sum), Is.EqualTo(alfaAccount.yearcommis));
        }
    }
    }
