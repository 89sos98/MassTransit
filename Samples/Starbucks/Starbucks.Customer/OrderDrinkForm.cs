﻿namespace Starbucks.Customer
{
	using System;
	using System.ComponentModel;
	using System.Windows.Forms;
	using Magnum;
	using MassTransit;
	using Messages;
	using Microsoft.Practices.ServiceLocation;

	public partial class OrderDrinkForm :
		Form,
		Consumes<PaymentDueMessage>.For<Guid>,
		Consumes<DrinkReadyMessage>.For<Guid>
	{
		private IServiceBus _bus;
		private Guid _transactionId;
		private UnsubscribeAction _unsubscribeToken;


		public OrderDrinkForm()
		{
			InitializeComponent();
		}

		private IServiceBus Bus
		{
			get
			{
				if (_bus == null)
					_bus = ServiceLocator.Current.GetInstance<IServiceBus>();

				return _bus;
			}
		}

		public void Consume(DrinkReadyMessage message)
		{
			MessageBox.Show(string.Format("Hey, {0}, your {1} is ready.", message.Name, message.Drink));

			_unsubscribeToken();
			_unsubscribeToken = null;
		}

		public Guid CorrelationId
		{
			get { return _transactionId; }
		}

		public void Consume(PaymentDueMessage message)
		{
			
			string prompt = string.Format("Payment due: ${0} Would you like to add a tip?", message.Amount);

			DialogResult result = MessageBox.Show(prompt, "Payment Due", MessageBoxButtons.YesNoCancel);

			decimal payment = message.Amount;
			if (result == DialogResult.Yes)
				payment += payment*0.2m;

			if (result != DialogResult.Cancel)
			{
				var submitPaymentMessage = new SubmitPaymentMessage
					{
						CorrelationId = message.CorrelationId,
						PaymentType = PaymentType.CreditCard,
						Amount = payment,
					};

				Bus.Publish(submitPaymentMessage);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string drink = comboBox1.Text;
			string size = comboBox2.Text;
			string name = txtName.Text;

			_transactionId = CombGuid.Generate();

			if (_unsubscribeToken != null)
				_unsubscribeToken();
			_unsubscribeToken = Bus.Subscribe(this);

			var message = new NewOrderMessage
				{
					CorrelationId = _transactionId,
					Item = drink,
					Name = name,
					Size = size,
				};

			Bus.Publish(message);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (_unsubscribeToken != null)
			{
				_unsubscribeToken();
				_unsubscribeToken = null;
			}
			if (_bus != null)
			{
				_bus.Dispose();
				_bus = null;
			}

			base.OnClosing(e);
		}
	}
}