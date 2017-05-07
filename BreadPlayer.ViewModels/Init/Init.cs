using BreadPlayer.Helpers.Interfaces;

namespace BreadPlayer.ViewModels
{
    public class Init
    {
        public static ISharedLogic SharedLogic { get; set; }
        public Init(ISharedLogic sharedLogic)
        {
            SharedLogic = sharedLogic;
        }
    }
}
