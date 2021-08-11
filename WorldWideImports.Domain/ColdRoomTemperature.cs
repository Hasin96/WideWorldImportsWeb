using System;
using System.Collections.Generic;

#nullable disable

namespace WorldWideImports.Domain
{
    public partial class ColdRoomTemperature
    {
        public long ColdRoomTemperatureId { get; set; }
        public int ColdRoomSensorNumber { get; set; }
        public DateTime RecordedWhen { get; set; }
        public decimal Temperature { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
