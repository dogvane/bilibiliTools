using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VTT
{
    class Program
    {
        static void Main(string[] args)
        {
            // 测试用程序

            var ret = VTTHelper.GetVTTFromFile("Maarten Balliauw. Let’s refresh our memory! Memory management in .NET. .NET Fest 2018-SqJvYHkHPQs.en.vtt");

            var jsonStr = JsonConvert.SerializeObject(ret, Formatting.Indented);
            Console.WriteLine(jsonStr);

        }
    }
}
