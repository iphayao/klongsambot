using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace klongsambot
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {            
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("https://www.agoda.com/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

    }
}
