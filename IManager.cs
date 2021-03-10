using System;
using System.Collections.Generic;
using System.Text;

namespace TestDrivenAssignment
{
    public interface IManager
    {
        bool engineerRequired { get; set; }

        string GetStatus();
        bool SetEngineerRequired(bool needsEngineer);
    }
}
