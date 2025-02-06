namespace ChatApp.Common.Actors;

public interface IActorRef {
    public static readonly IActorRef Nobody = new NobodyInner();

    void Tell(IMessage message, IActorRef? sender = null);

    private sealed class NobodyInner : IActorRef {
        public void Tell(IMessage message, IActorRef? sender = null) {
            // Do nothing
        }
    }
}
