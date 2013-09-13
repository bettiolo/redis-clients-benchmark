using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RedisClientsBenchmark.Ui.CommandLine
{
	public class LocalRedisBenchmark : IDisposable
	{
		private Process _redisProcess;
		private string _redisServerPath;
		private string _redisServerconfig;

		public LocalRedisBenchmark()
		{
			if (!LoadRedisServerFolderFromNugetPackage())
			{
				return;
			}
			StartLocalRedisServer();
			var benchmarks = new Benchmarks();
			var results = benchmarks.Execute();
			foreach (var result in results)
			{
				Console.WriteLine(result.GetSummary());
				Console.WriteLine();
			}
		}

		private bool LoadRedisServerFolderFromNugetPackage()
		{
			Console.WriteLine("Loading redis configuration");
			// this will run in 
			// /src/Project/bin/[Debug|Release]/
			// we need to get the redis server from
			// /packages/redis-64.*/redis-server.exe
			var assemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
			var rootDirectory = new DirectoryInfo(Path.Combine(assemblyDirectory, @"..\..\..\..\"));
			var redisFolders = rootDirectory.GetDirectories(@"packages\redis-64.*");
			if (!redisFolders.Any())
			{
				Console.WriteLine("Redis nuget package has not been found, please check that the package has been restored.");
				return false;
			}
			var redisDirectory = redisFolders.Single();
			_redisServerPath = Path.Combine(redisDirectory.FullName, @"tools\redis-server.exe");
			_redisServerconfig = Path.Combine(redisDirectory.FullName, @"tools\redis.conf");
			return true;
		}

		private void StartLocalRedisServer()
		{
			var processStartInfo = new ProcessStartInfo(_redisServerPath)
			{
				Arguments = "\"" + _redisServerconfig + "\" loglevel verbose",
				UseShellExecute = true,
				CreateNoWindow = false,
				WindowStyle = ProcessWindowStyle.Normal,
			};
			Console.WriteLine("Starting local redis server");
			Console.WriteLine("EXEC {0} {1}", processStartInfo.FileName, processStartInfo.Arguments);
			_redisProcess = Process.Start(processStartInfo);
		}

		public void Dispose()
		{
			if (_redisProcess != null)
			{
				_redisProcess.Close();
				_redisProcess.CloseMainWindow();
				_redisProcess.Dispose();
			}
		}
	}
}
