using System;
using System.Collections.Generic;

namespace Morcon.Models
{
    public enum DeviceType
    {
        TEMP_SENSOR = 1,
        HUM_SENSOR = 2,
        WIND_SENSOR = 3,
        ALARM = 4,
        MOV_DETECTOR = 5
    }

    public enum MessageType
    {
        JSON = 1,
        TEXT = 2,
        BINARY = 3
    }

    public class Device
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public DeviceType Type { get; set; }

        public List<DeviceMessage> Messages { get; set; } = new();
    }

    public class DeviceMessage
    {
        public long Id { get; set; }
        public MessageType Type { get; set; }
        public long ByteSize { get; set; }
        public DateTime Timestamp { get; set; }

        public long DeviceId { get; set; }
        public Device Device { get; set; } = null!;
    }

    public class MessageSessions
    {
        public long Id { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }

        public long DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        public List<DeviceMessage> Messages { get; set; } = new();
    }
}
