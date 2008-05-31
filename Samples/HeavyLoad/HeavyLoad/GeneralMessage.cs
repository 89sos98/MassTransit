namespace HeavyLoad
{
	using System;
	using MassTransit.ServiceBus;

	[Serializable]
	public class GeneralMessage : IMessage
	{
		private int[] _values;

		public GeneralMessage()
		{
			_values = new int[64];
		}

		public int[] Values
		{
			get { return _values; }
			set { _values = value; }
		}
	}
}