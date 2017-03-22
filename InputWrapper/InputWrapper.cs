namespace InputWrappers
{
    public partial class Wrappers
    {
        public interface IInputWrapper
        {
            int GetButtonCount();
            bool Subscribe(dynamic passedCallback);
            void Poll();
            bool HasSubscriptions();
        }

        public interface IInputWrapperMetadata
        {
            string Name { get; }
        }
    }
}
