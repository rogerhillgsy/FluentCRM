
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    /// <summary>
    /// FluentCRM class used to encapsulate join access from a Activity to the RegardingOpportunity of that entity.
    /// </summary>
    public class FluentRegardingOpportunity : FluentOpportunity
    {
        /// <summary>
        /// Internal-use function used to get the name of the "this entity" attribute to be used to join to the specified "foreign" entity .
        /// </summary>
        /// <param name="foreignEntityName"></param>
        /// <returns>Name of "this entity" attribute to be used to join to the given "foreign" entity.</returns>
        public override string JoinAttribute(string foreignEntityName)
        {
            return "opportunityid";
        }

        /// <summary>
        /// Internal-use function used to get the name of the "this entity" attribute to be used to join to the specified "foreign" entity .
        /// </summary>
        /// <param name="foreignEntityName"></param>
        /// <returns>Name of "this entity" attribute to be used to join to the given "foreign" entity.</returns>
        public override string JoinFromAttribute(string foreignEntityName)
        {
            if (  !(new HashSet<string>(){"task","phonecall","appointment","email","letter"}.Contains( foreignEntityName) ))
            {
                throw new NotImplementedException("Joining to opportunity - left entity must be an activity");
            }

            return "regardingobjectid";
        }

        /// <summary>
        /// Factory method to return an instance of the FluentCRM entity class with the given CRM connection.
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentRegardingOpportunity(service);
        }

        private FluentRegardingOpportunity(IOrganizationService service) : base(service)
        {
        }

        /// <summary>
        /// Parameterless constructor required by the language, but not necessarily used.
        /// </summary>
        public FluentRegardingOpportunity() : base()
        {
        }
    }
}
