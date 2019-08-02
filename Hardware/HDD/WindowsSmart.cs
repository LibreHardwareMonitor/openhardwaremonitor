﻿// Mozilla Public License 2.0
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors
// All Rights Reserved

using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenHardwareMonitor.Interop;

namespace OpenHardwareMonitor.Hardware.HDD {

  internal class WindowsSmart : ISmart {

    private SafeHandle handle = null;
    private int driveNumber;

    public bool IsValid {
      get { return !handle.IsInvalid; }
    }

    public WindowsSmart(int driveNumber) {
      this.driveNumber = driveNumber;
      handle = Kernel32.CreateFile(@"\\.\PhysicalDrive" + driveNumber,
        FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
    }

    protected void Dispose(bool disposing) {
      if (disposing) {
        if (!handle.IsClosed)
          handle.Close();
      }
    }

    public void Dispose() {
      Close();
    }

    public void Close() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public bool EnableSmart() {
      if (handle.IsClosed)
        throw new ObjectDisposedException("WindowsATASmart");

      Kernel32.DriveCommandParameter parameter = new Kernel32.DriveCommandParameter();
      Kernel32.DriveCommandResult result;
      uint bytesReturned;

      parameter.DriveNumber = (byte)driveNumber;
      parameter.Registers.Features = Kernel32.RegisterFeature.SmartEnableOperations;
      parameter.Registers.LBAMid = Kernel32.SMART_LBA_MID;
      parameter.Registers.LBAHigh = Kernel32.SMART_LBA_HI;
      parameter.Registers.Command = Kernel32.RegisterCommand.SmartCmd;

      return Kernel32.DeviceIoControl(
        handle,
        Kernel32.DriveCommand.SendDriveCommand,
        ref parameter, Marshal.SizeOf(parameter), out result,
        Marshal.SizeOf<Kernel32.DriveCommandResult>(), out bytesReturned, IntPtr.Zero);
    }

    public Kernel32.DriveAttributeValue[] ReadSmartData() {
      if (handle.IsClosed)
        throw new ObjectDisposedException("WindowsATASmart");

      Kernel32.DriveCommandParameter parameter = new Kernel32.DriveCommandParameter();
      Kernel32.DriveSmartReadDataResult result;
      uint bytesReturned;

      parameter.DriveNumber = (byte)driveNumber;
      parameter.Registers.Features = Kernel32.RegisterFeature.SmartReadData;
      parameter.Registers.LBAMid = Kernel32.SMART_LBA_MID;
      parameter.Registers.LBAHigh = Kernel32.SMART_LBA_HI;
      parameter.Registers.Command = Kernel32.RegisterCommand.SmartCmd;

      bool isValid = Kernel32.DeviceIoControl(handle,
        Kernel32.DriveCommand.ReceiveDriveData, ref parameter, Marshal.SizeOf(parameter),
        out result, Marshal.SizeOf<Kernel32.DriveSmartReadDataResult>(),
        out bytesReturned, IntPtr.Zero);

      return (isValid) ? result.Attributes : new Kernel32.DriveAttributeValue[0];
    }

    public Kernel32.DriveThresholdValue[] ReadSmartThresholds() {
      if (handle.IsClosed)
        throw new ObjectDisposedException("WindowsATASmart");

      Kernel32.DriveCommandParameter parameter = new Kernel32.DriveCommandParameter();
      Kernel32.DriveSmartReadThresholdsResult result;
      uint bytesReturned = 0;

      parameter.DriveNumber = (byte)driveNumber;
      parameter.Registers.Features = Kernel32.RegisterFeature.SmartReadThresholds;
      parameter.Registers.LBAMid = Kernel32.SMART_LBA_MID;
      parameter.Registers.LBAHigh = Kernel32.SMART_LBA_HI;
      parameter.Registers.Command = Kernel32.RegisterCommand.SmartCmd;

      bool isValid = Kernel32.DeviceIoControl(handle,
        Kernel32.DriveCommand.ReceiveDriveData, ref parameter, Marshal.SizeOf(parameter),
        out result, Marshal.SizeOf<Kernel32.DriveSmartReadThresholdsResult>(),
        out bytesReturned, IntPtr.Zero);

      return (isValid) ? result.Thresholds : new Kernel32.DriveThresholdValue[0];
    }

    private string GetString(byte[] bytes) {
      char[] chars = new char[bytes.Length];
      for (int i = 0; i < bytes.Length; i += 2) {
        chars[i] = (char)bytes[i + 1];
        chars[i + 1] = (char)bytes[i];
      }
      return new string(chars).Trim(new char[] { ' ', '\0' });
    }

    public bool ReadNameAndFirmwareRevision(out string name, out string firmwareRevision) {
      if (handle.IsClosed)
        throw new ObjectDisposedException("WindowsATASmart");

      Kernel32.DriveCommandParameter parameter = new Kernel32.DriveCommandParameter();
      Kernel32.DriveIdentifyResult result;
      uint bytesReturned;

      parameter.DriveNumber = (byte)driveNumber;
      parameter.Registers.Command = Kernel32.RegisterCommand.IdCmd;

      bool valid = Kernel32.DeviceIoControl(handle,
        Kernel32.DriveCommand.ReceiveDriveData, ref parameter, Marshal.SizeOf(parameter),
        out result, Marshal.SizeOf<Kernel32.DriveIdentifyResult>(),
        out bytesReturned, IntPtr.Zero);

      if (!valid) {
        name = null;
        firmwareRevision = null;
        return false;
      }

      name = GetString(result.Identify.ModelNumber);
      firmwareRevision = GetString(result.Identify.FirmwareRevision);
      return true;
    }
  }
}