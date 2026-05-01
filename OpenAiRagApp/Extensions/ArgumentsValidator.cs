using static OpenAiRagApp.Extensions.Constants;

namespace OpenAiRagApp.Extensions
{
    static class ArgumentsValidator
    {
        public static bool ValidateArguments(string[] args, out string action, out bool isSeedingNeeded)
        {
            string firstArg = args?.Length > 0 ? args[0] : string.Empty;

            if (Enum.TryParse(firstArg, true, out Mode mode))
                action = mode.ToString();
            else
                action = Mode.INVALID.ToString();

            isSeedingNeeded = args?.Length > 1 && args[1] == "1";

            return action != Mode.INVALID.ToString();
        }
    }
}
