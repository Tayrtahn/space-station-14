namespace Content.Shared.Speech
{
    public sealed partial class SpeechSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public void SetSpeech(EntityUid uid, bool value, SpeechComponent? component = null)
        {
            if (value && !Resolve(uid, ref component))
                return;

            component = EnsureComp<SpeechComponent>(uid);

            if (component.Enabled == value)
                return;

            component.Enabled = value;

            Dirty(uid, component);
        }

        [SubscribeLocalEvent]
        private void OnSpeakAttempt(SpeakAttemptEvent args)
        {
            if (!TryComp(args.Uid, out SpeechComponent? speech) || !speech.Enabled)
                args.Cancel();
        }
    }
}
