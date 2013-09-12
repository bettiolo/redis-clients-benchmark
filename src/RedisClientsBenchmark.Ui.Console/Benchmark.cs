using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RedisClientsBenchmark.Client.BookSleeve;
using RedisClientsBenchmark.Client.CsRedis;
using RedisClientsBenchmark.Client.Generic;
using RedisClientsBenchmark.Client.RedisBoost;
using RedisClientsBenchmark.Client.ServiceStackRedis;
using SimpleSpeedTester.Core;
using SimpleSpeedTester.Interfaces;

namespace RedisClientsBenchmark.Ui.CommandLine
{
	public class Benchmark : IDisposable
	{
		private Process _redisProcess;
		private string _redisServerPath;
		private string _redisServerconfig;

		private const string QueueNamePrefix = "test-queue-";

		public Benchmark()
		{
			if (!LoadRedisServerFolderFromNugetPackage())
			{
				return;
			}
			StartLocalRedisServer();
			var results = ExecuteBenchmarks();
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

		private List<ITestResult> ExecuteBenchmarks()
		{
			const int Iterations = 30000;
			const string HostName = "localhost";
			const int Port = 6379;
			const int TimeoutInSeconds = 1;

			Console.WriteLine("Setting up benchmarks");
			var redisClients = new List<AbstractRedisClient>();
			redisClients.Add(new CsRedisClient(HostName, Port, TimeoutInSeconds));
			redisClients.Add(new BookSleveClient(HostName, Port, TimeoutInSeconds));
			redisClients.Add(new RedisBoostClient(HostName, Port, TimeoutInSeconds));
			redisClients.Add(new ServiceStackClient(HostName, Port, TimeoutInSeconds));

			Console.WriteLine("Benchmark ready, press enter to run.");
			Console.ReadLine();

			var testGroup = new TestGroup("Client Benchmarks");
			var queueIndex = 0;
			foreach (var redisClient in redisClients)
			{
				Console.WriteLine("Benchmarking: " + redisClient.Name);
				redisClient.SetLogErrorCallback(ex => Console.WriteLine(ex.Message));
				var scopedRedisClient = redisClient;
				var scopedIndex = queueIndex;
				testGroup.PlanAndExecute(
					"Testing: " + redisClient.Name,
					() => BenchmarkClient(scopedRedisClient, scopedIndex), 
					Iterations
				);
				queueIndex++;
			}

			queueIndex = 0;
			foreach (var redisClient in redisClients)
			{
				var queueLength = redisClient.LLen(QueueNamePrefix + queueIndex);
				Console.WriteLine("{0}{1}: {2} items", QueueNamePrefix, queueIndex, queueLength);
				redisClient.Dispose();
				queueIndex++;
			}

			Console.WriteLine("Benchmark ended");
			return testGroup.GetTestResults();
		}

		private void BenchmarkClient(AbstractRedisClient redisClient, int queueIndex)
		{
			redisClient.RPush(QueueNamePrefix + queueIndex, GetTestData());
		}

		private string GetTestData()
		{
			return
				"{" +
					"\"ApiConsumerId\":1," +
					"\"ApiMethodId\":2," +
					"\"RequestDateTime\":\"/Date(" + ToUnixEpoch(DateTime.Now) + ")/\"," +
					"\"Parameters\":\"http://example.org/endpoint\"," +
					"\"ConsumerIpAddress\":\"::1\"," +
					"\"HttpMethod\":\"GET\"," +
					"\"UserId\":0," +
					"\"Host\":\"MAK-VM\"," +
					"\"ShopId\":34," +
					"\"ErrorCode\":0," +
					"\"TimeTaken\":14," +
					"\"Cached\":false," +
					"\"UserAgent\":\"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.95 Safari/537.36\"" +
				"}";
		}

		public string ToUnixEpoch(DateTime dateTime)
		{
			var beginningOfUnixTime = new DateTime(1970, 1, 1);
			var universalDateTime = dateTime.ToUniversalTime();
			var timeSpan = new TimeSpan(universalDateTime.Ticks - beginningOfUnixTime.Ticks);
			return timeSpan.TotalMilliseconds.ToString("#");
		}

		public void Dispose()
		{
			if (_redisProcess != null)
			{
				_redisProcess.CloseMainWindow();
				_redisProcess.Dispose();				
			}
		}
	}
}
