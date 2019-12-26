using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using PowerBIEmbedded_AppOwnsData.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BundleConfigEx.Models;
using jQuery_AJAX_MVC.Models;
using Newtonsoft.Json;
using System.Data;
using System.Web;
using System.Data.SqlClient;
namespace PowerBIEmbedded_AppOwnsData.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string Username = ConfigurationManager.AppSettings["pbiUsername"];
        private static readonly string Password = ConfigurationManager.AppSettings["pbiPassword"];
        private static readonly string AuthorityUrl = ConfigurationManager.AppSettings["authorityUrl"];
        private static readonly string ResourceUrl = ConfigurationManager.AppSettings["resourceUrl"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["clientId"];
        private static readonly string ApiUrl = ConfigurationManager.AppSettings["apiUrl"];
        private static readonly string GroupId = ConfigurationManager.AppSettings["groupId"];
        private static readonly string ReportId = ConfigurationManager.AppSettings["reportId"];

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult AjaxMethod(string name)
        {
            var ids = name.Split('\n');

          
            DataTable dt = new DataTable();
            dt.Columns.Add("Product");
            dt.Columns.Add("Profit");
            for (int i=0; i<ids.Length;i++)
            {
                if (ids[i] != string.Empty && i !=0)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = ids[i].Split(',')[0];
                    dr[1] = ids[i].Split(',')[1];
                    dt.Rows.Add(dr);
                }
            }
            //dt.Rows.Add(ids);
            //  var x = dt;
            string dbConnString = @"data source=xxN\xx;Database=xxxxxxx;Uid=xxxxx;Pwd=xxxxxx;";
           // string consString =  ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(dbConnString))
            {
                using (SqlCommand cmd = new SqlCommand("spInsertProducts"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@ProductType", dt);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            PersonModel person = new PersonModel
            {
                Name = "Completed Successfully",
                DateTime = DateTime.Now.ToString()
            };
            return Json(person);
        }
        

        public void ExportData(string s)
        {
            Console.WriteLine("CSCSCSCS: " + s);

        }

        public String ExportD(String s)
        {
            Console.WriteLine("CSCSCSCS: " + s);
            return s;
        }


        public async Task<ActionResult> EmbedReport(string username, string roles)
        {
            var result = new EmbedConfig();
            try
            {
                result = new EmbedConfig { Username = username, Roles = roles };
                var error = GetWebConfigErrors();
                if (error != null)
                {
                    result.ErrorMessage = error;
                    return View(result);
                }

                // Create a user password cradentials.
                var credential = new UserPasswordCredential(Username, Password);

                // Authenticate using created credentials
                var authenticationContext = new AuthenticationContext(AuthorityUrl);
                var authenticationResult = await authenticationContext.AcquireTokenAsync(ResourceUrl, ClientId, credential);

                if (authenticationResult == null)
                {
                    result.ErrorMessage = "Authentication Failed.";
                    return View(result);
                }

                var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

                // Create a Power BI Client object. It will be used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(ApiUrl), tokenCredentials))
                {
                    // Get a list of reports.
                    var reports = await client.Reports.GetReportsInGroupAsync(GroupId);

                    Report report;
                    if (string.IsNullOrEmpty(ReportId))
                    {
                        // Get the first report in the group.
                        report = reports.Value.FirstOrDefault();
                    }
                    else
                    {
                        report = reports.Value.FirstOrDefault(r => r.Id == ReportId);
                    }

                    if (report == null)
                    {
                        result.ErrorMessage = "Group has no reports.";
                        return View(result);
                    }

                    var datasets = await client.Datasets.GetDatasetByIdInGroupAsync(GroupId, report.DatasetId);
                    result.IsEffectiveIdentityRequired = datasets.IsEffectiveIdentityRequired;
                    result.IsEffectiveIdentityRolesRequired = datasets.IsEffectiveIdentityRolesRequired;
                    GenerateTokenRequest generateTokenRequestParameters;
                    // This is how you create embed token with effective identities
                    if (!string.IsNullOrEmpty(username))
                    {
                        var rls = new EffectiveIdentity(username, new List<string> { report.DatasetId });
                        if (!string.IsNullOrWhiteSpace(roles))
                        {
                            var rolesList = new List<string>();
                            rolesList.AddRange(roles.Split(','));
                            rls.Roles = rolesList;
                        }
                        // Generate Embed Token with effective identities.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view", identities: new List<EffectiveIdentity> { rls });
                    }
                    else
                    {
                        // Generate Embed Token for reports without effective identities.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                    }

                    var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(GroupId, report.Id, generateTokenRequestParameters);

                    if (tokenResponse == null)
                    {
                        result.ErrorMessage = "Failed to generate embed token.";
                        return View(result);
                    }

                    // Generate Embed Configuration.
                    result.EmbedToken = tokenResponse;
                    result.EmbedUrl = report.EmbedUrl;
                    result.Id = report.Id;

                    return View(result);
                }
            }
            catch (HttpOperationException exc)
            {
                result.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}", exc.Response.StatusCode, (int)exc.Response.StatusCode, exc.Response.Content, exc.Response.Headers["RequestId"].FirstOrDefault());
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.ToString();
            }

            return View(result);
        }

        public async Task<ActionResult> EmbedDashboard()
        {
            var error = GetWebConfigErrors();
            if (error != null)
            {
                return View(new EmbedConfig()
                {
                    ErrorMessage = error
                });
            }

            // Create a user password cradentials.
            var credential = new UserPasswordCredential(Username, Password);

            // Authenticate using created credentials
            var authenticationContext = new AuthenticationContext(AuthorityUrl);
            var authenticationResult = await authenticationContext.AcquireTokenAsync(ResourceUrl, ClientId, credential);

            if (authenticationResult == null)
            {
                return View(new EmbedConfig()
                {
                    ErrorMessage = "Authentication Failed."
                });
            }

            var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

            // Create a Power BI Client object. It will be used to call Power BI APIs.
            using (var client = new PowerBIClient(new Uri(ApiUrl), tokenCredentials))
            {
                // Get a list of dashboards.
                var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(GroupId);

                // Get the first report in the group.
                var dashboard = dashboards.Value.FirstOrDefault();

                if (dashboard == null)
                {
                    return View(new EmbedConfig()
                    {
                        ErrorMessage = "Group has no dashboards."
                    });
                }

                // Generate Embed Token.
                var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                var tokenResponse = await client.Dashboards.GenerateTokenInGroupAsync(GroupId, dashboard.Id, generateTokenRequestParameters);

                if (tokenResponse == null)
                {
                    return View(new EmbedConfig()
                    {
                        ErrorMessage = "Failed to generate embed token."
                    });
                }

                // Generate Embed Configuration.
                var embedConfig = new EmbedConfig()
                {
                    EmbedToken = tokenResponse,
                    EmbedUrl = dashboard.EmbedUrl,
                    Id = dashboard.Id
                };

                return View(embedConfig);
            }
        }

        public async Task<ActionResult> EmbedTile()
        {
            var error = GetWebConfigErrors();
            if (error != null)
            {
                return View(new TileEmbedConfig()
                {
                    ErrorMessage = error
                });
            }

            // Create a user password cradentials.
            var credential = new UserPasswordCredential(Username, Password);

            // Authenticate using created credentials
            var authenticationContext = new AuthenticationContext(AuthorityUrl);
            var authenticationResult = await authenticationContext.AcquireTokenAsync(ResourceUrl, ClientId, credential);

            if (authenticationResult == null)
            {
                return View(new TileEmbedConfig()
                {
                    ErrorMessage = "Authentication Failed."
                });
            }

            var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");

            // Create a Power BI Client object. It will be used to call Power BI APIs.
            using (var client = new PowerBIClient(new Uri(ApiUrl), tokenCredentials))
            {
                // Get a list of dashboards.
                var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(GroupId);

                // Get the first report in the group.
                var dashboard = dashboards.Value.FirstOrDefault();

                if (dashboard == null)
                {
                    return View(new TileEmbedConfig()
                    {
                        ErrorMessage = "Group has no dashboards."
                    });
                }

                var tiles = await client.Dashboards.GetTilesInGroupAsync(GroupId, dashboard.Id);

                // Get the first tile in the group.
                var tile = tiles.Value.FirstOrDefault();

                // Generate Embed Token for a tile.
                var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                var tokenResponse = await client.Tiles.GenerateTokenInGroupAsync(GroupId, dashboard.Id, tile.Id, generateTokenRequestParameters);

                if (tokenResponse == null)
                {
                    return View(new TileEmbedConfig()
                    {
                        ErrorMessage = "Failed to generate embed token."
                    });
                }

                // Generate Embed Configuration.
                var embedConfig = new TileEmbedConfig()
                {
                    EmbedToken = tokenResponse,
                    EmbedUrl = tile.EmbedUrl,
                    Id = tile.Id,
                    dashboardId = dashboard.Id
                };

                return View(embedConfig);
            }
        }

        /// <summary>
        /// Check if web.config embed parameters have valid values.
        /// </summary>
        /// <returns>Null if web.config parameters are valid, otherwise returns specific error string.</returns>
        private string GetWebConfigErrors()
        {
            // Client Id must have a value.
            if (string.IsNullOrEmpty(ClientId))
            {
                return "ClientId is empty. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }

            // Client Id must be a Guid object.
            Guid result;
            if (!Guid.TryParse(ClientId, out result))
            {
                return "ClientId must be a Guid object. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }

            // Group Id must have a value.
            if (string.IsNullOrEmpty(GroupId))
            {
                return "GroupId is empty. Please select a group you own and fill its Id in web.config";
            }

            // Group Id must be a Guid object.
            if (!Guid.TryParse(GroupId, out result))
            {
                return "GroupId must be a Guid object. Please select a group you own and fill its Id in web.config";
            }

            // Username must have a value.
            if (string.IsNullOrEmpty(Username))
            {
                return "Username is empty. Please fill Power BI username in web.config";
            }

            // Password must have a value.
            if (string.IsNullOrEmpty(Password))
            {
                return "Password is empty. Please fill password of Power BI username in web.config";
            }

            return null;
        }
    }
}
