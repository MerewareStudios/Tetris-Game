using System;

namespace HomaGames.OneAsset
{
    internal class EditorAssetFactoryException : Exception
    {
        public EditorAssetFactoryException(Type type, Exception innerException) : base(
            $"Editor failed to create an asset of type:\n{type.FullName}.", innerException)
        {
        }
    }
}