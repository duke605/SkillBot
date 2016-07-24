using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillBot.Extensions {
    static class StringExtensions {

        public static int ToInt(this String str, int defaultTo = -1)
        {
            try
            {
                return Int32.Parse(str, NumberStyles.AllowThousands);
            }
            catch (Exception)
            {
                return defaultTo;
            }
        }
    }
}
