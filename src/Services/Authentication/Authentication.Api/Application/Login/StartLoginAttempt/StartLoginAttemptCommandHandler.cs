using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Domain.Login;
using Common.Domain.SeedWork;
using MediatR;

namespace Authentication.Api.Application.Login.StartLoginAttempt
{
    public class StartLoginAttemptCommandHandler : IRequestHandler<StartLoginAttemptCommand, LoginAttemptDto>
    {
        private readonly ILoginAttemptRepository _loginAttemptRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StartLoginAttemptCommandHandler(ILoginAttemptRepository loginAttemptRepository, IUnitOfWork unitOfWork)
        {
            _loginAttemptRepository = loginAttemptRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<LoginAttemptDto> Handle(StartLoginAttemptCommand request, CancellationToken cancellationToken)
        {
            await CancelLoginAttemptsForUserId(request.UserId, cancellationToken);
            var loginAttempt = await CreateLoginAttempt(request.UserId, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return LoginAttemptDto.FromLoginAttempt(loginAttempt);
         }

        private async Task CancelLoginAttemptsForUserId(Guid userId, CancellationToken cancellationToken)
        {
            var loginAttempts = (await _loginAttemptRepository.GetAllByUserIdAsync(userId, cancellationToken)).ToList();
            loginAttempts.ForEach(l => _loginAttemptRepository.Delete(l));
        }
        
        private async Task<LoginAttempt> CreateLoginAttempt(Guid userId, CancellationToken cancellationToken)
        {
            var loginAttempt = LoginAttempt.Create(userId, TimeSpan.FromMinutes(10));
            await _loginAttemptRepository.AddAsync(loginAttempt, cancellationToken);
            return loginAttempt;
        }
    }
}
