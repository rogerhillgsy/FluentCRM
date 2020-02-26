using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCRM.Utility
{
    /// <summary>
    /// Dictionary to facilitate storing lists of callbacks in a dictionary.
    /// An example of usage would be to set up list of closures to be called for a range of GUIDs.
    /// Invocation deals with the fact that there may be more than one closure associated with a particular Guid.
    /// This class is aimed at helping with the issues around the 10-entity limit on FetchXMl/SDK joinsin CRM.
    /// </summary>
    /// <typeparam name="TK">Type of key</typeparam>
    /// <typeparam name="T">Type use to invoke closures.</typeparam>
    public class CallbackDictionary<TK, T>
    {
        /// <summary>
        ///  Dictionary of keys, and a list of closures to be called for that key.
        /// </summary>
        readonly Dictionary<TK, List<Action<T>>> _dict = new Dictionary<TK, List<Action<T>>>();

        /// <summary>
        /// Instantiate a new CallbackDictionary
        /// </summary>
        public CallbackDictionary()
        {

        }

        /// <summary>
        /// Adds a closure with the given Key.
        /// Note that unlike a normal dictionary, a CallbackDictionary can have any number of entries added with the same key.
        /// When invoked for a particular key. All of the closures wih that key will be invoked.
        /// </summary>
        /// <param name="key">The key under which the closure is stored</param>
        /// <param name="action">The action (closure) to be run when this key is invoked.</param>
        public void Add(TK key, Action<T> action)
        {
            if (!_dict.ContainsKey(key))
            {
                _dict.Add(key, new List<Action<T>>());
            }
            _dict[key].Add(action);
        }

        /// <summary>
        /// Invoke all closures that have been associated with a particular key.
        /// </summary>
        /// <param name="key">Key for which closures are to be invoked.</param>
        /// <param name="arg">Argument to be passed to each closure</param>
        /// <param name="ignoreException">By default an exception will be thrown if the dictionary does not contain a particular key.</param>
        /// <exception cref="ArgumentException"></exception>
        public void Invoke(TK key, T arg, bool ignoreException = false)
        {
            if (!_dict.ContainsKey(key))
            {
                if (!ignoreException)
                    throw new ArgumentException($"CallbackDictionary key not found on invoke: {key}");
            }
            else
            {
                _dict[key].ForEach(a => a.Invoke(arg));
            }

            var x = _dict.Keys;
        }

        /// <summary>
        /// Return a list of keys in a CallbackDictionary.
        /// </summary>
        public Dictionary<TK, List<Action<T>>>.KeyCollection Keys => _dict.Keys;
    }
}
