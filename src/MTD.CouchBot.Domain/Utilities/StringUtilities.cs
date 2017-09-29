namespace MTD.CouchBot.Domain.Utilities
{
    public static class StringUtilities
    {
        public static string ScrubChatMessage(string message)
        {
            return message.Replace("_", "\\_").Replace("*", "\\*");
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }

            return str.ToUpper();
        }
    }
}
