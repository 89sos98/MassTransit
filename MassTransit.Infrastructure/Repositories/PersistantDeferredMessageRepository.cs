namespace MassTransit.Infrastructure.Repositories
{
    using System;
    using Services;
    using Services.Messages;

    public class PersistantDeferredMessageRepository :
        IDeferredMessageRepository
    {
        private Magnum.Infrastructure.Repository.NHibernateRepository _repository;

        public void Dispose()
        {
            _repository.Dispose();
        }

        public void Add(Guid id, DeferMessage message)
        {
            _repository.Save(new DeferredMessage(id, message.DeliverAt, message.Message));
        }

        public DeferMessage Get(Guid id)
        {
            DeferredMessage msg = _repository.Get<DeferredMessage>(id);
            DeferMessage result = new DeferMessage(msg.Id, msg.DeliverAt, msg.GetMessage<object>());
            return result;
        }

        public bool Contains(Guid id)
        {
            return _repository.Get<DeferredMessage>(id) != null;
        }

        public void Remove(Guid id)
        {
            _repository.Delete(_repository.Get<DeferredMessage>(id));
        }
    }
}