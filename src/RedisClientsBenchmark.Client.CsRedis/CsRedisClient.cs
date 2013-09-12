using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ctstone.Redis;
using RedisClientsBenchmark.Client.Generic;

namespace RedisClientsBenchmark.Client.CsRedis
{
	public class CsRedisClient : AbstractRedisClient
	{
		private readonly RedisClientAsync _redisClient;

		public CsRedisClient(string hostName, int port, int timeoutInSeconds)
		{
			_redisClient = new RedisClientAsync(hostName, port, timeoutInSeconds * 1000);
			_redisClient.ExceptionOccurred += (sender, ex) => LogError((Exception)ex.ExceptionObject);
		}

		public override void RPush(string key, string value)
		{
			_redisClient.RPush(key, value);
		}

		public override long LLen(string key)
		{
			var queueLengthTask = _redisClient.LLen(key);
			queueLengthTask.Wait();
			return queueLengthTask.Result;
		}

		public override long Del(string key)
		{
			throw new NotImplementedException();
		}

		public override string Name
		{
			get { return "CsRedis"; }
		}

		public override void Dispose()
		{
			_redisClient.Dispose();
		}
	}
}
