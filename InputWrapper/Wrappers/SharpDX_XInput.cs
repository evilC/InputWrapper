using System.ComponentModel.Composition;
using SharpDX.XInput;

namespace InputWrappers
{
    public partial class Wrappers
    {
        [Export(typeof(IInputWrapper))]
        [ExportMetadata("Name", "SharpDX_XInput")]
        public class SharpDX_XInput : IInputWrapper
        {
            private Controller Controller;

            public SharpDX_XInput()
            {

            }

            public int GetButtonCount()
            {
                return 10;
            }

            public bool Subscribe(dynamic passedCallback)
            {
                Controller = new Controller(UserIndex.One);
                return true;
            }

            public bool HasSubscriptions()
            {
                return false;
            }

            public void Poll()
            {

            }


        }
    }
}
