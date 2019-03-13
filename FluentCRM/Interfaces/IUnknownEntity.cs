using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public interface IUnknownEntity
    {
        IOrganizationService Service { get; }

        /// <summary>
        /// Add selection criteria where the given attribute meets a subsequently specified condition
        /// </summary>
        /// <param name="attributeName">Logical name of the attribute used as criteria</param>
        /// <returns></returns>
        INeedsWhereCriteria Where(string attributeName);
        IEntitySet Id(Guid id);
        IUnknownEntity Trace(Action<string> action);
        IUnknownEntity Timer(Action<string> timerAction);
        ICanExecute Join<T>(Action<IJoinable> target) where T : FluentCRM, new();

        ICreateEntity Create(Dictionary<string, Object> newAttributes);

    }
}
