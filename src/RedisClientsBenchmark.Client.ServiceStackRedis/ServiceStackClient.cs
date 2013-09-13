using System;
using System.Text;
using RedisClientsBenchmark.Client.Generic;
using ServiceStack.Redis;

namespace RedisClientsBenchmark.Client.ServiceStackRedis
{
	public class ServiceStackClient : AbstractRedisClient
	{

		private readonly RedisClient _redisClient;

		public ServiceStackClient(string hostName, int port, int timeoutInSeconds)
		{
			_redisClient = new RedisClient(hostName, port);
			_redisClient.ConnectTimeout = timeoutInSeconds * 1000;
		}

		public override void RPush(string key, string value)
		{
			try
			{
				_redisClient.RPush(key, Encoding.UTF8.GetBytes(value));
			}
			catch (Exception ex)
			{
				LogError(ex);
			}	
		}

		public override long LLen(string key)
		{
			return _redisClient.LLen(key);
		}

		public override bool Del(string key)
		{
			return _redisClient.Del(key) > 0;
		}

		public override string Name
		{
			get { return "ServiceStack"; }
		}

		public override void Dispose()
		{
			_redisClient.Dispose();
		}

	}
}
