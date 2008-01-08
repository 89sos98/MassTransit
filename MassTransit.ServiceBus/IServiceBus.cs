using System;

namespace MassTransit.ServiceBus
{

	/// <summary>
	/// The base service bus interface
	/// </summary>
	public interface IServiceBus : 
        IDisposable
	{
		IEndpoint Endpoint { get; }

	    IEndpoint PoisonEndpoint { get; }

	    void Subscribe<T>(MessageReceivedCallback<T> callback) where T : IMessage;
        void Subscribe<T>(MessageReceivedCallback<T> callback, Predicate<T> condition) where T : IMessage;

		/// <summary>
		/// Publishes a message to all subscribed consumers for the message type
		/// </summary>
		/// <typeparam name="T">The type of the message</typeparam>
		/// <param name="messages">The messages to be published</param>
		void Publish<T>(params T[] messages) where T : IMessage;

		/// <summary>
		/// Submits a request message to the default destination for the message type
		/// </summary>
		/// <typeparam name="T">The type of message</typeparam>
        /// <param name="destinationEndpoint">The destination for the message</param>
        /// <param name="messages">The messages to be sent</param>
		/// <returns>An IAsyncResult that can be used to wait for the response</returns>
        IServiceBusAsyncResult Request<T>(IEndpoint destinationEndpoint, params T[] messages) where T : IMessage;

		/// <summary>
		/// Sends a list of messages to the specified destination
		/// </summary>
		/// <param name="destinationEndpoint">The destination for the message</param>
		/// <param name="messages">The list of messages</param>
        void Send<T>(IEndpoint destinationEndpoint, params T[] messages) where T : IMessage;
	}
}