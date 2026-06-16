using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OperenciaManager.Commands;

namespace OperenciaManager.Core
{
    public static class CommandProcessor
    {
        private static readonly Dictionary<string, IChatCommand> Commands = new Dictionary<string, IChatCommand>();

        /// <summary>
        /// Regisztrálja az összes elérhető parancsot.
        /// </summary>
        public static void RegisterCommands()
        {
            var commandType = typeof(IChatCommand);
            var commands = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
            {
                return !type.IsAbstract && !type.IsInterface && commandType.IsAssignableFrom(type);
            });

            foreach (var command in commands)
            {
                var instance = (IChatCommand)Activator.CreateInstance(command);
                Register(instance);
            }

            // Itt bővítheted további parancsokkal: Register(new HelpCommand()), stb.
            Log.Out("[OperenciaManager] Parancsok regisztrálva: " + string.Join(", ", Commands.Keys));
        }

        /// <summary>
        /// Egy parancs regisztrálása a rendszerbe.
        /// </summary>
        private static void Register(IChatCommand command)
        {
            if (!Commands.TryGetValue(command.Name.ToLower(), out var _))
            {
                Commands.Add(command.Name.ToLower(), command);
            }
        }

        /// <summary>
        /// Feldolgozza a beérkező chat parancsot.
        /// </summary>
        public static ModEvents.EModEventResult HandleCommand(ref ModEvents.SChatMessageData data)
        {
            var message = data.Message;
            var clientInfo = data.ClientInfo;
            if (string.IsNullOrWhiteSpace(message) || !message.StartsWith("/"))
                return ModEvents.EModEventResult.Continue;

            var parts = message.Trim().Split(' ');
            var cmdName = parts[0].ToLower();

            if (Commands.TryGetValue(cmdName, out var command))
            {
                // Csak az argumentumokat adjuk át
                var args = parts.Skip(1).ToArray();
                command.Execute(clientInfo, args);
                return ModEvents.EModEventResult.StopHandlersAndVanilla;
            }

            return ModEvents.EModEventResult.Continue;
        }

    }
}
