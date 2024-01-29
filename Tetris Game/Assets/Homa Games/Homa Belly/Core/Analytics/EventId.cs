using System;

namespace HomaGames.HomaBelly
{
    public class EventId
    {
        public Guid Id { get; }

        public EventId()
        {
            Id = Guid.NewGuid();
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}