using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InputWrappers;
using System.Threading;

namespace InputWrapper
{
    public class SubscriptionHandler
    {
        [ImportMany(typeof(InputWrapperBase.IInputWrapper))]
        IEnumerable<Lazy<InputWrapperBase.IInputWrapper, IDictionary<string, object>>> _interfaceMeta;
        private bool threadRunning = false;

        public SubscriptionHandler()
        {
            Compose();
        }

        public void Subscribe(SubscriptionRequest subReq){
            var wrapper = GetWrapper(subReq.WrapperName);
            wrapper.Subscribe(subReq);
            SetMonitorState();
        }

        #region Monitor Thread handling
        private void SetMonitorState()
        {
            int count = 0;
            foreach (Lazy<InputWrapperBase.IInputWrapper, IDictionary<string, object>> inputWrapper in _interfaceMeta)
            {
                if (inputWrapper.Value.HasSubscriptions())
                {
                    count++;
                }
            }

            if (threadRunning && count == 0)
                threadRunning = false;
            else if (!threadRunning && count > 0)
                MonitorSticks();
        }

        private void MonitorSticks()
        {
            var t = new Thread(new ThreadStart(() =>
            {
                threadRunning = true;
                //Debug.WriteLine("InputWrapper| MonitorSticks starting");
                while (threadRunning)
                {
                    foreach (Lazy<InputWrapperBase.IInputWrapper, IDictionary<string, object>> inputWrapper in _interfaceMeta)
                    {
                        inputWrapper.Value.Poll();
                    }
                    Thread.Sleep(1);
                }
                //Debug.WriteLine("InputWrapper| MonitorSticks stopping");
            }));
            t.Start();
        }
        #endregion

        #region Querying
        public List<string> GetPluginNames()
        {
            List<string> ret = new List<string>();
            foreach (Lazy<InputWrapperBase.IInputWrapper, IDictionary<string, object>> inputWrapper in _interfaceMeta)
            {
                ret.Add(inputWrapper.Metadata["Name"].ToString());
            }
            return ret;
        }
        #endregion

        #region Wrapper handling
        public InputWrapperBase.IInputWrapper GetWrapper(string wrapperName)
        {
            Lazy<InputWrapperBase.IInputWrapper> wrapper = null;
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
        #endregion
    }

    #region Public Classes
    public class SubscriptionRequest
    {
        public SubscriptionRequest()
        {
            SubscriberId = "0";
            InputSubId = 0;
        }

        public InputWrapperBase.InputType InputType { get; set; }
        public string SubscriberId { get; set; }
        public string WrapperName { get; set; }
        public string StickId { get; set; }
        public int InputId { get; set; }
        public dynamic Handler { get; set; }
        // used, eg, for DirectInput POV number
        public int InputSubId { get; set; }
    }
    #endregion
}
