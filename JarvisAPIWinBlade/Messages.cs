using Jarvis.API.Lang;

namespace Jarvis.API
{
    /// <summary>
    /// Hard-Coded functions for getting attributes of Blade Messages.
    /// </summary>
    public static class Messages
    {
        /// <summary>
        /// Creates the message that is used internally when finding message attributes.
        /// </summary>
        /// <param name="msg">The original message</param>
        /// <returns>The raw message</returns>
        public static BladeMsg GetRaw(BladeMsg msg) =>
            new BladeMsg()
            {
                Origin = msg.Origin,
                Data = Raw(msg)
            };

        /// <summary>
        /// Tests if a message is most likely a question (Case Insensitive).
        /// </summary>
        /// <param name="msg">The message to check</param>
        /// <returns>Whether or not the message is most likely a question</returns>
        public static bool IsQuestion(BladeMsg msg)
        {
            string raw = Raw(msg) ?? string.Empty;
            return raw.Contains('?') ||
                raw.StartsWith("who") ||
                raw.StartsWith("what") ||
                raw.StartsWith("why") ||
                raw.StartsWith("where") ||
                raw.StartsWith("when") ||
                raw.StartsWith("which") ||
                raw.StartsWith("how");
        }

        /// <summary>
        /// Checks if a message has certain keywords,
        /// if it contains all of them it will return true (Case Insensitive).
        /// </summary>
        /// <param name="msg">The message to check</param>
        /// <param name="keywords">The keywords to check for</param>
        /// <returns>Whether or not the message contains all the keywords</returns>
        public static bool HasKeywords(BladeMsg msg, string[] keywords)
        {
            string[]? words = GetWords(msg);
            if (words != null)
            {
                for (int i = 0; i < keywords.Length; i++)
                {
                    bool hasKeyword = false;
                    for (int w = 0; w < words.Length; w++)
                    {
                        if (keywords[i] == words[w])
                        {
                            hasKeyword = true;
                            break;
                        }
                    }
                    if (!hasKeyword) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the raw words in a message.
        /// </summary>
        /// <param name="msg">The message to split up</param>
        /// <returns>All of the raw words in the message</returns>
        private static string[]? GetWords(BladeMsg msg) =>
            Raw(msg)?.Split(Word.separators, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Lowercases and trims a message.
        /// </summary>
        /// <param name="msg">The message to convert</param>
        /// <returns>A raw string of the message's data</returns>
        private static string? Raw(BladeMsg msg) => Polish(msg).Data?.ToLower().Trim();

        /// <summary>
        /// Removes irrelevant characters from the message.
        /// </summary>
        /// <param name="msg">The message to fix</param>
        /// <returns>The cleaned up message</returns>
        public static BladeMsg Polish(BladeMsg msg)
        {
            msg.Data = msg.Data?.Replace(Environment.NewLine, "\n");
            return msg;
        }
    }
}
