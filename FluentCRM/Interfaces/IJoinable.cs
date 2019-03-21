using System;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public interface IJoinable
    {
        /// <summary>
        /// Read an attribute from the current entity and call the action closure with the attribute value as argument.
        /// If the attribute has a null value, the closure will not be called.
        /// If multiple attributes are specified, then the first non-null value will be used to call the action closure.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the value of the attribute (if not null)</param>
        /// <param name="attribute">The logical name of the attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional attributes that we will try to use if the first attribute is null</param>
        /// <typeparam name="T">The expected type of the attribute that will be returned.</typeparam>
        IJoinableEntitySet UseAttribute<T>( Action<T> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read an attribute from the current entity and call the action closure with the attribute value as argument.
        /// If the attribute has a null value, the closure will not be called.
        /// If multiple attributes are specified, then the first non-null value will be used to call the action closure.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the name of the attribute and the value of the attribute (if not null)</param>
        /// <param name="attribute">The logical name of the attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional attributes that we will try to use if the first attribute is null</param>
        /// <typeparam name="T">The expected type of the attribute that will be returned.</typeparam>
        IJoinableEntitySet UseAttribute<T>( Action<string, T> action, string attribute, params string[] optionalAttributes);


        /// <summary>
        /// Read a set of attributes from the current entity and call the action closure with the entity as argument.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the entity</param>
        /// <param name="attribute">The logical name of the first attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional list of attributes that we will also try to read</param>
        IJoinableEntitySet UseEntity( Action<EntityWrapper, string> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read a set of attributes from the current entity and call the action closure with the entity 
        /// as argument for each non-null attribute.
        /// If all of the attributes have a null value, the closure will not be called.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the entity (if at least one not null argument)</param>
        /// <param name="attribute">The logical name of the first attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional list of attributes that we will also try to read</param>
        IJoinableEntitySet UseEntity( Action<string, EntityWrapper, string> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Join the current entity to the specified FluentCRM entity.
        /// </summary>
        /// <returns>FluentCRM entity</returns>
        /// <param name="target">Closure used to configure the joined entity.</param>
        /// <typeparam name="T">The type of entity joined to.</typeparam>
        IJoinableEntitySet Join<T>(Action<IJoinable> target) where T : FluentCRM, new();

        /// <summary>
        /// Return the logical name associated with the FluentCRM object.
        /// </summary>
        /// <value>The logical name of the FluentCRM entity.</value>
        string LogicalName { get; }

        /// <summary>
        /// Returns the name of the lookup attribute to be used to join to the given entity.
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="joinEntity">Join entity.</param>
        string JoinAttribute(string joinEntity);

        /// <summary>
        /// Used to spectify that an outer join is to be used.
        /// </summary>
        /// <returns>The outer.</returns>
        IJoinableEntitySet Outer();

        /// <summary>
        /// Internal function used to return a new instance of the joined-to FluentCRM object.
        /// <returns>FluentCRM object</returns>
        /// <param name="service">OrganizationService used to connect to CRM.</param>
        IJoinable Factory(IOrganizationService service);

        /// <summary>
        /// Select joined entity records where the given attribute meets some condition
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeName">Logical name that is the subject of the condition</param>
        IJoinableNeedsWhereCriteria Where(string attributeName);
    }
}
