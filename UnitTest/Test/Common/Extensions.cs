namespace Test.Common
{
    public static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            return false;
        }
    }
}