using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.Hooks
{
    public class Main
    {
        public static void Apply()
        {
            FastResetHandler.Apply();
            SaintPopcornTut.Apply();
        }
    }
}
