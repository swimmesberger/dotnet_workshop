namespace ChatApp.Actor.Abstractions;

public static class CanTellExtensions {
    public static void Tell(this ICanTell actorRef, IMessage message, IActorRef? sender = null) {
        actorRef.Tell(message, new RequestOptions {
            Sender = sender
        });
    }
}
