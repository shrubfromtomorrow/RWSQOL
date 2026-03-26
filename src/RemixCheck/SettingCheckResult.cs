using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.RemixCheck
{
    /// <summary>
    /// A container for information relevant to the validity of a SettingCheck.
    /// </summary>
    public class SettingCheckResult
    {
        public string ModName;
        public string SettingName;
        public string TabName;
        public string Reason;
        public bool IsValid;
        public bool IsConditional;
    }
}
