using System;

namespace HomaGames.Geryon
{
    public class InvalidTypeParameterException : Exception
    {
        public InvalidTypeParameterException()
        {
        }

        public InvalidTypeParameterException(string message) : base(message)
        {
        }
    }
}