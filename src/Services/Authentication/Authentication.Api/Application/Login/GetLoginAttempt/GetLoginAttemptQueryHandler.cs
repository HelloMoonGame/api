using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Domain.Login;
using MediatR;

namespace Authentication.Api.Application.Login.GetLoginAttempt
{
    public class GetLoginAttemptQueryHandler : IRequestHandler<GetLoginAttemptQuery, LoginAttemptDto>
    {
        private readonly ILoginAttemptRepository _loginAttemptRepository;

        public GetLoginAttemptQueryHandler(ILoginAttemptRepository loginAttemptRepository)
        {
            _loginAttemptRepository = loginAttemptRepository;
        }

        public async Task<LoginAttemptDto> Handle(GetLoginAttemptQuery request, CancellationToken cancellationToken)
        {
            var loginAttempt = await _loginAttemptRepository.GetByIdAsync(request.LoginAttemptId, cancellationToken);
            return loginAttempt == null ? null : LoginAttemptDto.FromLoginAttempt(loginAttempt);
        }
    }
}
