using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using MassTransit.Host.Config;
using MassTransit.Host.Config.Util.Arguments;
using MassTransit.ServiceBus;

namespace MassTransit.Host
{
	public class Controller
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (Controller));

		private readonly IArgumentMapFactory _argumentMapFactory = new ArgumentMapFactory();
		private readonly IArgumentParser _argumentParser = new ArgumentParser();
		private IHostConfigurator _configurator;

		private string _configuratorName;
		private bool _installService;
		private bool _isService;
		private bool _uninstallService;
		private Assembly _configuratorAssembly;

		[Argument(Key = "config", Required = true, Description = "The configuration provider to use for the host")]
		public string Configurator
		{
			get { return _configuratorName; }
			set
			{
				_configuratorName = value;
				_log.InfoFormat("Configurator Name: {0}", value);
			}
		}

		[Argument(Key = "install", Description = "Install the host service")]
		public bool InstallService
		{
			get { return _installService; }
			set
			{
				_installService = value;
				_log.InfoFormat("Install: {0}", value);
			}
		}

		[Argument(Key = "service", Description = "Set when starting as a service")]
		public bool IsService
		{
			get { return _isService; }
			set
			{
				_isService = value;
				_log.InfoFormat("Service: {0}", value);
			}
		}

		[Argument(Key = "uninstall", Description = "Uninstall the host service")]
		public bool UninstallService
		{
			get { return _uninstallService; }
			set
			{
				_uninstallService = value;
				_log.InfoFormat("Uninstall: {0}", value);
			}
		}

		public void Start(string[] args)
		{
			try
			{
				_log.Info("Starting Host");

				IEnumerable<IArgument> arguments = _argumentParser.Parse(args);

				IArgumentMap mapper = _argumentMapFactory.CreateMap(this);

				IEnumerable<IArgument> remaining = mapper.ApplyTo(this, arguments);

				if (string.IsNullOrEmpty(_configuratorName))
				{
					Console.WriteLine("Usage: {0}", mapper.Usage);
				}
				else if (_installService)
				{
					RegisterService(arguments, true);
				}
				else if (_uninstallService)
				{
					RegisterService(arguments, false);
				}
				else if (_isService)
				{
					Console.WriteLine("Service not working yet.");
				}
				else
				{
					Console.WriteLine("Starting up as a console application");

					LoadConfiguration(remaining);

					if (_configurator != null)
					{
						foreach (IMessageService service in _configurator.Services)
						{
							service.Start();
						}

						Console.WriteLine("The service is running, press Control+C to exit.");

						Console.ReadKey();

						Console.WriteLine("Exiting.");

						foreach (IMessageService service in _configurator.Services)
						{
							service.Stop();
						}

						_configurator.Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				_log.Error("Controller caught exception", ex);

				Console.WriteLine("An exception occurred: {0}", ex.Message);
			}
		}

		private void RegisterService(IEnumerable<IArgument> arguments, bool install)
		{
			HostServiceInstaller installer = new HostServiceInstaller("MassTransitHost", "MassTransit Message Host", "Mass Transit Host");

			IArgumentMap installerMap = _argumentMapFactory.CreateMap(installer);
			IEnumerable<IArgument> remaining = installerMap.ApplyTo(installerMap, arguments);

			if (install)
			{
				LoadConfiguration(remaining);

				installer.Register(_configuratorAssembly,  _configurator.GetType().AssemblyQualifiedName);
			}
			else
			{
				installer.Unregister();
			}
		}

		private void LoadConfiguration(IEnumerable<IArgument> arguments)
		{
			Assembly assembly = Assembly.Load(_configuratorName);

			Console.WriteLine("Loading assembly: {0}", assembly.FullName);

			Type checkType = typeof (IHostConfigurator);

			Type[] types = assembly.GetTypes();
			foreach (Type t in types)
			{
				_log.DebugFormat("Checking Type: {0}", t.FullName);

				if (checkType.IsAssignableFrom(t) && !t.IsAbstract)
				{
					Console.WriteLine("Type: {0}", t.Name);

					object configurator = Activator.CreateInstance(t);
					if (configurator != null)
					{
						IArgumentMap mapper = _argumentMapFactory.CreateMap(configurator);

						Dictionary<string, string> argumentsUsed = new Dictionary<string, string>();

						mapper.ApplyTo(configurator, arguments,
							delegate(string name, string value)
						{
							argumentsUsed.Add(name, value);
							return true;
						});

						Configure(assembly, (IHostConfigurator)configurator);
						return;
					}
				}
			}

			throw new HostConfigurationException("No valid configuration provider specified.");
		}

		public void Configure(Assembly assembly, IHostConfigurator hostConfigurator)
		{
			_configurator = hostConfigurator;
			_configuratorAssembly = assembly;

			_configurator.Configure();
		}
	}
}