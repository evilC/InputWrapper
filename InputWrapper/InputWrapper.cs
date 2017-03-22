namespace InputWrappers
{
    public partial class Wrappers
    {
        public interface IInputWrapper
        {
            int GetButtonCount();
            bool Subscribe();
        }

        public interface IInputWrapperMetadata
        {
            string Name { get; }
        }
    }
}
