using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server;

namespace Server
{
    class Program
    {
        private static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            var port = 8080;
            Globals.Delay = 400;

            if (args.Length > 0)
            {
                int.TryParse(args[0], out port);
            }

            string url = string.Format("http://*:{0}", port);

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                _quitEvent.WaitOne(); 
            }
        }
    }

    /// <summary>
    /// Used by OWIN's startup process. 
    /// </summary>
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();

            RandomGenerator randomGenerator = new RandomGenerator(Globals.Delay);
            Task.Factory.StartNew(async () => await randomGenerator.OnRandomMonitor());
        }
    }
    /// <summary>
    /// Echoes messages sent using the Send message by calling the
    /// addMessage method on the client. Also reports to the console
    /// when clients connect and disconnect.
    /// </summary>
    public class MyHub : Hub
    {
        public void SendStatus(bool isPaused)
        {
            //Clients.All.addMessage(isPaused);
            if (isPaused)
            {
                Console.WriteLine("Client {0} paused the flow", Context.ConnectionId);
            }               
            else
            {
                Console.WriteLine("Client {0} resumed the flow", Context.ConnectionId);
            }
        }

        public override Task OnConnected()
        {
            Task t = base.OnConnected();
            Console.WriteLine("Client connected: " + Context.ConnectionId);
            Clients.All.setDelay(Globals.Delay);
            return t;
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }
}

