﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mesen.Config;

namespace Mesen.Utilities
{
	public class SingleInstance : IDisposable
	{
		public static SingleInstance Instance { get; private set; } = new SingleInstance();

		private static Guid _identifier = new Guid("{A46696B7-2D1C-4CC5-A52F-43BCAF094AEF}");

		private Mutex? _mutex;
		private bool _disposed = false;
		private bool _firstInstance = false;

		public bool FirstInstance => _firstInstance;
		public event EventHandler<ArgumentsReceivedEventArgs>? ArgumentsReceived;

		public void Init(string[] args)
		{
			_mutex = new Mutex(true, "Global\\" + _identifier.ToString(), out _firstInstance);

			if(_firstInstance || !ConfigManager.Config.Preferences.SingleInstance) {
				_firstInstance = true;
				Task.Run(() => ListenForArguments());
			} else {
				PassArgumentsToFirstInstance(args);
			}
		}

		private bool PassArgumentsToFirstInstance(string[] args)
		{
			try {
				using(NamedPipeClientStream client = new NamedPipeClientStream(_identifier.ToString())) {
					using(StreamWriter writer = new StreamWriter(client)) {
						client.Connect(200);

						foreach(string argument in args) {
							writer.WriteLine(argument);
						}
					}
				}
				return true;
			} catch(Exception ex) {
				Console.WriteLine("Error: " + ex.ToString());
			}

			return false;
		}

		private void ListenForArguments()
		{
			try {
				while(true) {
					using NamedPipeServerStream server = new NamedPipeServerStream(_identifier.ToString());
					using StreamReader reader = new StreamReader(server);
					server.WaitForConnection();

					List<string> args = new List<string>();
					while(server.IsConnected) {
						string? arg = reader.ReadLine();
						if(!string.IsNullOrWhiteSpace(arg)) {
							args.Add(arg);
						} else {
							server.Disconnect();
							break;
						}
					}

					Task.Run(() => {
						ArgumentsReceived?.Invoke(this, new ArgumentsReceivedEventArgs(args.ToArray()));
					});
				}
			} catch(Exception ex) {
				Console.WriteLine("ListenForArguments error: " + ex.ToString());
			} finally {
				Thread.Sleep(10000);
				Task.Run(() => ListenForArguments());
			}
		}

		#region IDisposable

		protected virtual void Dispose(bool disposing)
		{
			if(!_disposed) {
				if(_mutex != null && _firstInstance) {
					_mutex.ReleaseMutex();
					_mutex = null;
				}
				_disposed = true;
			}
		}

		~SingleInstance()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}

	public class ArgumentsReceivedEventArgs : EventArgs
	{
		public string[] Args { get; }

		public ArgumentsReceivedEventArgs(string[] args)
		{
			Args = args;
		}
	}
}