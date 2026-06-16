using System.Text;

namespace OperenciaManager.Commands
{
    public class ListEntityCommand : IChatCommand
    {
        public string Name => "/listentity";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            // Natív admin szint lekérdezés
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            
            if (level > 2)
            {
                Core.ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.[-]");
                return;
            }

            var entities = GameManager.Instance.World?.Entities?.list;
            if (entities == null || entities.Count == 0)
            {
                Core.ChatHook.SendPrivate(clientInfo, "[FFFF00]Nincs aktív entity a világban.[-]");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[00FF00]Aktív entity-k listája:[-]");

            foreach (var entity in entities)
            {
                string type = entity.GetType().Name;
                int id = entity.entityId;
                string name = entity is EntityPlayer player ? player.EntityName : type;

                sb.AppendLine($"• [999999]ID: {id}[-] | [00CCCC]Típus: {type}[-] | [FFFFFF]Név: {name}[-]");
            }

            Core.ChatHook.SendPrivate(clientInfo, sb.ToString());
        }
    }
}
