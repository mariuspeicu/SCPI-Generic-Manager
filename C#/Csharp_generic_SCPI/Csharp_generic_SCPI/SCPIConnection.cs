using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp_generic_SCPI
{
    interface SCPIConnection
    {        
        void Connect(int timeOutMs);
        void Disconnect();
        string Query(string command);
        void Write(string command);
        void SetTimeout(int timeOutMs);
    }
}
