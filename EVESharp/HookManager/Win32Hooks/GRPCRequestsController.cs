
using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using SharedComponents.EVE;
using SharedComponents.EveMarshal;
using SharedComponents.IPC;
using SharedComponents.Py;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;
using SharedComponents.SharedMemory;
using System.Net;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace HookManager.Win32Hooks
{
    public class GRPCRequestsController : IHook, IDisposable
    {
        #region Fields

        private readonly LocalHook _publishHook;

        private readonly Delegate _origPublishFunc;

        #endregion Fields

        #region Constructors

        public GRPCRequestsController(IntPtr funcAddr, PySharp pySharp)
        {
            Error = false;
            Name = nameof(GRPCRequestsController);
            try
            {

                _publishHook = LocalHook.Create(
                    funcAddr,
                    new Delegate(DetourGetResponses),
                    this);


                _origPublishFunc = Marshal.GetDelegateForFunctionPointer<Delegate>(funcAddr);
                _publishHook.ThreadACL.SetExclusiveACL(new Int32[] { });

                Error = false;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr Delegate(IntPtr self, IntPtr args, IntPtr x);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        private void Log(string s)
        {
            Debug.WriteLine(s);
            WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        public void Dispose()
        {
            _publishHook.Dispose();
        }

        private HashSet<string> _responseTypes = new HashSet<string>()
        {
            "type.evetech.net/eve_public.app.eveonline.career.goal.GetAllResponse",
            "type.evetech.net/eve_public.pirate.corruption.api.GetSystemInfoResponse",
            "type.evetech.net/eve_public.corporationgoal.api.GetAllResponse",
            "type.evetech.net/eve_public.goal.contribution.api.GetMyContributorSummariesResponse",
            "type.evetech.net/eve_public.pirate.suppression.api.GetStageThresholdsResponse",
            "type.evetech.net/eve_public.pirate.corruption.api.GetStageThresholdsResponse",
            "type.evetech.net/eve_public.pirate.suppression.api.GetSystemInfoResponse",
            "type.evetech.net/eve_public.character.skill.plan.GetActiveResponse",
            "type.evetech.net/eve_public.entitlement.character.GetAllResponse"
        };


        private IntPtr DetourGetResponses(IntPtr self, IntPtr args, IntPtr x)
        {
            //Log($"DetourGetResponses Hook proc!");
            var res = _origPublishFunc(self, args, x);
            try
            {
                using (var pySharp = new PySharp(false))
                {
                    var argList = new PyObject(pySharp, args, false);
                    var resPy = new PyObject(pySharp, res, false);

                    if (resPy.GetPyType() == PyType.ListType)
                    {
                        var resList = resPy.ToList();
                        if (resList.Count > 0)
                        {
                            int n = 0;
                            foreach (var item in resList)
                            {
                                //HookManager.Log.RemoteWriteLine($"{n}  -- " + item.GetItemAt(1).LogObject());
                                var response = item.GetItemAt(1);
                                var payload = response["payload"];
                                var type_url = payload["type_url"].ToUnicodeString();

                                if (_responseTypes.Contains(type_url))
                                {
                                    HookManager.Log.WriteLine($"GRPC Request-Type response received. Type [{type_url}]");
                                }
                                else
                                {
                                    Log($"Warn: GRPC Request-Type response unknown Type [{type_url}]");
                                }

                                //HookManager.Log.RemoteWriteLine($"type_url: {type_url}");
                                //HookManager.Log.RemoteWriteLine($"{n}  -- " + response.LogObject());
                                //HookManager.Log.RemoteWriteLine($"{n}  -- IsValid " + payload.IsValid + " Type " + payload.GetPyType());
                                n++;
                            }
                        }
                    }
                    return res;

                }
            }
            catch (Exception e)
            {
                Log($"Error: {nameof(GRPCRequestsController)} " + e);
            }
            return res;
        }

        #endregion Methods

    }
}
