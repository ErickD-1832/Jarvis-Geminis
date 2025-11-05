using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Models;

namespace Jarvis.Service
{
  public interface JarvisServices
    {
        Task<string> GetProgrammingResponseAsync(string Message);
    }
}