using FusionLibrary;

namespace BackToTheFutureV
{
    public class TextHandler : CustomText
    {
        public static TextHandler Me { get; } = new TextHandler();

        public TextHandler() : base("BTTFV")
        {

        }
    }
}
