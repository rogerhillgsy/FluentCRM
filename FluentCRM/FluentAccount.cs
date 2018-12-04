using System;
using System.Collections.Generic;
using FluentCRM.Interfaces;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public class FluentAccount  : Base_Classes.FluentCRM
    {
        private const string _logicalName = "account";

        private FluentAccount(Guid id, IOrganizationService service) : base (_logicalName,id,service) {}

        private FluentAccount(IOrganizationService service) : base(_logicalName, service) {}

        private FluentAccount(Guid id) : base(_logicalName, id) {}

        public static IEntitySet Account(Guid id, IOrganizationService service)
        {
            return new FluentAccount(id, service);
        }

        public static IUnknownEntity Account(IOrganizationService service)
        {
            return new FluentAccount(service);
        }

        public static IEntitySet Account(Guid id)
        {
            return new FluentAccount(id);
        }

        public static IUnknownEntity Account()
        {
            return new FluentAccount();
        }

        public FluentAccount() : base(_logicalName) {}

        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentAccount(service);
        }

        private readonly Dictionary<string,string> _joinOn = new Dictionary<string, string>{};

        public override string JoinAttribute(string rightEntityName)
        {
            if (_joinOn.ContainsKey(rightEntityName))
            {
                return _joinOn[rightEntityName];
            }
            else
            {
                return "accountid";
            }
        }
    }
}
