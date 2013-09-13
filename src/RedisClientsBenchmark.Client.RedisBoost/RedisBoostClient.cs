using RedisBoost;
using RedisClientsBenchmark.Client.Generic;

namespace RedisClientsBenchmark.Client.RedisBoost
{
	public class RedisBoostClient : AbstractRedisClient
	{

		private readonly bool _async;
		private readonly IRedisClient _redisClient;

		public RedisBoostClient(string hostName, int port, int timeoutInSeconds, bool async = true)
		{
			_async = async;
			var redisClientConnectTask = RedisClient.ConnectAsync(hostName, port);
			redisClientConnectTask.Wait();
			_redisClient = redisClientConnectTask.Result;
			// _redisClient.Error += (sender, args) => LogError(args.Exception);
		}

		public override void RPush(string key, string value)
		{
			if (_async)
			{
				_redisClient.RPushAsync(key, value);
			}
			else
			{
				var rpushTask = _redisClient.RPushAsync(key, value);
				rpushTask.Wait();
			}
		}

		public override long LLen(string key)
		{
			var llenTask = _redisClient.LLenAsync(key);
			llenTask.Wait();
			return llenTask.Result;
		}

		public override bool Del(string key)
		{
			var delTask = _redisClient.DelAsync(key);
			delTask.Wait();
			return delTask.Result > 0;
		}

		public override string Name
		{
			get { return "RedisBoost"; }
		}

		public override void Dispose()
		{
			_redisClient.Dispose();
		}

	}
}
