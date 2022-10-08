using System;
using LibreHardwareMonitor.Hardware;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace LibreHardwareMonitorJsonServer
{
    // Json.NET custom contract resolver. Needed in order to use the custom converter, since the custom converter is
    // intended to be used with annotations, and we can't annotate properties in a compiled library.
    class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            // Don't serialize the Values or Parameters properties. The Values property is an array of all the sensor
            // readings in the past 24 hours, making the JSON size very, very large if enough time has passed. The
            // Parameters property only holds strings on how it calculated the Value property, and can be ignored since
            // it won't ever change.
            if (property.PropertyName == "Values" || property.PropertyName == "Parameters")
            {
                property.ShouldSerialize = (x) => false;
            }

            return property;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = base.CreateObjectContract(objectType);
            // Use the ToString JSON converter if the type is LibreHardwareMonitor.Hardware.Identifier. Otherwise, the
            // default serialization will serialize it as an object, and since it has no public properties, it will
            // serialize to "Identifier: {}".
            if (objectType == typeof(Identifier))
            {
                contract.Converter = new ToStringJsonConverter();
            }

            return contract;
        }
    }
}
