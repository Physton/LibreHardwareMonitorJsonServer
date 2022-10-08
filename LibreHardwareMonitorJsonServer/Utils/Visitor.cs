using LibreHardwareMonitor.Hardware;

namespace LibreHardwareMonitorJsonServer
{
    // This is a copy-paste of the Visitor from LibreHardwareMonitor's GUI project.
    class Visitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();

            foreach (IHardware subHardware in hardware.SubHardware)
            {
                subHardware.Accept(this);
            }
        }

        public void VisitParameter(IParameter parameter) { }
        public void VisitSensor(ISensor sensor) { }
    }
}
