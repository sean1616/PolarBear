using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenAI.Infrastructure;
using OpenAI_API;
using OpenAI_API.Completions;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace PD.NavigationPages
{
    /// <summary>
    /// Window_ChatGPT.xaml 的互動邏輯
    /// </summary>
    public partial class Window_ChatGPT : Window
    {
        // Create a client object with your API key
        static string apiKey = "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";

        public Window_ChatGPT()
        {
            InitializeComponent();

            
            
        }

        async Task Main(string[] args)
        {
            //string endpoint = "https://api.openai.com/v1/engine/davinci-codex/completions";

            //string endpoint = "https://api.openai.com/v1/models";
            //endpoint = "https://api.openai.com/v1/engine/davinci-codex/completions";

            txt_reply.Text = "";

            string prompt = txt_send.Text;
            int maxTokens = 100;
            //bool stopSequence = true;

            HttpClient _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/completions");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");

            //string requestBody = "{\"prompt\":\"" + prompt + "\",\"max_tokens\":" + maxTokens + ",\"stop\":[" + stopSequence.ToString().ToLower() + "]}";
            string model = "text-davinci-003";

            string cmd = $"{{\"model\": \"{model}\", \"prompt\": \"{txt_send.Text}\", \"max_tokens\": {maxTokens}}}";
            //string cmd = $"{{\"model\": \"{model}\", \"prompt\": \"{txt_send.Text}\"}}";
            var content = new StringContent(
                cmd,
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_httpClient.BaseAddress.ToString(), content);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);

            JToken json = JToken.Parse(responseString);
            string extractedText = json.SelectToken("choices[0].text").ToString();
            extractedText = extractedText.Replace("\\n\\n", Environment.NewLine);

            //RestoreBounds.
            txt_reply.Text += extractedText;
        }

        public string UseChatGPT(string query)
        {
            string OutPutResult = "";
            var openai = new OpenAIAPI(apiKey);
            CompletionRequest completionRequest = new CompletionRequest();
            completionRequest.Prompt = query;
            completionRequest.Model = OpenAI_API.Models.Model.DavinciText;

            var completions = openai.Completions.CreateCompletionAsync(completionRequest);

            if (completions.Result == null)
                return "";

            foreach(var completion in completions.Result.Completions)
            {
                OutPutResult += completion.Text;
            }

            return OutPutResult;
        }

        private void Txt_send_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                //string msg = UseChatGPT(txt_send.Text);
                //txt_reply.Text = msg;


                string[] ag = new string[10];
                Main(ag);
            }
        }
    }
}
