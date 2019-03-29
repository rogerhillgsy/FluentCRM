using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;

namespace TestFluentCRM
{
    public class TestUtilities
    {
        public static List<Entity> List1()
        {
            var account1 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account1",
                ["name2"] = "Account name 2",
                ["name3"] = "name3",
                ["phone1"] = "123456",
                ["address1_country"] = "UK",
                ["statecode"] = 0,
                ["description"] = "",
            };

            var account2 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account2",
                ["name2"] = "",
                ["phone1"] = "654321",
                ["address1_country"] = "UK",
                ["statecode"] = 0,
                ["description"] = "",
            };

            var account3 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account3",
                ["name2"] = null,
                ["phone1"] = "222333",
                ["address1_country"] = "US",
                ["statecode"] = 0,
                ["description"] = "",
            };

            var account4 = new Entity("account")
            {
                Id = Guid.NewGuid(),
                ["name"] = "Account4",
                ["phone1"] = "222333",
                ["address1_country"] = "US",
                ["statecode"] = 0,
                ["description"] = "",
            };

            var contact1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["parentcustomerid"] = account1.ToEntityReference(),
                ["telephone1"] = "12345677",
                ["telephone2"] = "23456789",
                ["mobilephone"] = "07454115454",
                ["phone"] = "776543212",
                ["size"] = 5
            };

            var contact2 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Sam",
                ["lastname"] = "Spade",
                ["parentcustomerid"] = account1.ToEntityReference(),
                ["telephone2"] = "3456789",
                ["mobilephone"] = "07454113434",
                ["phone"] = "76543212",
                ["size"] = 10
            };

            var contact3 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "Watson",
                ["parentcustomerid"] = account2.ToEntityReference(),
                ["telephone1"] = "34567789",
                ["mobilephone"] = "07454116464",
                ["phone"] = "796543212"
            };

            account1["primarycontactid"] = contact1.ToEntityReference();
            account2["primarycontactid"] = contact3.ToEntityReference();

            return new List<Entity> {account1, account2, account3, account4, contact1, contact2, contact3};
        }

        public static XrmFakedContext TestContext1()
        {
            var list = List1();
            var context = new XrmFakedContext();
            context.Initialize( list);

            return context;
        }

        public static XrmFakedContext TestContext2()
        {
            var list = List1();
            var account1Copy = list.First().Clone();
            list.Add(account1Copy);
            var context = new XrmFakedContext();
            context.Initialize( list);

            return context;
        }

        public static XrmFakedContext TestContext3(Entity additionalEntity, params Entity[] additionalEntities)
        {
            var list = List1();
            list.Add(additionalEntity);
            list.AddRange(additionalEntities);
            var context = new XrmFakedContext();
            context.Initialize( list);

            return context;
        }

    }
}
