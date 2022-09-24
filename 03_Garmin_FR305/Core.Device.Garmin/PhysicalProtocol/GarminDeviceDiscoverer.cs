using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using SC._03_Garmin_FR305.Core.Device.Garmin.Interfaces;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol
{
    public partial class GarminDeviceDiscoverer : IGarminDeviceDiscoverer
    {
        #region Private Fields

        private static List<IGarminDevice> devices = new List<IGarminDevice>();
        private static Guid garminGuid = new Guid("{2C9C45C2-8E7D-4C08-A12D-816BBAE722C0}");
        private IntPtr handleDeviceInfo = IntPtr.Zero;

        #endregion

        #region Private Methods

        /// <summary>
        /// Function returns a handle to a file created on the device
        /// </summary>
        /// <param name="devicePath"></param>
        /// <returns></returns>
        private SafeFileHandle GetHandleFromDevicePath(string devicePath)
        {
            //TODO: Need to remove devicePathHandle, devicePathHandleAddress as it is not used
            GCHandle devicePathHandle = GCHandle.Alloc(devicePath, GCHandleType.Pinned);
            IntPtr devicePathHandleAddress = devicePathHandle.AddrOfPinnedObject();

            try
            {
                return UnsafeNativeMethods.CreateFile(
                                    devicePath,
                                    FileAccess.ReadWrite,
                                    FileShare.None,
                                    IntPtr.Zero,
                                    FileMode.Open,
                                    FileAttributes.Normal,
                                    IntPtr.Zero);
            }
            finally
            {
                devicePathHandle.Free();
            }
        }

        //TODO: this method should actually be in the GarminDevice class. Also requiredSize should be a private variable
        private int GetUSBPacketSize(SafeHandle garminDeviceHandle, ref UInt32 requiredSize)
        {
            uint IOCTL_USB_PACKET_SIZE = Utilities.CTL_CODE(
                                UnsafeNativeMethods.FILE_DEVICE_UNKNOWN,
                                0x851,
                                UnsafeNativeMethods.METHOD_BUFFERED,
                                UnsafeNativeMethods.FILE_ANY_ACCESS);

            GCHandle USBPacketSizeHandle = GCHandle.Alloc(new int(), GCHandleType.Pinned);
            IntPtr USBPacketSizeHandleAddress = USBPacketSizeHandle.AddrOfPinnedObject();

            if (UnsafeNativeMethods.DeviceIoControl(
                    garminDeviceHandle,
                    IOCTL_USB_PACKET_SIZE,
                    IntPtr.Zero,
                    0,
                    USBPacketSizeHandleAddress,
                    (uint)Marshal.SizeOf(USBPacketSizeHandleAddress),
                    ref requiredSize,
                    IntPtr.Zero))
            {
                return Marshal.ReadInt32(USBPacketSizeHandleAddress);
            }
            else
            {
                return (int)GarminUSBConstants.ASYNC_DATA_SIZE;
            }
        }

        #endregion

        #region Public Properties

        public List<IGarminDevice> Devices
        {
            get { return devices; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Function sets the handle to the device information set.
        /// </summary>
        public void Initialize()
        {
            bool initSuccess = false;

            OnStartInitialize(EventArgs.Empty);

            try
            {
                ///Get device information set that contains all devices of a specified class.
                handleDeviceInfo = UnsafeNativeMethods.SetupDiGetClassDevs(
                                                ref garminGuid,
                                                IntPtr.Zero,
                                                IntPtr.Zero,
                                                UnsafeNativeMethods.DiGetClassFlags.DIGCF_PRESENT |
                                                UnsafeNativeMethods.DiGetClassFlags.DIGCF_DEVICEINTERFACE);

                if (handleDeviceInfo.ToInt32() == UnsafeNativeMethods.INVALID_HANDLE_VALUE)
                {
                    initSuccess = false;
                    //TODO: should return an exception stating why we exiting the application
                    UnsafeNativeMethods.SetupDiDestroyDeviceInfoList(handleDeviceInfo);
                }
                else
                {
                    initSuccess = true;
                }
            }
            finally
            {
                OnFinishInitialize(new FinishInitializeEventArgs(initSuccess));
            }
        }

        public void FindDevices()
        {
            bool Success = true;
            uint memberIndex = 0;

            devices.Clear();

            OnStartFindDevices(EventArgs.Empty);

            try
            {
                while (Success)
                {
                    // create a Device Interface Data structure
                    UnsafeNativeMethods.SP_DEVICE_INTERFACE_DATA deviceInterfaceData =
                                         new UnsafeNativeMethods.SP_DEVICE_INTERFACE_DATA();
                    deviceInterfaceData.cbSize = (UInt32)Marshal.SizeOf(deviceInterfaceData);


                    // start the enumeration of all devices
                    Success = UnsafeNativeMethods.SetupDiEnumDeviceInterfaces(
                                                    handleDeviceInfo,
                                                    IntPtr.Zero,
                                                    ref garminGuid,
                                                    memberIndex,
                                                    ref deviceInterfaceData);

                    if (Marshal.GetLastWin32Error() == UnsafeNativeMethods.ERROR_NO_MORE_ITEMS)
                    {
                        Success = false;
                        break;
                    }

                    if (!Success)
                    {
                        break;
                    }

                    UnsafeNativeMethods.SP_DEVINFO_DATA deviceInfoData = new UnsafeNativeMethods.SP_DEVINFO_DATA();
                    deviceInfoData.cbSize = (UInt32)Marshal.SizeOf(deviceInfoData);

                    UnsafeNativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData =
                                                        new UnsafeNativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA();
                    if (IntPtr.Size == 8) // for 64 bit operating systems
                    {
                        deviceInterfaceDetailData.cbSize = 8;
                    }
                    else
                    {
                        deviceInterfaceDetailData.cbSize = 4 + (UInt32)Marshal.SystemDefaultCharSize; //i.e 5
                    }

                    // now we can get some more detailed information
                    UInt32 nRequiredSize = 0;
                    UInt32 nBytes = UnsafeNativeMethods.BUFFER_SIZE;

                    if (!UnsafeNativeMethods.SetupDiGetDeviceInterfaceDetail(
                                        handleDeviceInfo,
                                        ref deviceInterfaceData,
                                        ref deviceInterfaceDetailData,
                                        nBytes,
                                        out nRequiredSize,
                                        ref deviceInfoData))
                    {
                        //TODO: Log the windows error?
                        //Marshal.GetLastWin32Error()
                    }
                    else
                    {
                        IGarminDevice garminDevice = new GarminDevice();

                        // devices.Add(new GarminDevice());
                        // garminDevice.StartSession();

                        //TODO: Need to make sure that the devicepath.length != 0
                        garminDevice.Handle = GetHandleFromDevicePath(deviceInterfaceDetailData.DevicePath);
                        garminDevice.PacketSize = GetUSBPacketSize(garminDevice.Handle, ref nRequiredSize);
                        // garminDevice.StartSession();

                        devices.Add(garminDevice);
                    }

                    memberIndex++;
                }
            }
            finally
            {
                OnFinishFindDevices(new FinishFindDevicesEventArgs(devices.Count));
            }
        }

        public void FinalizeAll()
        {
            foreach (GarminDevice garminDevice in devices)
            {
                garminDevice.Dispose();
            }
        }

        #endregion
    }
}
