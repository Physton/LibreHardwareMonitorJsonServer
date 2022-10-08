
// Copyright (C) 2022 Emerson Pinter - All Rights Reserved
//

/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LibreHardwareMonitorJsonServer
{
    public struct DataSensorType
    {
        private string name;
        private string nameCN;
        private List<DataSensor> sensors;

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "nameCN")]
        public string NameCN { get; set; }

        [DataMember(Name = "sensors")]
        public List<DataSensor> Sensors { get => sensors; set => sensors = value ?? new List<DataSensor>(); }
    }
}
