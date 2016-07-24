using System;
using System.Globalization;
using System.Text;

namespace SkillBot.Extensions {
    static class StringExtensions {

        /// <summary>
        /// Converts a string into an in
        /// </summary>
        /// <param name="str">The string that represents and integer</param>
        /// <param name="defaultTo">The return value if the string can not be parsed</param>
        /// <returns>The representation of the string</returns>
        public static int ToInt(this string str, int defaultTo = -1)
        {
            try
            {
                return int.Parse(str, NumberStyles.AllowThousands);
            }
            catch (Exception)
            {
                return defaultTo;
            }
        }

        public static decimal ToDecimal(this string str, decimal defaultTo = -1)
        {
            try {
                return decimal.Parse(str, NumberStyles.AllowThousands);
            } catch (Exception) {
                return defaultTo;
            }
        }

        public static double ToDouble(this string str, double defaultTo = -1) {
            try {
                return double.Parse(str, NumberStyles.AllowThousands);
            } catch (Exception) {
                return defaultTo;
            }
        }

        /// <summary>
        /// Converts a string to a byte array
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>a byte array</returns>
        public static byte[] ToBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}
