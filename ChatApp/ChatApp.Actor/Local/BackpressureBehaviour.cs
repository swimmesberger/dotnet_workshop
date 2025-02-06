namespace ChatApp.Actor.Local;

public enum BackpressureBehaviour {
    None,
    /// <summary>Throws an exception if the channel is full.</summary>
    FailFast,
    /// <summary>Waits for space to be available in order to complete the write operation.</summary>
    Wait,
    /// <summary>Removes and ignores the newest item in the channel in order to make room for the item being written.</summary>
    DropNewest,
    /// <summary>Removes and ignores the oldest item in the channel in order to make room for the item being written.</summary>
    DropOldest,
    /// <summary>Drops the item being written.</summary>
    DropWrite
}
