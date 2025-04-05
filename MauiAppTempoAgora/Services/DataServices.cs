using MauiAppTempoAgora.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;

namespace MauiAppTempoAgora.Services
{
    public class DataServices
    {
        // Propriedade estática para armazenar a última mensagem de erro
        public static string UltimaMensagemErro { get; private set; } = string.Empty;

        public static async Task<Tempo?> GetPrevisao(string cidade)
        {
            Tempo? t = null;
            UltimaMensagemErro = string.Empty; // Resetar mensagem de erro

            string chave = "5742507aefcaaac9ce6a8e9b4b00b74c";
            string url = $"https://api.openweathermap.org/data/2.5/weather?" +
                         $"q={cidade}&APPID={chave}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Configurar timeout para verificar problemas de conexão mais rapidamente
                    client.Timeout = TimeSpan.FromSeconds(10);

                    HttpResponseMessage resp = await client.GetAsync(url);

                    // Verificar status da resposta
                    if (resp.IsSuccessStatusCode)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        var rascunho = JObject.Parse(json);
                        DateTime time = new();
                        DateTime sunrise = time.AddSeconds((double)rascunho["sys"]["sunrise"]).ToLocalTime();
                        DateTime sunset = time.AddSeconds((double)rascunho["sys"]["sunset"]).ToLocalTime();
                        t = new();
                        {
                            t.lon = (double)rascunho["coord"]["lon"];
                            t.lat = (double)rascunho["coord"]["lat"];
                            t.temp_min = (double)rascunho["main"]["temp_min"];
                            t.temp_max = (double)rascunho["main"]["temp_max"];
                            t.visibility = (int)rascunho["visibility"];
                            t.description = (string)rascunho["weather"][0]["description"];
                            t.speed = (double)rascunho["wind"]["speed"];
                            t.sunrise = sunrise.ToString();
                            t.sunset = sunset.ToString();
                        }
                        ;
                    }
                    else if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Cidade não encontrada (código 404)
                        UltimaMensagemErro = $"A cidade '{cidade}' não foi encontrada. Verifique se o nome está correto.";
                    }
                    else
                    {
                        // Outros erros de API
                        UltimaMensagemErro = $"Ocorreu um erro ao buscar informações: {resp.StatusCode}";
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Problemas de conexão com a internet
                if (ex.InnerException is System.Net.Sockets.SocketException ||
                    ex.InnerException is System.Net.WebException)
                {
                    UltimaMensagemErro = "Sem conexão com a internet. Verifique sua conexão e tente novamente.";
                }
                else
                {
                    UltimaMensagemErro = $"Erro de conexão: {ex.Message}";
                }
            }
            catch (TaskCanceledException)
            {
                // Timeout - provavelmente sem conexão
                UltimaMensagemErro = "Tempo de espera esgotado. Verifique sua conexão com a internet.";
            }
            catch (Exception ex)
            {
                // Outros erros inesperados
                UltimaMensagemErro = $"Ocorreu um erro inesperado: {ex.Message}";
            }

            return t;
        }
    }
}