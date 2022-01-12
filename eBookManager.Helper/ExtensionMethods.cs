using eBookManager.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBookManager.Helper
{
    public static class ExtensionMethods
    {
        //ToInt() class extension method to take a string value and try parse it to an integer value
        public static int ToInt(this string value, int defaultInteger = 0)
        {
            try
            {
                if (int.TryParse(value, out int validInteger))
                {
                    //Out variables
                    return validInteger;
                }
                else
                {
                    return defaultInteger;
                }
            }
            catch
            {

                return defaultInteger;
            }
        }
    }
}
