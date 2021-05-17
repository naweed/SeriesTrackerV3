using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGENO.Framework.Enums
{
    public enum StorageStrategy
    {
        //Local, isolated folder.
        Local,
        //Cloud, isolated folder. 100k cumulative limit.
        Roaming,
        //Local, temporary folder (not for settings).
        Temporary
    }
}
