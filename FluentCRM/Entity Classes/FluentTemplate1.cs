using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public class Fluent$entity$  : FluentCRM
    {
        private const string _logicalName = "$entitylogicalname$";

        private Fluent$entity$(Guid id, IOrganizationService service) : base (_logicalName,id,service) {}

        private Fluent$entity$(IOrganizationService service) : base(_logicalName, service) {}

        private Fluent$entity$(Guid id) : base(_logicalName, id) {}

        public static IEntitySet $entity$(Guid id, IOrganizationService service)
        {
            return new Fluent$entity$(id, service);
        }

        public static IUnknownEntity $entity$(IOrganizationService service)
        {
            return new Fluent$entity$(service);
        }

        public static IEntitySet $entity$(Guid id)
        {
            return new Fluent$entity$(id);
        }

        public static IUnknownEntity $entity$()
        {
            return new Fluent$entity$();
        }

        public Fluent$entity$() : base(_logicalName) {}

        public override IJoinable Factory(IOrganizationService service)
        {
            return new Fluent$entity$(service);
        }

        private readonly Dictionary<string,string> _joinOn = new Dictionary<string, string>
        {
            // if the join to another entity is through the primary id field, (1:N join) nothing is needed here.
            // If it is through another field (N:1 join) then the details of the foreign entity and lookup field need to be given here.
            //
            // { "foreign entity logical name", "logical name of lookup field in this entity" }
            //   { "account", "parentcustomerid" } 
        };

        public override string JoinAttribute(string rightEntityName)
        {
            if (_joinOn.ContainsKey(rightEntityName))
            {
                return _joinOn[rightEntityName];
            }
            else
            {
                return "$entitylogicalname$id";
            }
        }
    }
}
