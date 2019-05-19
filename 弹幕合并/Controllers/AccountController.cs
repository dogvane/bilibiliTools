using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using 弹幕合并.Bussiness;
using 弹幕合并.Common;

namespace 弹幕合并.Controllers
{
    /// <summary>
    /// this class is for swagger to generate AuthToken Header filed on swagger UI
    /// </summary>
    public class AddAuthTokenHeaderParameter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();
            //var attrs = context.ApiDescription.ActionAttributes();
            //foreach (var attr in attrs)
            {
                // 如果 Attribute 是我们自定义的验证过滤器
                //if (attr.GetType() == typeof(Auth))
                {
                    operation.Parameters.Add(new NonBodyParameter()
                    {
                        Name = "Authorization",
                        In = "header",
                        Type = "string",
                        Required = false
                    });
                }
            }
        }
    }

    /// <summary>
    /// 权限验证
    /// </summary>
    [ApiController]
    public class AccountController : BaseController {
        private readonly IConfiguration _configuration;
        public AccountController (IConfiguration configuration) : base () {
            _configuration = configuration;
        }

        /// <summary>
        /// 登陆的请求对象
        /// </summary>
        public class LoginRequest
        {
            /// <summary>
            /// 用户名
            /// </summary>
            public string username { get; set; }

            /// <summary>
            /// 密码
            /// </summary>
            public string pwd { get; set; }

            public override string ToString()
            {
                return string.Format("{0} {1}", username, pwd);
            }
        }

        public static AccountBussiness bu = new AccountBussiness();

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route ("api/account/login")]
        public ServerReturn Login (LoginRequest request) {
            Logger.Info("Login request {0}", request);

            var account = bu.GetAccount(request.username);
            if (account == null)
            {
                return new ServerReturn { error = 1, error_msg = "账号或者密码错误" };
            }

            var pwd = NETCore.Encrypt.EncryptProvider.Sha256(request.username + request.pwd + "bilibilitools");
            if (pwd != account.Pwd)
            {
                return new ServerReturn { error = 1, error_msg = "账号或者密码错误" };
            }

            var userid = account.Id;

            //   生成jwt令牌
            var claims = new [] {
                new Claim ("userid", userid.ToString ()),   // 这里将用户id带入令牌里，以后的Controller就可以直接获得用户id，做后面的操作
            };
            //sign the token using a secret key.This secret will be shared between your API and anything that needs to check that the token is legit.
            var skey = _configuration["SecurityKey"];
            Logger.Info($"SecurityKey {skey}", skey);

            var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (skey));
            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha256);
            //.NET Core’s JwtSecurityToken class takes on the heavy lifting and actually creates the token.
            /**
             * Claims (Payload)
                Claims 部分包含了一些跟这个 token 有关的重要信息。 JWT 标准规定了一些字段，下面节选一些字段:

                iss: The issuer of the token，token 是给谁的
                sub: The subject of the token，token 主题
                exp: Expiration Time。 token 过期时间，Unix 时间戳格式
                iat: Issued At。 token 创建时间， Unix 时间戳格式
                jti: JWT ID。针对当前 token 的唯一标识
                除了规定的字段外，可以包含其他任何 JSON 兼容的字段。
             * */
            var token = new JwtSecurityToken (
                issuer: "chsarptools.com",
                audience: "chsarptools.com",
                claims : claims,
                expires : DateTime.Now.AddDays (30),
                signingCredentials : creds);

            var takenCode = new JwtSecurityTokenHandler ().WriteToken (token);

            return new ServerReturn { data = new { userid, token = takenCode } };
        }

        [HttpPost]
        [Route ("api/account/register")]
        public ServerReturn Register(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.username))
            {
                return new ServerReturn {error = 1, error_msg = "用户名不能为空"};
            }

            if (string.IsNullOrEmpty(request.pwd))
            {
                return new ServerReturn { error = 1, error_msg = "密码" };
            }

            var account = bu.GetAccount(request.username);
            if (account != null)
            {
                return new ServerReturn {  error = 1, error_msg = "用户已存在"};
            }

            var pwd = NETCore.Encrypt.EncryptProvider.Sha256(request.username + request.pwd + "bilibilitools");

            bu.Register(request.username, pwd);

            return Login(request);
        }
    }
}
