﻿// ==========================================================================
//  AppMasterLanguageSet.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppMasterLanguageSetEvent")]
    public sealed class AppMasterLanguageSet : AppEvent
    {
        public Language Language { get; set; }
    }
}
