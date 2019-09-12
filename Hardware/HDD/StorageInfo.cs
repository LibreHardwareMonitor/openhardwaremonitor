﻿// Mozilla Public License 2.0
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors
// All Rights Reserved

using OpenHardwareMonitor.Interop;

namespace OpenHardwareMonitor.Hardware.HDD {
  internal abstract class StorageInfo {
    public Kernel32.STORAGE_BUS_TYPE BusType { get; protected set; }

    public string DeviceId { get; set; }

    public ulong DiskSize { get; set; }

    public int Index { get; protected set; }

    public string Name => (Vendor + " " + Product).Trim();

    public string Product { get; protected set; }

    public byte[] RawData { get; protected set; }

    public bool Removable { get; protected set; }

    public string Revision { get; protected set; }

    public string Scsi { get; set; }

    public string Serial { get; protected set; }

    public string Vendor { get; protected set; }
  }
}