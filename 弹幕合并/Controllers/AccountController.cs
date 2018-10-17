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
using 弹幕合并.Common;

namespace 弹幕合并.Controllers
{
    /// <summary>
    /// 权限验证
    /// </summary>
    public class AccountController : BaseController {
        private readonly IConfiguration _configuration;
        public AccountController (IConfiguration configuration) : base () {
            _configuration = configuration;
        }

        /// <summary>
        /// 登陆的请求对象
        /// </summary>
        public class LoginRequest {
            /// <summary>
            /// 用户名
            /// </summary>
            public string username { get; set; }

            /// <summary>
            /// 密码
            /// </summary>
            public string pwd { get; set; }

            public override string ToString () {
                return string.Format ("{0} {1}", username, pwd);
            }
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route ("api/account/login")]
        public ServerReturn Login (LoginRequest request) {
            Logger.Info("Login request {0}", request);

            //var userid = BU.Account.CheckLogin (request.username, request.pwd);
            var userid = 1; // 上面实现自己从数据库验证账户名和密码信息，返回用户id，可以是数字，也可以是字符串，看你的项目决定
            if (userid == 0) {
                return new ServerReturn {
                error = 1,
                error_msg = "用户名或者密码错误"
                };
            }

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
        [Authorize]
        [Route ("api/bu/doyourtask")]
        public ServerReturn DoYourTask()
        {
            var userid = UserId;

            Logger.Info($"DoYourTask userid:{userid}", userid);

            // todo 你的业务代码里可以使用userid查询用户信息了

            return new ServerReturn { data = "我叫执行成功" };
        }
    }
}
