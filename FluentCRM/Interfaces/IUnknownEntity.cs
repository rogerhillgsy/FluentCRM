using System;
using System.Collections.Generic;
using System.Text;
using FluentCRM.Base_Classes;
using Microsoft.Xrm.Sdk;

namespace FluentCRM.Interfaces
{
    public interface IUnknownEntity
    {
        IOrganizationService Service { get; }

        INeedsWhereCriteria Where(string attributeName);
        IEntitySet Id(Guid id);
        IUnknownEntity Trace(Action<string> action);
        IUnknownEntity Timer(Action<string> timerAction);
        ICanExecute Join<T>(Action<IJoinable> target) where T : Base_Classes.FluentCRM, new();
    }
}
