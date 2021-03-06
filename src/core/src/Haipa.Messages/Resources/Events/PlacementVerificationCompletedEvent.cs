﻿using System;

namespace Haipa.Messages.Resources.Events
{
    [SubscribesMessage(MessageSubscriber.Controllers)]
    public class PlacementVerificationCompletedEvent
    {
        public Guid CorrelationId { get; set; }
        public string AgentName { get; set; }
        public bool Confirmed { get; set; }
    }
}