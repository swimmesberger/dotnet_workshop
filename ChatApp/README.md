# ChatApp

TODO:
* Implement durable mailboxes for actors
  * Use database queueing like in masstransit
* Add source generator to generate actor calling based on interface methods to simplify actor usage and implementation
* Add support for actor hierarchy
* Add support for actor supervision
* Do not create service scopes based on the class constructor but use FromServices attribute for singleton state without abstraction??
