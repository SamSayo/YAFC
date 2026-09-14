using Zio;

namespace YAFC.GameCarts
{
    public interface IGameCart : IDisposable
    {
        /// <summary>
        /// File system with GameCart contents
        /// Root is "/"
        /// </summary>
        IFileSystem fs { get; }

        /// <summary>
        /// Main GameCart's lua script
        /// </summary>
        UPath MainScript { get; }
        /// <summary>
        /// Sprite sheet
        /// </summary>
        UPath SpriteSheet { get; }
        /// <summary>
        /// Tracker music in XM (MilkyTracker) format
        /// </summary>
        UPath Music { get; }
        /// <summary>
        /// GameCart's metadata
        /// </summary>
        UPath Metadata { get; }
    }
}
