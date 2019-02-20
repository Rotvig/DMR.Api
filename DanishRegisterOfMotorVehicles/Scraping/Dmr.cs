using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DanishRegisterOfMotorVehicles.Api.Scraping
{
    public class Scraper
    {
        private const string TOKEN_URL =
            "https://motorregister.skat.dk/dmr-front/dmr.portal?_nfpb=true&_nfpb=true&_pageLabel=vis_koeretoej_side&_nfls=false";

        private const string DATA_URL =
            "https://motorregister.skat.dk/dmr-front/dmr.portal?_nfpb=true&_windowLabel=kerne_vis_koeretoej&kerne_vis_koeretoej_actionOverride=%2Fdk%2Fskat%2Fdmr%2Ffront%2Fportlets%2Fkoeretoej%2Fnested%2FfremsoegKoeretoej%2Fsearch&_pageLabel=vis_koeretoej_side";

        private const string HIDDEN_TOKEN_NAME = "dmrFormToken";
        private const string SEARCH_FORM_NAME = "kerne_vis_koeretoej{actionForm.soegeord}";
        private readonly Parser _parser = new Parser();
        private string _token = string.Empty;

        private readonly WebClient _webClient = new WebClient();

        private void Authenticate(string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                _token = token;
                return;
            }

            _token = _parser.GetAuthenticationToken(Encoding.UTF8.GetString(_webClient.DownloadData(TOKEN_URL)));
            if (string.IsNullOrEmpty(_token))
                throw new Exception("Form token not found error");
        }

        private string GetVehicleHtml(string licencePlate)
        {
            Authenticate();

            var payload = new NameValueCollection
            {
                {HIDDEN_TOKEN_NAME, _token},
                {"kerne_vis_koeretoejwlw-radio_button_group_key:{actionForm.soegekriterie}", "REGISTRERINGSNUMMER"},
                {SEARCH_FORM_NAME, licencePlate},
                {"kerne_vis_koeretoejactionOverride:search", "Søg"}
            };
            return Encoding.UTF8.GetString(_webClient.UploadValues(DATA_URL, "POST", payload));
        }

        private async Task<string> GetSubPageHtml(string url = "")
        {
            var stream = _webClient.OpenRead(new Uri(
                @"https://motorregister.skat.dk/dmr-front/dmr.portal?_nfpb=true&_windowLabel=kerne_vis_koeretoej&kerne_vis_koeretoej_actionOverride=%2Fdk%2Fskat%2Fdmr%2Ffront%2Fportlets%2Fkoeretoej%2Fnested%2FvisKoeretoej%2FselectTab&kerne_vis_koeretoejdmr_tabset_tab=1&_pageLabel=vis_koeretoej_side"));
            return await new StreamReader(stream).ReadToEndAsync();
        }

        public async Task<EntityContainer> LookupVehicle(string licencePlate)
        {
            var entities = _parser.ParseHtmlDocToVehicle(GetVehicleHtml(licencePlate));
            entities.AddRange(_parser.ParseHtmlDocToVehicle(await GetSubPageHtml()));
            
            return new EntityContainer()
            {
                LiscensePlate = licencePlate,
                Entities = entities,
                Age = DateTime.Now
            };
        }
    }
}