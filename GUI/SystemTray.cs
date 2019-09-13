﻿// Mozilla Public License 2.0
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors
// All Rights Reserved

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI
{
    public class SystemTray : IDisposable
    {
        private IComputer _computer;
        private PersistentSettings _settings;
        private UnitManager _unitManager;
        private List<SensorNotifyIcon> _sensorList = new List<SensorNotifyIcon>();
        private bool _mainIconEnabled = false;
        private NotifyIconAdv _mainIcon;

        public SystemTray(IComputer computer, PersistentSettings settings, UnitManager unitManager)
        {
            _computer = computer;
            _settings = settings;
            _unitManager = unitManager;
            computer.HardwareAdded += new HardwareEventHandler(HardwareAdded);
            computer.HardwareRemoved += new HardwareEventHandler(HardwareRemoved);

            _mainIcon = new NotifyIconAdv();

            ContextMenu contextMenu = new ContextMenu();
            MenuItem hideShowItem = new MenuItem("Hide/Show");
            hideShowItem.Click += delegate (object obj, EventArgs args)
            {
                SendHideShowCommand();
            };
            contextMenu.MenuItems.Add(hideShowItem);
            contextMenu.MenuItems.Add(new MenuItem("-"));
            MenuItem exitItem = new MenuItem("Exit");
            exitItem.Click += delegate (object obj, EventArgs args)
            {
                SendExitCommand();
            };
            contextMenu.MenuItems.Add(exitItem);
            _mainIcon.ContextMenu = contextMenu;
            _mainIcon.DoubleClick += delegate (object obj, EventArgs args)
            {
                SendHideShowCommand();
            };
            _mainIcon.Icon = EmbeddedResources.GetIcon("smallicon.ico");
            _mainIcon.Text = "Open Hardware Monitor";
        }

        private void HardwareRemoved(IHardware hardware)
        {
            hardware.SensorAdded -= new SensorEventHandler(SensorAdded);
            hardware.SensorRemoved -= new SensorEventHandler(SensorRemoved);
            foreach (ISensor sensor in hardware.Sensors)
                SensorRemoved(sensor);
            foreach (IHardware subHardware in hardware.SubHardware)
                HardwareRemoved(subHardware);
        }

        private void HardwareAdded(IHardware hardware)
        {
            foreach (ISensor sensor in hardware.Sensors)
                SensorAdded(sensor);
            hardware.SensorAdded += new SensorEventHandler(SensorAdded);
            hardware.SensorRemoved += new SensorEventHandler(SensorRemoved);
            foreach (IHardware subHardware in hardware.SubHardware)
                HardwareAdded(subHardware);
        }

        private void SensorAdded(ISensor sensor)
        {
            if (_settings.GetValue(new Identifier(sensor.Identifier,
              "tray").ToString(), false))
                Add(sensor, false);
        }

        private void SensorRemoved(ISensor sensor)
        {
            if (Contains(sensor))
                Remove(sensor, false);
        }

        public void Dispose()
        {
            foreach (SensorNotifyIcon icon in _sensorList)
                icon.Dispose();
            _mainIcon.Dispose();
        }

        public void Redraw()
        {
            foreach (SensorNotifyIcon icon in _sensorList)
                icon.Update();
        }

        public bool Contains(ISensor sensor)
        {
            foreach (SensorNotifyIcon icon in _sensorList)
                if (icon.Sensor == sensor)
                    return true;
            return false;
        }

        public void Add(ISensor sensor, bool balloonTip)
        {
            if (Contains(sensor))
                return;
            else
            {
                _sensorList.Add(new SensorNotifyIcon(this, sensor, balloonTip, _settings, _unitManager));
                UpdateMainIconVisibilty();
                _settings.SetValue(new Identifier(sensor.Identifier, "tray").ToString(), true);
            }
        }

        public void Remove(ISensor sensor)
        {
            Remove(sensor, true);
        }

        private void Remove(ISensor sensor, bool deleteConfig)
        {
            if (deleteConfig)
            {
                _settings.Remove(new Identifier(sensor.Identifier, "tray").ToString());
                _settings.Remove(new Identifier(sensor.Identifier, "traycolor").ToString());
            }
            SensorNotifyIcon instance = null;
            foreach (SensorNotifyIcon icon in _sensorList)
            {
                if (icon.Sensor == sensor)
                    instance = icon;
            }
            if (instance != null)
            {
                _sensorList.Remove(instance);
                UpdateMainIconVisibilty();
                instance.Dispose();
            }
        }

        public event EventHandler HideShowCommand;

        public void SendHideShowCommand()
        {
            if (HideShowCommand != null)
                HideShowCommand(this, null);
        }

        public event EventHandler ExitCommand;

        public void SendExitCommand()
        {
            if (ExitCommand != null)
                ExitCommand(this, null);
        }

        private void UpdateMainIconVisibilty()
        {
            if (_mainIconEnabled)
                _mainIcon.Visible = _sensorList.Count == 0;
            else
                _mainIcon.Visible = false;
        }

        public bool IsMainIconEnabled
        {
            get { return _mainIconEnabled; }
            set
            {
                if (_mainIconEnabled != value)
                {
                    _mainIconEnabled = value;
                    UpdateMainIconVisibilty();
                }
            }
        }
    }
}
