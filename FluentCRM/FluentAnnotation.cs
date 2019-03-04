using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public class FluentAnnotation  : FluentCRM
    {
        private const string _logicalName = "note";

        private FluentAnnotation(Guid id, IOrganizationService service) : base (_logicalName,id,service) {}

        private FluentAnnotation(IOrganizationService service) : base(_logicalName, service) {}

        private FluentAnnotation(Guid id) : base(_logicalName, id) {}

        public static IEntitySet Annotation(Guid id, IOrganizationService service)
        {
            return new FluentAnnotation(id, service);
        }

        public static IUnknownEntity Annotation(IOrganizationService service)
        {
            return new FluentAnnotation(service);
        }

        public static IEntitySet Annotation(Guid id)
        {
            return new FluentAnnotation(id);
        }

        public static IUnknownEntity Annotation()
        {
            return new FluentAnnotation();
        }

        public FluentAnnotation() : base(_logicalName) {}

        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentAnnotation(service);
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
                return "objectid";
            }
        }
    }
}
