﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using YouLearn.Api.Controllers.Base;
using YouLearn.Api.Security;
using YouLearn.Domain.Args.Usuario;
using YouLearn.Domain.Interfaces.Services;
using YouLearn.Infra.Transaction;

namespace YouLearn.Api.Controllers
{
    //base controller criado para chamar o response async e usar o unit of work para dar commit quando nao tiver notificacoes
    public class UsuarioController : BaseController
    {
        private readonly IServiceUsuario _serviceUsuario;

        // unit of work passado para usar o commit do basecontroller
        public UsuarioController(IUnitOfWork unitOfWork, IServiceUsuario serviceUsuario) : base(unitOfWork)
        {
            _serviceUsuario = serviceUsuario;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/v1/Usuario/Adicionar")]
        public async Task<IActionResult> Adicionar([FromBody]AdicionarUsuarioRequest request)
        {
            try
            {
                var response = _serviceUsuario.AdicionarUsuario(request);
                return await ResponseAsync(response, _serviceUsuario);
            }
            catch (Exception ex)
            {
                return await ResponseExceptionAsync(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/v1/Usuario/Autenticar")]
        public object Autenticar(
            [FromBody]AutenticarUsuarioRequest request,
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {
            bool credenciaisValidas = false;
            AutenticarUsuarioResponse response = _serviceUsuario.AutenticarUsuario(request);

            credenciaisValidas = response != null;

            if (credenciaisValidas)
            {
                // e o que fica dentro do token que vou pegar depois
                ClaimsIdentity identity = new ClaimsIdentity(
                    new GenericIdentity(response.Id.ToString(), "Id"),
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        //new Claim(JwtRegisteredClaimNames.UniqueName, response.Usuario)
                        new Claim("Usuario", JsonConvert.SerializeObject(response))
                    }
                );

                DateTime dataCriacao = DateTime.Now;
                DateTime dataExpiracao = dataCriacao +
                    TimeSpan.FromSeconds(tokenConfigurations.Seconds);

                var handler = new JwtSecurityTokenHandler();
                // onde cria o token 
                var securityToken = handler.CreateToken(new SecurityTokenDescriptor
                {
                    Issuer = tokenConfigurations.Issuer,
                    Audience = tokenConfigurations.Audience,
                    SigningCredentials = signingConfigurations.SigningCredentials,
                    Subject = identity,
                    NotBefore = dataCriacao,
                    Expires = dataExpiracao
                });
                var token = handler.WriteToken(securityToken);

                // retorno para usuario
                return new
                {
                    authenticated = true,
                    created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                    expiration = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                    accessToken = token,
                    message = "OK",
                    PrimeiroNome = response.PrimeiroNome
                };
            }
            else
            {
                return new
                {
                    authenticated = false,
                    _serviceUsuario.Notifications
                };
            }
        }
    }
}
