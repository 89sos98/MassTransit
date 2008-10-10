namespace ClientGUI
{
    using System.Windows.Forms;
    using MassTransit.ServiceBus;
    using MassTransit.ServiceBus.Services.Timeout.Messages;

    public class TimeoutWatcher :
        Consumes<TimeoutExpired>.All
    {
        #region All Members

        public void Consume(TimeoutExpired message)
        {
            MessageBox.Show("Timeout Acheived", "bob", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}