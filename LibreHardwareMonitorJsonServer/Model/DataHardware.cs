using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LibreHardwareMonitorJsonServer
{
    public struct DataHardware
    {
        private string hardwareType;
        private string hardwareTypeCN;
        private string identifier;
        private string name;
        private string parent;
        private List<DataSensorType> sensorTypes;
        private List<DataHardware> subHardware;

        [DataMember(Name = "hardwareType")]
        public string HardwareType { get; set; }

        [DataMember(Name = "hardwareTypeCN")]
        public string HardwareTypeCN { get; set; }

        [DataMember(Name = "identifier")]
        public string Identifier { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "parent")]
        public string Parent { get => parent; set => parent = value ?? ""; }

        [DataMember(Name = "sensorTypes")]
        public List<DataSensorType> SensorTypes { get => sensorTypes; set => sensorTypes = value ?? new List<DataSensorType>(); }

        [DataMember(Name = "subHardware")]
        public List<DataHardware> SubHardware { get => subHardware; set => subHardware = value ?? new List<DataHardware>(); }
    }
}