﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using SharpDX.DirectInput;
using System;

using InputWrapper;
using InputWrappers;

[Export(typeof(IInputWrapper))]
[ExportMetadata("Name", "SharpDX_DirectInput")]
public class SharpDX_DirectInput : IInputWrapper
{
    static private DirectInput directInput;

    private Dictionary<Guid, StickMonitor> MonitoredSticks = new Dictionary<Guid, StickMonitor>();

    public SharpDX_DirectInput()
    {
        directInput = new DirectInput();
    }

    #region Interface Methods
    public bool Subscribe(SubscriptionRequest subReq)
    {
        var guid = new Guid(subReq.StickId);
        if (!MonitoredSticks.ContainsKey(guid))
        {
            MonitoredSticks.Add(guid, new StickMonitor(subReq));
        }
        return MonitoredSticks[guid].Add(subReq);
    }

    public bool UnSubscribe(SubscriptionRequest subReq)
    {
        var stickId = new Guid(subReq.StickId);
        if (MonitoredSticks.ContainsKey(stickId))
        {
            lock (MonitoredSticks)
            {
                var ret = MonitoredSticks[stickId].Remove(subReq);
                if (!MonitoredSticks[stickId].HasSubscriptions())
                {
                    MonitoredSticks.Remove(stickId);
                }
                return true;
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
        foreach (var monitoredStick in MonitoredSticks)
        {
            monitoredStick.Value.Poll();
        }

    }
    #endregion

    #region Monitors

    #region Stick
    public class StickMonitor
    {
        public Joystick joystick;
        private Dictionary<JoystickOffset, InputMonitor> monitors = new Dictionary<JoystickOffset, InputMonitor>();

        private Guid stickGuid;

        public StickMonitor(SubscriptionRequest subReq)
        {
            stickGuid = new Guid(subReq.StickId);
            joystick = new Joystick(directInput, stickGuid);
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();
        }

        public bool Add(SubscriptionRequest subReq)
        {
            var inputId = GetInputIdentifier(subReq.InputType, subReq.InputId);
            if (!monitors.ContainsKey(inputId))
            {
                monitors.Add(inputId, new InputMonitor());
            }
            return monitors[inputId].Add(subReq);
        }

        public bool Remove(SubscriptionRequest subReq)
        {
            var inputId = GetInputIdentifier(subReq.InputType, subReq.InputId);
            if (monitors.ContainsKey(inputId))
            {
                var ret = monitors[inputId].Remove(subReq);
                if (!monitors[inputId].HasSubscriptions())
                {
                    monitors.Remove(inputId);
                }
                return ret;
            }
            return false;
        }

        private JoystickOffset GetInputIdentifier(InputType inputType, int inputId)
        {
            return directInputMappings[inputType][inputId - 1];
        }

        public bool HasSubscriptions()
        {
            foreach (var monitor in monitors)
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
            joystick.Poll();
            var data = joystick.GetBufferedData();
            // Iterate each report
            foreach (var state in data)
            {
                if (monitors.ContainsKey(state.Offset))
                {
                    monitors[state.Offset].ProcessPollResult(state.Value);
                }
            }

        }
    }
    #endregion

    #region Input
    public class InputMonitor
    {
        Dictionary<string, dynamic> subscriptions = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
        Dictionary<int, PovDirectionMonitor> povDirectionMonitors = new Dictionary<int, PovDirectionMonitor>();

        public bool Add(SubscriptionRequest subReq)
        {
            if (subReq.InputSubId == 0)
            {
                subscriptions.Add(subReq.SubscriberId, subReq.Handler);
                return true;
            }
            else
            {
                if (!povDirectionMonitors.ContainsKey(subReq.InputSubId))
                {
                    povDirectionMonitors[subReq.InputSubId] = new PovDirectionMonitor(subReq);
                }
                return povDirectionMonitors[subReq.InputSubId].Add(subReq);
            }
        }

        public bool Remove(SubscriptionRequest subReq)
        {
            if (subReq.InputSubId == 0)
            {
                if (subscriptions.ContainsKey(subReq.SubscriberId))
                {
                    subscriptions.Remove(subReq.SubscriberId);
                    return true;
                }
            }
            else
            {
                if (povDirectionMonitors.ContainsKey(subReq.InputSubId))
                {
                    var ret = povDirectionMonitors[subReq.InputSubId].Remove(subReq);
                    if (!povDirectionMonitors[subReq.InputSubId].HasSubscriptions())
                    {
                        povDirectionMonitors.Remove(subReq.InputSubId);
                    }
                    return ret;
                }
            }
            return false;
        }

        public bool HasSubscriptions()
        {
            if (subscriptions.Count > 0)
            {
                return true;
            }
            foreach (var povDirection in povDirectionMonitors)
            {
                if (povDirection.Value.HasSubscriptions())
                {
                    return true;
                }
            }
            return false;
        }

        public void ProcessPollResult(int value)
        {
            foreach (var subscription in subscriptions)
            {
                subscription.Value(value);
            }

            foreach (var monitor in povDirectionMonitors)
            {
                monitor.Value.ProcessPollResult(value);
            }
        }
    }
    #endregion

    #region POV Direction
    class PovDirectionMonitor
    {
        Dictionary<string, dynamic> subscriptions = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
        private bool state = false;
        private int angle;
        private int direction;
        public int tolerance = 4500;

        public PovDirectionMonitor(SubscriptionRequest subReq)
        {
            direction = subReq.InputSubId;
            angle = (direction - 1) * 9000;
        }

        public bool Add(SubscriptionRequest subReq)
        {
            subscriptions.Add(subReq.SubscriberId, subReq.Handler);
            return true;
        }

        public bool Remove(SubscriptionRequest subReq)
        {
            if (subscriptions.ContainsKey(subReq.SubscriberId))
            {
                subscriptions.Remove(subReq.SubscriberId);
                return true;
            }
            return false;
        }

        public bool HasSubscriptions()
        {
            return subscriptions.Count > 0;
        }

        public void ProcessPollResult(int value)
        {
            bool newState = ValueMatchesAngle(value);
            if (newState != state)
            {
                state = newState;
                var ret = Convert.ToInt32(state);
                foreach (var subscription in subscriptions)
                {
                    subscription.Value(ret);
                }
            }
        }

        private bool ValueMatchesAngle(int value)
        {
            if (value == -1)
                return false;
            var diff = AngleDiff(value, angle);
            return value != -1 && AngleDiff(value, angle) <= tolerance;
        }

        private int AngleDiff(int a, int b)
        {
            var result1 = a - b;
            if (result1 < 0)
                result1 += 36000;

            var result2 = b - a;
            if (result2 < 0)
                result2 += 36000;

            return Math.Min(result1, result2);
        }
    }
    #endregion

    #endregion

    #region Lookup Tables
    // Maps SharpDX "Offsets" (Input Identifiers) to both iinput type and input index (eg x axis to axis 1)
    private static Dictionary<InputType, List<JoystickOffset>> directInputMappings = new Dictionary<InputType, List<JoystickOffset>>(){
                {
                    InputType.AXIS, new List<JoystickOffset>()
                    {
                        JoystickOffset.X,
                        JoystickOffset.Y,
                        JoystickOffset.Z,
                        JoystickOffset.RotationX,
                        JoystickOffset.RotationY,
                        JoystickOffset.RotationZ,
                        JoystickOffset.Sliders0,
                        JoystickOffset.Sliders1
                    }
                },
                {
                    InputType.BUTTON, new List<JoystickOffset>()
                    {
                        JoystickOffset.Buttons0, JoystickOffset.Buttons1, JoystickOffset.Buttons2, JoystickOffset.Buttons3, JoystickOffset.Buttons4,
                        JoystickOffset.Buttons5, JoystickOffset.Buttons6, JoystickOffset.Buttons7, JoystickOffset.Buttons8, JoystickOffset.Buttons9, JoystickOffset.Buttons10,
                        JoystickOffset.Buttons11, JoystickOffset.Buttons12, JoystickOffset.Buttons13, JoystickOffset.Buttons14, JoystickOffset.Buttons15, JoystickOffset.Buttons16,
                        JoystickOffset.Buttons17, JoystickOffset.Buttons18, JoystickOffset.Buttons19, JoystickOffset.Buttons20, JoystickOffset.Buttons21, JoystickOffset.Buttons22,
                        JoystickOffset.Buttons23, JoystickOffset.Buttons24, JoystickOffset.Buttons25, JoystickOffset.Buttons26, JoystickOffset.Buttons27, JoystickOffset.Buttons28,
                        JoystickOffset.Buttons29, JoystickOffset.Buttons30, JoystickOffset.Buttons31, JoystickOffset.Buttons32, JoystickOffset.Buttons33, JoystickOffset.Buttons34,
                        JoystickOffset.Buttons35, JoystickOffset.Buttons36, JoystickOffset.Buttons37, JoystickOffset.Buttons38, JoystickOffset.Buttons39, JoystickOffset.Buttons40,
                        JoystickOffset.Buttons41, JoystickOffset.Buttons42, JoystickOffset.Buttons43, JoystickOffset.Buttons44, JoystickOffset.Buttons45, JoystickOffset.Buttons46,
                        JoystickOffset.Buttons47, JoystickOffset.Buttons48, JoystickOffset.Buttons49, JoystickOffset.Buttons50, JoystickOffset.Buttons51, JoystickOffset.Buttons52,
                        JoystickOffset.Buttons53, JoystickOffset.Buttons54, JoystickOffset.Buttons55, JoystickOffset.Buttons56, JoystickOffset.Buttons57, JoystickOffset.Buttons58,
                        JoystickOffset.Buttons59, JoystickOffset.Buttons60, JoystickOffset.Buttons61, JoystickOffset.Buttons62, JoystickOffset.Buttons63, JoystickOffset.Buttons64,
                        JoystickOffset.Buttons65, JoystickOffset.Buttons66, JoystickOffset.Buttons67, JoystickOffset.Buttons68, JoystickOffset.Buttons69, JoystickOffset.Buttons70,
                        JoystickOffset.Buttons71, JoystickOffset.Buttons72, JoystickOffset.Buttons73, JoystickOffset.Buttons74, JoystickOffset.Buttons75, JoystickOffset.Buttons76,
                        JoystickOffset.Buttons77, JoystickOffset.Buttons78, JoystickOffset.Buttons79, JoystickOffset.Buttons80, JoystickOffset.Buttons81, JoystickOffset.Buttons82,
                        JoystickOffset.Buttons83, JoystickOffset.Buttons84, JoystickOffset.Buttons85, JoystickOffset.Buttons86, JoystickOffset.Buttons87, JoystickOffset.Buttons88,
                        JoystickOffset.Buttons89, JoystickOffset.Buttons90, JoystickOffset.Buttons91, JoystickOffset.Buttons92, JoystickOffset.Buttons93, JoystickOffset.Buttons94,
                        JoystickOffset.Buttons95, JoystickOffset.Buttons96, JoystickOffset.Buttons97, JoystickOffset.Buttons98, JoystickOffset.Buttons99, JoystickOffset.Buttons100,
                        JoystickOffset.Buttons101, JoystickOffset.Buttons102, JoystickOffset.Buttons103, JoystickOffset.Buttons104, JoystickOffset.Buttons105, JoystickOffset.Buttons106,
                        JoystickOffset.Buttons107, JoystickOffset.Buttons108, JoystickOffset.Buttons109, JoystickOffset.Buttons110, JoystickOffset.Buttons111, JoystickOffset.Buttons112,
                        JoystickOffset.Buttons113, JoystickOffset.Buttons114, JoystickOffset.Buttons115, JoystickOffset.Buttons116, JoystickOffset.Buttons117, JoystickOffset.Buttons118,
                        JoystickOffset.Buttons119, JoystickOffset.Buttons120, JoystickOffset.Buttons121, JoystickOffset.Buttons122, JoystickOffset.Buttons123, JoystickOffset.Buttons124,
                        JoystickOffset.Buttons125, JoystickOffset.Buttons126, JoystickOffset.Buttons127
                    }
                },
                {
                    InputType.POV, new List<JoystickOffset>()
                    {
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers3
                    }
                }
            };
    #endregion

}
