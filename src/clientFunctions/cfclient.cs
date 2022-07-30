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
using dBridges.dispatchers;
using dBridges.exceptions;
using dBridges.Utils;
using RSG;
using dBridges.Messages;
//using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using dBridges.responseHandler;
using System.Collections.Concurrent;

namespace dBridges.clientFunctions
{
    public class cfclient
    {
      
        private readonly Action_dispatcher dispatch;

        private readonly object dbcore;
        public bool enable;
        public Action<object> functions;
        private ConcurrentDictionary<string, string> c_sid_functionname;
        private delegate void iCallBack(object response, object rspend, object rsub);
        readonly SemaphoreSlim _cfsidLock;

        private Random generator;
        private List<string> functionNames = new List<string>()
                                            { "cf.callee.queue.exceeded", "cf.response.tracker" };


        public cfclient(object dBCoreObject)
        {
            this.dispatch = new Action_dispatcher();

            this.dbcore = dBCoreObject;
            this.enable = false;
            this.functions = null;
            this.c_sid_functionname = new ConcurrentDictionary<string, string>();

            this._cfsidLock = new SemaphoreSlim(1, 1);

            this.generator = new Random();
        }



        public bool verify_function()
        {
            bool mflag = false;
            if (this.enable)
            {
                if (this.functions == null) { throw new dBError("E009"); }
                if (!this.functions.GetType().Name.StartsWith("Action")) { throw new dBError("E010"); }
                mflag = true;
            } else {
                mflag = true;
            }

            return mflag;
        }

        public void regfn(string functionName, Delegate callback)
        {
            if (string.IsNullOrEmpty(functionName) || string.IsNullOrWhiteSpace(functionName)) { throw new dBError("E110"); }
            if (!callback.GetType().Name.StartsWith("Action")) { throw new dBError("E111"); }
            if (this.functionNames.Contains(functionName)) { throw new dBError("E110"); }

            this.dispatch.bind(functionName, callback);
        }


        public void unregfn(string functionName, Delegate callback = null)
        {
            if (this.functionNames.Contains(functionName)) return;
            this.dispatch.unbind(functionName, callback);
        }


        public void bind(string eventName, Delegate callback)
        {
            if(string.IsNullOrEmpty(eventName) || string.IsNullOrWhiteSpace(eventName)){ throw new dBError("E066");}
            if (!callback.GetType().Name.StartsWith("Action")) { throw new dBError("E067"); }

            if (!this.functionNames.Contains(eventName)) { throw new dBError("E066"); }
            this.dispatch.bind(eventName, callback);
        }


        public void unbind(string eventName, Delegate callback=null)
        {
            if (!this.functionNames.Contains(eventName)) return;
            this.dispatch.unbind(eventName, callback);
        }


        public async Task handle_dispatcher(string functionName, string returnSubect, string sid, string payload)
        {
            
            CResponseHandler response = new CResponseHandler(functionName, returnSubect, sid, this.dbcore, "cf");
            //Console.WriteLine(payload);
            
            await this.dispatch.emit_cf(functionName, payload, response);

        }

        public async Task  handle_callResponse(string sid, string payload , bool isend, string rsub)
        {

            if (this.c_sid_functionname.ContainsKey(sid)){
                await this.dispatch.emit_clientfunction(sid, payload, isend, rsub);
            }
        }


        public async Task handle_tracker_dispatcher(string responseid, object errorcode)
        {
            await this.dispatch.emit_cf("cf.response.tracker", responseid, errorcode);
        }


        
        public async Task handle_exceed_dispatcher()
        {
            dBError err = new dBError("E070");
            err.updateCode("CALLEE_QUEUE_EXCEEDED");
            await this.dispatch.emit_cf("cf.callee.queue.exceeded", err, null);
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







        public async Task<IPromise<object>> call(string sessionid, string functionName, string inparameter, UInt64 ttlms, Action<object> progress_callback)
        {
            var promise = new Promise<object>();
            string sid = "";

            bool sid_created = true;
            try
            {
                int loop_index = 0;
                int loop_counter = 3;
                bool mflag = false;
                await _cfsidLock.WaitAsync().ConfigureAwait(false);
                sid = util.GenerateUniqueId();
                do
                {
                    if (this.c_sid_functionname.ContainsKey(sid))
                    {
                        sid = this.GetUniqueSid(sid);
                        loop_index++;
                    }else{
                        try
                        {
                            this.c_sid_functionname.TryAdd(sid, functionName);
                            mflag = true;
                        }
                        catch( Exception){
                        }
                       
                    }
                } while ((loop_index < loop_counter) && (!mflag));

                if (!mflag) {
                    
                    sid =  this.generator.Next().ToString();
                    if (!this.c_sid_functionname.ContainsKey(sid))
                    {
                        try
                        {
                            this.c_sid_functionname.TryAdd(sid, functionName);
                        }catch (Exception){
                            sid_created = false;
                        }

                    }else{
                        sid_created = false;
                    }  
                }
            }
            finally
            {
                _cfsidLock.Release();   
            }

            if (!sid_created){
                promise.Reject(new dBError("E107"));
                return promise;
            }

            bool cstatus;
            cstatus = await util.updatedBNewtworkCF(this.dbcore, MessageType.CF_CALL, sessionid, functionName, null, sid, inparameter, false, false);
            if (!cstatus)
            {
               promise.Reject(new dBError("E068"));
            }

            System.Timers.Timer timer = new System.Timers.Timer(ttlms);
            timer.Elapsed +=  async (s, ev) =>
            {
                System.Timers.Timer st = s as System.Timers.Timer;
                st.Stop();
                this.dispatch.unbind(sid);
                string v_value = "";
                bool is_removed = false;
                try
                {
                    is_removed = this.c_sid_functionname.TryRemove(sid, out v_value);
                }catch(Exception){
                }

                dBError dberror = new dBError("E069");
                await util.updatedBNewtworkCF(this.dbcore, MessageType.CF_CALL_TIMEOUT, null, sid, null, null, null, false, false);
                promise.Reject(dberror);

            };

            timer.Enabled = true;
            timer.Start();


            void innercall(object response, object rspend, object rsub) {
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
                    this.dispatch.unbind(sid);
                    string v_value = "";
                    bool is_removed = false;
                    try
                    {
                        is_removed = this.c_sid_functionname.TryRemove(sid, out v_value);
                    }
                    catch (Exception){
                    }

                    
                    if (!string.IsNullOrEmpty(srsub))
                    {
                        if (srsub.ToLower() == "exp")
                        {
                            dynamic brs_object = JsonConvert.DeserializeObject<dynamic>(sresponse);
                           
                            String c = brs_object["c"];
                            String m = brs_object["m"];
                            dBError dberror;
                            dberror = new dBError("E071");
                            dberror.updateCode(c, m);
                            promise.Reject(dberror);

                        }else{
                            dBError dberror;
                            dberror = new dBError("E070");
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
            this.dispatch.bind(sid, callback);
            return promise;
        }


        public async void resetqueue()
        {
            bool m_status = await util.updatedBNewtworkCF(this.dbcore, MessageType.CF_CALLEE_QUEUE_EXCEEDED, null, null, null, null, null, false, false);
            if (!m_status) { throw new dBError("E068"); }
        }

    }
}
