using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperenciaManager.Commands
{
    public interface IChatCommand
    {
        string Name { get; }
        void Execute(ClientInfo clientInfo, string[] args);
    }
}
