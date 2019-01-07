using System;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace FluentCRM
{
    public class EntityWrapper
    {
        private IOrganizationService _service;
        private Action<string> _tracer;
        public string Alias { get; set; }
        public Entity Entity { get; }
        private static MemoryCache _optionSetLabelCache;        

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
                if (!string.IsNullOrEmpty(Alias)) key = Alias + key;
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

        public T GetAttributeValue<T>(string attribute)
        {
            return Entity.GetAttributeValue<T>(attribute);
        }

        public bool Contains(string attribute)
        {
            return Entity.Contains(attribute);
        }

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
