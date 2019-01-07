using System;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public interface IUnknownEntity
    {
        IOrganizationService Service { get; }

        INeedsWhereCriteria Where(string attributeName);
        IEntitySet Id(Guid id);
        IUnknownEntity Trace(Action<string> action);
        IUnknownEntity Timer(Action<string> timerAction);
        ICanExecute Join<T>(Action<IJoinable> target) where T : FluentCRM, new();
    }
}
