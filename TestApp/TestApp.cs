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
            //var guidStr = "da2e2e00-19ea-11e6-8002-444553540000"; // evilc 1
            var guidStr = "83f38eb0-7433-11e6-8007-444553540000"; // evilc 1w
            var iw = new SubscriptionHandler();
            //var plugins = iw.GetPluginNames();

            var handler1 = new Action<int>((value) => { Console.WriteLine("Value 1: " + value); });
            var sr1 = new SubscriptionRequest()
            {
                InputType = InputWrappers.InputType.AXIS,
                WrapperName = "SharpDX_XInput",
                StickId = "1",
                Handler = handler1,
                InputId = 1
            };
            iw.Subscribe(sr1);

            var handler2 = new Action<int>((value) => { Console.WriteLine("Value 2: " + value); });
            var sr2 = new SubscriptionRequest()
            {
                InputType = InputWrappers.InputType.BUTTON,
                WrapperName = "SharpDX_XInput",
                StickId = "1",
                Handler = handler2,
                InputId = 1
            };
            iw.Subscribe(sr2);

            var handler3 = new Action<int>((value) => { Console.WriteLine("Value 3: " + value); });
            var sr3 = new SubscriptionRequest()
            {
                InputType = InputWrappers.InputType.POV,
                WrapperName = "SharpDX_XInput",
                StickId = "1",
                Handler = handler3,
                InputId = 1
            };
            iw.Subscribe(sr3);

            var handler4 = new Action<int>((value) => { Console.WriteLine("Value 4: " + value); });
            var sr4 = new SubscriptionRequest()
            {
                InputType = InputWrappers.InputType.POV,
                WrapperName = "SharpDX_DirectInput",
                StickId = guidStr,
                Handler = handler4,
                InputId = 1,
                InputSubId = 1
            };
            iw.Subscribe(sr4);
        }
    }
}
