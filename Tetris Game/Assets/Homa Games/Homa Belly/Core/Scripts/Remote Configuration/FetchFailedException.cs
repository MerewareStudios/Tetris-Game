using System;

namespace HomaGames.HomaBelly
{
    internal class FetchFailedException : Exception
    {
        public FetchFailedException(string message = "") : base(message) { }
    }
}
