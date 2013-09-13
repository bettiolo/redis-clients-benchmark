using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ctstone.Redis;
using RedisClientsBenchmark.Client.Generic;

namespace RedisClientsBenchmark.Client.CsRedis
{
	public class CsRedisClient : AbstractRedisClient
	{

		private readonly bool _async;
		private readonly RedisClientAsync _redisClient;

		public CsRedisClient(string hostName, int port, int timeoutInSeconds, bool async = true)
		{
			_async = async;
			_redisClient = new RedisClientAsync(hostName, port, timeoutInSeconds * 1000);
			_redisClient.ExceptionOccurred += (sender, ex) => LogError((Exception)ex.ExceptionObject);
		}

		public override void RPush(string key, string value)
		{
			if (_async)
			{
				_redisClient.RPush(key, value);
			}
			else
			{
				var rpushTask = _redisClient.RPush(key, value);
				rpushTask.Wait();
			}
		}

		public override long LLen(string key)
		{
			var llenTask = _redisClient.LLen(key);
			llenTask.Wait();
			return llenTask.Result;
		}

		public override bool Del(string key)
		{
			var delTask = _redisClient.Del(key);
			delTask.Wait();
			return delTask.Result > 0;
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
