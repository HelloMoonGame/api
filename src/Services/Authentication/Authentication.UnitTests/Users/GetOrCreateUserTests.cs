using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authentication.Api.Application.Users.GetOrCreateUser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Authentication.UnitTests.Users
{
    [TestClass]
    public class GetOrCreateUserTests
    {
        [TestMethod]
        public void Cannot_create_GetOrCreateUserCommand_without_mail_address()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new GetOrCreateUserCommand(null));
        }
    }
}
