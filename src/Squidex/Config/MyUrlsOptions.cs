﻿// ==========================================================================
//  MyUrlsOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Config
{
    public sealed class MyUrlsOptions
    {
        public bool EnforceHTTPS { get; set; }

        public string BaseUrl { get; set; }

        public string BuildUrl(string path, bool trailingSlash = true)
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new ConfigurationException("Configure BaseUrl with 'urls:baseUrl'.");
            }

            var url = $"{BaseUrl.TrimEnd('/')}/{path.Trim('/')}";

            if (trailingSlash &&
                url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf(";", StringComparison.OrdinalIgnoreCase) < 0)
            {
                url = url + "/";
            }

            return url;
        }
    }
}
