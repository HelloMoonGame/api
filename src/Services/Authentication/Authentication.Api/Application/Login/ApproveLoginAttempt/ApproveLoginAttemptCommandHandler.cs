using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Domain.Login;
using Common.Domain.SeedWork;
using MediatR;

namespace Authentication.Api.Application.Login.ApproveLoginAttempt
{
    public class ApproveLoginAttemptCommandHandler : IRequestHandler<ApproveLoginAttemptCommand, LoginAttemptDto>
    {
        private readonly ILoginAttemptRepository _loginAttemptRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveLoginAttemptCommandHandler(ILoginAttemptRepository loginAttemptRepository,
            IUnitOfWork unitOfWork)
        {
            _loginAttemptRepository = loginAttemptRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<LoginAttemptDto> Handle(ApproveLoginAttemptCommand request, CancellationToken cancellationToken)
        {
            var loginAttempt = await _loginAttemptRepository.GetByIdAsync(request.LoginAttemptId, cancellationToken);
            if (loginAttempt == null || loginAttempt.Secret != request.Secret)
                return null;
            
            loginAttempt.Approve();
            await _unitOfWork.CommitAsync(cancellationToken);
            
            return LoginAttemptDto.FromLoginAttempt(loginAttempt);
        }
    }
}
