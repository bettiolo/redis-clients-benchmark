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

		public abstract bool Del(string key);

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

		public bool Cleanup(string queueName)
		{
			return Del(queueName);
		}

		protected void ExecuteAndHandle(Action action)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				LogError(ex);
			}
		}

		protected T ExecuteAndHandle<T>(Func<T> action)
		{
			try
			{
				return action();
			}
			catch (Exception ex)
			{
				LogError(ex);
				return default(T);
			}
		}
	}
}
