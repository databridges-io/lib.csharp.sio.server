/*
	DataBridges C# server Library
	https://www.databridges.io/



	Copyright 2022 Optomate Technologies Private Limited.

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

	    http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSG;
using dBridges.dispatchers;
using dBridges.exceptions;
using dBridges.Messages;
using dBridges.Utils;
using dBridges.responseHandler;
//using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Threading;
using System.Timers;

using System.Collections.Concurrent;

namespace dBridges.remoteprocedure
{
    public class CrpCaller
    {
       

        private readonly Action_dispatcher dispatch;
        private readonly object dbcore;
        private readonly object rpccore;

        private Dictionary<string, string> sid_functionname;
        private ConcurrentDictionary<string, string> c_sid_functionname;
        private string serverName;
        private bool _isOnline;
        private string callerTYPE;
        private delegate void iCallBack(object response, object rspend, object rsub);
        readonly SemaphoreSlim _rpcsidLock;

        private Random generator;

        public CrpCaller(string serverName, object dBCoreObject, object rpccoreobject, string callertype = "rpc")
        {
         
            this.dispatch = new Action_dispatcher();

            this.dbcore = dBCoreObject;
            this.rpccore = rpccoreobject;
            this.sid_functionname = new Dictionary<string, string>();
            this.c_sid_functionname = new ConcurrentDictionary<string, string>();
            this.serverName = serverName;
            this._isOnline = false;
            this.callerTYPE = callertype;
            this._rpcsidLock = new SemaphoreSlim(1, 1);
            this.generator = new Random();
        }


        public string getServerName() {
            return this.serverName;
        }

        public bool isOnline() {
            return this._isOnline;
        }

        public void set_isOnline(bool value) {
            this._isOnline = value;
        }


        public void bind(string eventName, Delegate callback)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrWhiteSpace(eventName))
            {
                throw new dBError("E076");
            }

            if (callback == null)
            {
                throw new dBError("E077");
            }

            this.dispatch.bind(eventName, callback);
        }


        public void unbind(string eventName, Delegate callback=null)
        {
            this.dispatch.unbind(eventName, callback);
        }


        public void unbind()
        {
            this.dispatch.unbind();
        }


        public async Task handle_dispatcher(string functionName, string returnSubect, string sid, string payload)
        {

            CResponseHandler response = new CResponseHandler(functionName, returnSubect, sid, this.dbcore, "rpc");
            await this .dispatch.emit_clientfunction(functionName, payload, response);

        }

        public async Task  handle_callResponse(string sid, string payload, bool isend, string rsub)
        {
           
            if (this.c_sid_functionname.ContainsKey(sid))
            {
               await this.dispatch.emit_clientfunction(sid, payload, isend, rsub);
            }
            else
            {
                Console.WriteLine("late response: " + sid + "::" + payload);

            }
        }


        public async Task handle_tracker_dispatcher(string responseid, object errorcode)
        {
            await this.dispatch.emit_cf("rpc.response.tracker", responseid, errorcode);
        }



        public async Task handle_exceed_dispatcher()
        {
            dBError err = new dBError("E054");
            err.updateCode("CALLEE_QUEUE_EXCEEDED");
            await this.dispatch.emit_cf("rpc.callee.queue.exceeded", err, null);
        }


        private string GetUniqueSid(string sid)
        {
            String nsid = this.generator.Next().ToString();

            if (this.c_sid_functionname.ContainsKey(nsid))
            {
                nsid = this.generator.Next().ToString();
            }


            return nsid;
        }




        public async Task<IPromise<object>> call(string functionName, string inparameter, UInt64 ttlms, Action<object> progress_callback)
        {
            var promise = new Promise<object>();

            string sid = "";

            bool sid_created = true;
            try
            {
                int loop_index = 0;
                int loop_counter = 3;
                bool mflag = false;

                
                await _rpcsidLock.WaitAsync().ConfigureAwait(false);
                sid = util.GenerateUniqueId();
                
                do
                {
                
                    if (this.c_sid_functionname.ContainsKey(sid))
                    {
                        sid = this.GetUniqueSid(sid);
                        loop_index++;
                    }
                    else
                    {
                        try
                        {
                
                            this.c_sid_functionname.TryAdd(sid, functionName);
                            mflag = true;
                        }
                        catch (Exception exp)
                        {
                            Console.WriteLine("exp message : " + exp.Message + " sid :" + sid);
                        }

                    }
                } while ((loop_index < loop_counter) && (!mflag));

                if (!mflag)
                {

                    sid = this.generator.Next().ToString();
                    if (!this.c_sid_functionname.ContainsKey(sid))
                    {
                        try
                        {
                            this.c_sid_functionname.TryAdd(sid, functionName);
                        }
                        catch (Exception exp)
                        {
                            Console.WriteLine("exp message : " + exp.Message + " sid :" + sid);
                            sid_created = false;
                        }

                    }
                    else
                    {
                        sid_created = false;
                    }

                }
            }
            finally
            {
                _rpcsidLock.Release();
            }

            if (!sid_created)
            {
                if (this.callerTYPE.ToLower() == "rpc")
                {
                    promise.Reject(new dBError("E108"));
                }
                else
                {
                    promise.Reject(new dBError("E109"));
                }

                return promise;
            }


            (this.rpccore as CRpc).store_object(sid, this);
            bool cstatus;

            if (this.callerTYPE.ToLower() == "rpc")
            {
               cstatus = await util.updatedBNewtworkCF(this.dbcore, MessageType.CALL_RPC_FUNCTION, this.serverName, functionName, null, sid, inparameter, false, false);
            }
            else
            {
               cstatus =  await util.updatedBNewtworkCF(this.dbcore, MessageType.CALL_CHANNEL_RPC_FUNCTION, this.serverName, functionName, null, sid, inparameter, false, false);
            }
            if (!cstatus) {
                if (this.callerTYPE.ToLower() == "rpc")
                    promise.Reject(new dBError("E079"));
                else
                    promise.Reject(new dBError("E033"));
            }

            System.Timers.Timer timer = new System.Timers.Timer(ttlms);
            timer.Elapsed += async (s, ev) =>
            {
                System.Timers.Timer st = s as System.Timers.Timer;
                st.Stop();
                this.unbind(sid);
                (this.rpccore as CRpc).delete_object(sid);
           
                string v_value = "";
                bool is_removed = false;
                try
                {
                    is_removed = this.c_sid_functionname.TryRemove(sid, out v_value);
                }
                catch (Exception exp)
                {
                    Console.WriteLine("in timer exception :" + exp.Message + " sid : " + sid);
                }

                dBError dberror;
                if (this.callerTYPE.ToLower() == "rpc")
                    dberror = new dBError("E080");
                else
                    dberror = new dBError("E042");

                await util.updatedBNewtworkCF(this.dbcore, MessageType.RPC_CALL_TIMEOUT, null, sid, null, null, null, false, false);
                promise.Reject(dberror);

            };

            timer.Enabled = true;
            timer.Start();


           
            void innercall(object response, object rspend, object rsub){
                bool isend = (bool)rspend;
                string sresponse = (string)response;
                string srsub = (string)rsub;

                if (!isend)
                {
                    Task.Factory.StartNew(() => progress_callback(response));
                    return;
                }
                else
                {
                    timer.Enabled = false;
                    timer.Stop();
                    timer.Dispose();
                    this.unbind(sid);
                    string v_value = "";
                    bool is_removed = false;
                    try
                    {
                        is_removed = this.c_sid_functionname.TryRemove(sid, out v_value);
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine("in callback exception :" + exp.Message + " sid : " + sid);
                    }

                    (this.rpccore as CRpc).delete_object(sid);

                    if (!string.IsNullOrEmpty(srsub))
                    {
                        if (srsub.ToLower() == "exp")
                        {
                            //JavaScriptSerializer js = new JavaScriptSerializer();

                            dynamic brs_object = JsonConvert.DeserializeObject<dynamic>(sresponse);
                            dBError dberror;
                            if (this.callerTYPE.ToLower() == "rpc")
                                dberror = new dBError("E055");
                            else
                                dberror = new dBError("E041");

                            String c = brs_object["c"];
                            String m = brs_object["m"];

                            dberror.updateCode(c, m);
                            promise.Reject(dberror);

                        }
                        else
                        {
                            dBError dberror;
                            if (this.callerTYPE.ToLower() == "rpc")
                                dberror = new dBError("E054");
                            else
                                dberror = new dBError("E040");

                            dberror.updateCode(srsub.ToUpper());
                            promise.Reject(dberror);
                        }


                    }
                    else
                    {
                        promise.Resolve(response);
                    }
                }

            }

            Action<object, object, object> callback = innercall;
            this.bind(sid, callback);

            return promise;
        }



        public async Task emit_channel(string eventName, string EventInfo, string channelName, object metadata)
        {
           await this.dispatch.emit(eventName, EventInfo, channelName, metadata);
        }

        public async Task emit_channel(string eventName, string EventInfo, object metadata)
        {
            await this .dispatch.emit_channel(eventName, EventInfo, metadata);
        }

        public async Task emit_channel(string eventName, object EventInfo, object metadata)
        {
            await this.dispatch.emit_channel(eventName, EventInfo, metadata);
        }

        public async Task emit(string eventName, object EventInfo, string Name = "", object metadata = null)
        {
            await this .dispatch.emit(eventName, EventInfo, Name, metadata);
        }

    }
}
