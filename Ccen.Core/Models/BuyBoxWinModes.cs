using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum BuyBoxWinModes
    {
        None = 0,
        Win = 1,
        NotWin = 10,
    }

    public static class BuyBoxWinModeHelper
    {
        public static string GetName(BuyBoxWinModes mode)
        {
            switch (mode)
            {
                case BuyBoxWinModes.Win:
                    return "Win";
                case BuyBoxWinModes.NotWin:
                    return "Not win";
            }
            return "-";
        }
    }
}
