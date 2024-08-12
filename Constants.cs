using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipBoardManager
{
    internal interface Constants
    {
        public static String DEL_BUTTON = "DelButton";
        
        public const int MOD_ALT = 0x1;
        public const int MOD_CONTROL = 0x2;
        public const int MOD_SHIFT = 0x4;
        public const int MOD_WIN = 0x8;

        // Define a unique id for the hotkey
        public const int HOTKEY_ID = 9000;
        public const int HOTKEY_ID_LAST = 7001;
        public const int HOTKEY_ID_DELETE_ALL = 7002;
        public const int WM_HOTKEY = 0x0312;

    }
}
