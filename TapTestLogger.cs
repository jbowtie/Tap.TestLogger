using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Tap.TestLogger
{
    public class TestResultModel
    {
        public bool IsOk {get;set;}
        public string Desc {get;set;}
        public string SkipReason {get;set;}
        public string ErrorMessage {get;set;}
        public string StackTrace {get;set;}
        public string Format(int testNumber)
        {
            var status = IsOk ? "ok" : "not ok";
            var output = $"{status} {testNumber} {Desc}";
            if(!string.IsNullOrWhiteSpace(this.SkipReason))
            {
                output += $" # skip {SkipReason}";
            }
            return output;
        }
    }

    [FriendlyName("tap")]
    [ExtensionUri("logger://tap/v13")]
    public class TapTestLogger: ITestLogger
    {
        private IList<TestResultModel> _results;
        private string _logfile;

        public void Initialize(TestLoggerEvents events, string testResultsDirPath)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            events.TestRunMessage += this.OnTestRunMessage;
            events.TestResult += this.OnTestResult;
            events.TestRunComplete += this.OnTestRunComplete;

            _results = new List<TestResultModel>();

            if (!Directory.Exists(testResultsDirPath))
            {
                Directory.CreateDirectory(testResultsDirPath);
            }
            _logfile = Path.Combine(testResultsDirPath, "TestResults.txt");
        }

        // For the initial version we will do nothing with the messages
        internal void OnTestRunMessage(object sender, TestRunMessageEventArgs ev)
        {
        }

        // capture the result (we should be able to stream to the output)
        internal void OnTestResult(object sender, TestResultEventArgs ev)
        {
            // add to the output
            // ok/not ok
            // test number(?)
            // description (test name?)
            // directive # skip reason
            // on failure add comment block with error message
            var result = ev.Result;
            var m = new TestResultModel
            {
                IsOk = result.Outcome != TestOutcome.Failed,
                Desc = result.TestCase.DisplayName,
                SkipReason = result.Outcome == TestOutcome.Skipped ? result.Messages[0].Text : null,
                ErrorMessage = result.ErrorMessage,
                StackTrace = result.ErrorStackTrace,
            };
            _results.Add(m);
        }

        // finish up
        internal void OnTestRunComplete(object sender, TestRunCompleteEventArgs e)
        {
            using(var writer = File.CreateText(_logfile))
            {
                writer.WriteLine("TAP version 13");
                writer.WriteLine($"1..{_results.Count}");
                // maybe group by assembly/class (requires waiting for full run)
                // here we'll capture the plan (1..n)
                var index = 0;
                foreach(var result in _results)
                {
                    index++;
                    writer.WriteLine(result.Format(index));
                    if(!string.IsNullOrWhiteSpace(result.ErrorMessage))
                    {
                        writer.WriteLine("  ---");
                        writer.WriteLine($"  message: '{result.ErrorMessage}'");
                        writer.WriteLine($"  severity: fail");
                        writer.WriteLine($"  data:");
                        foreach(var l in result.StackTrace.Split('\n'))
                        {
                            writer.WriteLine($"  {l.TrimEnd()}");
                        }
                        writer.WriteLine("  ...");
                    }
                }
            }
        }
    }
}
