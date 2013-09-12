using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RedisClientsBenchmark.Ui.CommandLine
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var benchmark = new LocalRedisBenchmark())
			{
				Console.WriteLine("Press any key to exit");
				Console.ReadLine();
			}
		}
	}
}
