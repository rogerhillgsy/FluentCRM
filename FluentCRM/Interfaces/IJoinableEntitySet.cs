using System;

namespace FluentCRM
{
    /// <summary>
    /// Entity set produced by a Join operation
    /// </summary>
    public interface IJoinableEntitySet
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
        IJoinableEntitySet UseAttribute<T>( T defaultValue, Action<T> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read an attribute from the current entity and call the action closure with the attribute value as argument.
        /// If the attribute has a null value, the closure will not be called.
        /// If multiple attributes are specified, then the first non-null value will be used to call the action closure.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="defaultValue">If all of the candidate attributes are null, the action closure will be called with this value.</param>
        /// <param name="action">Closure to be called with the name of the attribute and the value of the attribute (if not null)</param>
        /// <param name="attribute">The logical name of the attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional attributes that we will try to use if the first attribute is null</param>
        /// <typeparam name="T">The expected type of the attribute that will be returned.</typeparam>
        IJoinableEntitySet UseAttribute<T>( T defaultValue, Action<string, T> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read a set of attributes from the current entity and call the action closure with the entity as argument.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the entity</param>
        /// <param name="attribute">The logical name of the first attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional list of attributes that we will also try to read</param>
        IJoinableEntitySet UseEntity( Action<EntityWrapper,string> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Read a set of attributes from the current entity and call the action closure with the entity 
        /// as argument for each non-null attribute.
        /// If all of the attributes have a null value, the closure will not be called.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the entity (if at least one not null argument)</param>
        /// <param name="attribute">The logical name of the first attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional list of attributes that we will also try to read</param>
        IJoinableEntitySet UseEntity( Action<string, EntityWrapper,string> action, string attribute, params string[] optionalAttributes);

        /// <summary>
        /// Update a given attribute in the current entity. Do so in a "weak" fashion.
        /// If the value is unchanged, no update will occur. (Including setting a null value to null)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="updateValue">Value to be used to update the attribute</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        IJoinableEntitySet WeakUpdate<T>(string attributeToUpdate, T updateValue);

        /// <summary>
        /// Update a given attribute in the current entity. Do so in a "weak" fashion.
        /// If the value is unchanged, no update will occur. (Including setting a null value to null)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="getUpdateValue">Closure that returns the value to be used to update the attribute.</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        IJoinableEntitySet WeakUpdate<T>(string attributeToUpdate, Func<T,T> getUpdateValue);
        /// <summary>
        /// Update a given attribute in the current entity. Ignore any existing value and force the update to happen whether the is a change or not.
        /// Note that this is not recommended as it can lead to unnecessary updates and triggering of plugins and workflows.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="updateValue">Value to be used to update the attribute</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        IJoinableEntitySet HardUpdate<T>(string attributeToUpdate, T updateValue);

        /// <summary>
        /// Update a given attribute in the current entity. Ignore any existing value and force the update to happen whether the is a change or not.
        /// Note that this is not recommended as it can lead to unnecessary updates and triggering of plugins and workflows.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="getUpdateValue">Closure that returns the value to be used to update the attribute.</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        IJoinableEntitySet HardUpdate<T>(string attributeToUpdate, Func<T,T> getUpdateValue);

        /// <summary>
        /// Calls the action function with a value that indicates whether the specified entity records exists.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Closure called with a boolean value indicating if the entity record existed.</param>
        IJoinableEntitySet Exists(Action<bool> action);

        /// <summary>
        /// Calls one of two action functions to indicate if a record existed (or not)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="whenTrue">Closure called when one or more matching records were found.</param>
        /// <param name="whenFalse">Closure called when one or no matching records were found.</param>
        IJoinableEntitySet Exists(Action whenTrue, Action whenFalse = null);

        /// <summary>
        /// Indicates that an additional where-clause is being introduced.
        /// </summary>
        /// <value>FluentCRM object</value>
        IJoinableAnotherWhere And { get; }

        /// <summary>
        /// Clear the specified attribute(s)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToClear">First attribute to be cleared</param>
        /// <param name="additionalAttributesToClear">Additional attributes to clear.</param>
        IJoinableEntitySet Clear(string attributeToClear, params string[] additionalAttributesToClear);

        /// <summary>
        /// Delete the entity from CRM.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        IJoinableEntitySet Delete();

        /// <summary>
        /// Join the current entity to the specified FluentCRM entity.
        /// </summary>
        /// <returns>FluentCRM entity</returns>
        /// <param name="target">Closure used to configure the joined entity.</param>
        /// <typeparam name="T">The type of entity joined to.</typeparam>
        IJoinableEntitySet Join<T>(Action<IJoinable> target) where T : FluentCRM, new();

        /// <summary>
        /// Closure called after each entity has been read and all closures called (i.e. from UseAttribute)
        /// Called in context of joined entity.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Called after the entity has been read with the entity value.</param>
        IJoinableEntitySet AfterEachRecord(Action<EntityWrapper> action);
    }
}