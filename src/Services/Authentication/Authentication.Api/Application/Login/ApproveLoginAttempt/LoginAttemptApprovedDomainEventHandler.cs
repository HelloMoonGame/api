using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Application.Users.ConfirmEmailAddress;
using Authentication.Api.Domain.Login;
using MediatR;

namespace Authentication.Api.Application.Login.ApproveLoginAttempt
{
    public class LoginAttemptApprovedDomainEventHandler : INotificationHandler<LoginAttemptApprovedEvent>
    {
        private readonly IMediator _mediator;

        public LoginAttemptApprovedDomainEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(LoginAttemptApprovedEvent notification, CancellationToken cancellationToken)
        {
            return _mediator.Send(new ConfirmEmailAddressCommand(notification.UserId), cancellationToken);
        }
    }
}
