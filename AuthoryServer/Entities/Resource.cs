namespace AuthoryServer.Entities
{
    public class Resource
    {
        public int MaxValue { get; set; }
        public int Value { get; set; }
        public int RegenValue { get; set; }

        public void SetFull()
        {
            Value = MaxValue;
        }

        public bool IsFull()
        {
            return MaxValue == Value;
        }
    }
}
