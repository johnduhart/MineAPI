namespace MineAPI.Protocol
{
    public static class IntExtensions
    {
        public static int GetVarIntLength(this int value)
        {
            int size = 0;

            while (true)
            {
                if ((value & 0xFFFFFF80u) == 0)
                {
                    size++;
                    break;
                }
                size++;
                value >>= 7;
            }

            return size;
        }
    }
}