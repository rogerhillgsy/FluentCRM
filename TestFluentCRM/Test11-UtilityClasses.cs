using System;
using FluentCRM.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestFluentCRM
{
    [TestClass]
    public class Test11_UtilityClasses
    {
        [TestMethod]
        public void TestCallbackDictionary()
        {
            var t = new CallbackDictionary<string, string>();
            var a = string.Empty;
            var b1 = string.Empty;
            var b2 = string.Empty;

            t.Add("a", (s) => a = s);
            t.Add("b", (s) => b1 = s);
            t.Add("b", (s) => b2 = s);

            // Invoke with "a". just The A callbacks should be called.
            t.Invoke("a", "one");

            Assert.AreEqual("one", a);
            Assert.AreEqual(string.Empty, b1);
            Assert.AreEqual(string.Empty, b2);
            a = string.Empty;

            t.Invoke("b", "two");

            Assert.AreEqual(String.Empty, a);
            Assert.AreEqual("two", b1);
            Assert.AreEqual("two", b2);
            b1 = string.Empty;
            b2 = string.Empty;

            Assert.ThrowsException<ArgumentException>(() => t.Invoke("c", "three"));
            Assert.AreEqual(String.Empty, a);
            Assert.AreEqual(string.Empty, b1);
            Assert.AreEqual(string.Empty, b2);

            t.Invoke("c", "four", true);
            Assert.AreEqual(String.Empty, a);
            Assert.AreEqual(string.Empty, b1);
            Assert.AreEqual(string.Empty, b2);
        }
    }
}
