using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
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

        static async Task RunAsync() {
            client.BaseAddress = new Uri("https://www.agoda.com/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            SearchItem item;
            // 4064  - Singapore
            // 5085  - Tokyo
            // 9590  - Osaka
            // 1784  - Kyoto ***
            // 79849 - Hakone ***
            // 9395  - Bangkok
            // 8584  - Pattaya
            // 7401  - Chiang Mai
            // 16056 - Phuket
            // 14865 - Krabi
            // 17198 - Samui
            string[] cities = {"4064", "9395", "5085", "9590", "1784", "79849", "9590", "8584", "7401", "16056", "14865", "17198"};
            //string[] cities = {"1784"};
            foreach(string city in cities) {
                item = GenSearchItem(city);
                string resultDir = string.Format("{0}//{1}", Directory.GetCurrentDirectory(), "result");

                string resultHTMLDir = string.Format("{0}//{1}", resultDir, "HTML");
                string resultJSONDir = string.Format("{0}//{1}", resultDir, "JSON");

                string resultHTML = string.Format("{0}//{1}.htm", resultHTMLDir, city);
                string resultJSON = string.Format("{0}//{1}.json", resultJSONDir, city);

                if (!Directory.Exists(resultHTMLDir))
                    Directory.CreateDirectory(resultHTMLDir);
                
                if (!Directory.Exists(resultJSONDir))
                    Directory.CreateDirectory(resultJSONDir);

                if (File.Exists(resultHTML))
                    File.Delete(resultHTML);
                
                if (File.Exists(resultJSON))
                    File.Delete(resultJSON);

                HttpResponseMessage response = await client.GetAsync(string.Format("pages/agoda/default/DestinationSearchResult.aspx?{0}", GenUriParameter(item)));
                if (response.IsSuccessStatusCode)
                {
                    string resString = await response.Content.ReadAsStringAsync();
                    File.AppendAllText(resultHTML, resString);

                    List<HotelInfo> hotelsInfo = GetHotelInfo(resString);
                    foreach (HotelInfo hotelInfo in hotelsInfo) {
                        string result = string.Empty;
                        result += string.Format("{0}\n", "{");
                        result += string.Format("\t\"No\": \"{0}\",\n"         , hotelsInfo.IndexOf(hotelInfo) + 1);
                        result += string.Format("\t\"ID\": \"{0}\",\n"         , hotelInfo.ID);
                        result += string.Format("\t\"RoomID\": \"{0}\",\n"     , hotelInfo.RoomID);
                        result += string.Format("\t\"Name\": \"{0}\",\n"       , hotelInfo.Name);
                        result += string.Format("\t\"StarRate\": \"{0}\",\n"   , hotelInfo.StarRate);
                        result += string.Format("\t\"AreaCity\": \"{0}\",\n"   , hotelInfo.AreaCity);
                        result += string.Format("\t\"HighPrice\": \"{0}\",\n"  , hotelInfo.HighPrice);
                        result += string.Format("{0}\n", "},");
                        File.AppendAllText(resultJSON, result);
                    }
                }
            }
        }

        static private SearchItem GenSearchItem() {
            SearchItem item = new SearchItem();

            item.CityID = "4064";
            item.CheckIn = DateTime.Now.ToString("yyyy-MM-dd");
            item.CheckOut = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
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

        static private SearchItem GenSearchItem(string cityID) {
            SearchItem item = GenSearchItem();
            item.CityID = cityID;
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

        static private List<HotelInfo> GetHotelInfo(string data) {
            List<HotelInfo> hotelsInfo = new List<HotelInfo>();

            string tagHotelItem = "data-selenium=\"hotel-item\"";
            string tagHotelId = "data-hotelid";
            string tagRoomId = "data-roomid";
            string tagPrice = "price soft-red";
            string tagHighPrice = "price dark-gray1";
            string tagHotelName = "hotel-name";
            string tagAreaCity = "areacity-name-text";
            string tagStarRate = "ficon ficon-star-";

            int idxHotelItem = 0;

            try {
                while(true) {
                    idxHotelItem = data.IndexOf(tagHotelItem, idxHotelItem);

                    if(idxHotelItem == -1) {
                        break;
                    }
                    else {
                        HotelInfo hotelInfo = new HotelInfo();
                        int idx = idxHotelItem;
                        // Looking for data-hotelid
                        hotelInfo.ID = GetAttValue(tagHotelId, data, idx);
                        // Looking for data-roomid
                        hotelInfo.RoomID = GetAttValue(tagRoomId, data, idx);
                        // Looking for hotelname
                        hotelInfo.Name = GetElmValue(tagHotelName, data, idx);
                        // Looking for areacity
                        hotelInfo.AreaCity = GetElmValue(tagAreaCity, data, idx);
                        // Looking for price
                        hotelInfo.LowPrice = GetElmValue(tagPrice, data, idx);
                        // Looking for high price
                        hotelInfo.HighPrice = GetElmValue(tagHighPrice, data, idx);
                        // Looking for starrate
                        hotelInfo.StarRate = GetElmValueStar(tagStarRate, data, idx);

                        if(!string.IsNullOrEmpty(hotelInfo.AreaCity)) {
                            hotelsInfo.Add(hotelInfo);
                        }
                        
                        idxHotelItem++;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return hotelsInfo;
        }

        static string GetAttValue(string tagelement, string data, int idx) {
            string value = string.Empty;
            int loc = 0;

            loc = data.IndexOf(tagelement, idx);
            if(loc != -1)
                value = GetAttValue(data, loc);

            return value;
        }

        static string GetAttValue(string data, int idx) {
            int attStrt = 0; 
            int attEnd  = 0;
            string tagAtt = "\"";
            string value = string.Empty;
            attStrt = data.IndexOf(tagAtt, idx) + 1;
            attEnd = data.IndexOf(tagAtt, attStrt);
            
            value = data.Substring(attStrt, attEnd - attStrt);

            return value;
        }

        static string GetElmValue(string tagclass, string data, int idx) {
            string value = string.Empty;
            int loc = 0;

            loc = data.IndexOf(string.Format("class=\"{0}\"", tagclass), idx);
            if(loc != -1)
                value = GetElmValue(data, loc);

            return value;
        }

        static string GetElmValue(string data, int idx) {
            int elmStr = 0;
            int elmEnd  = 0;

            string tagElmStrOpen = "<";
            string tagElmStrClose = "</";
            string tagElmEd = ">";
            string tagSpace = " ";

            string element = string.Empty;
            string value = string.Empty;

            // get HTML element name
            elmStr = data.Substring(0, idx).LastIndexOf(tagElmStrOpen) + 1;
            elmEnd = data.IndexOf(tagSpace, elmStr);
            element = data.Substring(elmStr, elmEnd - elmStr);

            elmStr = data.IndexOf(tagElmEd, idx) + 1;
            elmEnd = data.IndexOf(string.Format("{0}{1}", tagElmStrClose, element), elmStr);

            value = Regex.Replace(data.Substring(elmStr, elmEnd - elmStr), @"\t|\n|\r", "");
            if (value.Contains(tagElmStrOpen)) {
                value = value.Substring(0, value.IndexOf(tagElmStrOpen));
            }

            return value.Trim();
        }

        static string GetElmValueStar(string tagclass, string data, int idx) {
            string value = string.Empty;
            string tagSpace = " ";

            int loc = 0;
            int spc = 0;

            loc = data.IndexOf(string.Format("class=\"{0}", tagclass), idx) + tagclass.Length + 7;
            if(loc != -1) {
                spc = data.IndexOf(tagSpace, loc);
                value = data.Substring(loc, spc - loc);
                int star;
                if(int.TryParse(value, out star)) {
                    value = (star > 10) ? string.Format("{0}.{1}", (star / 10), (star % 10)) : value;
                }
            }
                
            return value;
        }

    }
}
