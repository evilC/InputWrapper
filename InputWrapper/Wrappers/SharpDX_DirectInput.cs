using System.ComponentModel.Composition;
using SharpDX.XInput;

namespace InputWrappers
{
    public partial class Wrappers
    {
        [Export(typeof(IInputWrapper))]
        [ExportMetadata("Name", "SharpDX_DirectInput")]
        public class SharpDX_DirectInput : IInputWrapper
        {
            dynamic callback = null;

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
                callback(1);
            }

        }
    }
}
