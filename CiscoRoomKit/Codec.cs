using System;
using Crestron.SimplSharp;

namespace CiscoRoomKit
{
    public class Codec : Device
    {
        public event EventHandler<DataEventArgs> OnResponse;
        public event EventHandler<DataEventArgs> OnCallStatus;

        public string VideoNumber { get; set; }
        public string CallStatus { get; private set; }
        public ushort CallConnected { get; private set; }

        public Codec()
        {
            OnConnect += RegisterEvents;
            OnDataReceived += HandleDataReceived;
            OnResponse += HandleResponse;
        }

        private void RegisterEvents(object sender, EventArgs args)
        {
            SendCommand("");
            SendCommand("echo off");
            SendCommand("xfeedback deregisterall");
            SendCommand("xpreferences outputmode terminal");
            SendCommand("xfeedback register /Status/Call");
        }

        private void HandleDataReceived(object sender, DataEventArgs args)
        {
            foreach (var text in args.Message.Split('\r'))
            {
                var clean_text = text.Trim();

                if (clean_text.Length > 0)
                {
                    if (OnResponse != null)
                    {
                        OnResponse(this, new DataEventArgs { Message = clean_text });
                    }
                }
            }
        }

        private void HandleResponse(object sender, DataEventArgs args)
        {
            if (args.Message.StartsWith("*s Call "))
            {
                var status = args.Message.Remove(0, 8);
                HandleCallStatus(status);
            }
        }

        private void HandleCallStatus(string msg)
        {
            if (msg.Contains("Status"))
            {
                CallStatus = msg.Remove(0, msg.LastIndexOf(':') + 1).Trim();

                if (CallStatus == "Connected")
                    CallConnected = 1;
                else
                    CallConnected = 0;

                if (OnCallStatus != null)
                {
                    OnCallStatus(this, new DataEventArgs { Message = CallStatus });
                }
            }
            else if (msg.Contains("(ghost=True)"))
            {
                CallStatus = "";
                CallConnected = 0;

                if (OnCallStatus != null)
                {
                    OnCallStatus(this, new DataEventArgs { Message = CallStatus });
                }
            }
        }

        public void Dial()
        {
            if (VideoNumber.Length > 0)
            {
                var cmd = String.Format("xcommand Dial Number: {0}", VideoNumber);
                
                SendCommand(cmd);
            }
        }

        public void HangUp()
        {
            SendCommand("xcommand Call Disconnect");
        }
    }
}