using GestorTarea.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace GestorTarea.Controllers
{
    public class ProyectoController : Controller
    {
        private readonly GestorTareasEntities db = new GestorTareasEntities();

        // GET: Proyecto - Vista principal con grid de proyectos
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndexP(int id)
        {
            try
            {
                int usuarioId = ObtenerUsuarioActualId();
                string rolUsuario = Session["Rol"]?.ToString();

                var proyecto = db.Proyectos
                    .Include(p => p.Tareas)
                    .Include(p => p.Usuarios) // Incluir datos del usuario creador
                    .FirstOrDefault(p => p.IdProyecto == id);

                if (proyecto == null)
                {
                    TempData["Error"] = "Proyecto no encontrado";
                    return RedirectToAction("Crear");
                }

                if (proyecto.IdUsuarioResponsable != usuarioId)
                {
                    TempData["Error"] = "No tienes permisos para acceder a este proyecto";
                    return RedirectToAction("Index");
                }

                ViewBag.TotalTareas = proyecto.Tareas?.Count() ?? 0;
                ViewBag.TareasCompletadas = proyecto.Tareas?.Count(t => t.Estado == "Completada") ?? 0;

                // Datos del usuario creador
                ViewBag.NombreCreador = proyecto.Usuarios?.Name ?? "Usuario desconocido";

                // Verificar si puede eliminar (creador o administrador)
                ViewBag.PuedeEliminar = proyecto.IdUsuarioResponsable == usuarioId || rolUsuario == "Administrador";

                return View(proyecto);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al cargar el proyecto";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public JsonResult ObtenerProyectos()
        {
            try
            {
                int usuarioId = ObtenerUsuarioActualId();

                var proyectos = db.Proyectos
                    .Include(p => p.Usuarios) // Incluir datos del usuario creador
                    .Where(p => p.IdUsuarioResponsable == usuarioId)
                    .OrderByDescending(p => p.FechaInicio)
                    .Select(p => new
                    {
                        id = p.IdProyecto,
                        titulo = p.Nombre,
                        descripcion = p.Descripcion,
                        fechaInicio = p.FechaInicio,
                        estado = p.Estado,
                        members = "solo",
                        nombreCreador = p.Usuarios.Name ?? "Usuario desconocido"
                    })
                    .ToList();

                return Json(new { success = true, proyectos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener proyectos" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CrearProyecto()
        {
            try
            {
                var json = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

                string title = data.title;
                string description = data.description;
                string members = data.members;

                if (string.IsNullOrWhiteSpace(title))
                {
                    return Json(new
                    {
                        success = false,
                        message = "El título es obligatorio"
                    });
                }

                if (title.Length > 100)
                {
                    return Json(new
                    {
                        success = false,
                        message = "El título no puede exceder 100 caracteres"
                    });
                }

                int usuarioId = ObtenerUsuarioActualId();

                var proyecto = new Proyectos
                {
                    Nombre = title.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    FechaInicio = DateTime.Now,
                    Estado = "Activo",
                    IdUsuarioResponsable = usuarioId
                };

                db.Proyectos.Add(proyecto);
                db.SaveChanges();

                var nombreCreador = db.Usuarios.FirstOrDefault(u => u.IdUsuario == usuarioId)?.Name ?? "Usuario";

                return Json(new
                {
                    success = true,
                    message = "Proyecto creado exitosamente",
                    proyecto = new
                    {
                        id = proyecto.IdProyecto,
                        nombre = proyecto.Nombre,
                        descripcion = proyecto.Descripcion,
                        fechaInicio = proyecto.FechaInicio,
                        estado = proyecto.Estado,
                        nombreCreador = nombreCreador
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        // POST: Eliminar proyecto
        [HttpPost]
        public JsonResult EliminarProyecto(int id)
        {
            try
            {
                int usuarioId = ObtenerUsuarioActualId();
                string rolUsuario = Session["Rol"]?.ToString();

                var proyecto = db.Proyectos.Find(id);
                if (proyecto == null)
                {
                    return Json(new { success = false, message = "Proyecto no encontrado" });
                }

                // Verificar permisos (solo creador o administrador)
                if (proyecto.IdUsuarioResponsable != usuarioId && rolUsuario != "Administrador")
                {
                    return Json(new { success = false, message = "No tienes permisos para eliminar este proyecto" });
                }

                // Eliminar todas las tareas asociadas al proyecto
                var tareasDelProyecto = db.Tareas.Where(t => t.IdProyecto == id).ToList();
                foreach (var tarea in tareasDelProyecto)
                {
                    db.Tareas.Remove(tarea);
                }

                // Eliminar el proyecto
                db.Proyectos.Remove(proyecto);
                db.SaveChanges();

                return Json(new { success = true, message = "Proyecto eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el proyecto: " + ex.Message });
            }
        }

        // Método auxiliar mejorado para obtener el ID del usuario actual
        private int ObtenerUsuarioActualId()
        {
            // Si ya tienes el ID en sesión, usarlo
            if (Session["UsuarioId"] != null)
            {
                return (int)Session["UsuarioId"];
            }

            // Si tienes el nombre del usuario en sesión, obtener el ID
            if (Session["Usuario"] != null)
            {
                var nombreUsuario = Session["Usuario"].ToString();
                var usuario = db.Usuarios.FirstOrDefault(u => u.Name == nombreUsuario && u.Activo);
                if (usuario != null)
                {
                    // Guardar el ID en sesión para futuras consultas
                    Session["UsuarioId"] = usuario.IdUsuario;
                    return usuario.IdUsuario;
                }
            }

            return 0; // Usuario no encontrado
        }

        // GET: Proyecto/Crear (mantener para compatibilidad)
        public ActionResult Crear()
        {
            ViewBag.Usuarios = db.Usuarios.Where(u => u.Activo).ToList();
            return View();
        }

        // POST: Crear proyecto (método original - mantener para compatibilidad)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Models.Proyectos proyecto)
        {
            ViewBag.Usuarios = db.Usuarios.Where(u => u.Activo).ToList();
            if (ModelState.IsValid)
            {
                proyecto.FechaInicio = DateTime.Now;
                proyecto.Estado = "Activo";
                db.Proyectos.Add(proyecto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(proyecto);
        }

        // POST: Crear proyecto AJAX (método original - mantener para compatibilidad)
        [HttpPost]
        public JsonResult CrearAjax(Proyectos proyecto)
        {
            if (ModelState.IsValid)
            {
                proyecto.FechaInicio = DateTime.Now;
                proyecto.Estado = "Activo";
                db.Proyectos.Add(proyecto);
                db.SaveChanges();
                return Json(new { success = true, id = proyecto.IdProyecto });
            }
            return Json(new { success = false });
        }

        // GET: Detalles del proyecto
        public ActionResult Detalles(int id)
        {
            var proyecto = db.Proyectos.Find(id);
            if (proyecto == null)
                return HttpNotFound();
            return View(proyecto);
        }

        // GET: Editar proyecto
        public ActionResult Editar(int id)
        {
            var proyecto = db.Proyectos.Find(id);
            if (proyecto == null)
                return HttpNotFound();

            ViewBag.Usuarios = db.Usuarios.Where(u => u.Activo).ToList();
            return View(proyecto);
        }

        // POST: Editar proyecto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Proyectos proyecto)
        {
            if (ModelState.IsValid)
            {
                db.Entry(proyecto).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Usuarios = db.Usuarios.Where(u => u.Activo).ToList();
            return View(proyecto);
        }

        // POST: Eliminar tarea
        [HttpPost]
        public JsonResult EliminarTarea(int id)
        {
            try
            {
                int usuarioId = ObtenerUsuarioActualId();
                string rolUsuario = Session["Rol"]?.ToString();

                var tarea = db.Tareas.Include("Proyectos").FirstOrDefault(t => t.IdTarea == id);
                if (tarea == null)
                    return Json(new { success = false, message = "Tarea no encontrada." });

                // Verificar permisos: creador del proyecto, asignado de la tarea o administrador
                if (tarea.Proyectos.IdUsuarioResponsable != usuarioId &&
                    tarea.IdUsuarioAsignado != usuarioId &&
                    rolUsuario != "Administrador")
                {
                    return Json(new { success = false, message = "No tienes permiso para eliminar esta tarea." });
                }

                db.Tareas.Remove(tarea);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Listar proyectos (vista alternativa)
        public ActionResult Indexes()
        {
            int usuarioId = ObtenerUsuarioActualId();
            var proyectos = db.Proyectos
                .Where(p => p.IdUsuarioResponsable == usuarioId)
                .OrderByDescending(p => p.FechaInicio)
                .ToList();

            return View(proyectos);
        }

        // GET: Listar tareas de un proyecto
        public ActionResult ListTareas(int proyectoId)
        {
            var proyecto = db.Proyectos.Include("Tareas").FirstOrDefault(p => p.IdProyecto == proyectoId);
            if (proyecto == null)
                return HttpNotFound();

            int usuarioId = ObtenerUsuarioActualId();
            if (proyecto.IdUsuarioResponsable != usuarioId)
                return new HttpUnauthorizedResult();

            var tareas = proyecto.Tareas.ToList();
            return View(tareas);
        }

        [HttpPost]
        public JsonResult GuardarTarea(int idProyecto, string titulo, string descripcion, string prioridad, string estado, DateTime? fechaVencimiento, int progreso)
        {
            try
            {
                var nuevaTarea = new Tareas
                {
                    IdProyecto = idProyecto,
                    Titulo = titulo,
                    Descripcion = descripcion,
                    Prioridad = prioridad,
                    Estado = estado,
                    FechaCreacion = DateTime.Now,
                    FechaLimite = fechaVencimiento,
                    Progreso = progreso,
                    IdUsuarioAsignado = ObtenerUsuarioActualId()
                };

                db.Tareas.Add(nuevaTarea);
                db.SaveChanges();

                return Json(new { success = true, id = nuevaTarea.IdTarea });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public JsonResult ObtenerTarea(int id)
        {
            using (var db = new GestorTareasEntities())
            {
                var tarea = db.Tareas.Find(id);
                return Json(tarea, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EditarTarea(Tareas tareaEditada)
        {
            try
            {
                using (var db = new GestorTareasEntities())
                {
                    var tarea = db.Tareas.Find(tareaEditada.IdTarea);
                    if (tarea != null)
                    {
                        tarea.Titulo = tareaEditada.Titulo;
                        tarea.Descripcion = tareaEditada.Descripcion;
                        tarea.Prioridad = tareaEditada.Prioridad;
                        tarea.Estado = tareaEditada.Estado;
                        tarea.FechaLimite = tareaEditada.FechaLimite;
                        tarea.Progreso = tareaEditada.Progreso;

                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                    return Json(new { success = false, message = "Tarea no encontrada." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult ObtenerTareasPorProyecto(int idProyecto)
        {
            try
            {
                int usuarioId = ObtenerUsuarioActualId();

                var proyecto = db.Proyectos.Include("Tareas").FirstOrDefault(p => p.IdProyecto == idProyecto);
                if (proyecto == null || proyecto.IdUsuarioResponsable != usuarioId)
                {
                    return Json(new { success = false, message = "No autorizado o proyecto no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                var tareas = proyecto.Tareas.Select(t => new
                {
                    id = t.IdTarea,
                    titulo = t.Titulo,
                    descripcion = t.Descripcion,
                    prioridad = t.Prioridad,
                    estado = t.Estado,
                    fechaLimite = t.FechaLimite?.ToString("yyyy-MM-dd"),
                    progreso = t.Progreso,
                    usuarioId = t.IdUsuarioAsignado,
                    fechaCreacion = t.FechaCreacion
                }).ToList();

                return Json(new { success = true, tareas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}