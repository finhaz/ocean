using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocean.Interfaces
{
    // 纯接口，无任何硬件依赖
    // 接口：无任何依赖
    public interface ISerialPortService
    {
        List<string> GetPortNames();
    }
}
