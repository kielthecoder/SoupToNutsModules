using System;
using Crestron.SimplSharp;

namespace CiscoRoomKit
{
    public class Codec : Device
    {
        public event EventHandler<DataEventArgs> OnResponse;

        public string Standby { get; private set; }

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

            RegisterControlSystem();
            
            SendCommand("xfeedback register /Status/Standby");
        }

        private void RegisterControlSystem()
        {
            SendCommand("xconfiguration Peripherals Profile ControlSystems: 1");

            var name = "Crestron Control System";
            var adapter = CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);
            var id = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, adapter);
            var cmd = String.Format("xcommand Peripherals Connect ID: \"{0}\" Name: \"{1}\" Type: ControlSystem", id, name);

            CrestronConsole.PrintLine(cmd);
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
            var msg = args.Message;

            if (msg.StartsWith("*s "))
            {
                HandleStatusResponse(msg);
            }
        }

        private void HandleStatusResponse(string msg)
        {

        }
    }
}