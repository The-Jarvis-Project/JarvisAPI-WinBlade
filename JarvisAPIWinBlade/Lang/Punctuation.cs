namespace Jarvis.API.Lang
{
    /// <summary>
    /// Class to represent a punctuation mark
    /// </summary>
    public class Punctuation
    {
        /// <summary>
        /// How the punctuation mark is used in a sentence.
        /// </summary>
        public enum Usage
        {
            /// <summary>
            /// The punctuation mark is supposed to be here.
            /// </summary>
            Intentional,

            /// <summary>
            /// The punctuation mark isn't supposed to be here.
            /// </summary>
            Accidental
        }

        /// <summary>
        /// Where the punctuation mark is in a sentence.
        /// </summary>
        public enum Location
        {
            /// <summary>
            /// The punctuation is in front of the text.
            /// </summary>
            Beginning,

            /// <summary>
            /// The punctuation is in the text.
            /// </summary>
            Middle,

            /// <summary>
            /// The punctuation is at the end of the text.
            /// </summary>
            End
        }

        /// <summary>
        /// The punctuation mark.
        /// </summary>
        public char Mark { get; }

        /// <summary>
        /// How this punctuation mark is used in the sentence.
        /// </summary>
        public Usage Use { get; }

        /// <summary>
        /// Where the punctuation mark is in the sentence.
        /// </summary>
        public Location Loc { get; }

        /// <summary>
        /// Creates a new punctuation object.
        /// </summary>
        /// <param name="mark">The punctuation mark character</param>
        /// <param name="usage">How the punctuation mark is used</param>
        /// <param name="location">Where the punctuation mark is</param>
        public Punctuation(char mark, Usage usage, Location location)
        {
            Mark = mark;
            Use = usage;
            Loc = location;
        }
    }
}
