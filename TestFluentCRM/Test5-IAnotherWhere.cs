using System;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestFluentCRM
{
    [TestClass]
    public class Test5_IAnotherWhere
    {
        [TestMethod]
        public void TestWhere()
        {
            var context = TestUtilities.TestContext2();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .Count( c => Assert.AreEqual(2, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .And
                .Where("lastname").Equals("Doe")
                .Count( c => Assert.AreEqual(1, c))
                .Execute();

            FluentContact.Contact(context.GetOrganizationService())
                .Where("firstname").Equals("John")
                .And
                .Where("lastname").Equals("Smith")
                .Count( c => Assert.AreEqual(0, c))
                .Execute();        }
    }
}
