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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Win32;

namespace WPF_AzureAI
{
    public partial class MainWindow : Window
    {
        const string subscriptionKey = "e8230d69dd1f4440bef4abaca1135f93";
        const string uriBase = "https://southeastasia.api.cognitive.microsoft.com/vision/v1.0/analyze";

        const string subscriptionKeyFace = "4de8511232f74d94b5c6c64e3f0db8d0";
        const string uriBaseFace = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0";
        static string bindingString = string.Empty;
        string imageFilePath = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            //imageFilePath = NewPic.FileName.ToString();//"C:\\Users\\Myn\\Pictures\\IMG_20171007_104353933.jpg";
            MakeAnalysisRequest(imageFilePath,"ComVesAPI");
            RichTextBox.Document.Blocks.Clear();
            RichTextBox.Document.Blocks.Add(new Paragraph(new Run(bindingString)));

        }

        static async void MakeAnalysisRequest(string imageFilePath, string APIName)
        {
            HttpClient client = new HttpClient();
            string requestParameters;
            string uri;

            if (APIName == "ComVesAPI")
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                 requestParameters = "visualFeatures=Categories,Description,Color&language=en";

                 uri = uriBase + "?" + requestParameters;
            }
           else
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKeyFace);

                 requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

                 uri = uriBaseFace + "?" + requestParameters;
            }

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                response = await client.PostAsync(uri, content);

                string contentString = await response.Content.ReadAsStringAsync();

                bindingString=JsonPrettyPrint(contentString);
                
            }
           
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }


        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Title = "Select a picture";
            opf.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
           "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
           "Portable Network Graphic (*.png)|*.png";
            if (opf.ShowDialog() == true)
            {
                NewPic.Source = new BitmapImage(new Uri(opf.FileName));
                imageFilePath = opf.FileName;
            }
        }

        private void ButtonFace_Click(object sender, RoutedEventArgs e)
        {
            MakeAnalysisRequest(imageFilePath,"FaceAPI");
            RichTextBox.Document.Blocks.Clear();
            RichTextBox.Document.Blocks.Add(new Paragraph(new Run(bindingString)));
        }
    }
}
