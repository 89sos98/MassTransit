namespace HeavyLoad
{
	using System;
	using BatchLoad;
	using Correlated;
	using Load;
	using log4net;

	internal class Program
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (Program));

		private static void Main(string[] args)
		{
			_log.Info("HeavyLoad - MassTransit Load Generator");

			Console.WriteLine("HeavyLoad - MassTransit Load Generator");

		//	RunLocalMsmqLoadTest();

			RunContainerLoadTest();

		//	RunTransactionLoadTest();

		//	RunLoopbackLoadTest();

		//	RunWcfLoadTest();

		//	RunCorrelatedMessageTest();

		//	RunBatchLoadTest();

			RunLocalActiveMqLoadTest();

			Console.WriteLine("End of line.");
			Console.ReadLine();
		}

		private static void RunLocalActiveMqLoadTest()
		{
			StopWatch stopWatch = new StopWatch();

			using (ActiveMQLoadTest test = new ActiveMQLoadTest())
			{
				test.Run(stopWatch);
			}

			Console.WriteLine("ActiveMQ Load Test: ");
			Console.WriteLine(stopWatch.ToString());
		}

		private static void RunBatchLoadTest()
		{
			StopWatch stopWatch = new StopWatch();

			using (BatchLoadTest test = new BatchLoadTest())
			{
				test.Run(stopWatch);
			}

			Console.WriteLine("Batch Load Test: ");
			Console.WriteLine(stopWatch.ToString());
		}

		private static void RunLocalMsmqLoadTest()
		{
			StopWatch stopWatch = new StopWatch();

            using (LocalLoadTest test = new LocalLoadTest())
			{
				test.Run(stopWatch);
			}

			Console.WriteLine("Local MSMQ Load Test: ");
			Console.WriteLine(stopWatch.ToString());
		}

		private static void RunContainerLoadTest()
		{
			StopWatch stopWatch = new StopWatch();

			using (ContainerLoadTest test = new ContainerLoadTest())
			{
				test.Run(stopWatch);
			}

			Console.WriteLine("Container Load Test: ");
			Console.WriteLine(stopWatch.ToString());
		}

		private static void RunTransactionLoadTest()
		{
			StopWatch stopWatch = new StopWatch();

            using (TransactionLoadTest test = new TransactionLoadTest())
			{
				test.Run(stopWatch);
			}

			Console.WriteLine("Transaction Load Test: ");
			Console.WriteLine(stopWatch.ToString());
		}

		private static void RunLoopbackLoadTest()
		{
			StopWatch stopWatch = new StopWatch();

			using (LoopbackLoadTest test = new LoopbackLoadTest())
			{
				test.Run(stopWatch);
			}

			Console.WriteLine("Loopback Load Test: ");
			Console.WriteLine(stopWatch.ToString());
		}

		private static void RunWcfLoadTest()
		{
			StopWatch stopWatch = new StopWatch();

			using (WcfLoadTest test = new WcfLoadTest())
			{
				test.Run(stopWatch);
			}

			Console.WriteLine("WCF Load Test: ");
			Console.WriteLine(stopWatch.ToString());
		}

	    private static void RunCorrelatedMessageTest()
		{
			StopWatch stopWatch = new StopWatch();

			using (CorrelatedMessageTest test = new CorrelatedMessageTest())
			{
				test.Run(stopWatch);
			}

			Console.WriteLine("Correlated Message Test: ");
			Console.WriteLine(stopWatch.ToString());
		}
	}
}