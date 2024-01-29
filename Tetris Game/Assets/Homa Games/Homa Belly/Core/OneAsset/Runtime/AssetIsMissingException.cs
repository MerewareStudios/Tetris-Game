using System;

namespace HomaGames.OneAsset
{
    internal class AssetIsMissingException : Exception
    {
        internal AssetIsMissingException(Type assetType, string assetPath) : base(
            $"Asset of type {assetType} is missing from path: {assetPath}")
        {
        }
    }
}