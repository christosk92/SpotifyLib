﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SpotifyLib.Helpers;
using SpotifyProto;

namespace SpotifyLib.Exceptions
{
    public class ContentRestrictedException : Exception
    {
        public static void CheckRestrictions([NotNull] string country,
            [NotNull] List<Restriction> restrictions)
        {
            if (restrictions.Any(x => IsRestricted(country, x)))
                throw new ContentRestrictedException();
        }

        private static bool IsInList([NotNull] string list,
            [NotNull] string match)
        {
            for (var i = 0; i < list.Length; i += 2)
                if (list.Substring(i, 2).Equals(match))
                    return true;

            return false;
        }

        private static bool IsRestricted(
            [NotNull] string countryCode,
            [NotNull] Restriction restriction)
        {
            if (!restriction.HasCountriesAllowed)
                return restriction.HasCountriesForbidden
                       && IsInList(restriction.CountriesForbidden, countryCode);
            var allowed = restriction.CountriesAllowed;
            if (allowed.IsEmpty()) return true;

            if (!IsInList(restriction.CountriesForbidden, countryCode))
                return true;
            return restriction.HasCountriesForbidden
                   && IsInList(restriction.CountriesForbidden, countryCode);
        }
    }
}