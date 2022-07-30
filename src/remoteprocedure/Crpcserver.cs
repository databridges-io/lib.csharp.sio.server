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
using dBridges.dispatchers;
using dBridges.exceptions;
using dBridges.Messages;
using dBridges.Utils;
using dBridges.responseHandler;

namespace dBridges.remoteprocedure
{
    

   public class Crpcserver
    {
       
        private Action_dispatcher dispatch;
        private object dbcore;
        private bool _isOnline;


        public Action<object> functions;
        public string sid;
        private string serverName;

       private List<string> functionNames = new List<string>()
                                            {"rpc.callee.queue.exceeded", "rpc.response.tracker", "dbridges:rpc.server.registration.success",
                                              "dbridges:rpc.server.registration.fail", "dbridges:rpc.server.online", "dbridges:rpc.server.offline",
                                               "dbridges:rpc.server.unregistration.success", "dbridges:rpc.server.unregistration.fail"};


        public Crpcserver(string servername, string sid, object dBCoreObject)
        {
           
            this.dispatch = new Action_dispatcher();

            this.dbcore = dBCoreObject;
            this._isOnline = false;
            this.functions = null;
            this.sid = sid;
            this.serverName = servername;

        }


        public string getServerName() 
        {
            return this.serverName;
         }

        public bool isOnline() {
            return this._isOnline;
        }

        public void set_isOnline(bool value) {
            this._isOnline = value;
        }

        public bool verify_function()
        {
            bool mflag = false;
            if (this.functions is null)  throw new dBError("E072");
            if (!this.functions.GetType().Name.StartsWith("Action")) { throw new dBError("E073"); }

            mflag = true;

            return mflag;
        }

        public async Task register()
        {
            try
            {
                if(this.verify_function())
                {
                    Task.Factory.StartNew(() => this.functions(this)).ConfigureAwait(false);
                }
            }
            catch (dBError dberr){
                throw dberr;
            }
            bool cstatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.REGISTER_RPC_SERVER,
                                this.serverName, this.sid, null);

            if (!cstatus)
            {
                throw new dBError("E047");
            }
        }


        public async Task unregister()
        {
            bool cstatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.UNREGISTER_RPC_SERVER, this.serverName, this.sid, null);
            if (!cstatus)
            {
                throw new dBError("047");
            }

           // this.dispatch.unbind();
          //  this.functions = null;
        }



       public void regfn(string functionName, Delegate callback)
       {
            if (string.IsNullOrEmpty(functionName) || string.IsNullOrWhiteSpace(functionName))  { throw new dBError("E112");}

            if (callback == null) { throw new dBError("E113"); }
            if (this.functionNames.Contains(functionName)) throw (new dBError("E112"));
            if (!this.dispatch.isExists(functionName))
            {
                this.dispatch.bind(functionName, callback);
            }
	    }



        public void unregfn(string functionName, Delegate callback)
        {
            if (this.functionNames.Contains(functionName)) return;
                this.dispatch.unbind(functionName, callback);
        }





        public void bind(string eventName, Delegate callback)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrWhiteSpace(eventName))
            {
                throw new dBError("E074");
            }

            if(callback == null)
            {
                throw new dBError("E075");
            }

            if (!this.functionNames.Contains(eventName)) throw (new dBError("E074"));

            this.dispatch.bind(eventName, callback);
        }


        public void unbind(string eventName, Delegate callback) {
            if (!this.functionNames.Contains(eventName)) return;
                this.dispatch.unbind(eventName, callback);
        }

        public async Task handle_dispatcher(string functionName, string returnSubect, string sid, string payload)
        {
            CResponseHandler response = new CResponseHandler(functionName, returnSubect, sid, this.dbcore, "rpc");
            await this.dispatch.emit_cf(functionName, payload, response);
        }


        public async Task handle_tracker_dispatcher(string responseid, object errorcode)
        {
            await this.dispatch.emit_cf("rpc.response.tracker", responseid, errorcode);
        }



        public async Task handle_exceed_dispatcher()
        {
            dBError err = new dBError("E070");
            err.updateCode("CALLEE_QUEUE_EXCEEDED");
            await this.dispatch.emit_cf("rpc.callee.queue.exceeded", err, null);
        }



        public async Task emit(string eventName, string EventInfo, string Name = "", object metadata = null)
        {
            await this.dispatch.emit(eventName, EventInfo, Name, metadata);
        }



        public async Task emit(string eventName, object EventInfo, string Name = "", object metadata = null)
        {
            await this.dispatch.emit(eventName, EventInfo, Name, metadata);
        }

        public async Task emit_channel(string eventName, string EventInfo, string channelName, object metadata) {
            await this.dispatch.emit(eventName, EventInfo, channelName, metadata);
        }

        public async Task emit_channel(string eventName, string EventInfo, object metadata) {
            await this.dispatch.emit_channel(eventName, EventInfo, metadata);
        }

        public async Task emit_channel(string eventName, object EventInfo, object metadata)
        {
            await this.dispatch.emit_channel(eventName, EventInfo, metadata);
        }


        public async Task  handle_dispatcher_WithObject(string functionName, string returnSubect, string sid, string payload, string sourceip, string sourceid)
        {
            
            CResponseHandler response = new CResponseHandler(functionName, returnSubect, sid, this.dbcore, "rpc");
            dBExtraData extraData = new dBExtraData();
            if (!string.IsNullOrEmpty(sourceid))
            {
                string[] strData = sourceid.Split("#".ToCharArray());
                if (strData.Length > 0) extraData.sessionid = strData[0];
                if (strData.Length > 1) extraData.libtype = strData[1];
                if (strData.Length > 2) extraData.sourceipv4 = strData[2];
                if (strData.Length >= 3) extraData.sourceipv6 = strData[3];
            }
            extraData.info = sourceip;
            dBParams dbparam = new dBParams(payload, extraData);

            await this.dispatch.emit_clientfunction(functionName, dbparam, response);
        }



        public async void resetqueue()
        {
            bool m_status = await util.updatedBNewtworkCF(this.dbcore, MessageType.RPC_CALLEE_QUEUE_EXCEEDED, null, null, null, null, null, false, false);
            if (!m_status) { throw new dBError("E079"); }
        }

    }
}
