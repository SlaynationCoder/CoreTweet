﻿using System;
using System.IO;
using System.Linq;

namespace RestApisGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Reading API templates");
            var apis = Directory.GetFiles("ApiTemplates").Where(x => !x.Contains("test.api"))
                .Select(ApiParent.Parse).ToArray();

            Console.WriteLine("Generating RestApis.cs");
            using (var writer = new StreamWriter(Path.Combine("CoreTweet.Shared", "RestApis.cs")))
            {
                RestApisCs.Generate(apis, writer);
            }

            Console.WriteLine("Generating ParameterRecords.fs");
            using (var writer = new StreamWriter(Path.Combine("CoreTweet.FSharp", "ParameterRecords.fs")))
            {
                ParameterRecordsFs.Generate(apis, writer);
            }
        }
    }
}
