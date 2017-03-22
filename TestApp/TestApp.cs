using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InputWrapper;

namespace TestApp
{
    class TestApp
    {
        static void Main(string[] args)
        {
            var iw = new SubscriptionHandler();
            //var plugins = iw.GetPluginNames();
            //foreach (var pluginName in plugins)
            //{
            //    iw.DoSomething(pluginName);
            //}
            iw.Subscribe("SharpDX_DirectInput", new Action<int>((value) => { Console.WriteLine("Value: " + value); }));
        }
    }
}
