using System.ComponentModel.Composition;
//using SharpDX.XInput;
using SharpDX.DirectInput;
using System;

namespace InputWrappers
{
    public partial class Wrappers
    {
        [Export(typeof(IInputWrapper))]
        [ExportMetadata("Name", "SharpDX_DirectInput")]
        public class SharpDX_DirectInput : IInputWrapper
        {
            public Joystick joystick;
            static private DirectInput directInput;

            dynamic callback = null;

            public SharpDX_DirectInput()
            {
                directInput = new DirectInput();
                joystick = new Joystick(directInput, new Guid("83f38eb0-7433-11e6-8007-444553540000"));
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
            }

            public int GetButtonCount()
            {
                return 128;
            }

            public bool Subscribe(dynamic passedCallback)
            {
                callback = passedCallback;
                return true;
            }

            public bool HasSubscriptions()
            {
                return callback != null;
            }

            public void Poll()
            {
                joystick.Poll();
                var data = joystick.GetBufferedData();
                // Iterate each report
                foreach (var state in data)
                {
                    if (state.Offset == JoystickOffset.X)
                    {
                        callback(state.Value);
                    }
                }
                
            }

        }
    }
}
