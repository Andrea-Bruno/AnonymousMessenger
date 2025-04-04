﻿using Foundation;

namespace Cryptogram.iOS.Call
{
    public static class NSDataExtensions
    {
        public static NSData ToNSData(this string message)
        {
            return NSData.FromString(message, NSStringEncoding.UTF8);
        }

        public static string FromNSData(this NSData data)
        {
            var stringRepresentation = data.ToString(NSStringEncoding.UTF8);
            return stringRepresentation;
        }
    }
}