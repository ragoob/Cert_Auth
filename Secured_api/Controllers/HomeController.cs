using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Secured_api.Controllers
{
    [Route("")]
    [ApiController]
    [AllowAnonymous]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task< IActionResult> Get()
        {
            var cert = await HttpContext.Connection.GetClientCertificateAsync();
            return Ok(new {
                cert.Thumbprint,
                cert.Subject,
                cert.SubjectName,
                cert.FriendlyName

            });
        }
    }
}