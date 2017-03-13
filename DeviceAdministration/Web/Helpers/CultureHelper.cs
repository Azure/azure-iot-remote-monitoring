// ---------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Models;

    public sealed class CultureHelper
    {
        private static readonly IEnumerable<string> ValidCultures = new[] { "af", "af-ZA", "sq", "sq-AL", "gsw-FR", "am-ET", "ar", "ar-DZ", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE", "hy", "hy-AM", "as-IN", "az", "az-Cyrl-AZ", "az-Latn-AZ", "ba-RU", "eu", "eu-ES", "be", "be-BY", "bn-BD", "bn-IN", "bs-Cyrl-BA", "bs-Latn-BA", "br-FR", "bg", "bg-BG", "ca", "ca-ES", "zh-HK", "zh-MO", "zh-CN", "zh-Hans", "zh-SG", "zh-TW", "zh-Hant", "co-FR", "hr", "hr-HR", "hr-BA", "cs", "cs-CZ", "da", "da-DK", "prs-AF", "div", "div-MV", "nl", "nl-BE", "nl-NL", "en", "en-AU", "en-BZ", "en-CA", "en-029", "en-IN", "en-IE", "en-JM", "en-MY", "en-NZ", "en-PH", "en-SG", "en-ZA", "en-TT", "en-GB", "en-US", "en-ZW", "et", "et-EE", "fo", "fo-FO", "fil-PH", "fi", "fi-FI", "fr", "fr-BE", "fr-CA", "fr-FR", "fr-LU", "fr-MC", "fr-CH", "fy-NL", "gl", "gl-ES", "ka", "ka-GE", "de", "de-AT", "de-DE", "de-LI", "de-LU", "de-CH", "el", "el-GR", "kl-GL", "gu", "gu-IN", "ha-Latn-NG", "he", "he-IL", "hi", "hi-IN", "hu", "hu-HU", "is", "is-IS", "ig-NG", "id", "id-ID", "iu-Latn-CA", "iu-Cans-CA", "ga-IE", "xh-ZA", "zu-ZA", "it", "it-IT", "it-CH", "ja", "ja-JP", "kn", "kn-IN", "kk", "kk-KZ", "km-KH", "qut-GT", "rw-RW", "sw", "sw-KE", "kok", "kok-IN", "ko", "ko-KR", "ky", "ky-KG", "lo-LA", "lv", "lv-LV", "lt", "lt-LT", "wee-DE", "lb-LU", "mk", "mk-MK", "ms", "ms-BN", "ms-MY", "ml-IN", "mt-MT", "mi-NZ", "arn-CL", "mr", "mr-IN", "moh-CA", "mn", "mn-MN", "mn-Mong-CN", "ne-NP", "no", "nb-NO", "nn-NO", "oc-FR", "or-IN", "ps-AF", "fa", "fa-IR", "pl", "pl-PL", "pt", "pt-BR", "pt-PT", "pa", "pa-IN", "quz-BO", "quz-EC", "quz-PE", "ro", "ro-RO", "rm-CH", "ru", "ru-RU", "smn-FI", "smj-NO", "smj-SE", "se-FI", "se-NO", "se-SE", "sms-FI", "sma-NO", "sma-SE", "sa", "sa-IN", "sr", "sr-Cyrl-BA", "sr-Cyrl-SP", "sr-Latn-BA", "sr-Latn-SP", "nso-ZA", "tn-ZA", "si-LK", "sk", "sk-SK", "sl", "sl-SI", "es", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR", "es-DO", "es-EC", "es-SV", "es-GT", "es-HN", "es-MX", "es-NI", "es-PA", "es-PY", "es-PE", "es-PR", "es-ES", "es-US", "es-UY", "es-VE", "sv", "sv-FI", "sv-SE", "syr", "syr-SY", "tg-Cyrl-TJ", "tzm-Latn-DZ", "ta", "ta-IN", "tt", "tt-RU", "te", "te-IN", "th", "th-TH", "bo-CN", "tr", "tr-TR", "tk-TM", "ug-CN", "uk", "uk-UA", "wen-DE", "ur", "ur-PK", "uz", "uz-Cyrl-UZ", "uz-Latn-UZ", "vi", "vi-VN", "cy-GB", "wo-SN", "sah-RU", "ii-CN", "yo-NG" };
        private static readonly IEnumerable<string> ImplementedCultureNames = new[] { "cs", "de", "en", "es", "fr", "hu", "it", "ja", "nl", "pl", "pt-BR", "pt-PT", "ru", "sv", "tr", "zh-Hans", "zh-Hant" };

        public static CultureInfo GetClosestCulture(string cultureName)
        {
            // make sure it's not null or empty
            if (string.IsNullOrEmpty(cultureName))
            {
                return GetDefaultCulture(); // return Default culture
            }

            // make sure it is a valid culture first
            if (!ValidCultures.Any(culture => culture.Equals(cultureName, StringComparison.Ordinal)))
            {
                return GetDefaultCulture(); // return Default culture if it is invalid
            }

            // Get the neutral culture of specific culture passed in
            var neutralCultureName = GetNeutralCultureName(cultureName);

            // If the neutral culture is implemented, accept the specific culture
            if (ImplementedCultureNames.Any(culture => culture.StartsWith(neutralCultureName, StringComparison.Ordinal)))
            {
                return new CultureInfo(cultureName); // accept it
            }

            // Find a close match. For example, if you have "en-US" defined and the user requests "en-GB", 
            // the function will return closes match that is "en-US" because at least the language is the same (ie English)  
            var closestCultureName = ImplementedCultureNames.FirstOrDefault(culture => culture.StartsWith(neutralCultureName, StringComparison.Ordinal));

            return closestCultureName != null ? new CultureInfo(closestCultureName) : GetDefaultCulture();
        }

        public static CultureInfo GetDefaultCulture()
        {
            // The first culture in implemented list is cs, change default culture to en if it's exist
            var defaultCulture = "en";

            if (ImplementedCultureNames.Any(x => x.Equals(defaultCulture, StringComparison.OrdinalIgnoreCase)))
            {
                return new CultureInfo(defaultCulture);
            }
            else
            {
                return new CultureInfo(ImplementedCultureNames.First());
            }
        }

        public static CultureInfo GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture;
        }

        public static string GetNeutralCultureName(string name)
        {
            if (!name.Contains("-"))
            {
                return name;
            }

            return name.Split('-')[0]; // Read first part only. E.g. "en", "es"
        }

        public static IEnumerable<CultureInfo> GetImplementedCultures()
        {
            var cultureName = new Collection<CultureInfo>();

            foreach (var culture in ImplementedCultureNames)
            {
                cultureName.Add(new CultureInfo(culture));
            }

            return cultureName;
        }

        public static IEnumerable<LanguageModel> GetLanguages()
        {
            var currentCulture = GetCurrentCulture();
            var languages = new Collection<LanguageModel>();

            foreach (var culture in GetImplementedCultures())
            {
                languages.Add(new LanguageModel
                {
                    Name = culture.NativeName,
                    CultureName = culture.Name,
                    IsCurrent = culture.Name == currentCulture.Name
                });
            }

            return languages;
        }
    }
}