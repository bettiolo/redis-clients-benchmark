﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using RedisClientsBenchmark.Client.BookSleeve;
using RedisClientsBenchmark.Client.CsRedis;
using RedisClientsBenchmark.Client.Generic;
using RedisClientsBenchmark.Client.RedisBoost;
using RedisClientsBenchmark.Client.ServiceStackRedis;
using SimpleSpeedTester.Core;
using SimpleSpeedTester.Interfaces;

namespace RedisClientsBenchmark.Ui.CommandLine
{
	public class Benchmarks
	{
		private static DateTime _beginningOfUnixTime = new DateTime(1970, 1, 1);
		private const string QueueNamePrefix = "benchmark-test-queue-";
		private const int Iterations = 100;
		private const string HostName = "localhost";
		private const int Port = 6379;
		private const int TimeoutInSeconds = 1;
		private const bool Async = true;

		public List<ITestResult> Execute()
		{
			Console.WriteLine("Setting up benchmarks");
			var redisClients = new List<AbstractRedisClient>
			{
				new CsRedisClient(HostName, Port, TimeoutInSeconds, Async),
				new BookSleveClient(HostName, Port, TimeoutInSeconds, Async),
				new ServiceStackClient(HostName, Port, TimeoutInSeconds),
				new RedisBoostClient(HostName, Port, TimeoutInSeconds, Async),
			};

			Console.WriteLine("Benchmark ready, press enter to run.");
			Console.ReadLine();

			var testGroup = new TestGroup("Client Benchmarks");
			var queueIndex = 0;
			var sw = new Stopwatch();
			foreach (var redisClient in redisClients)
			{
				sw.Start();
				Console.WriteLine("Benchmarking: " + redisClient.Name);
				redisClient.SetLogErrorCallback(ex => Console.WriteLine(ex.Message));
				var queueName = QueueNamePrefix + queueIndex;
				redisClient.Cleanup(queueName);
				BenchmarkClient(redisClient, queueName, testGroup, Iterations);
				if (!redisClient.Cleanup(queueName))
				{
					Console.WriteLine("{0}: Error cleaning up", queueName);
				}
				else
				{
					Console.WriteLine("{0}: cleaned up", queueName);
				}
				redisClient.Dispose();
				queueIndex++;
				Console.WriteLine("Elapsed: {0}", sw.Elapsed);
				Console.WriteLine();
				sw.Reset();
			}

			Console.WriteLine("Benchmark ended");
			return testGroup.GetTestResults();
		}

		private void BenchmarkClient(AbstractRedisClient redisClient, string queueName, TestGroup testGroup, int iterations)
		{
			testGroup.PlanAndExecute(
				"Testing: " + redisClient.Name,
				() => redisClient.RPush(queueName, GetTestData()),
				iterations
				);
			var queueLength = redisClient.LLen(queueName);
			Console.WriteLine("{0}: {1} items", queueName, queueLength);
		}

		private string GetTestData()
		{
			return
				"{" +
				"\"Parameter1\":Value1," +
				"\"Parameter2\":Value2," +
				"\"RequestDateTime\":\"/Date(" + ToUnixEpoch(DateTime.Now) + ")/\"," +
				"\"ApiUrl\":\"http://example.org/endpoint\"," +
				"\"ConsumerIpAddress\":\"::1\"," +
				"\"HttpMethod\":\"GET\"," +
				"\"UserId\":0," +
				"\"Host\":\"HOSTNAME\"," +
				"\"ErrorCode\":0," +
				"\"TimeTaken\":0," +
				"\"Cached\":false," +
				"\"UserAgent\":\"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.95 Safari/537.36\"" +
				"}";
		}

		private string ToUnixEpoch(DateTime dateTime)
		{
			var universalDateTime = dateTime.ToUniversalTime();
			var timeSpan = new TimeSpan(universalDateTime.Ticks - _beginningOfUnixTime.Ticks);
			return timeSpan.TotalMilliseconds.ToString("#");
		}
	}

}