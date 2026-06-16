using OperenciaManager.Commands;

namespace OperenciaManager.Commands
{
    public class InfoCommand : IChatCommand
    {
        public string Name => "/info";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            string message = "OperenciaManager ModAPI v0.2.5 by Jim";
            Core.ChatHook.SendPrivate(clientInfo, message);
        }
    }
}
