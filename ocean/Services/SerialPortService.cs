using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ocean.Interfaces;
namespace ocean.Services
{
    public class SerialPortService : ISerialPortService
    {
        public List<string> GetPortNames()
        {
            return SerialPort.GetPortNames().ToList();
        }
    }
}
