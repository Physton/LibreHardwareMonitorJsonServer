using LibreHardwareMonitor.Hardware;
using Newtonsoft.Json;
using SimpleHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace LibreHardwareMonitorJsonServer
{
    static class HttpApi
    {
        public static Computer computer;
        public static Visitor visitor;
        public static JsonSerializerSettings jsonSettings;

        public static void run(int port = 9986)
        {
            // Create a computer instance with all sensors enabled.
            computer = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsBatteryEnabled = true,
                IsPsuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true,
            };

            // Initialize the sensors.
            computer.Open();

            visitor = new Visitor();
            jsonSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CustomContractResolver()
            };

            routeIndex();
            routeList();
            routeSensor();
            routeSensors();

            Console.WriteLine("              .======.");
            Console.WriteLine("              |      |         专");
            Console.WriteLine("              |      |         注");
            Console.WriteLine("              |      |         于");
            Console.WriteLine(" .============'      '============.");
            Console.WriteLine(" |        _  Physton.com   _   互 |");
            Console.WriteLine(" |       /_;-.__ / _\\  _.-;_\\  联 |");
            Console.WriteLine(" |         `-._`'`_/'`.-'      网 |");
            Console.WriteLine(" '============.`\\   /`============'");
            Console.WriteLine("              | |  / |         项");
            Console.WriteLine("     P        |/-.(  |         目");
            Console.WriteLine("      h       |\\_._\\ |         工");
            Console.WriteLine("       y      | \\ \\`;|         程");
            Console.WriteLine("        s     |  > |/|         研");
            Console.WriteLine("         t    | / // |         究");
            Console.WriteLine("          o   | |//  |         与");
            Console.WriteLine("           n  | \\(\\  |         开");
            Console.WriteLine("              |  ``  |         发");
            Console.WriteLine("              |      |         ！");
            Console.WriteLine("              |      |");
            Console.WriteLine("  \\    _  _\\| \\//  |//_   _ \\// _");
            Console.WriteLine(" ^ `^`^ ^`` `^ ^` ``^^`  `^^` `^ `^");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Running HTTP server on: " + port);
            Console.WriteLine("API Example:");
            Console.WriteLine("\tTree: http://127.0.0.1:" + port);
            Console.WriteLine("\tList: http://127.0.0.1:" + port + "/list");
            Console.WriteLine("\tSensor: http://127.0.0.1:" + port + "/sensor?identifier=/ram/data/0");
            Console.WriteLine("\tSensors: http://127.0.0.1:" + port + "/sensors?identifiers=/ram/data/0,/ram/data/1");

            Console.WriteLine("");

            var cts = new CancellationTokenSource();
            var ts = HttpServer.ListenAsync(port, cts.Token, Route.OnHttpRequestAsync, useHttps: false);
            AppExit.WaitFor(cts, ts);
        }


        private static DataHardware toHardware(IHardware h)
        {
            return new DataHardware
            {
                Name = h.Name,
                HardwareType = h.HardwareType.ToString(),
                HardwareTypeCN = h.HardwareTypeCN,
                Identifier = h.Identifier.ToString(),
                Parent = h.Parent?.Identifier.ToString(),
                SensorTypes = new List<DataSensorType>(),
                SubHardware = new List<DataHardware>()
            };
        }

        private static DataSensor toDataSensor(ISensor sensor, IHardware hardware = null, bool sensorType = true)
        {
            float value = sensor.Value == null ? 0.0f : (float)sensor.Value;
            if (float.IsNaN(value) || float.IsInfinity(value) || float.IsNegativeInfinity(value) || float.IsPositiveInfinity(value))
            {
                value = 0.0f;
            }

            string unit = sensor.Unit;
            if (sensor.SensorType.ToString() == "Throughput")
            {
                const uint GB = 1024 * 1024 * 1024; //定义GB的计算常量
                const uint MB = 1024 * 1024; //定义MB的计算常量
                const uint KB = 1024; //定义KB的计算常量

                if (value / GB >= 1)
                {
                    value = (float)Math.Round(value / GB, 2);
                    unit = "GB/s";
                }
                else if (value / MB >= 1)
                {
                    value = (float)Math.Round(value / MB, 2);
                    unit = "MB/s";
                }
                else if (value / KB >= 1) //如果当前Byte的值大于等于1KB
                {
                    value = (float)Math.Round(value / KB, 2);
                    unit = "KB/s";
                }
                else
                {
                    unit = "B/s";
                }
            }

            return new DataSensor()
            {
                Name = sensor.Name,
                HardwareId = hardware == null ? null : hardware.Identifier.ToString(),
                HardwareName = hardware == null ? null : hardware.Name,
                HardwareType = hardware == null ? null : hardware.HardwareType.ToString(),
                HardwareTypeCN = hardware == null ? null : hardware.HardwareTypeCN,
                Identifier = sensor.Identifier.ToString(),
                SensorType = sensorType ? sensor.SensorType.ToString() : null,
                SensorTypeCN = sensorType ? sensor.SensorTypeCN : null,
                Value = value,
                // Max = (float)sensor.Max,
                // Min = (float)sensor.Min,
                Unit = unit,
            };
        }

        private static void responseJson(HttpListenerResponse response, object value, bool simple = false)
        {
            var json = JsonConvert.SerializeObject(value, jsonSettings);

            response.AddHeader("Cache-Control", "no-cache");
            response.AddHeader("Access-Control-Allow-Origin", "*");


            var data = Encoding.UTF8.GetBytes(json);

            response.ContentLength64 = data.Length;
            response.ContentType = "application/json";
            response.OutputStream.Write(data, 0, data.Length);
        }

        private static int getSensorTypeIndex(DataHardware dataHardware, string sensorType)
        {
            int i = 0;
            foreach(DataSensorType dataSensorType in dataHardware.SensorTypes)
            {
                if (dataSensorType.Name == sensorType)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        private static void addSensorToSensorType(DataHardware dataHardware, IHardware hardware)
        {
            foreach (ISensor sensor in hardware.Sensors)
            {
                string type = sensor.SensorType.ToString();
                int index = getSensorTypeIndex(dataHardware, type);
                if (index == -1)
                {
                    DataSensorType dataSensorType = new DataSensorType();
                    dataSensorType.Name = type;
                    dataSensorType.NameCN = sensor.SensorTypeCN;
                    dataSensorType.Sensors = new List<DataSensor>();
                    dataHardware.SensorTypes.Add(dataSensorType);
                    index = dataHardware.SensorTypes.Count - 1;
                }
                dataHardware.SensorTypes[index].Sensors.Add(toDataSensor(sensor, null, false));
            }
        }

        private static void routeIndex()
        {
            Route.Add("/", (rq, rp, args) =>
            {
                // Update the sensors.
                computer.Accept(visitor);

                List<DataHardware> dataHardwares = new List<DataHardware>();
                foreach (IHardware hardware in computer.Hardware)
                {
                    DataHardware dataHardware = toHardware(hardware);
                    
                    foreach (IHardware subHardware in hardware.SubHardware)
                    {
                        DataHardware dataSubHardware = toHardware(subHardware);
                        addSensorToSensorType(dataSubHardware, subHardware);
                        dataHardware.SubHardware.Add(dataSubHardware);
                    }

                    addSensorToSensorType(dataHardware, hardware);
                    dataHardwares.Add(dataHardware);
                }
                responseJson(rp, dataHardwares);
            });
        }

        private static void routeList()
        {
            Route.Add("/list", (rq, rp, args) =>
            {
                // Update the sensors.
                computer.Accept(visitor);

                List<DataSensor> dataSensors = new List<DataSensor>();
                foreach (IHardware h in computer.Hardware)
                {
                    foreach (IHardware sh in h.SubHardware)
                    {
                        foreach (ISensor s in sh.Sensors)
                        {
                            dataSensors.Add(toDataSensor(s, sh));
                        }
                    }

                    foreach (ISensor s in h.Sensors)
                    {
                        dataSensors.Add(toDataSensor(s, h));
                    }
                }
                responseJson(rp, dataSensors);
            });
        }

        private static void routeSensor()
        {
            Route.Add((rq, args) =>
            {
                if (rq.Url.AbsolutePath.ToLower().TrimEnd('/') != "/sensor")
                    return false;

                args["identifier"] = rq.QueryString.Get("identifier") ?? "";
                args["identifier"] = args["identifier"].Trim();
                return true;
            },
           (rq, rp, args) =>
           {
               DataSensor dataSensor = new DataSensor();

                // Update the sensors.
               computer.Accept(visitor);

               foreach (IHardware h in computer.Hardware)
               {
                   foreach (IHardware sh in h.SubHardware)
                   {
                       foreach (ISensor s in sh.Sensors)
                       {
                           if (s.Identifier.ToString() == args["identifier"])
                           {
                               dataSensor = toDataSensor(s, sh);
                               goto loop;
                           }
                       }
                   }

                   foreach (ISensor s in h.Sensors)
                   {
                       if (s.Identifier.ToString() == args["identifier"])
                       {
                           dataSensor = toDataSensor(s, h);
                           goto loop;
                       }
                   }
               }
           loop:;

               responseJson(rp, dataSensor);
           });
        }

        private static void routeSensors()
        {
            Route.Add((rq, args) =>
            {
                if (rq.Url.AbsolutePath.ToLower().TrimEnd('/') != "/sensors")
                    return false;

                args["identifiers"] = rq.QueryString.Get("identifiers") ?? "";
                args["identifiers"] = args["identifiers"].Trim();
                return true;
            },
            (rq, rp, args) =>
            {
                var identifiers = args["identifiers"] == "" ? new string[0] : args["identifiers"].Split(',');

                List<DataSensor> dataSensors = new List<DataSensor>();

                // Update the sensors.
                computer.Accept(visitor);

                foreach (IHardware h in computer.Hardware)
                {
                    foreach (IHardware sh in h.SubHardware)
                    {
                        foreach (ISensor s in sh.Sensors)
                        {
                            if (Array.IndexOf(identifiers, s.Identifier.ToString()) > -1)
                            {
                                dataSensors.Add(toDataSensor(s, sh));
                            }
                        }
                    }

                    foreach (ISensor s in h.Sensors)
                    {
                        if (Array.IndexOf(identifiers, s.Identifier.ToString()) > -1)
                        {
                            dataSensors.Add(toDataSensor(s, h));
                        }
                    }
                }

                responseJson(rp, dataSensors);
            });
        }
    }
}
