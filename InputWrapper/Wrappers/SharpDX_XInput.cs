using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using SharpDX.XInput;
using InputWrapper;
using InputWrappers;

[Export(typeof(IInputWrapper))]
[ExportMetadata("Name", "SharpDX_XInput")]
public class SharpDX_XInput : IInputWrapper
{
    private Dictionary<int, StickMonitor> MonitoredSticks = new Dictionary<int, StickMonitor>();

    #region Interface Methods
    public bool Subscribe(SubscriptionRequest subReq)
    {
        var stickId = Convert.ToInt32(subReq.StickId);
        lock (MonitoredSticks)
        {
            if (!MonitoredSticks.ContainsKey(stickId))
            {
                MonitoredSticks.Add(stickId, new StickMonitor(subReq));
            }
            MonitoredSticks[stickId].Add(subReq);
        }
        return true;
    }

    public bool UnSubscribe(SubscriptionRequest subReq)
    {
        var stickId = Convert.ToInt32(subReq.StickId);
        if (MonitoredSticks.ContainsKey(stickId))
        {
            lock (MonitoredSticks)
            {
                var ret = MonitoredSticks[stickId].Remove(subReq);
                if (!MonitoredSticks[stickId].HasSubscriptions())
                {
                    MonitoredSticks.Remove(stickId);
                }
            }
        }
        return false;
    }

    public bool HasSubscriptions()
    {
        foreach (var monitor in MonitoredSticks)
        {
            if (monitor.Value.HasSubscriptions())
            {
                return true;
            }
        }
        return false;
    }

    public void Poll()
    {
        lock (MonitoredSticks)
        {
            foreach (var monitoredStick in MonitoredSticks)
            {
                monitoredStick.Value.Poll();
            }
        }
    }
    #endregion

    #region Monitors

    #region Stick
    public class StickMonitor
    {
        private Dictionary<int, InputMonitor> axisMonitors = new Dictionary<int, InputMonitor>();
        private Dictionary<int, InputMonitor> buttonMonitors = new Dictionary<int, InputMonitor>();
        private Dictionary<int, InputMonitor> povDirectionMonitors = new Dictionary<int, InputMonitor>();
        Dictionary<InputType, Dictionary<int, InputMonitor>> monitors = new Dictionary<InputType, Dictionary<int, InputMonitor>>();

        private enum xinputAxes { LeftThumbX = 1, LeftThumbY, RightThumbX, RightThumbY, LeftTrigger, RightTrigger }
        private static List<string> xinputAxisIdentifiers = new List<string>()
        {
            "LeftThumbX", "LeftThumbY", "LeftTrigger", "RightThumbX", "RightThumbY", "RightTrigger"
        };

        private static List<GamepadButtonFlags> xinputButtonIdentifiers = new List<GamepadButtonFlags>()
        {
            GamepadButtonFlags.A, GamepadButtonFlags.B, GamepadButtonFlags.X, GamepadButtonFlags.Y
            , GamepadButtonFlags.LeftShoulder, GamepadButtonFlags.RightShoulder, GamepadButtonFlags.Back, GamepadButtonFlags.Start
            , GamepadButtonFlags.LeftThumb, GamepadButtonFlags.RightThumb
        };

        private static List<GamepadButtonFlags> xinputDpadDirectionIdentifiers = new List<GamepadButtonFlags>()
        {
            GamepadButtonFlags.DPadUp, GamepadButtonFlags.DPadRight, GamepadButtonFlags.DPadDown, GamepadButtonFlags.DPadLeft
        };

        private Controller controller;


        public StickMonitor(SubscriptionRequest subReq)
        {
            monitors.Add(InputType.AXIS, axisMonitors);
            monitors.Add(InputType.BUTTON, buttonMonitors);
            monitors.Add(InputType.POV, povDirectionMonitors);

            controller = new Controller((UserIndex)(Convert.ToInt32(subReq.InputId)-1));
        }

        public void Add(SubscriptionRequest subReq)
        {
            var inputId = GetInputIdentifier(subReq);
            var monitor = monitors[subReq.InputType];
            if (!monitor.ContainsKey(inputId))
            {
                monitor.Add(inputId, new InputMonitor());
            }
            monitor[inputId].Add(subReq);
        }

        public bool Remove(SubscriptionRequest subReq)
        {
            var inputId = GetInputIdentifier(subReq);
            var monitor = monitors[subReq.InputType];
            if (monitor.ContainsKey(inputId))
            {
                var ret = monitor[inputId].Remove(subReq);
                if (!monitor[inputId].HasSubscriptions())
                {
                    monitor.Remove(inputId);
                }
                return ret;
            }
            return false;
        }

        public bool HasSubscriptions()
        {
            foreach (var monitorSet in monitors)
            {
                foreach (var monitor in monitorSet.Value)
                {
                    if (monitor.Value.HasSubscriptions())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int GetInputIdentifier(SubscriptionRequest subReq)
        {
            return Convert.ToInt32(subReq.InputId);
        }

        public void Poll()
        {
            var state = controller.GetState();
            foreach (var monitor in axisMonitors)
            {
                var value = Convert.ToInt32(state.Gamepad.GetType().GetField(xinputAxisIdentifiers[monitor.Key - 1]).GetValue(state.Gamepad));
                monitor.Value.ProcessPollResult(value);
            }

            foreach (var monitor in buttonMonitors)
            {
                var flag = state.Gamepad.Buttons & xinputButtonIdentifiers[monitor.Key - 1];
                var value = Convert.ToInt32(flag != GamepadButtonFlags.None);
                monitor.Value.ProcessPollResult(value);
            }

            foreach (var monitor in povDirectionMonitors)
            {
                var flag = state.Gamepad.Buttons & xinputDpadDirectionIdentifiers[monitor.Key - 1];
                var value = Convert.ToInt32(flag != GamepadButtonFlags.None);
                monitor.Value.ProcessPollResult(value);
            }
        }
    }
    #endregion

    #region Input
    public class InputMonitor
    {
        Dictionary<string, dynamic> subscriptions = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
        private int currentValue = 0;

        public bool Add(SubscriptionRequest subReq)
        {
            subscriptions.Add(subReq.SubscriberId, subReq.Handler);
            return true;
        }

        public bool Remove(SubscriptionRequest subReq)
        {
            if (subscriptions.ContainsKey(subReq.SubscriberId))
            {
                return subscriptions.Remove(subReq.SubscriberId);
            }
            return false;
        }

        public bool HasSubscriptions()
        {
            return subscriptions.Count > 0;
        }

        public void ProcessPollResult(int value)
        {
            // XInput does not report just the changed values, so filter out anything that has not changed
            if (currentValue == value)
                return;
            currentValue = value;
            foreach (var subscription in subscriptions)
            {
                subscription.Value(value);
            }
        }
    }
    #endregion

    #endregion
}
