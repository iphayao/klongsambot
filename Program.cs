using System;
using System.IO;
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
            SearchItem searchItem = GenSearchItem();
            RunAsync(searchItem).Wait();
            
        }

        static async Task RunAsync(SearchItem item) {
            client.BaseAddress = new Uri("https://www.agoda.com/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string testfile = "test.htm";

            if(File.Exists(testfile)) 
            {
                File.Delete(testfile);
            }

            HttpResponseMessage response = await client.GetAsync(string.Format("pages/agoda/default/DestinationSearchResult.aspx?{0}", GenUriParameter(item)));
            if(response.IsSuccessStatusCode)
            {
                string x = await response.Content.ReadAsStringAsync();
                File.AppendAllText("test.htm", x);
            }
        }

        static private SearchItem GenSearchItem() {
            SearchItem item = new SearchItem();

            item.CityID = "4064";
            item.CheckIn = "2017-04-04";
            item.CheckOut = "2017-04-15";
            item.Rooms = "1";
            item.Adults = "2";
            item.Children = "0";
            item.Origin = "TH";
            item.LanguageId = "1";
            item.CurrencyCode = "THB";
            item.PriceCurrency = "THB";
            item.HtmlLanguage = "en-us";
            item.TrafficType = "User";
            item.CultureInfoName = "en-US";

            return item;
        }

        static private string GenUriParameter(SearchItem item) {
            string parameter = string.Empty;
            
            parameter += string.Format("city={0}", item.CityID);
            parameter += string.Format("&checkIn={0}", item.CheckIn);
            parameter += string.Format("&checkOut={0}", item.CheckOut);
            parameter += string.Format("&rooms={0}", item.Rooms);
            parameter += string.Format("&adults={0}", item.Adults);
            parameter += string.Format("&children={0}", item.Children);
            parameter += string.Format("&origin={0}", item.Origin);
            parameter += string.Format("&languageId={0}", item.LanguageId);
            parameter += string.Format("&currencyCode={0}", item.CurrencyCode);
            parameter += string.Format("&priceCur={0}", item.PriceCurrency);
            parameter += string.Format("&trafficType={0}", item.TrafficType);
            parameter += string.Format("&htmlLanguage={0}", item.HtmlLanguage);
            parameter += string.Format("&cultureInfoName={0}", item.CultureInfoName);

            return parameter;
        }


        // static async Task<string> GetAsync()
        // {
        //     string ret = string.Empty;

        //     HttpResponseMessage response = await client.GetAsync();
        //     if(response.IsSuccessStatusCode)
        //     {
        //         ret = await response.Content.ReadAsStringAsync();
        //     }
        //     return ret;
        // }

    }
}
