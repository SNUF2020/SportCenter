using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A000.Datatypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A001;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A001.Enumerations;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A010.Enumerations;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1000.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1000.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1002.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1002.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1003.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1004.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1005.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1009.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.Types;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A600.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.LinkProtocol.Enumerations;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Enumerations;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Exceptions;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol
{
    public class GarminDevice : IGarminDevice
    {
        #region Private ReadOnly Fields

        private readonly ResourceManager resourceManager;
        private readonly uint IOCTL_ASYNC_IN = Utilities.CTL_CODE(UnsafeNativeMethods.FILE_DEVICE_UNKNOWN,
                                        0x850,
                                        UnsafeNativeMethods.METHOD_BUFFERED,
                                        UnsafeNativeMethods.FILE_ANY_ACCESS);

        #endregion

        #region Private Fields

        private bool isDisposed = false;

        private string deviceInterfacePath;
        private SafeFileHandle handle;
        private int packetSize;
        private uint deviceUnitId;
        private bool hasSessionStarted = false;

        private Product_Data_Type productDataType;
        private List<Protocol_Data_Type> protocolDataTypes;
        private List<Ext_Product_Data_Type> extProductDataTypes;

        #endregion

        #region Public Constructors

        public GarminDevice()
        {
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            resourceManager = new ResourceManager("OSMS.Core.Device.Garmin.Language.OSMS.Core.Device.Garmin",
                Assembly.GetExecutingAssembly());
        }

        #endregion

        #region Public Properties

        public string DeviceInterfacePath
        {
            set { deviceInterfacePath = value; }
        }

        /// <summary>
        /// Contains a handle to the file created on the device
        /// </summary>
        public SafeFileHandle Handle
        {
            get
            {
                if ((handle == null) || ((handle.DangerousGetHandle().ToInt32() == 0) && (packetSize == 0)))
                {
                    throw new GarminDeviceNotFoundException(resourceManager.GetString("Ex.GarminDeviceCannotBeFound"));
                }
                return handle;
            }
            set { handle = value; }
        }

        public int PacketSize
        {
            get { return packetSize; }
            set { packetSize = value; }
        }

        /// <summary>
        /// Property indicates that transfers can take place to and from the device
        /// </summary>
        public bool HasSessionStarted
        {
            get { return hasSessionStarted; }
        }

        public uint DeviceUnitId
        {
            get { return deviceUnitId; }
        }

        public Product_Data_Type ProductDataType
        {
            get { return productDataType; }
        }

        public List<Protocol_Data_Type> ProtocolDataTypes
        {
            get { return protocolDataTypes; }
        }

        #endregion

        #region Private Methods
        /*
        private void GetHandleFromDevicePath()
        {
            //TODO: Need to make sure that the devicepath.length != 0
            GCHandle devicePathHandle = GCHandle.Alloc(deviceInterfacePath, GCHandleType.Pinned);
            IntPtr devicePathHandleAddress = devicePathHandle.AddrOfPinnedObject();

            Handle = UnsafeNativeMethods.CreateFile(
                                deviceInterfacePath,
                                FileAccess.ReadWrite,
                                FileShare.None,
                                IntPtr.Zero,
                                FileMode.Open,
                                FileAttributes.Normal,
                                IntPtr.Zero);

            //TODO: Need to check that Handle != -1
        }
        */

        /// <summary>
        /// Function checks if the device supports a protocol
        /// </summary>
        /// <param name="protocolDataType">A protocol data type</param>
        /// <returns>True if the protocol is supported else false</returns>
        private bool IsProtocolSupported(Protocol_Data_Type protocolDataType)
        {
            return protocolDataTypes.Exists(delegate (Protocol_Data_Type p)
            {
                if ((p.tag == protocolDataType.tag) && (p.data == protocolDataType.data))
                {
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Function checks if the device supports a protocol
        /// </summary>
        /// <param name="tag">Protocol tag value</param>
        /// <param name="id">Protocol id</param>
        /// <returns>True if the protocol is supported else false</returns>
        private bool IsProtocolSupported(Protocol_Data_Type_Tag tag, UInt16 id)
        {
            Protocol_Data_Type p = new Protocol_Data_Type();
            p.tag = (byte)tag;
            p.data = id;

            return IsProtocolSupported(p);
            //return true;
        }

        private Product_Data_Type GetProductDataTypeFromArray(byte[] data)
        {
            USB_Packet usbPacket = Utilities.CreateUSBPacketFromByteArray(data);
            Product_Data_Type productDataType = new Product_Data_Type();

            productDataType.product_ID = BitConverter.ToUInt16(usbPacket.data, 0);
            productDataType.software_version = BitConverter.ToInt16(usbPacket.data, 2);

            const int noOfBytesBeforeProductDescription = 4;
            productDataType.product_description = new char[usbPacket.dataSize - noOfBytesBeforeProductDescription];

            ASCIIEncoding asciiEncoding = new ASCIIEncoding();

            int noOfCharCopied = asciiEncoding.GetChars(
                                                usbPacket.data,
                                                noOfBytesBeforeProductDescription,
                                                usbPacket.dataSize - noOfBytesBeforeProductDescription,
                                                productDataType.product_description,
                                                0);

            string temp = new string(productDataType.product_description).Replace("\0", "");
            Array.Resize(ref productDataType.product_description, temp.Length);
            Array.Copy(temp.ToCharArray(), productDataType.product_description, temp.Length);

            return productDataType;
        }

        private Ext_Product_Data_Type GetExtProductDataType(byte[] data)
        {
            USB_Packet usbPacket = Utilities.CreateUSBPacketFromByteArray(data);
            Ext_Product_Data_Type extProductDataType = new Ext_Product_Data_Type();

            extProductDataType.value = new char[usbPacket.dataSize];

            ASCIIEncoding asciiEncoding = new ASCIIEncoding();

            int noOfCharCopied = asciiEncoding.GetChars(
                                                usbPacket.data,
                                                0,
                                                usbPacket.dataSize,
                                                extProductDataType.value,
                                                0);

            return extProductDataType;
        }

        private List<Protocol_Data_Type> GetProtocolDataTypes(byte[] data)
        {
            USB_Packet usbPacket = Utilities.CreateUSBPacketFromByteArray(data);
            List<Protocol_Data_Type> list = new List<Protocol_Data_Type>();
            int sizeOfProtocolDataObject = Marshal.SizeOf(new Protocol_Data_Type());    //Should be 3.
            byte[] protocolDataTypeAsArray = new byte[sizeOfProtocolDataObject];
            int offset = 0;

            while (offset != usbPacket.dataSize)
            {
                Buffer.BlockCopy(usbPacket.data, offset, protocolDataTypeAsArray, 0, sizeOfProtocolDataObject);

                Protocol_Data_Type protocolDataType = new Protocol_Data_Type();
                protocolDataType.tag = /*(char)*/protocolDataTypeAsArray[0];
                protocolDataType.data = BitConverter.ToUInt16(protocolDataTypeAsArray, 1);

                list.Add(protocolDataType);

                offset = offset + sizeOfProtocolDataObject;
            }

            return list;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Function attempts to write a usb packet to the file.
        /// </summary>
        /// <param name="usbPacket"></param>
        /// <exception cref="OSMS.Core.Device.Garmin.PhysicalProtocol.GarminUnableToWriteToDeviceException"></exception>
        /// <returns>True if write attempt was a success.</returns>
        internal bool GarminWrite(USB_Packet usbPacket)
        {
            /* TODO:
            if (usbPacket == null)
                throw new ArgumentNullException();
             */

            //TODO: Place this check ouside this method to make sure that multiple write calls are made if
            //data size is greater than MAX_BUFFER_SIZE

            /*
             * the size of all the data sent to the device is the size of the 
             * usb data structure(3.2.2) plus the size of all the data sent (index 12+ of the structure)
             */
            int theBytesToWrite = Marshal.SizeOf(usbPacket) + usbPacket.dataSize;
            int theBytesReturned = 0;

            // prepare and format the data to be sent
            byte[] dataAsArray = null;

            Utilities.CreateUSBPacketByteArray(usbPacket, ref dataAsArray);

            GCHandle gcDataHandle = GCHandle.Alloc(dataAsArray, GCHandleType.Pinned);
            IntPtr gcDataHandleAddress = gcDataHandle.AddrOfPinnedObject();

            // prepare a destination address for the data to be written
            GCHandle gcReturnDataHandle = GCHandle.Alloc(theBytesReturned, GCHandleType.Pinned);
            IntPtr gcReturnDataHandleAddress = gcReturnDataHandle.AddrOfPinnedObject();

            try
            {
                bool success = UnsafeNativeMethods.WriteFile(
                                    Handle,
                                    gcDataHandleAddress,
                                    theBytesToWrite,
                                    gcReturnDataHandleAddress,
                                    new Overlapped());

                // format the data written at the addresses in a .net readable structure
                int returnedBytes = (int)Marshal.PtrToStructure(gcReturnDataHandleAddress, typeof(Int32));

                if (returnedBytes != theBytesToWrite)
                {
                    throw new GarminUnableToWriteToDeviceException(resourceManager.GetString("Ex.UnableToWriteToDevice"));
                }

                // If the packet size was an exact multiple of the USB packet 
                // size, we must make a final write call with no data (3.2.3.1)
                if (theBytesToWrite % packetSize == 0)
                {
                    success = UnsafeNativeMethods.WriteFile(
                        Handle,
                        IntPtr.Zero,
                        0,
                        gcReturnDataHandleAddress,
                        new Overlapped());
                }

                return success;
            }
            finally
            {
                gcDataHandleAddress = IntPtr.Zero;
                gcReturnDataHandleAddress = IntPtr.Zero;

                gcDataHandle.Free();
                gcReturnDataHandle.Free();
            }
        }

        internal USB_Packet GarminReadSingleton()
        {
            // Read async data until the driver returns less than the
            // max async data size, which signifies the end of a packet
            byte[] buffer = new byte[GarminUSBConstants.ASYNC_DATA_SIZE];  //TODO: shouldnt this be device.packetSize?
            uint noOfBytesReturned = 0;

            GCHandle tbHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr tpIntPtr = tbHandle.AddrOfPinnedObject();

            try
            {
                UnsafeNativeMethods.DeviceIoControl(
                                Handle,
                                IOCTL_ASYNC_IN,
                                IntPtr.Zero,
                                0,
                                tpIntPtr,
                                GarminUSBConstants.ASYNC_DATA_SIZE,
                                ref noOfBytesReturned,
                                IntPtr.Zero);

                return Utilities.CreateUSBPacketFromByteArray(buffer);
            }
            finally
            {
                tpIntPtr = IntPtr.Zero;
                tbHandle.Free();
            }
        }

        /// <summary>
        /// More of a generic read for command protocols that ends with 'Pid_Xfer_Cmplt'
        /// </summary>
        /// <returns></returns>
        internal IList<byte[]> GarminReadRecord()
        {
            // Read async data until the driver returns less than the
            // max async data size, which signifies the end of a packet
            byte[] buffer = new byte[GarminUSBConstants.ASYNC_DATA_SIZE];  //TODO: shouldn't this be device.packetSize?
            GCHandle tbHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr tpIntPtr = tbHandle.AddrOfPinnedObject();

            uint noOfBytesReturned = 0;
            IList<byte[]> byteArrayList = new List<byte[]>();
            Int16 packetId;

            try
            {
                do
                {
                    UnsafeNativeMethods.DeviceIoControl(
                                    Handle,
                                    IOCTL_ASYNC_IN,
                                    IntPtr.Zero,
                                    0,
                                    tpIntPtr,
                                    GarminUSBConstants.ASYNC_DATA_SIZE,
                                    ref noOfBytesReturned,
                                    IntPtr.Zero);

                    byte[] tBuffer = new byte[noOfBytesReturned];
                    Buffer.BlockCopy(buffer, 0, tBuffer, 0, (int)noOfBytesReturned);
                    byteArrayList.Add(tBuffer);

                    packetId = Utilities.GetPacketIDFromByteArray(tBuffer);

                } while ((L001_packet_id)packetId != L001_packet_id.Pid_Xfer_Cmplt);

                return byteArrayList;
            }
            finally
            {
                tpIntPtr = IntPtr.Zero;
                tbHandle.Free();
            }
        }

        /// <summary>
        /// Reads data from the device until the number of bytes returned is less than GarminUSBConstants.ASYNC_DATA_SIZE
        /// </summary>
        /// <returns></returns>
        internal IList<byte> GarminReadRecord2()
        {
            // Read async data until the driver returns less than the
            // max async data size, which signifies the end of a packet
            byte[] buffer = new byte[GarminUSBConstants.ASYNC_DATA_SIZE];  //TODO: shouldn't this be device.packetSize?
            GCHandle tbHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr tpIntPtr = tbHandle.AddrOfPinnedObject();

            uint noOfBytesReturned = 0;
            List<byte> byteArrayList = new List<byte>();
            Int16 packetId;

            try
            {
                do
                {
                    UnsafeNativeMethods.DeviceIoControl(
                                    Handle,
                                    IOCTL_ASYNC_IN,
                                    IntPtr.Zero,
                                    0,
                                    tpIntPtr,
                                    GarminUSBConstants.ASYNC_DATA_SIZE,
                                    ref noOfBytesReturned,
                                    IntPtr.Zero);

                    byte[] tBuffer = new byte[noOfBytesReturned];
                    Buffer.BlockCopy(buffer, 0, tBuffer, 0, (int)noOfBytesReturned);
                    byteArrayList.AddRange(tBuffer);

                    packetId = Utilities.GetPacketIDFromByteArray(tBuffer);

                } while (noOfBytesReturned == GarminUSBConstants.ASYNC_DATA_SIZE);

                return byteArrayList;
            }
            finally
            {
                tpIntPtr = IntPtr.Zero;
                tbHandle.Free();
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.

            if (!isDisposed)
            {
                if (disposing)
                {
                    if (handle != null)
                    {
                        UnsafeNativeMethods.CloseHandle(handle);
                        handle.Dispose();
                    }
                }

                // Indicate that the instance has been disposed.
                handle = null;
                isDisposed = true;
            }


        }

        protected virtual void OnSessionStarted(SessionStartedEventArgs e)
        {
            if (SessionStarted != null)
            {
                SessionStarted(this, e);
            }
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            //Dispose(true);
            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        public void Test()
        {
            if (isDisposed)
                throw new ObjectDisposedException(resourceManager.GetString("ResourceDisposed"));
        }

        /// <summary>
        /// Function attempts to initiate a session on the garmin device. 
        /// The DeviceUnitId and HasSessionStarted properties are set.</summary>
        /// <exception cref="OSMS.Core.Device.Garmin.PhysicalProtocol.GarminDeviceNotFoundException"></exception>
        /// <exception cref="OSMS.Core.Device.Garmin.PhysicalProtocol.GarminUnableToWriteToDeviceException"></exception>
        /// <returns>True if the session on the device has been successfully started else false is returned.</returns>
        public void StartSession()
        {
            hasSessionStarted = false;

            USB_Packet usbPacket = new USB_Packet(USB_PacketType.USB_PROTOCOL_LAYER, (Int16)USB_PacketId.Pid_Start_Session, 0);

            if (!GarminWrite(usbPacket))
                return;

            USB_Packet returnPacket = GarminReadSingleton();

            // packet returned should be 3.2.3.3 "Pid_Session_Started (6)". 
            // It should also contain the device's unit id. We expect only 1 packet
            if (returnPacket.packetId == (Int16)USB_PacketId.Pid_Session_Started)
            {
                deviceUnitId = BitConverter.ToUInt32(returnPacket.data, 0);
                hasSessionStarted = true;
            }
            else
            {
                deviceUnitId = 0;
                hasSessionStarted = false;
            }

            OnSessionStarted(new SessionStartedEventArgs(hasSessionStarted));

            FetchProductDetails();
            //TODO: must remove this function from here. It is only placed here for testing purposes
            //FetchDateAndTimeType();
            //FetchWorkoutLimits();
            //FetchCourseLimits();
            //FetchLaps();
            //IList<IA302<ITrkHdrType, ITrkPointType>> x = FetchTracks();
            //FetchWorkouts();
            //IList<IA1000> y = FetchRuns();
            //FetchUserProfile();
            //List<D1003_Workout_Occurrence_Type> d3 = FetchWorkoutOccurences();
        }

        private void FetchProductDetails()
        {
            bool isReceivingProtocolData = false;
            bool pollForData = true;
            uint noOfBytesReturned = 0;

            List<byte[]> byteArrayList = new List<byte[]>();

            // Read async data until the driver returns less than the
            // max async data size, which signifies the end of a packet
            byte[] buffer = new byte[GarminUSBConstants.ASYNC_DATA_SIZE];  //TODO: shouldn't this be device.packetSize?
            GCHandle tHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER, (short)L000_packet_id.Pid_Product_Rqst, 0);
                if (!GarminWrite(usbPacket))
                    return;

                do
                {
                    if (!isReceivingProtocolData)
                    {
                        noOfBytesReturned = 0;
                        byteArrayList.Clear();
                    }

                    UnsafeNativeMethods.DeviceIoControl(
                        Handle,
                        IOCTL_ASYNC_IN,
                        IntPtr.Zero,
                        0,
                        tHandle.AddrOfPinnedObject(),
                        GarminUSBConstants.ASYNC_DATA_SIZE,
                        ref noOfBytesReturned,
                        IntPtr.Zero);

                    byte[] tBuffer = new byte[noOfBytesReturned];
                    Buffer.BlockCopy(buffer, 0, tBuffer, 0, (int)noOfBytesReturned);
                    byteArrayList.Add(tBuffer);

                    switch (Utilities.GetPacketIDFromByteArray(byteArrayList[0]))
                    {
                        case (Int16)L000_packet_id.Pid_Product_Data:
                            productDataType = GetProductDataTypeFromArray(byteArrayList[0]);
                            break;

                        case (Int16)L000_packet_id.Pid_Ext_Product_Data:
                            if (extProductDataTypes == null)
                            {
                                extProductDataTypes = new List<Ext_Product_Data_Type>();
                            }
                            extProductDataTypes.Add(GetExtProductDataType(byteArrayList[0]));  //We only use the first returned string
                            break;

                        case (Int16)L000_packet_id.Pid_Protocol_Array:
                            isReceivingProtocolData = true;
                            if (noOfBytesReturned != GarminUSBConstants.ASYNC_DATA_SIZE)
                            {
                                if (protocolDataTypes == null)
                                {
                                    protocolDataTypes = new List<Protocol_Data_Type>();
                                }
                                protocolDataTypes.AddRange(GetProtocolDataTypes(Utilities.ByteArrayListToByteArray(byteArrayList)));
                                pollForData = false;
                            }

                            break;

                        default:
                            pollForData = false;
                            break;
                    }
                } while (pollForData);
            }
            finally
            {
                tHandle.Free();
            }
        }

        /// <summary>
        /// Function implements the Date and Time Initialization Protocol 
        /// See section : 6.10 A600 – Date and Time Initialization Protocol. Pg 17 of the Garmin Device Interface Specification (March 2006)
        /// </summary>
        /// <exception cref="OSMS.Core.Device.Garmin.PhysicalProtocol.GarminProtocolNotSupportedException"></exception>
        /// <returns>
        /// Date and Time Initialization Protocol Packet Sequence
        /// </returns>
        public D600_Date_Time_Type FetchDateAndTimeType()
        {
            ushort data = 600;
            if (IsProtocolSupported(new Protocol_Data_Type(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, data)))
            {
                USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                    (short)L001_packet_id.Pid_Command_Data,
                    sizeof(short),
                    BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Time));

                if (!GarminWrite(usbPacket))
                {
                    throw new Exception("//TODO: implement a custom exception");
                }

                if (!IsProtocolSupported(new Protocol_Data_Type(Protocol_Data_Type_Tag.Tag_Data_Type_Id, data)))
                {
                    string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                        (char)Protocol_Data_Type_Tag.Tag_Data_Type_Id,
                        data,
                        this.DeviceUnitId.ToString(),
                        new string(this.ProductDataType.product_description));

                    throw new GarminProtocolNotSupportedException(errorMsg);
                }

                D600_Date_Time_Type dateTimeType = new D600_Date_Time_Type();
                USB_Packet returnPacket = GarminReadSingleton();

                if (returnPacket.packetId == (Int16)L001_packet_id.Pid_Date_Time_Data)
                {
                    int offset = 0;

                    dateTimeType.month = returnPacket.data[offset];
                    offset = offset + 1;

                    dateTimeType.day = returnPacket.data[offset];
                    offset = offset + 1;

                    dateTimeType.year = BitConverter.ToUInt16(returnPacket.data, offset);
                    offset = offset + 2;

                    dateTimeType.hour = BitConverter.ToUInt16(returnPacket.data, offset);
                    offset = offset + 2;

                    dateTimeType.minute = returnPacket.data[offset];
                    offset = offset + 1;

                    dateTimeType.second = returnPacket.data[offset];
                    offset = offset + 1;
                }

                return dateTimeType;
            }
            else
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    data,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }
        }

        public D1005_Workout_Limits FetchWorkoutLimits()
        {
            ushort data = 1005;
            if (IsProtocolSupported(new Protocol_Data_Type(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, data)))
            {
                USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                    (short)L001_packet_id.Pid_Command_Data,
                    sizeof(short),
                    BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Workout_Limits));

                if (!GarminWrite(usbPacket))
                {
                    throw new Exception("//TODO: implement a custom exception");
                }

                if (!IsProtocolSupported(new Protocol_Data_Type(Protocol_Data_Type_Tag.Tag_Data_Type_Id, data)))
                {
                    string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                        (char)Protocol_Data_Type_Tag.Tag_Data_Type_Id,
                        data,
                        this.DeviceUnitId.ToString(),
                        new string(this.ProductDataType.product_description));

                    throw new GarminProtocolNotSupportedException(errorMsg);
                }

                D1005_Workout_Limits workoutLimits = new D1005_Workout_Limits();
                USB_Packet returnPacket = GarminReadSingleton();

                if (returnPacket.packetId == (Int16)L001_packet_id.Pid_Workout_Limits)
                {
                    int offset = 0;

                    workoutLimits.max_workouts = BitConverter.ToUInt32(returnPacket.data, offset);
                    offset = offset + 4;

                    workoutLimits.max_unscheduled_workouts = BitConverter.ToUInt32(returnPacket.data, offset);
                    offset = offset + 4;

                    workoutLimits.max_occurrences = BitConverter.ToUInt32(returnPacket.data, offset);
                    offset = offset + 4;
                }

                return workoutLimits;
            }
            else
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    data,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }
        }

        public D1013_Course_Limits_Type FetchCourseLimits()
        {
            //NB. There is an undocumented A1013 protocol that is on the Forerunner 305
            ushort appData = 1009;
            if (IsProtocolSupported(new Protocol_Data_Type(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, appData)))
            {
                USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                    (short)L001_packet_id.Pid_Command_Data,
                    sizeof(short),
                    BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Course_Limits));

                if (!GarminWrite(usbPacket))
                {
                    throw new Exception("//TODO: implement a custom exception");
                }

                ushort dataType = 1013;
                if (!IsProtocolSupported(new Protocol_Data_Type(Protocol_Data_Type_Tag.Tag_Data_Type_Id, dataType)))
                {
                    string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                        (char)Protocol_Data_Type_Tag.Tag_Data_Type_Id,
                        dataType,
                        this.DeviceUnitId.ToString(),
                        new string(this.ProductDataType.product_description));

                    throw new GarminProtocolNotSupportedException(errorMsg);
                }

                D1013_Course_Limits_Type courseLimits = new D1013_Course_Limits_Type();
                USB_Packet returnPacket = GarminReadSingleton();

                if (returnPacket.packetId == (Int16)L001_packet_id.Pid_Course_Limits)
                {
                    int offset = 0;

                    courseLimits.max_courses = BitConverter.ToUInt32(returnPacket.data, offset);
                    offset = offset + 4;

                    courseLimits.max_course_laps = BitConverter.ToUInt32(returnPacket.data, offset);
                    offset = offset + 4;

                    courseLimits.max_course_pnt = BitConverter.ToUInt32(returnPacket.data, offset);
                    offset = offset + 4;

                    courseLimits.max_course_trk_pnt = BitConverter.ToUInt32(returnPacket.data, offset);
                    offset = offset + 4;
                }

                return courseLimits;
            }
            else
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    appData,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }
        }

        /// <summary>
        /// Function implements the Lap transfer Protocol
        /// See section : 6.14 A906 – Lap Transfer Protocol. Pg 19 of the Garmin Device Interface Specification (March 2006)
        /// </summary>
        /// <exception cref="OSMS.Core.Device.Garmin.PhysicalProtocol.GarminProtocolNotSupportedException"></exception>
        /// <returns>
        /// A906 interface implemented by types D906, D1001, D1011, D1015
        /// </returns>
        public List<IA906> FetchLaps()
        {
            //TODO: If the datatype for this protocol IS FOUND then use it else use D1015 for Forerunner 305

            //Must support application protocol A906 and one of the following datatypes: D906, D1001, D1011, D1015
            ushort data = 906;
            if (!IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, data))
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    data,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }

            USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                (short)L001_packet_id.Pid_Command_Data,
                sizeof(short),
                BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Laps));

            if (!GarminWrite(usbPacket))
            {
                throw new Exception("//TODO: garminwrite fail. implement a custom exception");
            }

            IList<byte[]> rawLapRecords = new List<byte[]>(GarminReadRecord());

            List<IA906> lapRecords = new List<IA906>();
            L001_packet_id packetId;
            UInt16 noOfExpectedRecords;
            USB_Packet usbLapPacket;

            foreach (byte[] arr in rawLapRecords)
            {
                usbLapPacket = Utilities.CreateUSBPacketFromByteArray(arr);
                packetId = (L001_packet_id)usbLapPacket.packetId;

                switch (packetId)
                {
                    case L001_packet_id.Pid_Records:
                        noOfExpectedRecords = BitConverter.ToUInt16(arr, GarminUSBConstants.PACKET_HEADER_SIZE);
                        break;

                    case L001_packet_id.Pid_Lap:
                        //TODO: This is a terrible way of doing this and it needs to be rewritten. But hey, I'm tired and need to get things done. 
                        //Perhaps a factory ?? Think perhaps enum with all the datatypes??
                        if (IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 906))
                        {
                            lapRecords.Add(new D906_Lap_Type(usbLapPacket.data));
                        }
                        else if (IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 1001))
                        {
                            lapRecords.Add(new D1001_Lap_Type(usbLapPacket.data));
                        }
                        else if (IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 1011))
                        {
                            lapRecords.Add(new D1011_Lap_Type(usbLapPacket.data));
                        }
                        else if (IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 1015))
                        {
                            lapRecords.Add(new D1015_Lap_Type(usbLapPacket.data)); //Forerunner 305, software version 2.90, GPS 3.00
                        }
                        break;

                    case L001_packet_id.Pid_Xfer_Cmplt:
                        //We can safely ignore
                        break;

                    default:
                        break;
                }
            }

            return lapRecords;
        }

        /// <summary>
        /// Function implements the Track Log Transfer Protocol as defined by protocol A302
        /// See section : 6.7.4 A302 – Track Log Transfer Protocol. Pg 16 of the Garmin Device Interface Specification (March 2006)
        /// Also as per the documentation for D304_Trk_Point_Type, Two consecutive track points with invalid position, invalid altitude, invalid heart rate, invalid distance and invalid 
        /// cadence indicate a pause in track point recording during the time between the two points.
        /// </summary>
        /// <exception cref="OSMS.Core.Device.Garmin.PhysicalProtocol.GarminProtocolNotSupportedException"></exception>
        /// <returns>
        /// Currently supports only A302 interface implemented by types D304, D311
        /// </returns>
        public List<IA302<ITrkHdrType, ITrkPointType>> FetchTracks()
        {
            //Must support application protocol A302 and one of the following datatypes on the forerunner 305: D304, D311
            ushort data = 302;
            if (!IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, data))
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    data,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }

            USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                (short)L001_packet_id.Pid_Command_Data,
                sizeof(short),
                BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Trk));

            if (!GarminWrite(usbPacket))
            {
                throw new Exception("//TODO: garminwrite fail. implement a custom exception");
            }

            IList<byte[]> rawTrackRecords = new List<byte[]>(GarminReadRecord());

            L001_packet_id packetId;
            UInt16 noOfExpectedRecords;
            USB_Packet usbTrackPacket;

            bool createNewTrackRecords = false;
            A302<ITrkHdrType, ITrkPointType> a302 = null;
            List<IA302<ITrkHdrType, ITrkPointType>> returnValue = new List<IA302<ITrkHdrType, ITrkPointType>>();

            foreach (byte[] arr in rawTrackRecords)
            {
                
                usbTrackPacket = Utilities.CreateUSBPacketFromByteArray(arr);
                packetId = (L001_packet_id)usbTrackPacket.packetId;

                switch (packetId)
                {
                    case L001_packet_id.Pid_Records:
                        noOfExpectedRecords = BitConverter.ToUInt16(arr, GarminUSBConstants.PACKET_HEADER_SIZE);
                        break;

                    case L001_packet_id.Pid_Trk_Hdr:
                        if (IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 311))
                        {
                            createNewTrackRecords = true;

                            a302 = new A302<ITrkHdrType, ITrkPointType>(null, null);
                            a302.Header = new D311_Trk_Hdr_Type(usbTrackPacket.data);

                            returnValue.Add(a302);
                        }
                        break;

                    case L001_packet_id.Pid_Trk_Data:
                        if (IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 304))
                        {
                            if (createNewTrackRecords)
                            {
                                a302.TrackPoints = new List<ITrkPointType>();
                                createNewTrackRecords = false;
                            }

                            a302.TrackPoints.Add(new D304_Trk_Point_Type(usbTrackPacket.data));
                        }
                        break;

                    case L001_packet_id.Pid_Xfer_Cmplt:
                        //We can safely ignore
                        break;

                    default:
                        break;
                }
            }

            return returnValue;
        }

        public List<IA1002> FetchWorkouts()
        {
            //Must support application protocol A1002 and one of the following datatypes on the forerunner 305: D304, D311
            ushort appData = 1002;
            if (!IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, appData))
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    appData,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }

            USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                (short)L001_packet_id.Pid_Command_Data,
                sizeof(short),
                BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Workouts));

            if (!GarminWrite(usbPacket))
            {
                throw new Exception("//TODO: garminwrite fail. implement a custom exception");
            }

            List<byte[]> rawWorkouRecords = new List<byte[]>(GarminReadRecord());

            L001_packet_id packetId;
            UInt16 noOfExpectedRecords;
            USB_Packet usbTrackPacket;

            List<IA1002> returnValue = new List<IA1002>();

            #region Number of Workout Records

            /// The first packet returned contains the number of run records
            usbTrackPacket = Utilities.CreateUSBPacketFromByteArray(rawWorkouRecords[0]);
            packetId = (L001_packet_id)usbTrackPacket.packetId;

            if (packetId == L001_packet_id.Pid_Records)
            {
                noOfExpectedRecords = BitConverter.ToUInt16(rawWorkouRecords[0], GarminUSBConstants.PACKET_HEADER_SIZE);
            }

            #endregion

            #region Run Transfer Complete

            usbTrackPacket = Utilities.CreateUSBPacketFromByteArray(rawWorkouRecords[rawWorkouRecords.Count - 1]);
            packetId = (L001_packet_id)usbTrackPacket.packetId;

            if (packetId == L001_packet_id.Pid_Xfer_Cmplt)
            {
                // Transfer complete packet exists. We can ignore
            }

            #endregion

            #region  Remove first and last items as we already have the data we need

            rawWorkouRecords.RemoveAt(0);
            rawWorkouRecords.RemoveAt(rawWorkouRecords.Count - 1);

            #endregion

            #region Create proper workout data packets

            List<byte> tByteList = new List<byte>();
            List<byte[]> workoutList = new List<byte[]>();
            foreach (byte[] arr in rawWorkouRecords)
            {
                tByteList.AddRange(arr);

                // Last data of packet
                if (arr.Length < GarminUSBConstants.ASYNC_DATA_SIZE)
                {
                    workoutList.Add(tByteList.ToArray());
                    tByteList.Clear();
                }
            }

            #endregion

            foreach (byte[] arr in workoutList)
            {
                usbTrackPacket = Utilities.CreateUSBPacketFromByteArray(arr);
                packetId = (L001_packet_id)usbTrackPacket.packetId;

                switch (packetId)
                {
                    case L001_packet_id.Pid_Workout:
                        if (!IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 1008))  //TODO: Need to cater for 1002 as well as its the same as 1008
                        {
                            continue;
                        }
                        returnValue.Add(new D1008_Workout_Type(usbTrackPacket.data));

                        break;

                    default:
                        break;
                }

            }

            return returnValue;
        }

        public List<D1003_Workout_Occurrence_Type> FetchWorkoutOccurences()
        {
            //Must support application protocol A906 and one of the following datatypes: D906, D1001, D1011, D1015
            ushort data = 1003;
            if (!IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, data))
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    data,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }

            USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                (short)L001_packet_id.Pid_Command_Data,
                sizeof(short),
                BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Workout_Occurrences));

            if (!GarminWrite(usbPacket))
            {
                throw new Exception("//TODO: garminwrite fail in function FetchWorkoutOccurences. implement a custom exception");
            }

            List<byte[]> rawWorkoutOccurrenceRecords = new List<byte[]>(GarminReadRecord());

            List<D1003_Workout_Occurrence_Type> workoutOccurrenceRecords = new List<D1003_Workout_Occurrence_Type>();
            L001_packet_id packetId;
            UInt16 noOfExpectedRecords;
            USB_Packet usbLapPacket;

            foreach (byte[] arr in rawWorkoutOccurrenceRecords)
            {
                usbLapPacket = Utilities.CreateUSBPacketFromByteArray(arr);
                packetId = (L001_packet_id)usbLapPacket.packetId;

                switch (packetId)
                {
                    case L001_packet_id.Pid_Records:
                        noOfExpectedRecords = BitConverter.ToUInt16(arr, GarminUSBConstants.PACKET_HEADER_SIZE);
                        break;

                    case L001_packet_id.Pid_Workout_Occurrence:
                        //TODO: This is a terrible way of doing this and it needs to be rewritten. But hey, I'm tired and need to get things done. 
                        //Perhaps a factory ?? Think perhaps enum with all the datatypes??
                        if (IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 1003))
                        {
                            workoutOccurrenceRecords.Add(new D1003_Workout_Occurrence_Type(usbLapPacket.data));
                        }
                        break;

                    case L001_packet_id.Pid_Xfer_Cmplt:
                        //We can safely ignore
                        break;

                    default:
                        break;
                }
            }

            return workoutOccurrenceRecords;
        }

        public List<IA1000> FetchRuns()
        {
            //Must support application protocol A1000 and one of the following datatypes on the forerunner 305: D1009
            ushort data = 1000;
            if (!IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, data))
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    data,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }

            USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                (short)L001_packet_id.Pid_Command_Data,
                sizeof(short),
                BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Runs));

            if (!GarminWrite(usbPacket))
            {
                throw new Exception("//TODO: garminwrite fail. implement a custom exception");
            }

            IList<byte[]> rawRunRecords = new List<byte[]>(GarminReadRecord());

            L001_packet_id packetId;
            UInt16 noOfExpectedRecords;
            USB_Packet usbTrackPacket;

            List<IA1000> returnValue = new List<IA1000>();

            #region Number of Run Records

            /// The first packet returned contains the number of run records
            usbTrackPacket = Utilities.CreateUSBPacketFromByteArray(rawRunRecords[0]);
            packetId = (L001_packet_id)usbTrackPacket.packetId;

            if (packetId == L001_packet_id.Pid_Records)
            {
                noOfExpectedRecords = BitConverter.ToUInt16(rawRunRecords[0], GarminUSBConstants.PACKET_HEADER_SIZE);
            }

            #endregion

            #region Run Transfer Complete

            usbTrackPacket = Utilities.CreateUSBPacketFromByteArray(rawRunRecords[rawRunRecords.Count - 1]);
            packetId = (L001_packet_id)usbTrackPacket.packetId;

            if (packetId == L001_packet_id.Pid_Xfer_Cmplt)
            {
                // Transfer complete packet exists. We can ignore
            }

            #endregion

            #region  Remove first and last items as we already have the data we need

            rawRunRecords.RemoveAt(0);
            rawRunRecords.RemoveAt(rawRunRecords.Count - 1);

            #endregion

            #region Create proper run data packets

            List<byte> tByteList = new List<byte>();
            List<byte[]> runList = new List<byte[]>();
            foreach (byte[] arr in rawRunRecords)
            {
                tByteList.AddRange(arr);

                // Last data of packet
                if (arr.Length < GarminUSBConstants.ASYNC_DATA_SIZE)
                {
                    runList.Add(tByteList.ToArray());
                    tByteList.Clear();
                }
            }

            #endregion

            foreach (byte[] arr in runList)
            {
                usbTrackPacket = Utilities.CreateUSBPacketFromByteArray(arr);
                packetId = (L001_packet_id)usbTrackPacket.packetId;

                switch (packetId)
                {
                    case L001_packet_id.Pid_Run:
                        if (!IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Data_Type_Id, 1009))
                        {
                            continue;
                        }
                        returnValue.Add(new D1009_Run_Type(usbTrackPacket.data));

                        break;

                    default:
                        break;
                }

            }

            return returnValue;
        }

        public D1004_Fitness_User_Profile_Type FetchUserProfile()
        {
            ushort data = 1004;
            if (!IsProtocolSupported(Protocol_Data_Type_Tag.Tag_Appl_Prot_Id, data))
            {
                string errorMsg = string.Format(resourceManager.GetString("Ex.GarminProtocolNotSupportedByDevice"),
                    (char)Protocol_Data_Type_Tag.Tag_Appl_Prot_Id,
                    data,
                    this.DeviceUnitId.ToString(),
                    new string(this.ProductDataType.product_description));

                throw new GarminProtocolNotSupportedException(errorMsg);
            }

            USB_Packet usbPacket = new USB_Packet(USB_PacketType.APPLICATION_LAYER,
                (short)L001_packet_id.Pid_Command_Data,
                sizeof(short),
                BitConverter.GetBytes((short)Device_Command_Protocol_1.Cmnd_Transfer_Fitness_User_Profile));

            if (!GarminWrite(usbPacket))
            {
                throw new Exception("//TODO: garminwrite fail in funtion FetchUserProfile. implement a custom exception");
            }

            List<byte> fitnessProfile = new List<byte>(GarminReadRecord2());

            L001_packet_id packetId;
            USB_Packet usbTrackPacket;

            usbTrackPacket = Utilities.CreateUSBPacketFromByteArray(fitnessProfile.ToArray());
            packetId = (L001_packet_id)usbTrackPacket.packetId;

            D1004_Fitness_User_Profile_Type returnValue = new D1004_Fitness_User_Profile_Type();
            if (packetId == L001_packet_id.Pid_Fitness_User_Profile)
            {
                returnValue = new D1004_Fitness_User_Profile_Type(usbTrackPacket.data);
            }

            //TODO: A value type is always returned. need to implement scenario if if (packetId != L001_packet_id.Pid_Fitness_User_Profile)

            return returnValue;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format(@"Packetsize :  {0}", this.PacketSize));
            sb.AppendLine(String.Format(@"HasSessionStarted : {0}", this.HasSessionStarted));
            sb.AppendLine(String.Format(@"DeviceUnitId : {0}", this.DeviceUnitId));

            return sb.ToString();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// This event is raised after the StartSession method is called. 
        /// </summary>
        public event EventHandler<SessionStartedEventArgs> SessionStarted;

        #endregion
    }
}
