//            Copyright Keysight Technologies 2012-2019
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, you can obtain one at http://mozilla.org/MPL/2.0/.
using NUnit.Framework;
using OpenTap.Diagnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTap.EngineUnitTestUtils;

namespace OpenTap.Engine.UnitTests
{
    [TestFixture]
    public class LoggingTest
    {
        public class CountingListener : ILogListener
        {
            public int RecvdMessages = 0;

            public void EventsLogged(IEnumerable<Event> events)
            {
                int count = 0;
                using (IEnumerator<Event> enumerator = events.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        count++;
                }
                RecvdMessages += count;
            }

            public void Flush()
            {
            }
        }

        [Test]
        public void SimpleAsyncTest()
        {
            var ctx = OpenTap.Diagnostic.LogFactory.CreateContext();
            var counter = new CountingListener();

            var log = ctx.CreateLog("Test");
            ctx.AttachListener(counter);

            ctx.Async = true;
            ctx.MessageBufferSize = 0;

            log.LogEvent(0, "abc");
            log.LogEvent(0, "abc{0}", "t");
            log.LogEvent(0, 0, "abc");
            log.LogEvent(0, 0, "abc{0}", "t");

            ctx.Flush();

            Assert.AreEqual(4, counter.RecvdMessages);
        }

        [Test]
        public void SimpleSyncTest()
        {
            var ctx = OpenTap.Diagnostic.LogFactory.CreateContext();
            var counter = new CountingListener();

            var log = ctx.CreateLog("Test");
            ctx.AttachListener(counter);

            ctx.Async = false;
            ctx.MessageBufferSize = 0;

            log.LogEvent(0, "abc");
            log.LogEvent(0, "abc{0}", "t");
            log.LogEvent(0, 0, "abc");
            log.LogEvent(0, 0, "abc{0}", "t");

            ctx.Flush();

            Assert.AreEqual(4, counter.RecvdMessages);
        }

        [Test]
        public void SimpleBufferTest()
        {
            var ctx = OpenTap.Diagnostic.LogFactory.CreateContext();
            var counter = new CountingListener();

            var log = ctx.CreateLog("Test");
            ctx.AttachListener(counter);

            ctx.Async = false;
            ctx.MessageBufferSize = 1;

            log.LogEvent(0, "abc");
            log.LogEvent(0, "abc{0}", "t");
            log.LogEvent(0, 0, "abc");
            log.LogEvent(0, 0, "abc{0}", "t");

            ctx.Flush();

            Assert.AreEqual(4, counter.RecvdMessages);
        }

        [Test, Ignore("This is will generate false positives.")]
        public void CrudePerformanceTest()
        {
            int count = 10 * 1000 * 1000;
            double MaxNSPerMessage = 50;

            var ctx = OpenTap.Diagnostic.LogFactory.CreateContext();
            var counter = new CountingListener();

            var log = ctx.CreateLog("Test");
            ctx.AttachListener(counter);

            ctx.Async = true;
            ctx.MessageBufferSize = 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
                log.LogEvent(0, "test");
            sw.Stop();

            ctx.Flush();

            Assert.AreEqual(count, counter.RecvdMessages);
            Assert.IsTrue(sw.ElapsedTicks * 100.0 / count <= MaxNSPerMessage, "Time to insert messages was {0} ({1} ns/msg)", sw.Elapsed.TotalSeconds, sw.ElapsedTicks * 100.0 / count);
        }

        [Test]
        public void NullMessages()
        {
            var ts = Log.CreateSource("test");
            
            try
            {
                ts.Debug(null as string);
                Assert.Fail("An exception should have been thrown.");
            }
            catch (ArgumentNullException) { }
            try
            {
                ts.Debug(null as Exception);
                Assert.Fail("An exception should have been thrown.");
            }
            catch (ArgumentNullException) { }
            try
            {
                ts.Error(null as string);
                Assert.Fail("An exception should have been thrown.");
            }
            catch (ArgumentNullException) { }
            try
            {
                ts.Error(null as Exception);
                Assert.Fail("An exception should have been thrown.");
            }
            catch(ArgumentNullException) { }
            try
            {
                ts.Warning(null as string);
                Assert.Fail("An exception should have been thrown.");
            }
            catch (ArgumentNullException) { }
            try
            {
                ts.Info(null as string);
                Assert.Fail("An exception should have been thrown.");
            }
            catch (ArgumentNullException) { }
        }

        [Test]
        public void TicksTest()
        {
            Assert.AreEqual(10000000, TimeSpan.FromSeconds(1).Ticks);
        }
        
        void testPrint(string test, int x)
        {
            throw new Exception("Intended exception");
        }

        class ExceptionTest : Exception
        {
            public override string StackTrace
            {
                get
                {
                    return "{1}";
                }
            }
        }

        [Test]
        public void ExceptionDebugLog()
        {
            Log.CreateSource("ExceptionTest").Debug(new ExceptionTest());
        }


        class LogEventDurationListener : ILogListener
        {
            public string Id = Guid.NewGuid().ToString();

            public long duration = - 1;

            public void ResetDuration() => duration = -1;
            public void EventsLogged(IEnumerable<Event> Events)
            {
                foreach (var evt in Events)
                {
                    if (evt.Message.Contains(Id))
                        duration = evt.DurationNS;
                }
            }

            public void Flush()
            {
                
            }
        }
        [Test]
        public void LogEventDuration()
        {
            var listener = new LogEventDurationListener();
            Log.AddListener(listener);
            try
            {
                var source = Log.CreateSource("test");

                source.Debug(TimeSpan.FromSeconds(1), "{0}", listener.Id);
                source.Flush();
                Assert.AreEqual(listener.duration, 1_000_000_000);

                source.Debug(TimeSpan.FromSeconds(0), "{0}", listener.Id);
                source.Flush();
                Assert.AreEqual(listener.duration, 0);
            }
            finally
            {
                Log.RemoveListener(listener);
            }
        }
        
        [TestCase("{")]
        [TestCase("}")]
        [TestCase("{}")]
        [TestCase("")]
        public void SimpleTest(string logStr)
        {
            var redirectedLogging = new TestTraceListener();
            using (Session.Create())
            {
                Log.AddListener(redirectedLogging);
                var log = Log.CreateSource("test");
                log.Debug(logStr);   
            }

            var logResult = redirectedLogging.allLog.ToString()
                .Split(new []{"\n"},StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.TrimEnd('\r'))
                .ToArray();
            Assert.AreEqual(1, logResult.Length);
            Assert.IsTrue(logResult[0].EndsWith(logStr));
        }
        
    }
}
