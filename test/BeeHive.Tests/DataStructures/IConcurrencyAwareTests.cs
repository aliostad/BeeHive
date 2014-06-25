using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Xunit;

namespace BeeHive.Tests.DataStructures
{
    public class IConcurrencyAwareTests
    {
        [Fact]
        public void NoConflict_ETagNull_LastModifiedNull()
        {
            var concurrencyAware = new DummyConcurrencyAware();
            var concurrencyAware2 = new DummyConcurrencyAware();

            concurrencyAware.AssertNoConflict(concurrencyAware2);
            Assert.False(concurrencyAware.ConflictsWith(concurrencyAware2));

        }

        [Fact]
        public void NoConflict_ETagEqual_LastModifiedNull()
        {
            var concurrencyAware = new DummyConcurrencyAware()
            {
                ETag = "12345"
            };
            var concurrencyAware2 = new DummyConcurrencyAware()
            {
                ETag = "12345"
            };

            concurrencyAware.AssertNoConflict(concurrencyAware2);
            Assert.False(concurrencyAware.ConflictsWith(concurrencyAware2));

        }


        [Fact]
        public void NoConflict_ETagNull_LastModifiedEqual()
        {
            var lastMod = DateTimeOffset.Now;
            var concurrencyAware = new DummyConcurrencyAware()
            {
                LastModified = lastMod
            };
            var concurrencyAware2 = new DummyConcurrencyAware()
            {
                LastModified = lastMod
            };

            
            concurrencyAware.AssertNoConflict(concurrencyAware2);
            Assert.False(concurrencyAware.ConflictsWith(concurrencyAware2));
        }

        [Fact]
        public void Conflict_ETagNull_LastModifiedDifferent()
        {
            var lastMod = DateTimeOffset.Now;
            var concurrencyAware = new DummyConcurrencyAware()
            {
                LastModified = lastMod
            };
            var concurrencyAware2 = new DummyConcurrencyAware()
            {
                LastModified = lastMod.AddMilliseconds(1)
            };

            Assert.Throws<ConcurrencyConflictException>(() => concurrencyAware.AssertNoConflict(concurrencyAware2));
            
            Assert.True(concurrencyAware.ConflictsWith(concurrencyAware2));
        }

        [Fact]
        public void Conflict_ETagDifferent_LastModifiedNull()
        {
            var concurrencyAware = new DummyConcurrencyAware()
            {
                ETag = "12345"
            };
            var concurrencyAware2 = new DummyConcurrencyAware()
            {
                ETag = "12345x"
            };

            Assert.Throws<ConcurrencyConflictException>(() => concurrencyAware.AssertNoConflict(concurrencyAware2));

            Assert.True(concurrencyAware.ConflictsWith(concurrencyAware2));
        }

        [Fact]
        public void Inconsistent_ETag_LastModified()
        {
            var concurrencyAware = new DummyConcurrencyAware()
            {
                ETag = "12345"
            };
            var concurrencyAware2 = new DummyConcurrencyAware()
            {
                LastModified = DateTimeOffset.Now
            };

            Assert.Throws<InvalidOperationException>(() => concurrencyAware.AssertNoConflict(concurrencyAware2));
            Assert.Throws<InvalidOperationException>(() => concurrencyAware.ConflictsWith(concurrencyAware2));

        }


    }



    internal class DummyConcurrencyAware : IConcurrencyAware
    {
        public DateTimeOffset? LastModified { get; set; }
        public string ETag { get; set; }
    }
}
