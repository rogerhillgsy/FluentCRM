using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCRM.Interfaces
{
    /// <summary>
    ///  Interface to use after we have introduced a follow on Where() claus with an "And"
    /// </summary>
    public interface IAnotherWhere
    {
        INeedsWhereCriteria Where(string attributeName);
    }
}
