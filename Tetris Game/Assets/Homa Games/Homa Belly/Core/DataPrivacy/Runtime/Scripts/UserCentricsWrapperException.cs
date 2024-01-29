using System;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class UserCentricsWrapperException : Exception
    {
        public UserCentricsWrapperException()
        {
        }

        public UserCentricsWrapperException(string message) : base(message)
        {
        }

        public UserCentricsWrapperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}