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
            public int GetButtonCount()
            {
                return 128;
            }

            public bool Subscribe()
            {
                return true;
            }

        }
    }
}
