using System.ComponentModel.Composition;
using SharpDX.XInput;
using InputWrapper;

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

            public bool Subscribe(SubscriptionRequest subReq)
            {
                //Controller = new Controller(UserIndex.One);
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
