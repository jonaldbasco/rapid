using rapid.core.app.Models;
using rapid.core.app.Services;
using rapid.core.app.Source;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;

namespace rapid.core.app.Controllers
{
    public class CommandController : Controller
    {
        private readonly Kernel _kernel;
        private readonly JsonLoaderService _jsonLoader;
        private readonly RapidDBContext _context;
        private readonly HttpClient _client;

        public CommandController(IHttpClientFactory factory, Kernel kernel, JsonLoaderService jsonLoader, RapidDBContext context, HttpClient client)
        {
            _kernel = kernel;
            _jsonLoader = jsonLoader;
            _context = context;
            _client = client;
            _client = factory.CreateClient("IgnoreSSL");
            _client.BaseAddress = new Uri("http://localhost:8000/");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
