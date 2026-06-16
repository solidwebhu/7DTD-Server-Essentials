using OperenciaManager.Commands;
using OperenciaManager.Core;
using OperenciaManager.Systems;

public class ClaimRepairToggleCommand : IChatCommand
{
    public string Name { get { return "/javitas"; } }

    public void Execute(ClientInfo clientInfo, string[] args)
    {
        if (clientInfo == null)
            return;

        string crossId = clientInfo.CrossplatformId.CombinedString;

        bool newState = ClaimRepairState.Toggle(crossId);

        if (newState)
        {
            ChatHook.SendPrivate(clientInfo, "[00FF00]Claim javítás bekapcsolva.");
        }
        else
        {
            ChatHook.SendPrivate(clientInfo, "[FF0000]Claim javítás kikapcsolva.");
        }
    }
}
