using System;
using BookSleeve;
using RedisClientsBenchmark.Client.Generic;

namespace RedisClientsBenchmark.Client.BookSleeve
{
	public class BookSleveClient : AbstractRedisClient
	{

		private readonly bool _async;
		private readonly RedisConnection _redisClient;

		public BookSleveClient(string hostName, int port, int timeoutInSeconds, bool async = true)
		{
			_async = async;
			_redisClient = new RedisConnection(hostName, port, timeoutInSeconds * 1000);
			_redisClient.Error += (sender, args) => LogError(args.Exception);

			if (_redisClient.State != RedisConnectionBase.ConnectionState.Open)
			{
				var openAsync = _redisClient.Open();
				_redisClient.Wait(openAsync);
			}
		}

		public override void RPush(string key, string value)
		{
			if (_async) {
				_redisClient.Lists.AddLast(0, key, value);
			}
			else
			{
				var rpushTask = _redisClient.Lists.AddLast(0, key, value);
				rpushTask.Wait();
			}
		}

		public override long LLen(string key)
		{
			var llenTask = _redisClient.Lists.GetLength(0, key);
			llenTask.Wait();
			return llenTask.Result;
		}

		public override bool Del(string key)
		{
			var delTask = _redisClient.Keys.Remove(0, key);
			delTask.Wait();
			return delTask.Result;
		}

		public override string Name
		{
			get { return "BookSleve"; }
		}

		public override void Dispose()
		{
			_redisClient.Dispose();
		}

	}
}
