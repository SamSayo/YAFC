namespace YAFC.GameCarts
{
    public static class GameCartFactory
    {
        public static IGameCart Create(string cartPath)
        {
            var ext = Path.GetExtension(cartPath).ToLower();

            return ext switch
            {
                ".yafc" => new ArchiveGameCart(cartPath),
                _ => new DirGameCart(cartPath)
            };
        }
    }
}
