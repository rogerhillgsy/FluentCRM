using System;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace FluentCRM
{
    /// <summary>
    ///  Wrapper class for a CRM Entity that offers additional functionality.
    /// </summary>
    public class EntityWrapper
    {
        private IOrganizationService _service;
        private readonly Action<string> _tracer;

        /// <summary>
        /// Get or set the Alias value for fields in this entity.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Return the Underlying SDK entity type.
        /// </summary>
        public Entity Entity { get; }

        private static MemoryCache _optionSetLabelCache;        

        /// <summary>
        /// Construct a wrapper for the SDK Entity class.
        /// </summary>
        /// <param name="e">The underlying CRM SDK entity.</param>
        /// <param name="service">The Organization service to be used to connect to CRM.</param>
        /// <param name="tracer">Action function to be used to log and trace actions</param>
        public EntityWrapper(Entity e, IOrganizationService service, Action<string> tracer)
        {
            Entity = e;
            _service = service;
            _tracer = tracer;
        }

        /// <summary>
        /// Provide tradition array style access to late bound entities.
        /// However, this is done in a "Soft fail" way. If an attribute does not exist null is returned rather than throwing an exception.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                object rv = null;
                if (Entity.Attributes.ContainsKey(key))
                {
                    if (Alias == null)
                    {
                        rv = Entity.Attributes[key];
                    }
                    else
                    {
                        rv = Entity.GetAttributeValue<AliasedValue>(key).Value;
                    }
                }
                else
                {
                    Trace($"Attribute not found: {key}");
                }

                return rv;
            }

            set { Entity.Attributes[key] = value; }
        }

        /// <summary>
        /// Get typed value of a specific attribute 
        /// </summary>
        /// <typeparam name="T">Expected type of attribute to be returned.</typeparam>
        /// <param name="attribute">Logical name of attribute to return.</param>
        /// <returns></returns>
        public T GetAttributeValue<T>(string attribute)
        {
            var  rv  = default(T);
            if (!Entity.Attributes.ContainsKey(attribute))
            {
                Trace($"Attribute not found: {attribute}");
            }
            else
            {
                if (Entity.Attributes[attribute] is AliasedValue)
                {
                    AliasedValue obj = Entity.Attributes[attribute] as AliasedValue;

                    return (T) obj.Value;
                }
                else
                {
                    return Entity.GetAttributeValue<T>(attribute);
                }
            }

            return rv;
        }

        /// <summary>
        /// Find out if the underlying entity contains the given attribute.
        /// </summary>
        /// <param name="attribute">Logical name of the attribute.</param>
        /// <returns></returns>
        public bool Contains(string attribute)
        {
            return Entity.Contains(attribute);
        }

        /// <summary>
        /// Get text of Option set value.           
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public string OptionString(string attribute)
        {
            if (!Contains(attribute))
            {
                Trace($"Did not find optionset {attribute}");
                return null;
            }

            if (!(Entity[attribute] is OptionSetValue))
            {
                var message = $"Attribute {attribute} was not an option set - found {Entity[attribute].GetType()}";
                Trace(message);
                throw new ArgumentException(message,attribute);
            }

            var attributeValue = Entity.GetAttributeValue<OptionSetValue>(attribute);
            EnumAttributeMetadata metadata = null;
            var key = $"{Entity.LogicalName}/{attribute}";
            if (_optionSetLabelCache.Contains(key))
            {
                metadata = (EnumAttributeMetadata) _optionSetLabelCache[key];
                Trace($"Optionset values in cache for {attribute}");
            }
            else
            {   
                var attributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = Entity.LogicalName,
                    LogicalName = attribute,
                    RetrieveAsIfPublished = true
                };
                var attResponse = (RetrieveAttributeResponse) _service.Execute(attributeRequest);
                metadata = (EnumAttributeMetadata) attResponse.AttributeMetadata;
                _optionSetLabelCache[key] = metadata;
                Trace($"Added {metadata.OptionSet.Options.Count} optionset string s for {attribute} to cache");
            }

            return metadata.OptionSet.Options.FirstOrDefault(x => x.Value == attributeValue.Value)?.Label
                .UserLocalizedLabel.Label;
        }

        /// <summary>
        /// ID of the underlying entity represented by this EntityWrapper.
        /// </summary>
        public Guid Id => Entity.Id;

        private void Trace(string format, params string[] args)
        {
            _tracer?.Invoke(string.Format(format, args));
        }

        static EntityWrapper()
        {
            _optionSetLabelCache = new MemoryCache("OptionsetStringCache");
        }
    }
}
