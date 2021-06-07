using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenTap.Plugins.BasicSteps;

namespace OpenTap.UnitTests
{
    public class ResultTest
    {
        public class SimpleResultTest : TestStep
        {
            public class Result
            {
                public double A { get; set; }
                public double B { get; set; }
            }
            
            [Result]
            public Result StepResult { get; private set; }
            
            public override void Run()
            {
                StepResult = new Result {A = 1, B = 2};
            }
        }

        [Test]
        public void TestSimpleResults()
        {
            var plan = new TestPlan();
            var step = new SimpleResultTest();
            plan.Steps.Add(step);

            var rl = new RecordAllResultListener();
            
            plan.Execute(new []{rl});

            var t1 = rl.Results[0];
            Assert.AreEqual(1, t1.Rows);
            var columnA = t1.Columns.First(x => x.Name == "A");
            var columnB = t1.Columns.First(x => x.Name == "B");
            Assert.AreEqual(1.0, columnA.Data.GetValue(0));
            Assert.AreEqual(2.0, columnB.Data.GetValue(0));
        }

        [Test]
        public void TestResultMetadataSimple()
        {
            var metadata = new ResultParameter("", "MetaData1", "Value1", new MetaDataAttribute());
            var plan = new TestPlan();
            var pr = new TestPlanRun(plan, new List<IResultListener>(), DateTime.Now, 0,"",false);
            pr.Parameters.Add(metadata);
            var mt = pr.Parameters.Find(metadata.Name);
            Assert.IsTrue(mt.IsMetaData);
        }
        
        
        [Test]
        public void TestResultMetadata()
        {
            var step = new SimpleResultTest();
            var seq = new SequenceStep();
            var plan = new TestPlan();
            plan.Steps.Add(seq);
            seq.ChildTestSteps.Add(step);
            
            var rl = new SimpleResultTest2();
            var metadata = new ResultParameter("", "MetaData1", "Value1", new MetaDataAttribute());
            Assert.IsTrue(metadata.IsMetaData);
            var run = plan.Execute(new[] {rl}, new[] {metadata});
            Assert.IsTrue(run.Parameters.Find("MetaData1").IsMetaData);

        }

        class SimpleResultTest2 : ResultListener
        {
            
            Dictionary<Guid, TestRun> runs = new Dictionary<Guid, TestRun>();
            public override void OnTestPlanRunStart(TestPlanRun planRun) => runs.Add(planRun.Id, planRun);
            
            public override void OnTestPlanRunCompleted(TestPlanRun planRun, Stream logStream) => runs.Remove(planRun.Id);
            public override void OnTestStepRunStart(TestStepRun stepRun) => runs.Add(stepRun.Id, stepRun);
            public override void OnTestStepRunCompleted(TestStepRun stepRun) => runs.Remove(stepRun.Id);

            public override void OnResultPublished(Guid stepRunId, ResultTable result)
            {
                base.OnResultPublished(stepRunId, result);
                ResultParameters parameterList = new ResultParameters();
                Guid runid = stepRunId;
                while (runs.TryGetValue(runid, out TestRun subRun))
                {
                    foreach(var subparameter in subRun.Parameters.Where(parameter => parameter.IsMetaData))
                        if (parameterList.Find(subparameter.Name) == null)
                        {
                            parameterList.Add(subparameter);
                        }
                    if (subRun is TestStepRun run)
                        runid = run.Parent;
                    else break;
                }
            }
        }
        
    }
}