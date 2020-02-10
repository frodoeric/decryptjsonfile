using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DescriptJsonFile
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            int casas = 10;
            var retorno = GetJsonEncript("" +
                "https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=c45b388f3d2cae71c3169e48c101133ff8f315a8").Result;

            var dataJson = DescerializeJson(retorno);
            var decifrado = DecriptLetter(casas, dataJson.Cifrado);
            dataJson.Decifrado = decifrado;
            dataJson.Resumo_criptografico = Hash(decifrado);

            DownloadJsonFile(dataJson);
            var result = PostJsonFileAsync().Result;
        }

        private static async Task<string> PostJsonFileAsync()
        {
            using (var content = new MultipartFormDataContent("----MyBoundary"))
            {
                using (FileStream s = File.Open("C:\\Users\\frodo\\source\\repos\\DescriptJsonFile\\answer.json", FileMode.Open))
                using (var memoryStream = s)
                {
                    using (var stream = new StreamContent(memoryStream))
                    {
                        content.Add(stream, "answer", "answer.json");

                        using (HttpClient client = new HttpClient())
                        {
                            var responce = await client.PostAsync("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=c45b388f3d2cae71c3169e48c101133ff8f315a8", content);
                            string contents = await responce.Content.ReadAsStringAsync();
                            return contents;
                        }
                    }
                }
            }
        }

        private static void DownloadJsonFile(DataJson dataJson)
        {
            var json = SerializeJson(dataJson);
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("C:\\Users\\frodo\\source\\repos\\DescriptJsonFile\\answer.json", FileMode.Create, isoStore);
            using (StreamWriter str = new StreamWriter(isoStream))
            {
                str.Write(json);
            }
        }

        private static string DecriptLetter(int casas, string cifrado)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(cifrado);
            byte[] asciiBytesReturned = new byte[asciiBytes.Length];
            for (int i = 0; i < asciiBytes.Length; i++)
            {
                var num = Convert.ToInt32(asciiBytes[i]);

                if (num != 32 && num != 46)
                {
                    var converted = num - casas;
                    if (converted < 97)
                    {
                        var sobra = 97 - converted - 1;
                        var letterReturned = 122 - sobra;
                        asciiBytesReturned[i] = Convert.ToByte(letterReturned);
                    }
                    else
                    {
                        asciiBytesReturned[i] = Convert.ToByte(converted);
                    }
                }  
                else
                {
                    asciiBytesReturned[i] = Convert.ToByte(num);
                }
            }
            var decifrado = Encoding.ASCII.GetString(asciiBytesReturned);
            return decifrado;
        }

        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private static DataJson DescerializeJson(string retorno)
        {
            var dataJson = JsonConvert.DeserializeObject<DataJson>(retorno);
            return dataJson;
        }

        private static string SerializeJson(DataJson dataJson)
        {
            var json = JsonConvert.SerializeObject(dataJson);
            return json;
        }

        static async Task<string> GetJsonEncript(string path)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(path))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
