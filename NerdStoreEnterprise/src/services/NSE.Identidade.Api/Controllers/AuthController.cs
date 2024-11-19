using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSE.Identidade.API.Extensions;
using NSE.Identidade.API.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NSE.Identidade.API.Controllers
{
    [ApiController]
    [Route("api/identidade")]
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppSettings> appSettings)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _appSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
        }

        [HttpPost("registrar")]
        public async Task<ActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if(!ModelState.IsValid) return BadRequest();

            var user = new IdentityUser
            {
                Email = usuarioRegistro.Email,
                EmailConfirmed = true,
                UserName = usuarioRegistro.Email,
            };

            var result = await _userManager.CreateAsync(user, usuarioRegistro.Senha);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return Ok(await GerarJwt(user.Email));
            }

            // Vai checar por "user.Errors" depois
            return BadRequest();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return BadRequest();

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, false, true);

            if (result.Succeeded)
            {
                return Ok(await GerarJwt(usuarioLogin.Email));
            }

            // Vai checar por "user.Errors" depois
            return BadRequest();
        }

        private async Task<UsuarioRespostaLogin> GerarJwt(string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            //var userRoles = await _userManager.GetRolesAsync(user);
            List<string> userRoles = (List<string>)await _userManager.GetRolesAsync(user);

            // Gerando novas Claims
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // Criando um Id para o Token https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.7
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString())); // Definindo quando o Token irá expirar (Token ñ é aceito para datas inferiores a esta definida, geralmente, a msm data de criação) https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.5
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64)); // Definindo quando o Token foi criado https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.6

            // Adicionando cada uma das Roles do usuário ao Token
            //foreach (var role in userRoles)
            //{
            //    claims.Add(new Claim("role", role));
            //}
            userRoles.ForEach(role => claims.Add(new Claim("role", role)));

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims); // Adicionando as claims de usuário e as novas claims ao Identity

            // Gerando o manipulador do Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            });

            var encodedToken = tokenHandler.WriteToken(token);

            var response = new UsuarioRespostaLogin
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
                UsuarioToken = new UsuarioToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UsuarioClaim { Type = c.Type, Value = c.Value }),
                }
            };

            return response;
        }

        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
