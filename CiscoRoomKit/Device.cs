using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.Ssh.Common;

namespace CiscoRoomKit
{
    public class Device
    {
        private SshClient _ssh;

        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;

        public Device()
        {
        }

        public void Connect()
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

            if (OnConnect != null)
            {
                OnConnect(this, new EventArgs());
            }
        }

        public void Disconnect()
        {
            if (_ssh != null)
            {
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
    }
}