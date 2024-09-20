using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public static class Utils
{
    private static readonly EmailAddressAttribute EmailValidation = new();
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        return EmailValidation.IsValid(email);
    }
    public static readonly RandomNumberGenerator Generator = RandomNumberGenerator.Create();
    private static readonly SHA512 SHA = SHA512.Create();
    public static byte[] ToSHA512(this string val)
    {
        return SHA.ComputeHash(Encoding.UTF8.GetBytes(val));
    }
    public static DateTime FromUnixTimestamp(int time)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(time).ToLocalTime();
        return dateTime;
    }
    public static int ToUnixTimestamp(this DateTime dateTime)
        => (int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
}
