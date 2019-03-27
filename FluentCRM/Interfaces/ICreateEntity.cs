using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    /// <summary>
    /// Interface to use where we are looking to create a new entity.
    /// </summary>
    public interface ICreateEntity
    {
        /// <summary>
        /// Return the current Organization Service in use by the FluentCRM object.
        /// </summary>
        /// <value>The organization service.</value>
        IOrganizationService Service { get; }

        /// <summary>
        /// Calls the given closure with the Guid of the newly created entity.
        /// </summary>
        /// <returns>FluentCRM</returns>
        /// <param name="returnNewId">Closure called with the Guid of the newly created entity..</param>
        ICreateEntity Id(Action<EntityReference> returnNewId);

        /// <summary>
        /// Sets the attributes and values to be used when creating the new entity.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributes">List of name value pairs used to create the attribute</param>
        ICreateEntity Create(IDictionary<string, Object> attributes);

        /// <summary>
        /// Sets the attributes and values of optionsets to be used when creating the new entity.
        /// This will call out to CRM to retrieve the optionset metadata (which will then be cached)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributes">List of optionset name and value pairs used to create the attribute</param>
        ICreateEntity CreateOptionSets(IDictionary<string, string> attributes);

        /// <summary>
        /// Called to trigger the execution of the FluentCRM object. 
        /// Start reading all required attributes, carrying out any updates, deleting records etc.
        /// </summary>
        /// <param name="preExecute">Called immediately prior to making calls to CRM</param>
        /// <param name="postExecute">Called when execution has been completed.</param>
        void Execute( Action preExecute = null, Action<int,int> postExecute = null );
    }
}
