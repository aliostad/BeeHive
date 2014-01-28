BeeHive
=======

An Actor Helper Library for Windows Azure. Implementation is very simple - if you need a complex implementation of the Actor Model, fat chance you are doing it wrong.

This library helps implementing purely the business logic and the rest is taken care of.

Business process is broken down into a series/cascase/tree of events where each step only knows the event it is consuming and the event(s) it is publishing. Actors define the name of the queue they are supposed to read from (in the form of [QueueName] for Azure Service Bus Queues or [[TopicName]-[SubscriptionName]] for Azure Topics) and system 

Error handling and retry mechanism is under development.
