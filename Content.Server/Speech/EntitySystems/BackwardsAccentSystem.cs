using Content.Server.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server.Speech.EntitySystems
{
    public sealed partial class BackwardsAccentSystem : EntitySystem
    {
        public override void Initialize()
        {
        }

        public string Accentuate(string message)
        {
            var arr = message.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        [SubscribeLocalEvent]
        private void OnAccent(EntityUid uid, BackwardsAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
