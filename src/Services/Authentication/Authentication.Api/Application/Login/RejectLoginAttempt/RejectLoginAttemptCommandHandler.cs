using System;
using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Domain.Login;
using Common.Domain.SeedWork;
using MediatR;

namespace Authentication.Api.Application.Login.RejectLoginAttempt
{
    public class RejectLoginAttemptCommandHandler : IRequestHandler<RejectLoginAttemptCommand, LoginAttemptDto>
    {
        private readonly ILoginAttemptRepository _loginAttemptRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RejectLoginAttemptCommandHandler(ILoginAttemptRepository loginAttemptRepository, 
            IUnitOfWork unitOfWork)
        {
            _loginAttemptRepository = loginAttemptRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginAttemptDto> Handle(RejectLoginAttemptCommand request, CancellationToken cancellationToken)
        {
            var loginAttempt = await _loginAttemptRepository.GetByIdAsync(request.LoginAttemptId, cancellationToken);
            if (loginAttempt == null || loginAttempt.Secret != request.Secret)
                return null;
            
            _loginAttemptRepository.Delete(loginAttempt);
            await _unitOfWork.CommitAsync(cancellationToken);
            return LoginAttemptDto.FromLoginAttempt(loginAttempt);
        }
    }
}
