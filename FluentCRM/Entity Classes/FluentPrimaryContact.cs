using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    /// <summary>
    /// FluentCRM class used to encapsulate join access from an Account to the Primary contact of an Account (or Accounts)
    /// </summary>
    public class FluentPrimaryContact : FluentContact
    {
        /// <summary>
        /// Internal-use function used to get the name of the "this entity" attribute to be used to join to the specified "foreign" entity .
        /// </summary>
        /// <param name="foreignEntityName"></param>
        /// <returns>Name of "this entity" attribute to be used to join to the given "foreign" entity.</returns>
        public override string JoinAttribute(string foreignEntityName)
        {
            return "contactid";
        }

        /// <summary>
        /// Internal-use function used to get the name of the "this entity" attribute to be used to join to the specified "foreign" entity .
        /// </summary>
        /// <param name="foreignEntityName"></param>
        /// <returns>Name of "this entity" attribute to be used to join to the given "foreign" entity.</returns>
        public override string JoinFromAttribute(string foreignEntityName)
        {
            if (foreignEntityName != "account")
            {
                throw new NotImplementedException("Joining to primary contact - left entity must be an account");
            }

            return "primarycontactid";
        }

        /// <summary>
        /// Factory method to return an instance of the FluentCRM entity class with the given CRM connection.
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentPrimaryContact(service);
        }

        private FluentPrimaryContact(IOrganizationService service) : base(service)
        {
        }

        /// <summary>
        /// Parameterless constructor required by the language, but not necessarily used.
        /// </summary>
        public FluentPrimaryContact() : base()
        {
        }
    }
}
