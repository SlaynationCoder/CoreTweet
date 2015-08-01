﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RestApisGen
{
    static class ParameterRecordsFs
    {
        private class ParamInfo
        {
            public string Name;
            public string Type;
            public bool IsOptional;
        }

        public static void Generate(IEnumerable<ApiParent> apis, TextWriter writer)
        {
            writer.Write(@"// This file was automatically generated by CoreTweet.
// Do not modify this file directly.

[<AutoOpen>]
module CoreTweet.CoreTweetFSharp
open System.Collections.Generic
open System.Reflection
type private long = int64

[<assembly: AssemblyTitle(""CoreTweet.FSharp"")>]
[<assembly: AssemblyDescription(""F# records for CoreTweet"")>]
[<assembly: AssemblyCompany(""CoreTweet Development Team"")>]
[<assembly: AssemblyProduct(""CoreTweet"")>]
[<assembly: AssemblyCopyright(""(c) 2013-2015 CoreTweet Development Team"")>]
");

            foreach (var line in File.ReadAllLines(Path.Combine("CoreTweet.Shared", "AssemblyVersion.cs")))
            {
                if (line.StartsWith("[assembly:"))
                    writer.WriteLine("[<{0}>]", line.Substring(1, line.Length - 2));
            }
            writer.WriteLine("do()");

            foreach (var apiGroup in apis)
            {
                foreach (var endpoint in apiGroup.Endpoints.Where(x => x.Params.Length > 0))
                {
                    var parameters = new List<ParamInfo>();
                    var areAllOptional = endpoint.AnyOneGroups.Any(x => !x.Any());
                    var isEitherRequired = false;
                    if (!areAllOptional && endpoint.AnyOneGroups.Count > 0)
                    {
                        var eithered = Extensions.Combinate(endpoint.AnyOneGroups).ToArray();
                        var prmNames = eithered[0].SelectMany(x => x.Select(y => y.RealName)).ToArray();
                        isEitherRequired = eithered.All(x => x.SelectMany(y => y.Select(z => z.RealName)).SequenceEqual(prmNames));
                    }
                    foreach (var param in endpoint.Params)
                    {
                        var pi = parameters.Find(x => x.Name == param.RealName);
                        if (pi != null)
                        {
                            if (pi.Type != param.Type)
                                pi.Type = "obj";
                        }
                        else
                        {
                            parameters.Add(new ParamInfo()
                            {
                                Name = param.RealName,
                                Type = param.Type,
                                IsOptional = param.Kind == "required"
                                    ? false
                                    : param.Kind == "any one is required"
                                        ? !isEitherRequired
                                        : true
                            });
                        }
                    }

                    writer.WriteLine();
                    writer.WriteLine("type {0}Parameter = {{", apiGroup.Name + endpoint.Name);
                    foreach (var param in parameters)
                    {
                        writer.WriteLine("    {0}: {1}", param.Name, param.IsOptional ? (param.Type + " option") : param.Type);
                    }
                    writer.WriteLine('}');
                    writer.WriteLine();

                    writer.WriteLine("let {0}ParameterDefault = {{", apiGroup.Name + endpoint.Name);
                    foreach (var param in parameters)
                    {
                        writer.WriteLine("    {0} = {1}", param.Name,
                            param.IsOptional ? "None"
                                : param.Type == "long" ? "0L"
                                : param.Type == "int" ? "0"
                                : param.Type == "double" ? "0.0"
                                : "null"
                        );
                    }
                    writer.WriteLine('}');
                }
            }
        }
    }
}