using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;

using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;
using System.Diagnostics;
using ESRI.ArcGIS.GISClient;


//TODO: sign the project (project properties > signing tab > sign the assembly)
//      this is strongly suggested if the dll will be registered using regasm.exe <your>.dll /codebase


namespace SampleSOE
{
    [ComVisible(true)]
    [Guid("56b589de-50e3-4892-a31a-0d37c9274434")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",//use "MapServer" if SOE extends a Map service and "ImageServer" if it extends an Image service.
        AllCapabilities = "",
        DefaultCapabilities = "",
        Description = "",
        DisplayName = "SampleSOE",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class SampleSOE : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        private string soe_name;

        private IPropertySet configProps;
        private IServerObjectHelper serverObjectHelper;
        private ServerLogger logger;
        private IRESTRequestHandler reqHandler;
        private IMapServerDataAccess pObjMap;
        private IMapServer3 pMapServ;

        public SampleSOE()
        {
            soe_name = this.GetType().Name;
            logger = new ServerLogger();
            reqHandler = new SoeRestImpl(soe_name, CreateRestSchema()) as IRESTRequestHandler;
        }

        #region IServerObjectExtension Members

        public void Init(IServerObjectHelper pSOH)
        {
            Debugger.Launch();
            serverObjectHelper = pSOH;
        }

        public void Shutdown()
        {
        }

        #endregion

        #region IObjectConstruct Members

        public void Construct(IPropertySet props)
        {
            configProps = props;
        }

        #endregion

        #region IRESTRequestHandler Members

        public string GetSchema()
        {
            return reqHandler.GetSchema();
        }

        public byte[] HandleRESTRequest(string Capabilities, string resourceName, string operationName, string operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            return reqHandler.HandleRESTRequest(Capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties, out responseProperties);
        }

        #endregion

        private RestResource CreateRestSchema()
        {
            RestResource rootRes = new RestResource(soe_name, false, RootResHandler);

            RestOperation sampleOper = new RestOperation("sampleOperation",
                                                      new string[] { "parm1", "parm2" },
                                                      new string[] { "json" },
                                                      SampleOperHandler);

            rootRes.operations.Add(sampleOper);

            return rootRes;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
            result.AddString("hello", "world");

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] SampleOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            string parm1Value;
            bool found = operationInput.TryGetString("parm1", out parm1Value);
            if (!found || string.IsNullOrEmpty(parm1Value))
                throw new ArgumentNullException("parm1");

            string parm2Value;
            found = operationInput.TryGetString("parm2", out parm2Value);
            if (!found || string.IsNullOrEmpty(parm2Value))
                throw new ArgumentNullException("parm2");


            string outputSTR = null;

            IPropertySet propertySet = new PropertySet();
            propertySet.SetProperty("URL", "http://localhost:6080/arcgis/admin");
            propertySet.SetProperty("USER", "yourServerAdminUsername") ; 
            propertySet.SetProperty("PASSWORD", "yourServerAdminPassword");
            propertySet.SetProperty("ConnectionMode", esriAGSConnectionMode.esriAGSConnectionModeAdmin);
            propertySet.SetProperty("ServerType", esriAGSServerType.esriAGSServerTypeDiscovery);
            IAGSServerConnectionFactory agsServerConnectionFactory = new AGSServerConnectionFactory();
            IAGSServerConnectionAdmin agsServerConnection = agsServerConnectionFactory.Open(propertySet, 0) as IAGSServerConnectionAdmin;
            IServerObjectAdmin servAdmin = agsServerConnection.ServerObjectAdmin;
            IEnumServerDirectory pEnumServDir = servAdmin.GetServerDirectories();
            pEnumServDir.Reset();
            IServerDirectory2 pServDir = pEnumServDir.Next() as IServerDirectory2;
            while (pServDir != null)
            {
                string cleaningMd = pServDir.CleaningMode.ToString(); //cleanup mode
                string descrip = pServDir.Description.ToString(); //Description
                int fileAge = pServDir.MaxFileAge; //Max File Age
                string path = pServDir.Path; //physical path
                string url = pServDir.URL; //virtual path
                string type = pServDir.Type.ToString();
                pServDir = pEnumServDir.Next() as IServerDirectory2;
                outputSTR = outputSTR + "cleanup mode: " + cleaningMd + "\n" + "Description: " + descrip + "\n" + "Max File Age: " + fileAge + "\n" + "physical path: " + path + "\n" + "virtual path: " + url + "\n" + "type: " + type.ToString() + "\r\n";
            }
            JsonObject result = new JsonObject();
            result.AddString("parm1", parm1Value);
            result.AddString("parm2", outputSTR);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }


    }
}
