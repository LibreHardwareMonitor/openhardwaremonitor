﻿using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace LibreHardwareMonitor.UI
{
    public partial class AccessControlForm : Form
    {
        private readonly MainForm _parent;

        public AccessControlForm(MainForm m)
        {
            InitializeComponent();
            _parent = m;
        }

        private void AccessControl_Load(object sender, EventArgs e)
        {
            accessControlInput.Text = _parent.Server.AccessOrigin;
        }

        private void AccessControlOKButton_Click(object sender, EventArgs e)
        {
            _parent.Server.AccessOrigin = accessControlInput.Text;
            Close();
        }

        private void AccessControlCancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Origin"));
            }
            catch { }
        }
    }
}
