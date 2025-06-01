using System;
using System.Globalization;
using System.Linq;

public static class StringUtility
{
    public static string ConvertDoubleToString(double input, string currency = "usd")
    {
        string currencyCode = currency.ToLower();
        CultureInfo cultureInfo = new CultureInfo(currencyCode != "idr" ? "en-US" : "id-ID");
        string formattedNumber = input.ToString("N0", cultureInfo);
        return formattedNumber;
    }

    public static string ConvertDoubleToStringWithDecimal(double input, string currency = "usd")
    {
        string currencyCode = currency.ToLower();
        CultureInfo cultureInfo = new CultureInfo(currencyCode != "idr" ? "en-US" : "id-ID");
        string formattedNumber = input.ToString("N1", cultureInfo);
        return formattedNumber;
    }

    public static int ConvertStringToInteger(string numberString, int falloutValue = 0)
    {
        CultureInfo cultureInfo = new CultureInfo("en-US");
        int number;

        if (Int32.TryParse(numberString, NumberStyles.AllowThousands, cultureInfo, out number))
        {
            return number;
        }
        else
        {
            return falloutValue;
        }
    }

    public static int[] ConvertStringToIntArray(string input)
    {
        return input.Split(',')
                    .Select(int.Parse)
                    .ToArray();
    }
}
