using FusionLibrary;

namespace BackToTheFutureV
{
    internal class TextHandler : CustomText
    {
        public static TextHandler Me { get; } = new TextHandler();

        public TextHandler() : base("BTTFV")
        {

        }
    }
}
