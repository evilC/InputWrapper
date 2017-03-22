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
            var handler = new Action<int>((value) => { Console.WriteLine("Value: " + value); });
            var sr = new SubscriptionRequest()
            {
                InputType = InputWrappers.Wrappers.InputType.BUTTON,
                WrapperName = "SharpDX_DirectInput",
                StickGuid = new Guid("83f38eb0-7433-11e6-8007-444553540000"),
                Handler = handler,
                InputId = 1
            };
            iw.Subscribe(sr);
            //iw.Subscribe("SharpDX_DirectInput", "83f38eb0-7433-11e6-8007-444553540000", new Action<int>((value) => { Console.WriteLine("Value: " + value); }));
        }
    }
}
