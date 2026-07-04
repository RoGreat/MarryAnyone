using System.Diagnostics;

using TaleWorlds.Library;

namespace MarryAnyone
{
    public static class Log
    {
        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message));
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message, new Color(1f, 1f, 0f, 1f)));
        }

        [Conditional("DEBUG")]
        public static void Error(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message, new Color(1f, 0f, 0f, 1f)));
        }

    }
}
