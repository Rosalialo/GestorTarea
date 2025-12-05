using GestorTarea.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web.Http;  // Usar Web API

[RoutePrefix("api/tareas")]
public class TareasController : ApiController
{
    // Clave secreta para validar token (debe ser la misma que usaste para generarlo)
    private static string SecretKey = "123456";

    // Método para validar token y extraer claims
    private ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(SecretKey);
        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = System.TimeSpan.Zero
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, parameters, out validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    // GET api/tareas
    [HttpGet]
    [Route("lista")]
    public IHttpActionResult Get()
    {
        // Obtener token del header Authorization
        var authHeader = Request.Headers.Authorization;
        if (authHeader == null || authHeader.Scheme != "Bearer")
        {
            return Unauthorized();
        }

        var principal = ValidateToken(authHeader.Parameter);
        if (principal == null)
        {
            return Unauthorized();
        }

        // Extraer usuarioId de claims
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "UsuarioId");
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        int userId = int.Parse(userIdClaim.Value);

        using (var db = new GestorTareasEntities())
        {
            var tareas = db.Tareas
                .Where(t => t.IdUsuarioAsignado == userId)
                .Select(t => new
                {
                    t.IdTarea,
                    t.Titulo,
                    t.Estado,
                    t.Descripcion
                }).ToList();

            return Ok(tareas);
        }

    }

    [HttpPost]
    [Route("api/tareas/crear")]
    [Authorize]
    public IHttpActionResult CrearTarea([FromBody] Tareas tarea)
    {
        if (tarea == null || string.IsNullOrEmpty(tarea.Titulo))
            return BadRequest("Datos inválidos");

        using (var db = new GestorTareasEntities())
        {
            tarea.FechaCreacion = DateTime.Now;
            db.Tareas.Add(tarea);
            db.SaveChanges();
        }

        return Ok(new
        {
            Mensaje = "Tarea creada correctamente",
            tarea.IdTarea
        });
    }

    [HttpGet]
[Route("prueba")]
public IHttpActionResult Prueba()
{
    return Ok("API Tareas funciona");
}

}
