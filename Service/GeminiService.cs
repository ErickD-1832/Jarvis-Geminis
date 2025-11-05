using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.AIPlatform.V1;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace Jarvis.Service
{
    public class GeminiService : JarvisServices
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly string _geminiUrl;


   public GeminiService(IConfiguration configuration)
{
    _apiKey = configuration["Gemini:ApiKey"];
    _httpClient = new HttpClient();
    
    // 🔥 ENDPOINT CORRECTO para Google AI
    _geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
    
    // Agregar headers comunes
    _httpClient.DefaultRequestHeaders.Add("User-Agent", "Jarvis-App/1.0");
}

        public async Task<string> GetProgrammingResponseAsync(string userMessage)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                     text = $@"Eres un tutor experto en programación especializado en C# y .NET. 
                                    Responde como un mentor paciente y claro. 
                                    Proporciona ejemplos de código cuando sea apropiado.
                                    Explica conceptos de manera estructurada.
                                    Sé conciso pero completo.

                                    Pregunta del usuario: {userMessage}"
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 1024,
                    }

                };
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_geminiUrl, content);
                response.EnsureSuccessStatusCode();


                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    throw new HttpRequestException($"Error{response.StatusCode}:{errorContent}");
                }

                var responseContest = await response.Content.ReadAsStringAsync();

                var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseContest);
                // 🔥 Manejo seguro de la respuesta

                return geminiResponse?.candidates?[0]?.Content?.Parts?[0]?.Text
                  ?? "Lo siento, no pude generar una respuesta.";

            }
            catch (Exception ex)
            {
                // 🔥 Log más detallado

                Console.WriteLine($"Error completado: {ex}");
                return $"Error al comunicarse con Gemini: {ex.Message}";

            }
        }

        // 🔥 CLASES CORREGIDAS para deserialización
        private class GeminiResponse
        {
            [JsonProperty("candidates")]
            public List<Candidate> candidates { get; set; }
        }

        private class Candidate
        {
            [JsonProperty("content")]
            public Content Content { get; set; }
        }

        private class Part
        {
            [JsonProperty("text")]
            public string Text { get; set; } = string.Empty;
        }
    }
}

