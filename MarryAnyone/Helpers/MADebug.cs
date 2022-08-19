using System;
using TaleWorlds.Library;

namespace MarryAnyone.Helpers
{
    internal static class MADebug
    {
        public static void Print(string message)
        {
            MASettings settings = new();
            if (settings.Debug)
            {
                Color color = new(0.6f, 0.2f, 1f);
                InformationManager.DisplayMessage(new InformationMessage($"Marry Anyone: {message}", color));
            }
        }

        public static void Error(Exception exception)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Marry Anyone: {exception.Message}", Colors.Red));
        }
    }
}