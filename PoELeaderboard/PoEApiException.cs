using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard
{
    public class PoEApiException : Exception
    {
        public ErrorObject Error { get; private set; }

        public PoEApiException(string message, ErrorObject error) : base(message)
        {
            Error = error;
        }

    }
}
