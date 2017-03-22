using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InputWrappers;

namespace InputWrapper
{
    public class SubscriptionHandler
    {
        [ImportMany(typeof(Wrappers.IInputWrapper))]
        //IEnumerable<Lazy<Wrappers.IInputWrapper, Wrappers.IInputWrapperMetadata>> _interfaceMeta;
        IEnumerable<Lazy<Wrappers.IInputWrapper, IDictionary<string, object>>> _interfaceMeta;

        public SubscriptionHandler()
        {
            Compose();
        }

        public List<string> GetPluginNames()
        {
            List<string> ret = new List<string>();
            foreach (Lazy<Wrappers.IInputWrapper, IDictionary<string, object>> inputWrapper in _interfaceMeta)
            {
                ret.Add(inputWrapper.Metadata["Name"].ToString());
            }
            return ret;
        }

        public int DoSomething(string blah)
        {
            Lazy<Wrappers.IInputWrapper> wrapper = _interfaceMeta.Where(s => (string)s.Metadata["Name"] == blah).FirstOrDefault();
            Console.WriteLine("Wrapper " + blah + " has " + wrapper.Value.GetButtonCount() + " buttons");
            Console.ReadKey();
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
