using System;
using System.IO;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(GravityBox.Tests.COgheTestProgress))]

namespace GravityBox.Tests
{
    /// <summary>Records each completed case next to this run's XML, including failures during long regression runs.</summary>
    public sealed class COgheTestProgress : ITestRunCallback
    {
        [Serializable] private sealed class Result { public string name,status,message; public double seconds; }
        private string path;
        public void RunStarted(ITest test)
        {
            string[] args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-testResults")path=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[i+1])),"cases.jsonl");
        }
        public void TestStarted(ITest test) { }
        public void TestFinished(ITestResult result)
        {
            if(path==null||result.Test.IsSuite)return;
            File.AppendAllText(path,JsonUtility.ToJson(new Result{name=result.Test.FullName,status=result.ResultState.ToString(),message=result.Message,seconds=result.Duration})+Environment.NewLine);
        }
        public void RunFinished(ITestResult result) { }
    }
}
