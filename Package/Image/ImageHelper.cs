﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTap.Package
{
    internal static class ImageHelper
    {
        internal static ImageSpecifier GetImageFromString(string value)
        {
            if (value.IsXml())
            {
                return ImageXmlSerializer.DeserializeImageSpecifier(value);
            }
            if (value.IsJson())
            {
                return ImageJsonSerializer.DeserializeImageSpecifier(value);
            }
            if (ParseCommaSeparated(value) is ImageSpecifier r)
                return r;
            throw new FormatException("Image specifier could not be read.");
        }

        static bool IsJson(this string jsonData)
        {
            return jsonData.Trim().Substring(0, 1).IndexOfAny(new[] { '[', '{' }) == 0;
        }

        static bool IsXml(this string xmlData)
        {
            return xmlData.Trim().Substring(0, 1).IndexOfAny(new[] { '<' }) == 0;
        }
        static ImageSpecifier ParseCommaSeparated(this string imageString)
        {
            var pkgStrings = imageString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            var list = new List<PackageSpecifier>();
            foreach (var pkg in pkgStrings)
            {
                var pkgInfo = pkg.Trim().Split(':').Select(x => x.Trim()).ToArray();
                string pkgName = pkgInfo.FirstOrDefault();
                string pkgVersion = pkgInfo.Skip(1).FirstOrDefault();
                if (pkgInfo.Skip(2).Any())
                    return null;
                list.Add(new PackageSpecifier(pkgName, string.IsNullOrWhiteSpace(pkgVersion) ? VersionSpecifier.AnyRelease : VersionSpecifier.Parse(pkgVersion)));
            }

            if (list.Count == 0) return null;
            return new ImageSpecifier(list);
        }
    }

}
