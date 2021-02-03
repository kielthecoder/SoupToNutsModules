using System;
using Crestron.SimplSharp;

namespace CiscoRoomKit
{
    public class Codec : Device
    {
        public event EventHandler<DataEventArgs> OnResponse;

        public string VideoNumber { get; set; }

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

        }

        public void Dial()
        {
            if (VideoNumber.Length > 0)
            {
                var cmd = String.Format("xcommand Dial Number: \"{0}\" CallType: Auto", VideoNumber);
                
                SendCommand(cmd);
            }
        }

        public void HangUp()
        {
            SendCommand("xcommand Call Disconnect");
        }
    }
}