using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DanishRegisterOfMotorVehicles.Api.Scraper
{
    public class Scraper
    {
        const string TOKEN_URL = "https://motorregister.skat.dk/dmr-front/dmr.portal?_nfpb=true&_nfpb=true&_pageLabel=vis_koeretoej_side&_nfls=false";
        const string DATA_URL = "https://motorregister.skat.dk/dmr-front/dmr.portal?_nfpb=true&_windowLabel=kerne_vis_koeretoej&kerne_vis_koeretoej_actionOverride=%2Fdk%2Fskat%2Fdmr%2Ffront%2Fportlets%2Fkoeretoej%2Fnested%2FfremsoegKoeretoej%2Fsearch&_pageLabel=vis_koeretoej_side";
        const string HIDDEN_TOKEN_NAME = "dmrFormToken";
        const string SEARCH_FORM_NAME = "kerne_vis_koeretoej{actionForm.soegeord}";

        private  WebClient _webClient = new WebClient();
        private  Parser _parser = new Parser();
        private  string _token = string.Empty;

        public  string Token { get { return _token; } }
        public  bool IsAuthenticated { get { return !string.IsNullOrEmpty(_token); } }

        private  void Authenticate(string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                _token = token;
                return;
            }
            string result = string.Empty;
            try
            {
                byte[] b = _webClient.DownloadData(TOKEN_URL);
                _parser.LoadHtml(Encoding.UTF8.GetString(b));
                _token = _parser.GetAuthenticationToken();
                if (string.IsNullOrEmpty(_token))
                    throw new Exception("Form token not found error");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetVehicleHtml(string licencePlate)
        {
            try
            {
                Authenticate();

                NameValueCollection payload = new NameValueCollection() 
                { 
                     { HIDDEN_TOKEN_NAME, _token },
                     {"kerne_vis_koeretoejwlw-radio_button_group_key:{actionForm.soegekriterie}", "REGISTRERINGSNUMMER" },
                     { SEARCH_FORM_NAME, licencePlate },
                     { "kerne_vis_koeretoejactionOverride:search", "Søg" },
                };
                return Encoding.UTF8.GetString(_webClient.UploadValues(DATA_URL, "POST", payload));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<string> GetSubPageHtml(string url)
        {
            try
            {
                Stream stream = _webClient.OpenRead(new Uri(@"https://motorregister.skat.dk/dmr-front/dmr.portal?_nfpb=true&_windowLabel=kerne_vis_koeretoej&kerne_vis_koeretoej_actionOverride=%2Fdk%2Fskat%2Fdmr%2Ffront%2Fportlets%2Fkoeretoej%2Fnested%2FvisKoeretoej%2FselectTab&kerne_vis_koeretoejdmr_tabset_tab=1&_pageLabel=vis_koeretoej_side"));
                return await new StreamReader(stream).ReadToEndAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public  Request LookupVehicle(string licencePlate)
        {
            _parser.LoadHtml(GetVehicleHtml(licencePlate));
            
            var result = _parser.GetVehicle(); 
            var message = "OK";
            var success = true;
            if(result == null)
            {
                message = "Ingen køretøjer fundet";
                success = false;
            }
            return new Request()
            {
                Token = _token,
                Success = success,
                Message = message,
                Result = result
            };
        }

    }

}
