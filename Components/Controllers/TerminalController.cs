using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace ComBot_Revamped.Components.Controllers
{
    public class TerminalController : Hub
    {
        // Probably don't need to say this... Lock this when used in basically anything here.
        static List<string> sessions; // Sessions are stored internally as base64 strings... and probably externally too

        public async Task TermLogin(string username, string password)
        {
            await Clients.Caller.SendCoreAsync("OnLogin", new object[] { "SESSION TOKEN" });
        }
    }
}
