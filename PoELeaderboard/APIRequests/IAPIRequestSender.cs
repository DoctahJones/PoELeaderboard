using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoELeaderboard.APIRequests
{
    public interface IAPIRequestSender
    {
        string MakeRequest(string url);
        Task<string> MakeRequestAsync(string url, CancellationToken cancelToken, TimeSpan timeOut);

    }
}
