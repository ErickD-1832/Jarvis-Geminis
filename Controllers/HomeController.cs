using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Models;
using Jarvis.Service;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Jarvis.Controllers;

public class HomeController : Controller
{
    private readonly JarvisServices _geminiService;

    public HomeController(JarvisServices geminiService)
    {
        _geminiService = geminiService;
    }
    public IActionResult Index()
    {
        var model = new ChatViewModel();

        // Mensaje de bienvenida 

        model.Messages.Add(new ChatMessage
        {
            Message = "¡Hola! Soy tu tutor de programación en C#. ¿En qué puedo ayudarte hoy?",

            IsUser = false,

            Timestamp = DateTime.Now

        });
        return View(model);
    }

    [HttpPost]

    public async Task<IActionResult> SendMessage(ChatViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NewMessage))
        {
            return RedirectToAction("Index");
        }

        // Obtener mensajes existentes de la sesión o crear nueva lista
        var messages = HttpContext.Session.GetObject<List<ChatMessage>>("ChatMessages")
        ?? new List<ChatMessage>();

        // Agregar mensaje del usuario

        var userMessage = new ChatMessage
        {
            Message = model.NewMessage,
            IsUser = true,
            Timestamp = DateTime.Now
        };

        messages.Add(userMessage);

        // Obtener respuesta de Gemini

        var response = await _geminiService.GetProgrammingResponseAsync(model.NewMessage);

        var botMessage = new ChatMessage
        {
            Message = response,
            IsUser = false,
            Timestamp = DateTime.Now
        };

        messages.Add(botMessage);


        // Guardar en sesión

        HttpContext.Session.SetObject("ChatMessages", messages);

        var UpdatedModel = new ChatViewModel
        {
            Messages = messages,
            NewMessage = ""
        };
        return View("Index", UpdatedModel);
    }

}

// Extensiones para manejar sesiones
        public static class SessionExtensions
    {
    public static void SetObject(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }
    public static T GetObject<T>(this ISession session,string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }
    }
