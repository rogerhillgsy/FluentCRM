using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public interface IUnknownEntity
    {
        /// <summary>
        /// Return the current Organization Service in use by the FluentCRM object.
        /// </summary>
        /// <value>The organization service.</value>
        IOrganizationService Service { get; }

        /// <summary>
        /// Add selection criteria where the given attribute meets a subsequently specified condition
        /// </summary>
        /// <param name="attributeName">Logical name of the attribute used as criteria</param>
        /// <returns></returns>
        INeedsWhereCriteria Where(string attributeName);

        /// <summary>
        /// Used to specify the Guid of the entity record to be returned.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="id">Guid of the entity to be returned.</param>
        IEntitySet Id(Guid id);

        /// <summary>
        /// Cause trace messages to be written via the given action closure
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Action function that can be used to log trace messasges</param>
        IUnknownEntity Trace(Action<string> action);

        /// <summary>
        /// Output messages regarding the excution time of create, read and update operations.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="timerAction">Action function that can be used to log timer messasges</param>
        IUnknownEntity Timer(Action<string> timerAction);

        /// <summary>
        /// Join the current entity to the specified FluentCRM entity.
        /// </summary>
        /// <returns>FluentCRM entity</returns>
        /// <param name="target">Closure used to configure the joined entity.</param>
        /// <typeparam name="T">The type of entity joined to.</typeparam>
        ICanExecute Join<T>(Action<IJoinable> target) where T : FluentCRM, new();

        /// <summary>
        /// Sets the attributes and values to be used when creating the new entity.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="newAttributes">List of name value pairs used to create the attribute</param>
        ICreateEntity Create(IDictionary<string, Object> newAttributes);

    }
}
