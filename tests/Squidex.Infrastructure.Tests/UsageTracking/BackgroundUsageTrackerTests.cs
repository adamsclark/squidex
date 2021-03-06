﻿// ==========================================================================
//  BackgroundUsageTrackerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.UsageTracking
{
    public class BackgroundUsageTrackerTests
    {
        private readonly Mock<IUsageStore> usageStore = new Mock<IUsageStore>();
        private readonly Mock<ISemanticLog> log = new Mock<ISemanticLog>();
        private readonly BackgroundUsageTracker sut;

        public BackgroundUsageTrackerTests()
        {
            sut = new BackgroundUsageTracker(usageStore.Object, log.Object);
        }

        [Fact]
        public Task Should_throw_exception_if_tracking_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.TrackAsync("key1", 1, 1000));
        }

        [Fact]
        public Task Should_throw_exception_if_querying_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.QueryAsync("key1", DateTime.Today, DateTime.Today.AddDays(1)));
        }

        [Fact]
        public Task Should_throw_exception_if_querying_montly_usage_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.GetMonthlyCalls("key1", DateTime.Today));
        }

        [Fact]
        public async Task Should_sum_up_when_getting_monthly_calls()
        {
            var date = new DateTime(2016, 1, 15);

            IReadOnlyList<StoredUsage> originalData = new List<StoredUsage>
            {
                new StoredUsage(date.AddDays(1), 10, 15),
                new StoredUsage(date.AddDays(3), 13, 18),
                new StoredUsage(date.AddDays(5), 15, 20),
                new StoredUsage(date.AddDays(7), 17, 22)
            };

            usageStore.Setup(x => x.QueryAsync("key", new DateTime(2016, 1, 1), new DateTime(2016, 1, 31))).Returns(Task.FromResult(originalData));

            var result = await sut.GetMonthlyCalls("key", date);

            Assert.Equal(55, result);
        }

        [Fact]
        public async Task Should_fill_missing_days()
        {
            var dateFrom = DateTime.Today;
            var dateTo = DateTime.Today.AddDays(7);

            IReadOnlyList<StoredUsage> originalData = new List<StoredUsage>
            {
                new StoredUsage(dateFrom.AddDays(1), 10, 15),
                new StoredUsage(dateFrom.AddDays(3), 13, 18),
                new StoredUsage(dateFrom.AddDays(5), 15, 20),
                new StoredUsage(dateFrom.AddDays(7), 17, 22)
            };

            usageStore.Setup(x => x.QueryAsync("key", dateFrom, dateTo)).Returns(Task.FromResult(originalData));

            var result = await sut.QueryAsync("key", dateFrom, dateTo);

            result.ShouldBeEquivalentTo(new List<StoredUsage>
            {
                new StoredUsage(dateFrom.AddDays(0), 00, 00),
                new StoredUsage(dateFrom.AddDays(1), 10, 15),
                new StoredUsage(dateFrom.AddDays(2), 00, 00),
                new StoredUsage(dateFrom.AddDays(3), 13, 18),
                new StoredUsage(dateFrom.AddDays(4), 00, 00),
                new StoredUsage(dateFrom.AddDays(5), 15, 20),
                new StoredUsage(dateFrom.AddDays(6), 00, 00),
                new StoredUsage(dateFrom.AddDays(7), 17, 22)
            });
        }

        [Fact]
        public async Task Should_not_track_if_weight_less_than_zero()
        {
            await sut.TrackAsync("key1", -1, 1000);
            await sut.TrackAsync("key1", 0, 1000);

            sut.Next();

            await Task.Delay(100);

            usageStore.Verify(x => x.TrackUsagesAsync(It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<long>()), Times.Never());
        }

        [Fact]
        public async Task Should_aggregate_and_store_on_dispose()
        {
            var today = DateTime.Today;

            usageStore.Setup(x => x.TrackUsagesAsync(today, "key1", 1.0, 1000))
                .Returns(TaskHelper.Done)
                .Verifiable();

            usageStore.Setup(x => x.TrackUsagesAsync(today, "key2", 1.5, 5000))
                .Returns(TaskHelper.Done)
                .Verifiable();

            usageStore.Setup(x => x.TrackUsagesAsync(today, "key3", 0.9, 15000))
                .Returns(TaskHelper.Done)
                .Verifiable();

            await sut.TrackAsync("key1", 1, 1000);

            await sut.TrackAsync("key2", 1.0, 2000);
            await sut.TrackAsync("key2", 0.5, 3000);

            await sut.TrackAsync("key3", 0.3, 4000);
            await sut.TrackAsync("key3", 0.1, 5000);
            await sut.TrackAsync("key3", 0.5, 6000);

            sut.Next();

            await Task.Delay(100);

            usageStore.VerifyAll();
        }
    }
}
