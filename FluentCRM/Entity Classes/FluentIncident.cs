using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public class FluentIncident  : FluentCRM
    {
        private const string _logicalName = "incident";

        private FluentIncident(Guid id, IOrganizationService service) : base (_logicalName,id,service) {}

        private FluentIncident(IOrganizationService service) : base(_logicalName, service) {}

        private FluentIncident(Guid id) : base(_logicalName, id) {}

        public static IEntitySet Incident(Guid id, IOrganizationService service)
        {
            return new FluentIncident(id, service);
        }

        public static IUnknownEntity Incident(IOrganizationService service)
        {
            return new FluentIncident(service);
        }

        public static IEntitySet Incident(Guid id)
        {
            return new FluentIncident(id);
        }

        public static IUnknownEntity Incident()
        {
            return new FluentIncident();
        }

        public FluentIncident() : base(_logicalName) {}

        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentIncident(service);
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
                return "incidentid";
            }
        }
    }
}
