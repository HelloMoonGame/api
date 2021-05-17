using System;
using System.Runtime.Serialization;

namespace Authentication.Api.Application.Users.GetOrCreateUser
{
    [Serializable]
    public sealed class CouldNotCreateUserException : ApplicationException
    {   
        public CouldNotCreateUserException(string message) : base(message)
        {
            
        }
        
        private CouldNotCreateUserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
