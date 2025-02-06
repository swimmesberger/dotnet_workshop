using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Common.Actor.Local;

/// <summary>
/// (Thread-safe) Registry of actors.
/// </summary>
public sealed class LocalActorRegistry {
    private readonly List<LocalActorCell> _actors = [];

    public void Register(LocalActorCell actorCell) {
        lock (_actors) {
            _actors.Add(actorCell);
        }
    }

    public void Unregister(LocalActorCell actorCell) {
        lock (_actors) {
            _actors.Remove(actorCell);
        }
    }

    public List<LocalActorCell> CopyAndClear() {
        lock (_actors) {
            var actors = _actors.ToList();
            _actors.Clear();
            return actors;
        }
    }

    public void Clear() {
        lock (_actors) {
            _actors.Clear();
        }
    }

    public IActorRef? GetActor(Type actorType, string? id = null) {
        lock (_actors) {
            IEnumerable<LocalActorCell> cells = _actors;
            if (id != null) {
                cells = cells.Where(x => x.Context.Id.Equals(id)).ToList();
            }
            return cells.SingleOrDefault(x => x.ActorType == actorType);
        }
    }
}
