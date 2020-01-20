//#define DEBUG_LOG

using System;
using ANT_Managed_Library;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AntScaleLib
{
    public class AntScale
    {
        public delegate void NewReadingDelegate(Weight w);

        static Weight readings;

        const byte USER_ANT_CHANNEL = 0;         // ANT Channel to use
        const ushort USER_DEVICENUM = 0;        // Device number    
        const byte USER_DEVICETYPE = 119;          // Device type
        const byte USER_TRANSTYPE = 0;           // Transmission type

        const byte USER_RADIOFREQ = 57;          // RF Frequency + 2400 MHz
        const ushort USER_CHANNELPERIOD = 8192;  // Channel Period (8192/32768)s period = 4Hz

        byte[] USER_NETWORK_KEY;
        const byte USER_NETWORK_NUM = 0;         // The network key is assigned to this network number

        ANT_Device device;
        ANT_Channel channel;
        ANT_ReferenceLibrary.ChannelType channelType = ANT_ReferenceLibrary.ChannelType.BASE_Slave_Receive_0x00;

        public event NewReadingDelegate NewReading;

        public AntScale( string networkKey)
        {
            try
            {
                USER_NETWORK_KEY = new byte[networkKey.Length / 2];
                for (int i = 0; i < networkKey.Length; i += 2)
                {
                    USER_NETWORK_KEY[i/2] = Convert.ToByte( networkKey.Substring(i,2),16);
                }

                Log("Attempting to connect to an ANT USB device...");
                device = new ANT_Device();   // Create a device instance using the automatic constructor (automatic detection of USB device number and baud rate)
                device.deviceResponse += new ANT_Device.dDeviceResponseHandler(DeviceResponse);    // Add device response function to receive protocol event messages

                channel = device.getChannel(USER_ANT_CHANNEL);    // Get channel from ANT device
                channel.channelResponse += new dChannelResponseHandler(ChannelResponse);  // Add channel response function to receive channel event messages
                Log("Initialization was successful!");
            }
            catch (Exception ex)
            {
                if (device == null)    // Unable to connect to ANT
                {
                    throw new Exception("Could not connect to any device.\nDetails: \n   " + ex.Message);
                }
                else
                {
                    throw new Exception("Error connecting to ANT: " + ex.Message);
                }

            }
        }

        public void Log(string msg)
        {
            Trace.TraceInformation(msg);
        }

        public void Start()
        {
            try
            {
                ConfigureANT();

                // Clean up ANT
                return;
            }
            catch (Exception ex)
            {
                throw new Exception("Demo failed: " + ex.Message + Environment.NewLine);
            }
        }


        public void Stop()
        {
            channel.closeChannel();
            Log("Disconnecting module...");
            ANT_Device.shutdownDeviceInstance(ref device);  // Close down the device completely and completely shut down all communication
        }

        ////////////////////////////////////////////////////////////////////////////////
        // ConfigureANT
        //
        // Resets the system, configures the ANT channel and starts the demo
        ////////////////////////////////////////////////////////////////////////////////
        private void ConfigureANT()
        {
            Log("Resetting module...");
            device.ResetSystem();     // Soft reset
            System.Threading.Thread.Sleep(500);    // Delay 500ms after a reset

            // If you call the setup functions specifying a wait time, you can check the return value for success or failure of the command
            // This function is blocking - the thread will be blocked while waiting for a response.
            // 500ms is usually a safe value to ensure you wait long enough for any response
            // If you do not specify a wait time, the command is simply sent, and you have to monitor the protocol events for the response,
            Log("Setting network key...");
            if (device.setNetworkKey(USER_NETWORK_NUM, USER_NETWORK_KEY, 500))
                Log("Network key set");
            else
                throw new Exception("Error configuring network key");

            Log("Assigning channel...");
            if (channel.assignChannel(channelType, USER_NETWORK_NUM, 500))
                Log("Channel assigned");
            else
                throw new Exception("Error assigning channel");

            Log("Setting Channel ID...");
            if (channel.setChannelID(USER_DEVICENUM, false, USER_DEVICETYPE, USER_TRANSTYPE, 500))  // Not using pairing bit
                Log("Channel ID set");
            else
                throw new Exception("Error configuring Channel ID");

            Log("Setting Radio Frequency...");
            if (channel.setChannelFreq(USER_RADIOFREQ, 500))
                Log("Radio Frequency set");
            else
                throw new Exception("Error configuring Radio Frequency");

            Log("Setting Channel Period...");
            if (channel.setChannelPeriod(USER_CHANNELPERIOD, 500))
                Log("Channel Period set");
            else
                throw new Exception("Error configuring Channel Period");

            Log("Opening channel...");
            if (channel.openChannel(500))
            {
                Log("Channel opened");
            }
            else
            {
                throw new Exception("Error opening channel");
            }

            channel.setChannelSearchTimeout(255);

#if (ENABLE_EXTENDED_MESSAGES)
            // Extended messages are not supported in all ANT devices, so
            // we will not wait for the response here, and instead will monitor 
            // the protocol events
            Log("Enabling extended messages...");
            device0.enableRxExtendedMessages(true);
#endif
        }


        private void ChannelResponse(ANT_Response response)
        {
            try
            {
                switch ((ANT_ReferenceLibrary.ANTMessageID)response.responseID)
                {
                    case ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40:
                        {
                            switch (response.getChannelEventCode())
                            {
                                // This event indicates that a message has just been
                                // sent over the air. We take advantage of this event to set
                                // up the data for the next message period.   
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TX_0x03:
                                    {
                                        Log("EVENT_TX_0x03");
                                        //txBuffer[0]++;  // Increment the first byte of the buffer

                                        //// Broadcast data will be sent over the air on
                                        //// the next message period
                                        //if (bBroadcasting)
                                        //{
                                        //    channel0.sendBroadcastData(txBuffer);

                                        //    if (bDisplay)
                                        //    {
                                        //        // Echo what the data will be over the air on the next message period
                                        //        Log("Tx: (" + response.antChannel.ToString() + ")" + BitConverter.ToString(txBuffer));
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    string[] ac = { "|", "/", "_", "\\" };
                                        //    .Write("Tx: " + ac[iIndex++] + "\r");
                                        //    iIndex &= 3;
                                        //}
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_SEARCH_TIMEOUT_0x01:
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_CHANNEL_CLOSED_0x07:
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_FAIL_GO_TO_SEARCH_0x08:
                                    {
                                        if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.EVENT_CHANNEL_CLOSED_0x07)
                                        {
                                            // This event should be used to determine that the channel is closed.
                                            Log("Channel Closed");
                                            Log("Unassigning Channel...");
                                            if (channel.unassignChannel(500))
                                            {
                                                Log("Unassigned Channel");
                                                Log("Press enter to exit");
                                            }
                                        }
                                        if (readings != null)
                                            ProcessReadings();
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_RX_FAIL_0x02:
                                    {
                                        Log("Rx Fail");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_RX_FAILED_0x04:
                                    {
                                        Log("Burst receive has failed");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_COMPLETED_0x05:
                                    {
                                        Log("Transfer Completed");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_FAILED_0x06:
                                    {
                                        Log("Transfer Failed");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_CHANNEL_COLLISION_0x09:
                                    {
                                        Log("Channel Collision");
                                        break;
                                    }
                                case ANT_ReferenceLibrary.ANTEventID.EVENT_TRANSFER_TX_START_0x0A:
                                    {
                                        Log("Burst Started");
                                        break;
                                    }
                                default:
                                    {
                                        Log("Unhandled Channel Event " + response.getChannelEventCode());
                                        break;
                                    }
                            }
                            break;
                        }
                    case ANT_ReferenceLibrary.ANTMessageID.BROADCAST_DATA_0x4E:
                    case ANT_ReferenceLibrary.ANTMessageID.ACKNOWLEDGED_DATA_0x4F:
                    case ANT_ReferenceLibrary.ANTMessageID.BURST_DATA_0x50:
                    case ANT_ReferenceLibrary.ANTMessageID.EXT_BROADCAST_DATA_0x5D:
                    case ANT_ReferenceLibrary.ANTMessageID.EXT_ACKNOWLEDGED_DATA_0x5E:
                    case ANT_ReferenceLibrary.ANTMessageID.EXT_BURST_DATA_0x5F:
                        {
                            //if (bDisplay)
                            {
                                string msg = "";
                                if (response.isExtended()) // Check if we are dealing with an extended message
                                {
                                    ANT_ChannelID chID = response.getDeviceIDfromExt();    // Channel ID of the device we just received a message from
                                    msg = "Chan ID(" + chID.deviceNumber.ToString() + "," + chID.deviceTypeID.ToString() + "," + chID.transmissionTypeID.ToString() + ") - ";
                                }
                                if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.BROADCAST_DATA_0x4E || response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.EXT_BROADCAST_DATA_0x5D)
                                    msg = "Rx:(" + response.antChannel.ToString() + "): ";
                                else if (response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.ACKNOWLEDGED_DATA_0x4F || response.responseID == (byte)ANT_ReferenceLibrary.ANTMessageID.EXT_ACKNOWLEDGED_DATA_0x5E)
                                    msg = "Acked Rx:(" + response.antChannel.ToString() + "): ";
                                else
                                    msg = "Burst(" + response.getBurstSequenceNumber().ToString("X2") + ") Rx:(" + response.antChannel.ToString() + "): ";

                                msg += BitConverter.ToString(response.getDataPayload());
                                Log(msg);   // Display data payload
                                ProcessMessage(response.getDataPayload());
                            }
                            //else
                            //{
                            //    string[] ac = { "|", "/", "_", "\\" };
                            //    .Write("Rx: " + ac[iIndex++] + "\r");
                            //    iIndex &= 3;
                            //}
                            break;
                        }
                    default:
                        {
                            Log("Unknown Message " + response.responseID);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Log("Channel response processing failed with exception: " + ex.Message);
            }
        }



        private void ProcessReadings()
        {
            if (readings != null)
            {
                if (readings.DataPages != Weight.EXPECTED_DATAPAGES)
                {
                    Log($"Not all datapages received {readings.DataPages}");
                    return;
                }
                string json = readings.MakeJSON();
                Log(json);
                if (NewReading != null)
                    NewReading.Invoke(readings);

                readings = null;
            }
        }


        private void ProcessMessage(byte[] msg)
        {
            if (readings == null)
                readings = new Weight();

            byte dataPage = msg[0];
            if (dataPage == Weight.DATAPAGE01_BODY_WEIGHT)
            {
                UInt16 userProfile = BitConverter.ToUInt16(msg, 1);
                byte capabilities = msg[3];
                UInt16 bodyWeight = BitConverter.ToUInt16(msg, 6);
                if (userProfile == 0xFFFF || bodyWeight >= 0xFFFE)
                    Log("Invalid values.  Ignoring reading.");
                else
                    readings.UpdateDataPage1BodyWeight(userProfile, bodyWeight * 0.01);
            }
            else if (dataPage == Weight.DATAPAGE02_BODY_COMPOSITION_PERCENTAGE)
            {
                UInt16 userProfile = BitConverter.ToUInt16(msg, 1);
                UInt16 hydration = BitConverter.ToUInt16(msg, 4);
                UInt16 bodyFat = BitConverter.ToUInt16(msg, 6);
                if (userProfile == 0xFFFF || hydration >= 0xFFFE || bodyFat >= 0xFFFE)
                    Log("Invalid values.  Ignoring reading.");
                else
                    readings.UpdateDataPage2BodyCompositionPercentage(userProfile, hydration * 0.01, bodyFat * 0.01);
            }
            else if (dataPage == Weight.DATAPAGE03_METABOLIC_INFORMATION)
            {
                UInt16 userProfile = BitConverter.ToUInt16(msg, 1);
                UInt16 activeMetabolicRate = BitConverter.ToUInt16(msg, 4);
                UInt16 basalMetabolicRate = BitConverter.ToUInt16(msg, 6);
                if (userProfile == 0xFFFF || activeMetabolicRate >= 0xFFFE || basalMetabolicRate >= 0xFFFE)
                    Log("Invalid values.  Ignoring reading.");
                else
                    readings.UpdateDataPage3MetabolicInformation(userProfile, activeMetabolicRate * 0.25, basalMetabolicRate * 0.25);
            }
            else if (dataPage == Weight.DATAPAGE04_BODY_COMPOSITION_MASS)
            {
                UInt16 userProfile = BitConverter.ToUInt16(msg, 1);
                UInt16 muscleMass = BitConverter.ToUInt16(msg, 5);
                UInt16 boneMass = msg[7];
                if (userProfile == 0xFFFF || muscleMass >= 0xFFFE || boneMass >= 0xFE)
                    Log("Invalid values.  Ignoring reading.");
                else
                    readings.UpdateDataPage4BodyCompositionMass(userProfile, muscleMass * 0.01, boneMass * 0.1);
            }
            else if (dataPage == Weight.DATAPAGE58_USER_PROFILE)
            {
                UInt16 userProfile = BitConverter.ToUInt16(msg, 1);
                byte capabilities = msg[3];
                byte genderAndAge = msg[5];
                byte height = msg[6];
                byte desciptiveFlags = msg[7];
                if (userProfile == 0xFFFF || genderAndAge == 0 || height == 0)
                    Log("Invalid values.  Ignoring reading.");
                else
                    readings.UpdateDataPage58UserProfile(userProfile, (genderAndAge & 128) == 128 ? "M" : "F", (byte)(genderAndAge & 0x7F), height);
            }
        }

        private void DeviceResponse(ANT_Response response)
        {
            switch ((ANT_ReferenceLibrary.ANTMessageID)response.responseID)
            {
                case ANT_ReferenceLibrary.ANTMessageID.STARTUP_MESG_0x6F:
                    {
                        string msg = "RESET Complete, reason: ";

                        byte ucReason = response.messageContents[0];

                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_POR_0x00)
                            msg += "RESET_POR";
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_RST_0x01)
                            msg += "RESET_RST";
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_WDT_0x02)
                            msg += "RESET_WDT";
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_CMD_0x20)
                            msg += "RESET_CMD";
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_SYNC_0x40)
                            msg += "RESET_SYNC";
                        if (ucReason == (byte)ANT_ReferenceLibrary.StartupMessage.RESET_SUSPEND_0x80)
                            msg += "RESET_SUSPEND";
                        Log(msg);
                        break;
                    }
                case ANT_ReferenceLibrary.ANTMessageID.VERSION_0x3E:
                    {
                        Log("VERSION: " + new ASCIIEncoding().GetString(response.messageContents));
                        break;
                    }
                case ANT_ReferenceLibrary.ANTMessageID.RESPONSE_EVENT_0x40:
                    {
                        switch (response.getMessageID())
                        {
                            case ANT_ReferenceLibrary.ANTMessageID.CLOSE_CHANNEL_0x4C:
                                {
                                    if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.CHANNEL_IN_WRONG_STATE_0x15)
                                    {
                                        Log("Channel is already closed");
                                        Log("Unassigning Channel...");
                                        if (channel.unassignChannel(500))
                                        {
                                            Log("Unassigned Channel");
                                            Log("Press enter to exit");
                                        }
                                    }
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTMessageID.NETWORK_KEY_0x46:
                            case ANT_ReferenceLibrary.ANTMessageID.ASSIGN_CHANNEL_0x42:
                            case ANT_ReferenceLibrary.ANTMessageID.CHANNEL_ID_0x51:
                            case ANT_ReferenceLibrary.ANTMessageID.CHANNEL_RADIO_FREQ_0x45:
                            case ANT_ReferenceLibrary.ANTMessageID.CHANNEL_MESG_PERIOD_0x43:
                            case ANT_ReferenceLibrary.ANTMessageID.OPEN_CHANNEL_0x4B:
                            case ANT_ReferenceLibrary.ANTMessageID.UNASSIGN_CHANNEL_0x41:
                                {
                                    if (response.getChannelEventCode() != ANT_ReferenceLibrary.ANTEventID.RESPONSE_NO_ERROR_0x00)
                                    {
                                        Log(String.Format("Error {0} configuring {1}", response.getChannelEventCode(), response.getMessageID()));
                                    }
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTMessageID.RX_EXT_MESGS_ENABLE_0x66:
                                {
                                    if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.INVALID_MESSAGE_0x28)
                                    {
                                        Log("Extended messages not supported in this ANT product");
                                        break;
                                    }
                                    else if (response.getChannelEventCode() != ANT_ReferenceLibrary.ANTEventID.RESPONSE_NO_ERROR_0x00)
                                    {
                                        Log(String.Format("Error {0} configuring {1}", response.getChannelEventCode(), response.getMessageID()));
                                        break;
                                    }
                                    Log("Extended messages enabled");
                                    break;
                                }
                            case ANT_ReferenceLibrary.ANTMessageID.REQUEST_0x4D:
                                {
                                    if (response.getChannelEventCode() == ANT_ReferenceLibrary.ANTEventID.INVALID_MESSAGE_0x28)
                                    {
                                        Log("Requested message not supported in this ANT product");
                                        break;
                                    }
                                    break;
                                }
                            default:
                                {
                                    Log("Unhandled response " + response.getChannelEventCode() + " to message " + response.getMessageID()); break;
                                }
                        }
                        break;
                    }
            }
        }
    }

}
