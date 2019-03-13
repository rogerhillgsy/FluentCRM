using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public class FluentContact  : FluentCRM
    {
        private const string _logicalName = "contact";

        private FluentContact(Guid id, IOrganizationService service) : base (_logicalName,id,service) {}

        protected FluentContact(IOrganizationService service) : base(_logicalName, service) {}

        private FluentContact(Guid id) : base(_logicalName, id) {}

        public static IEntitySet Contact(Guid id, IOrganizationService service)
        {
            return new FluentContact(id, service);
        }

        public static IUnknownEntity Contact(IOrganizationService service)
        {
            return new FluentContact(service);
        }

        public static IEntitySet Contact(Guid id)
        {
            return new FluentContact(id);
        }

        public static IUnknownEntity Contact()
        {
            return new FluentContact();
        }

        public FluentContact() : base(_logicalName) {}

        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentContact(service);
        }

        private readonly Dictionary<string,string> _joinOn = new Dictionary<string, string>
        {
            {"account", "parentcustomerid" },
        };

        public override string JoinAttribute(string rightEntityName)
        {
            if (_joinOn.ContainsKey(rightEntityName))
            {
                return _joinOn[rightEntityName];
            }
            else
            {
                return "contactid";
            }
        }
    }
}
