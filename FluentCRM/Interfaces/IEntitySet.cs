using System;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    /// <summary>
    /// Interface that refers to an Fluent CRM instance that potentially refers to a set of one or more Entities, but is not ready to execute as no actions have been defined on it.
    /// </summary>
    public interface IEntitySet
    {
        /// <summary>
        /// Cause trace messages to be written via the given action closure
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Action function that can be used to log trace messasges</param>
        IEntitySet Trace(Action<string> action);

        /// <summary>
        /// Output messages regarding the excution time of create, read and update operations.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="timerFunction">Action function that can be used to log timer messasges</param>
        IEntitySet Timer(Action<string> timerFunction );

        /// <summary>
        /// Return the current Organization Service in use by the FluentCRM object.
        /// </summary>
        /// <value>The organization service.</value>
        IOrganizationService Service { get; }

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
        ICanExecute UseAttribute<T>(  Action<T> action, string attribute, params string[] optionalAttributes);

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
        ICanExecute UseAttribute<T>( Action<string, T> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read an attribute from the current entity and call the action closure with the attribute value as argument.
        /// If the attribute has a null value, the closure will not be called.
        /// If multiple attributes are specified, then the first non-null value will be used to call the action closure.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="defaultValue">Return this default value if the attribute is null or missing.</param>
        /// <param name="action">Closure to be called with the value of the attribute (if not null)</param>
        /// <param name="attribute">The logical name of the attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional attributes that we will try to use if the first attribute is null</param>
        /// <typeparam name="T">The expected type of the attribute that will be returned.</typeparam>
        ICanExecute UseAttribute<T>(  T defaultValue, Action<T> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read an attribute from the current entity and call the action closure with the attribute value as argument.
        /// If the attribute has a null value, the closure will not be called.
        /// If multiple attributes are specified, then the first non-null value will be used to call the action closure.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="defaultValue">Return this default value if the attribute is null or missing.</param>
        /// <param name="action">Closure to be called with the name of the attribute and the value of the attribute (if not null)</param>
        /// <param name="attribute">The logical name of the attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional attributes that we will try to use if the first attribute is null</param>
        /// <typeparam name="T">The expected type of the attribute that will be returned.</typeparam>
        ICanExecute UseAttribute<T>( T defaultValue, Action<string, T> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read a set of attributes from the current entity and call the action closure with the entity as argument.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the entity</param>
        /// <param name="attribute">The logical name of the first attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional list of attributes that we will also try to read</param>
        ICanExecute UseEntity( Action<EntityWrapper> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read a set of attributes from the current entity and call the action closure with the entity 
        /// as argument for each non-null attribute.
        /// If all of the attributes have a null value, the closure will not be called.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the entity (if at least one not null argument)</param>
        /// <param name="attribute">The logical name of the first attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional list of attributes that we will also try to read</param>
        ICanExecute UseEntity( Action<string, EntityWrapper> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Update a given attribute in the current entity. Do so in a "weak" fashion.
        /// If the value is unchanged, no update will occur. (Including setting a null value to null)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="updateValue">Value to be used to update the attribute</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        ICanExecute WeakUpdate<T>(string attributeToUpdate, T updateValue);

        /// <summary>
        /// Update a given attribute in the current entity. Do so in a "weak" fashion.
        /// If the value is unchanged, no update will occur. (Including setting a null value to null)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="getUpdateValue">Closure that returns the value to be used to update the attribute.</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        ICanExecute WeakUpdate<T>(string attributeToUpdate, Func<T,T> getUpdateValue);

        /// <summary>
        /// Update a given attribute in the current entity. Ignore any existing value and force the update to happen whether the is a change or not.
        /// Note that this is not recommended as it can lead to unnecessary updates and triggering of plugins and workflows.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="updateValue">Value to be used to update the attribute</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        ICanExecute HardUpdate<T>(string attributeToUpdate, T updateValue);

        /// <summary>
        /// Update a given attribute in the current entity. Ignore any existing value and force the update to happen whether the is a change or not.
        /// Note that this is not recommended as it can lead to unnecessary updates and triggering of plugins and workflows.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="getUpdateValue">Closure that returns the value to be used to update the attribute.</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        ICanExecute HardUpdate<T>(string attributeToUpdate, Func<T,T> getUpdateValue);

        /// <summary>
        /// Calls the action function with the number of records read from CRM when the FluentCRM object is executed.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Closure called with the count of the number of records read.</param>
        ICanExecute Count(Action<int?> action);

        /// <summary>
        /// Calls the action function with a value that indicates whether the specified entity records exists.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Closure called with a boolean value indicating if the entity record existed.</param>
        ICanExecute Exists(Action<bool> action);

        /// <summary>
        /// Calls one of two action functions to indicate if a record existed (or not)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="whenTrue">Closure called when one or more matching records were found.</param>
        /// <param name="whenFalse">Closure called when one or no matching records were found.</param>
        ICanExecute Exists(Action whenTrue, Action whenFalse = null);

        /// <summary>
        /// Use this to ensure that unique records are returned.
        /// Reading data from CRM in paged mode is not reliable, it can return the same record multiple times.
        /// Distinct will de-dupe read data base on the Id value and ensure that any closures are only called
        ///  once for each distinc entity.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        IEntitySet Distinct();

        /// <summary>
        /// Returns records (or calls closures) in the order indicated by the given attribute.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attribute">Attribute by which the returned records will be sorted</param>
        IEntitySet OrderByAsc(string attribute);

        /// <summary>
        /// Returns records (or calls closures) in the reverse order indicated by the given attribute.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attribute">Attribute by which the returned records will be sorted</param>
        IEntitySet OrderByDesc(string attribute);

        /// <summary>
        ///  Select first n matching records according to any ordering criteria.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        IEntitySet Top(int n);

        /// <summary>
        /// Indicates that an additional where-clause is being introduced.
        /// </summary>
        /// <value>FluentCRM object</value>
        IAnotherWhere And { get; }

        /// <summary>
        /// Clear the specified attribute(s)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToClear">First attribute to be cleared</param>
        /// <param name="additionalAttributesToClear">Additional attributes to clear.</param>
        ICanExecute Clear(string attributeToClear, params string[] additionalAttributesToClear);

        /// <summary>
        /// Delete the entity from CRM.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        ICanExecute Delete();

        /// <summary>
        /// Join the current entity to the specified FluentCRM entity.
        /// </summary>
        /// <returns>FluentCRM entity</returns>
        /// <param name="target">Closure used to configure the joined entity.</param>
        /// <typeparam name="T">The type of entity joined to.</typeparam>
        ICanExecute Join<T>(Action<IJoinable> target) where T : FluentCRM, new();

        /// <summary>
        /// Closure called before each entity is processed.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Called after the entity has been read with the entity value.</param>
        ICanExecute BeforeEachRecord(Action<EntityWrapper> action);
    }
}
