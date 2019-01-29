using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public class FluentPrimaryContact : FluentContact
    {
        public override string JoinAttribute(string rightEntityName)
        {
            return "contactid";
        }

        public override string JoinFromAttribute(string leftEntityName)
        {
            if (leftEntityName != "account")
            {
                throw  new NotImplementedException("Joining to primary contact - left entity must be an account");
            }
            return "primarycontactid";
        }

        // Various bits of boilerplate to make sure that typing stuff works for the join.
        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentPrimaryContact(service);
        }

        private FluentPrimaryContact(IOrganizationService service) : base(service) {}

        public FluentPrimaryContact() : base() {}

    }
}
