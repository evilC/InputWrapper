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

        public int DoSomething(string wrapperName)
        {
            var wrapper = GetWrapper(wrapperName);
            if (wrapper == null)
            {
                return 0;
            }
            Console.WriteLine("Wrapper " + wrapperName + " has " + wrapper.GetButtonCount() + " buttons");
            Console.ReadKey();
            return 1;
        }

        public Wrappers.IInputWrapper GetWrapper(string wrapperName)
        {
            Lazy<Wrappers.IInputWrapper> wrapper = null;
            try
            {
                wrapper = _interfaceMeta.Where(s => (string)s.Metadata["Name"] == wrapperName).FirstOrDefault();
                return wrapper.Value;
            }
            catch
            {
                return wrapper.Value;
            }
        }

        private void Compose()
        {
            AssemblyCatalog catalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            CompositionContainer container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);
        }
    }
}
