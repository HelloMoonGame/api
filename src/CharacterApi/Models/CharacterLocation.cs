namespace CharacterApi.Models
{
    public class CharacterLocation
    {
        public int X { get; }
        public int Y { get;  }

        public CharacterLocation(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
