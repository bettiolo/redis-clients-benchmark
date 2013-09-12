using RedisBoost;
using RedisClientsBenchmark.Client.Generic;

namespace RedisClientsBenchmark.Client.RedisBoost
{
	public class RedisBoostClient : AbstractRedisClient
	{
		private readonly IRedisClient _redisClient;

		public RedisBoostClient(string hostName, int port, int timeoutInSeconds)
		{
			var redisClientConnectTask = RedisClient.ConnectAsync(hostName, port);
			redisClientConnectTask.Wait();
			_redisClient = redisClientConnectTask.Result;
			// _redisClient.Error += (sender, args) => LogError(args.Exception);
		}

		public override void RPush(string key, string value)
		{
			_redisClient.RPushAsync(key, value);
		}

		public override long LLen(string key)
		{
			var queueLengthTask = _redisClient.LLenAsync(key);
			queueLengthTask.Wait();
			return queueLengthTask.Result;
		}

		public override long Del(string key)
		{
			throw new System.NotImplementedException();
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
