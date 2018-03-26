
namespace System
{
    public static class StringExtenions
    {
        public static bool IsEmpty(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return true;
            }

            return false;
        }

        public static bool IsNotEmpty(this string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            return true;
        }
    }
}
