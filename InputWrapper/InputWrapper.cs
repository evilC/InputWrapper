﻿using InputWrapper;

namespace InputWrappers
{
    public interface IInputWrapper
    {
        bool Subscribe(SubscriptionRequest subReq);
        bool UnSubscribe(SubscriptionRequest subReq);
        void Poll();
        bool HasSubscriptions();
    }

    public interface IInputWrapperMetadata
    {
        string Name { get; }
    }

    // Allows categorization of input types
    public enum InputType { AXIS, BUTTON, POV };
}
