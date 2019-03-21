using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace FluentCRM
{
    public abstract partial class FluentCRM : ICreateEntity
    {
        private Microsoft.Xrm.Sdk.Entity _newEntity = null;

        ICreateEntity IUnknownEntity.Create(IDictionary<string, object> newAttributes)
        {
            CheckCreateRequired();
            foreach (var attribute in newAttributes)
            {
                // Ignore null values
                if ( ( attribute.Value != null ) && 
                      ( !string.IsNullOrEmpty( attribute.Value.ToString())))
                {
                    _newEntity.Attributes.Add(attribute);
                }
            }
            return (ICreateEntity) this;
        }

        ICreateEntity ICreateEntity.Create(IDictionary<string, object> attributes)
        {
            return ((IUnknownEntity) this ).Create(attributes);
        }

        /// <summary>
        /// Add optionset values, checking that they are not null or empty and that a number was supplied.
        /// </summary>
        /// <param name="optionSets"></param>
        /// <returns></returns>
        public ICreateEntity CreateOptionSets(IDictionary<string, string> optionSets)
        {
            CheckCreateRequired();
            foreach (var attribute in optionSets)
            {
                // Ignore null values
                int intValue;
                if (!string.IsNullOrEmpty(attribute.Value))
                {
                    if (int.TryParse(attribute.Value, out intValue))
                    {

                        var intOption = new OptionSetValue(intValue);
                        _newEntity.Attributes.Add(new KeyValuePair<string, object>(attribute.Key, intOption));
                    }
                    else
                    {
                        Trace($"Attribute {attribute.Key} - expected integer - actual {attribute.Value}");
                    }

                }
            }

            return this;
        }

        public ICreateEntity Id(Action<EntityReference> extractId)
        {
            _postExecuteActions.Add(() => extractId(_newEntity.ToEntityReference()));
            return this;
        }

        void ICreateEntity.Execute(Action preExecute, Action<int, int> postExecute)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            if (_newEntity == null)
            {
                Trace($"Cannot create entity with no attibutes");
            }

            preExecute?.Invoke();

            Timer($"Fetched in {stopwatch.Elapsed.TotalSeconds}s");
            
            Trace("Create {0} entity - {1}", _newEntity.LogicalName, String.Join(",", _newEntity.Attributes.Keys));
            stopwatch.Restart();
            _newEntity.Id = Service.Create(_newEntity);
            Timer($"Created in {stopwatch.Elapsed.TotalSeconds}s");

            _postExecuteActions.ForEach((a) => a.Invoke());

            _newEntity = null;           
        }

        private void CheckCreateRequired()
        {
            if (_newEntity == null)
            {
                _newEntity = new Entity(LogicalName);
            }
        }   }
}
