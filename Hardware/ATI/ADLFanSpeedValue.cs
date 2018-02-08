﻿/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Runtime.InteropServices;

namespace OpenHardwareMonitor.Hardware.ATI
{
    [StructLayout(LayoutKind.Sequential)]
  public struct ADLFanSpeedValue {
    public int Size;
    public int SpeedType;
    public int FanSpeed;
    public int Flags;
  }
}
