using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.Ssh.Common;

namespace CiscoRoomKit
{
    public class DataEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class Device
    {
        private SshClient _ssh;
        private ShellStream _stream;

        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;
        public event EventHandler<DataEventArgs> OnDataReceived;
        public event EventHandler<DataEventArgs> OnError;

        public Device()
        {
        }

        public void Connect()
        {
            try
            {
                // Handle interactive authentication (typing in password)
                var auth = new KeyboardInteractiveAuthenticationMethod(User);
                auth.AuthenticationPrompt += HandleAuthentication;

                // Create connection info
                var conn = new ConnectionInfo(Host, User, auth);

                // Connect SSH session
                _ssh = new SshClient(conn);
                _ssh.ErrorOccurred += HandleError;
                _ssh.Connect();

                // Create stream
                _stream = _ssh.CreateShellStream("Terminal", 80, 24,
                    800, 600, 1024);
                _stream.DataReceived += HandleDataReceived;
                _stream.ErrorOccurred += HandleStreamError;

                if (OnConnect != null)
                {
                    OnConnect(this, new EventArgs());
                }
            }
            catch (SshConnectionException e)
            {
                if (OnError != null)
                {
                    OnError(this, new DataEventArgs() { Message = e.Message });
                }

                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (_ssh != null)
            {
                if (_stream != null)
                {
                    // Close and dispose the I/O stream
                    _stream.Close();
                    _stream.Dispose();
                }

                // Disconnect and dispose of SSH session
                _ssh.Disconnect();
                _ssh.Dispose();
                _ssh = null;

                if (OnDisconnect != null)
                {
                    OnDisconnect(this, new EventArgs());
                }
            }
        }

        public void SendCommand(string cmd)
        {
            // Make sure we can actually send data
            if (_ssh.IsConnected && _stream.CanWrite)
            {
                _stream.WriteLine(cmd);
            }
        }

        private void HandleAuthentication(object sender,
            AuthenticationPromptEventArgs args)
        {
            foreach (var prompt in args.Prompts)
            {
                // Look for password prompt and respond
                if (prompt.Request.Contains("Password:"))
                {
                    prompt.Response = Password;
                }
            }
        }

        private void HandleError(object sender,
            ExceptionEventArgs args)
        {
            if (!_ssh.IsConnected)
            {
                if (OnDisconnect != null)
                {
                    OnDisconnect(this, new EventArgs());
                }
            }
        }

        private void HandleDataReceived(object sender,
            ShellDataEventArgs args)
        {
            var stream = (ShellStream)sender;
            string data = "";

            // Gather all the available data
            while (stream.DataAvailable)
            {
                data += stream.Read();
            }

            if (data != "")
            {
                if (OnDataReceived != null)
                {
                    OnDataReceived(this, new DataEventArgs() { Message = data });
                }
            }
        }

        private void HandleStreamError(object sender,
            EventArgs args)
        {
            Disconnect();
        }
    }
}