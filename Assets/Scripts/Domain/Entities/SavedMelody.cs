namespace Assets.Scripts.Domain.Entities
{
    public sealed class SavedMelody
    {
        public Melody Melody { get; }
        public int Position { get; private set; }

        public bool IsProtected => Melody.IsProtected;

        public bool IsUserCreated => Melody.IsUserCreated;

        public SavedMelody(Melody melody, int position)
        {
            Melody = melody;
            Position = position;
        }

        public void SetPosition(int position) => Position = position;
    }
}
