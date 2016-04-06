using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class Light
    {
        private const string BASE_URL = "http://localhost:3000/";
        public async Task<bool> On(Models.Light light)
        {
            try
            {
                using (HttpClient webClient = new HttpClient())
                {
                    string json = JsonConvert.SerializeObject(light);
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await webClient.PostAsync($"{BASE_URL}light/on", content);

                    return response.StatusCode == System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Off(long lightId)
        {
            try
            {
                using (HttpClient webClient = new HttpClient())
                {
                    HttpResponseMessage response = await webClient.GetAsync($"{BASE_URL}light/off?lightId={lightId}");
                    return response.StatusCode == System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
