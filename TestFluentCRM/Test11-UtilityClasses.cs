using System;
using System.Text;
using FakeItEasy;
using FluentCRM;
using FluentCRM.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

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

        /// <summary>
        /// Test issues where {} in an exception message trace format string can cause an exception in the tracing code.
        /// </summary>
        [DataRow("Exception with curly braces {blah blah}")]
        [DataRow("Exception with positional arg {0}")]
        [DataRow("Exception with empty braces {}")]
        [DataRow("Exception with mismatched braces } {")]
        [DataTestMethod]
        public void TestTraceFormat( string badFormat)
        {
            var orgService = A.Fake<IOrganizationService>();
            var currentid = Guid.Empty;
            var problemSuffices = 0;
            var updates = 0;

            var targetId = Guid.NewGuid();
            var newValue = "123456";

            A.CallTo(orgService).Where(call => call.Method.Name == "Retrieve")
                .Throws(new Exception(badFormat));

            var builder = new StringBuilder();

            FluentOpportunity
                .Opportunity(targetId, orgService)
                .Trace(s => builder.AppendLine(s))
                .WeakUpdate("new_field", newValue)
                .Count(c => updates = c ?? 0)
                .Execute();

            Assert.AreEqual(0, updates);
            Assert.IsTrue( builder.ToString().Contains(badFormat));
        }
    }
}
