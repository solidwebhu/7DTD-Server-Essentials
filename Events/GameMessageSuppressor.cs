using OperenciaManager.Core;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public static class GameMessageSuppressor
    {
        public static void Init()
        {
            ModEvents.GameMessage.RegisterHandler(
                new ModEvents.ModEventInterruptibleHandlerDelegate<
                    ModEvents.SGameMessageData>(OnGameMessage)
            );

            Log.Out("[OperenciaManager] GameMessageSuppressor inicializálva.");
        }

        private static ModEvents.EModEventResult OnGameMessage(
            ref ModEvents.SGameMessageData data)
        {
            switch (data.MessageType)
            {
                case EnumGameMessages.JoinedGame:
                case EnumGameMessages.LeftGame:
                case EnumGameMessages.EntityWasKilled:
                    // ❌ gyári üzenet elnyomása
                    return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            return ModEvents.EModEventResult.Continue;
        }
    }
}
