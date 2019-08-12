namespace Microsoft.FeatureManagement.Utils
{
    static class StringExtensions
    {
        //
        // https://github.com/dotnet/corefx/blob/a10890f4ffe0fadf090c922578ba0e606ebdd16c/src/Common/src/System/Text/StringOrCharArray.cs#L140
        /// <summary>
        /// Provides a stable hash code for a given string
        /// </summary>
        /// <param name="str">The string to hash.</param>
        /// <returns>A hash code representing the provided string.</returns>
        public static int GetStableHashCode(this string str)
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; ++i)
            {
                int c = str[i];

                hash1 = unchecked((hash1 << 5) + hash1) ^ c;

                i++;

                if (i >= str.Length)
                {
                    break;
                }

                c = str[i];

                hash2 = unchecked((hash2 << 5) + hash2) ^ c;
            }

            return unchecked(hash1 + (hash2 * 1566083941));
        }
    }
}
