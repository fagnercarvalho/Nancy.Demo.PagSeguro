namespace Nancy.Demo.PagSeguro
{
    using System.Configuration;
    using System.Net.Http;
    using System.Text;
    using System.Xml.Linq;

    public class PagSeguroModule : NancyModule
    {
        public PagSeguroModule()
        {
            Get["/payment", true] = async (parameters, ct) =>
            {
                var httpClient = new HttpClient();

                var xml = XDocument.Load("payment.xml").ToString();
                var content = new StringContent(
                    xml,
                    Encoding.GetEncoding("ISO-8859-1"),
                    "application/xml");

                var email = ConfigurationManager.AppSettings["EMAIL"];
                var token = ConfigurationManager.AppSettings["TOKEN"];

                var response = await httpClient.PostAsync($@"https://ws.sandbox.pagseguro.uol.com.br/v2/checkout?email={email}&token={token}", content);

                string code = string.Empty;
                if (response.IsSuccessStatusCode)
                {
                    var xmlContent = await response.Content.ReadAsStreamAsync();
                    code = XDocument.Load(xmlContent).Root.Element("code").Value;

                    return Response.AsRedirect($@"https://sandbox.pagseguro.uol.com.br/v2/checkout/payment.html?code={code}");
                } else
                {
                    return Response.AsJson(response.ReasonPhrase, HttpStatusCode.InternalServerError);
                }
            };
        }
    }
}
