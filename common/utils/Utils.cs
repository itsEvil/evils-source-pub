using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace common.utils;

public static class Utils {
    public static DateTime FromUnixTimestamp(int time)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(time).ToLocalTime();
        return dateTime;
    }
    public static int ToUnixTimestamp(this DateTime dateTime)
        => (int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;

    private static readonly EmailAddressAttribute EmailValidation = new();
    public static bool IsValidEmail(string email) {
        if (string.IsNullOrEmpty(email))
            return false;

        return EmailValidation.IsValid(email);
    }
    private static readonly SHA256 SHA256 = SHA256.Create();
    public static byte[] ToSHA256(this string val) {
        return SHA256.ComputeHash(Encoding.UTF8.GetBytes(val));
    }

    public static readonly RandomNumberGenerator Generator = RandomNumberGenerator.Create();
}
