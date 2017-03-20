using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputWrapper
{
    public class SubscriptionHandler
    {
        [ImportMany(typeof(InputWrappers.IInputWrapper))]
        IEnumerable<Lazy<InputWrappers.IInputWrapper, InputWrappers.IInputWrapperMetadata>> _interfaceMeta;

        public SubscriptionHandler()
        {
            Compose();
        }

        public List<string> GetPluginNames()
        {
            List<string> ret = new List<string>();
            foreach (Lazy<InputWrappers.IInputWrapper, InputWrappers.IInputWrapperMetadata> inputWrapper in _interfaceMeta)
            {
                ret.Add(inputWrapper.Metadata.Name);
            }
            return ret;
        }

        public int DoSomething(string blah)
        {
            foreach (Lazy<InputWrappers.IInputWrapper, InputWrappers.IInputWrapperMetadata> inputWrapper in _interfaceMeta)
            {
                if (inputWrapper.Metadata.Name == blah)
                {
                    Console.WriteLine(inputWrapper.Value.GetButtonCount());
                }
            }
            //Console.ReadKey();
            return 1;
        }

        private void Compose()
        {
            AssemblyCatalog catalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            CompositionContainer container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);
        }
    }
}
