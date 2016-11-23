﻿// ==========================================================================
//  DocsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Squidex.Modules.Api.Docs
{
    [SwaggerIgnore]
    public sealed class DocsController : Controller
    {
        [HttpGet]
        [Route("docs/")]
        public IActionResult Docs()
        {
            return View();
        }
    }
}