using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisClientsBenchmark.Client.Generic
{
	public abstract class AbstractRedisClient : IDisposable
	{
		protected AbstractRedisClient()
		{
			TaskScheduler.UnobservedTaskException += (sender, args) =>
			{
				LogUnhandledError(args.Exception);
				args.SetObserved();
			};
		}

		private Action<Exception> _logErrorCallback;

		public abstract void RPush(string key, string value);

		public abstract long LLen(string key);

		public abstract long Del(string key);

		public abstract string Name { get; }

		public void SetLogErrorCallback(Action<Exception> logErrorCallback)
		{
			_logErrorCallback = logErrorCallback;
		}

		protected void LogError(Exception ex)
		{
			_logErrorCallback(ex);
		}

		private void LogUnhandledError(Exception ex)
		{
			var unhandledEx = new Exception("Unhandled exception in client " + Name, ex);
			_logErrorCallback(unhandledEx);
			throw unhandledEx;
		}

		public abstract void Dispose();
	}
}
