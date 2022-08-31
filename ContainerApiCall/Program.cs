using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ContainerApiCall
{
    internal class Program
    {
        static string _baseUrl = ""; //<your EMS base url here>
        static string _authKey = ""; //<the auth key provided by Chemical Safety>
        static string _token = "";

        static  void Main(string[] args)
        {
            int choice = -1;
            Console.WriteLine();
            Console.WriteLine(@"
  _                             __       _          
 /  |_   _  ._ _  o  _  _. |   (_   _. _|_ _ _|_    
 \_ | | (/_ | | | | (_ (_| |   __) (_|  | (/_ |_ \/ 
                                                 /  

");
            while (choice != 0)
            {
                Console.WriteLine();
                Console.WriteLine("Inventory API call example");
                Console.WriteLine();
                Console.WriteLine("1. GetSessionId");
                Console.WriteLine("2. AddToSession");
                Console.WriteLine("3. ProcessSession");
                Console.WriteLine("4. SessionResults");
                Console.WriteLine("5. GetData");
                Console.WriteLine("6. GetTempToken");
                Console.WriteLine("0. Exit");
                string userInput = Console.ReadLine();
                if (int.TryParse(userInput, out choice))
                {
                    switch (choice)
                    {
                        case 1:
                            GetSessionId().GetAwaiter().GetResult();
                            break;
                        case 2:
                            AddContainerToSession().GetAwaiter().GetResult();
                            break;
                        case 3:
                            ProcessSession().GetAwaiter().GetResult();
                            break;
                        case 4:
                            SessionResults().GetAwaiter().GetResult();
                            break;
                        case 5:
                            GetLookupData().GetAwaiter().GetResult();
                            break;
                        case 6:
                            GetTempToken().GetAwaiter().GetResult();
                            break;
                    }
                }
            }
        }

        
        static async Task GetTempToken()
        {
            var con = new Connect(_authKey, _baseUrl);
            _token = await con.GetToken();
            Console.WriteLine("Token received: " + _token);
        }

        private static void Con_MessageLogged(object sender, string message)
        {
            Console.WriteLine(message);
        }

        static async Task GetSessionId()
        {
            var con = new Connect(_authKey, _baseUrl,_token);
            con.MessageLogged += Con_MessageLogged;
            var d = new JavaScriptSerializer();
            var result = await con.GetData(Connect.CallMethods.GetSessionId, "");
            Console.WriteLine(result);
        }

        static async Task GetLookupData()
        {
            Console.WriteLine("Data to fetch: ( purchaseitem | lotnumber | facility | cas | vendor | bol | chemicalname | location | building | barcode | barcodeloc | employee | units | class | mspart )");
            string type = Console.ReadLine();
            var con = new Connect(_authKey, _baseUrl, _token);
            con.MessageLogged += Con_MessageLogged;
            var json = new PopupData
            {
                PopupType = type,
                Param1 = "",
                Param2 = "1000"
            };
            var d = new JavaScriptSerializer();
            var result = await con.GetData(Connect.CallMethods.GetData, d.Serialize( json));
            Console.WriteLine(result);
        }


        static async Task AddContainerToSession()
        {
            Console.WriteLine("Session Id");
            string sessionId = Console.ReadLine();
            var con = new Connect(_authKey, _baseUrl, _token);
            con.MessageLogged += Con_MessageLogged;

            var json = new ContainerUpload
            {
                Containers = new List<Container>() { new Container() { Barcode = "BCODE-1000",QuantityOnHand=10f,Unit="Pounds",Common="Acetone",Manufact="Sigma",ScanFlag=2} },
                SessionId=sessionId,
                MobileIdentifier="TEST CALLER"

            };
            var d = new JavaScriptSerializer();
            var result = await con.GetData(Connect.CallMethods.AddToSession, d.Serialize(json));
            Console.WriteLine(result);
        }

        static async Task ProcessSession()
        {
            Console.WriteLine("Session Id");
            string sessionId = Console.ReadLine();
            var con = new Connect(_authKey, _baseUrl, _token);
            con.MessageLogged += Con_MessageLogged;

            var json = new ProcessData
            {
                CustomProcessMode="default",
                Value= sessionId
               
            };
            var d = new JavaScriptSerializer();
            var result = await con.GetData(Connect.CallMethods.ProcessSession, d.Serialize(json));
            Console.WriteLine(result);
        }

        static async Task SessionResults()
        {
            Console.WriteLine("Session Id");
            string sessionId = Console.ReadLine();
            var con = new Connect(_authKey, _baseUrl, _token);
            con.MessageLogged += Con_MessageLogged;

            var json = new SessionResultsData
            {
                Value = sessionId

            };
            var d = new JavaScriptSerializer();
            var result = await con.GetData(Connect.CallMethods.SessionResults, d.Serialize(json));
            Console.WriteLine(result);
        }

        [Serializable]
        class SessionResultsData
        {
            public string Value { get; set; }
        }

        [Serializable]
        class ProcessData
        {
            public string CustomProcessMode { get; set; }
            public string Value { get; set; }
        }

        [Serializable]
        class PopupData
        {
            public string PopupType { get; set; }
            public string Param1 { get; set; }
            public string Param2 { get; set; }
        }

        [Serializable]
        class Container
        {
            public string Barcode { get; set; }
            public int ScanFlag { get; set; }
            public float QuantityOnHand { get; set; }
            public string PhysState { get; set; }
            public string Unit { get; set; }
            public string Common { get; set; }
            public string Manufact { get; set; }
            public string RecordId { get; set; }
            public string NewBarcode { get; set; }
            public string Number1 { get; set; }
            public string Cas { get; set; }
            public bool Surplus { get; set; }
            public string PartNumber { get; set; }
            public string OpenDate { get; set; }
            public string LotNumber { get; set; }
            public string PurchaseItemNumber { get; set; }
            public string Classify { get; set; }
            public int ItemBol { get; set; }
        }

        [Serializable]
        class ContainerUpload
        {
            public List<Container> Containers { get; set; }
            public string SessionId { get; set; }
            public string MobileIdentifier { get; set; }
        } 

    }
}
