namespace InputWrapper
{
    public partial class InputWrappers
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
