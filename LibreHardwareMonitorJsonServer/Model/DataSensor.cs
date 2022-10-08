
// Copyright (C) 2022 Emerson Pinter - All Rights Reserved
//

/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Runtime.Serialization;

namespace LibreHardwareMonitorJsonServer
{
    public struct DataSensor
    {
        private string identifier;
        private string name;
        private string sensorType;
        private string sensorTypeCN;
        private string hardwareId;
        private string hardwareName;
        private string hardwareType;
        private string hardwareTypeCN;
        private string unit;

        [DataMember(Name = "identifier")]
        public string Identifier { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "sensorType")]
        public string SensorType { get; set; }

        [DataMember(Name = "sensorTypeCN")]
        public string SensorTypeCN { get; set; }

        [DataMember(Name = "hardwareId")]
        public string HardwareId { get; set; }

        [DataMember(Name = "hardwareName")]
        public string HardwareName { get; set; }

        [DataMember(Name = "hardwareType")]
        public string HardwareType { get; set; }

        [DataMember(Name = "hardwareTypeCN")]
        public string HardwareTypeCN { get; set; }

        [DataMember(Name = "value")]
        public float Value { get; set; }

        // [DataMember(Name = "max")]
        // public float Max { get; set; }

        // [DataMember(Name = "min")]
        // public float Min { get; set; }

        [DataMember(Name = "unit")]
        public string Unit { get; set; }
    }
}
