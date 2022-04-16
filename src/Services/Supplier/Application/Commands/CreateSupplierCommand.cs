using MediatR;
using Seedwork.DomainObjects;
using Supplier.Domain;
using EasyNetQ;
using Supplier.Application.IntegrationEvents;

namespace Supplier.Application.Commands
{
    public class CreateSupplierCommand : IRequest<CreateSupplierCommandResponse>
    {
        public CreateSupplierCommand(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
    }

    public class CreateSupplierCommandResponse
    {
        public CreateSupplierCommandResponse(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
    }

    public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, CreateSupplierCommandResponse>
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IUnitOfWork _uow;
        private readonly IBus _bus;

        public CreateSupplierCommandHandler(ISupplierRepository supplierRepository, 
                                            IUnitOfWork uow)
        {
            _supplierRepository = supplierRepository;
            _uow = uow;
            _bus =  RabbitHutch.CreateBus(Environment.GetEnvironmentVariable("CONNECTION_STRING_RABBITMQ") 
                                          ?? throw new ArgumentException("CONNECTION_STRING_RABBITMQ não foi definida em Supplier"));
        }

        public async Task<CreateSupplierCommandResponse> Handle(CreateSupplierCommand command, CancellationToken cancellationToken)
        {
            var supplier = new Supplier.Domain.Supplier(command.Name, command.Email);

            _supplierRepository.Add(supplier);
            await _uow.Commit();

            await _bus.PubSub.PublishAsync(new SupplierCreatedIntegrationEvent(supplier.Id, supplier.Name, supplier.Email));

            return new CreateSupplierCommandResponse(supplier.Id, supplier.Name, supplier.Email);            
        }
    }
}