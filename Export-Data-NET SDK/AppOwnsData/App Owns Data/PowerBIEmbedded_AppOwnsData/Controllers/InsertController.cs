using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BundleConfigEx.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PowerBIEmbedded_AppOwnsData.Controllers
{
    public class InsertController : Controller
    {
        // GET: Insert
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(SaveData objdata)
        {
            try
            {
                //Sqlconnection con = new sqlconnection(configurationmanager.connectionstrings[“myconnection”].tostring());
                //con.open();
                //sqlcommand cmd = new sqlcommand(“usp_insertdata”, con);
                //cmd.commandtype = commandtype.storedprocedure;
                //cmd.parameters.addwithvalue(“@address”, objdata.address);
                //cmd.parameters.addwithvalue(“@name”, objdata.name);
                //cmd.executenonquery();
                //con.close();
                string x = "Hi";
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }
    }
}
    
