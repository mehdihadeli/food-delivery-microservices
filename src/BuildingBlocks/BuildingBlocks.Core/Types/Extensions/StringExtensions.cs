using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BuildingBlocks.Core.Types.Extensions;

public static class StringExtensions
{
    public static T ConvertTo<T>(this object input)
    {
        return ConvertTo<T>(input.ToString());
    }

    public static T ConvertTo<T>(this string input)
    {
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(input);
        }
        catch (NotSupportedException)
        {
            return default;
        }
    }

    public static bool IsValidJson(this string strInput)
    {
        if (string.IsNullOrWhiteSpace(strInput))
        {
            return false;
        }

        strInput = strInput.Trim();
        if (
            (strInput.StartsWith("{", StringComparison.Ordinal) && strInput.EndsWith("}", StringComparison.Ordinal))
            || (strInput.StartsWith("[", StringComparison.Ordinal) && strInput.EndsWith("]", StringComparison.Ordinal))
        )
        {
            try
            {
                var obj = JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException jex)
            {
                Console.WriteLine(jex.Message);
                return false;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        return false;
    }

    // Ref: https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
    public static int GetDeterministicHashCode(this string str)
    {
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}
