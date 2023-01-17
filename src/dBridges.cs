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
using dBridges.clientFunctions;
using dBridges.connections;

using dBridges.dispatchers;

using dBridges.events;
using dBridges.exceptions;
using dBridges.Messages;
using dBridges.channel;
using SocketIOClient;
using dBridges.Utils;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Globalization;

using SocketIOClient.Transport;
using Newtonsoft.Json;
using System.Threading;
using RSG;
using dBridges.remoteprocedure;
using System.Text.Json;
using System.Diagnostics;

namespace dBridges
{
    public class dBridges
    {

        public string appkey;
        public string appsecret; 
        private SocketIO ClientSocket;
        
        private SocketIOOptions options;
        public string auth_url;
        public connectStates connectionstate;
        public station channel;
        public string sessionid;
        public double maxReconnectionDelay;
        public double minReconnectionDelay;
        public double reconnectionDelayGrowFactor;
        public uint minUptime;
        public uint connectionTimeout;
        public uint maxReconnectionRetries;
        private uint uptimeTimeout;
        private uint retryCount;
        public bool autoReconnect;
        private uint lifeCycle;
        private bool isServerReconnect;
        public  cfclient  cf;
        public CRpc rpc;

        readonly SemaphoreSlim _sendLock;

        public dBridges()
        {

            this._sendLock = new SemaphoreSlim(1, 1);
            this.ClientSocket = null;
            this.sessionid = "";
             
            this.connectionstate = new connectStates(this);
            this.channel = new station(this);
            this.options = null;

            

            Random random = new Random();

            this.maxReconnectionRetries = 10;
            this.maxReconnectionDelay = 120000;
            this.minReconnectionDelay = 1000 + random.NextDouble() * 4000;
            this.reconnectionDelayGrowFactor = 1.3;
            this.minUptime = 500;
            this.connectionTimeout = 10000;
            this.autoReconnect = true;

            this.uptimeTimeout = 0;
            this.retryCount = 0;
            
            this.lifeCycle = 0;
            this.isServerReconnect = false;

            this.appkey = null;
            this.appsecret = null;
            this.cf = new cfclient(this);
            this.rpc = new CRpc(this);
        }



        private  string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private  string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);

        }


        private byte[] string_hex(string key)
        {
            var bytes = new byte[key.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(key.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            return bytes;
        }


        private string hex_string(byte[] value)
        {
            string hs = BitConverter.ToString(value);
            return hs.Replace("-", "").ToLower();
        }

        private string getauth_sign(string appkey, string secretkey)
        {
            try
            {

                double dt = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000;
                string utcgmtsc = Math.Round(dt).ToString();
                var hash = new HMACSHA256(Encoding.UTF8.GetBytes(secretkey));
                byte[] hmackey = hash.ComputeHash(Encoding.UTF8.GetBytes(appkey + "." + utcgmtsc));
                string shmackey = hex_string(hmackey);
                string new_appkey = Base64Encode(appkey) + ":" + Base64Encode(utcgmtsc) + ":" + Base64Encode(shmackey);

                return new_appkey;
            }
            catch (Exception )
            {
                return "";
            }

        }



        private async  Task<CApiResponce> GetDBRInfo(string url, string apikey)
        {
            CApiResponce result;
            using (var client = new HttpClient())
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    var stringContent = new StringContent("{}", UnicodeEncoding.UTF8, "application/json"); // use MediaTypeNames.Application.Json in Core 3.0+ and Standard 2.1+

                    client.DefaultRequestHeaders.Add("x-api-key", apikey);
                    client.DefaultRequestHeaders.Add("lib-transport", "sio");

                    var response = await client.PostAsync(new Uri(url), stringContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        result = new CApiResponce();
                        result.update((int)response.StatusCode, response.ReasonPhrase);
                        return result;
                    }
                    else
                    {

                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<CApiResponce>(jsonResponse);
                        result.update((int)response.StatusCode, response.ReasonPhrase);
       
                        return result;
                    }
                }
                catch (Exception e)
                {
                    result = new CApiResponce();
                    result.update(999, e.Message);
                    return result;
                }
            }

        }

        public async Task disconnect()
        {
            await this.ClientSocket.DisconnectAsync();
        }

        public async Task connect()
        {
            if (this.retryCount == 0 && !(this.connectionstate.get_newLifeCycle()))
            {
                this.connectionstate.set_newLifeCycle(true);
            }

            if (string.IsNullOrEmpty(this.auth_url) || string.IsNullOrWhiteSpace(this.auth_url)){ 
                if(this.connectionstate.get_newLifeCycle()) throw (new dBError("E001"));
               await this.shouldRestart("E001");
                return;
            }
            
            
            if (string.IsNullOrEmpty(this.appkey) || string.IsNullOrWhiteSpace(this.appkey)) {
                if (this.connectionstate.get_newLifeCycle()) throw (new dBError("E002"));
                await this.shouldRestart("E002");
                return;
            }
            if (string.IsNullOrEmpty(this.appsecret) || string.IsNullOrWhiteSpace(this.appsecret)) {
                if (this.connectionstate.get_newLifeCycle()) throw (new dBError("E003"));
                await this.shouldRestart("E003");
                return;
            }

            string new_appkey = this.getauth_sign(this.appkey, this.appsecret);
            if (string.IsNullOrEmpty(new_appkey) || string.IsNullOrWhiteSpace(new_appkey)) {
                if (this.connectionstate.get_newLifeCycle()) throw (new dBError("E005"));
               await this.shouldRestart("E005");
                return;
            }


            try
            {
                this.cf.verify_function();
            }
            catch (dBError dberror)
            {

                if (this.connectionstate.get_newLifeCycle()) throw dberror;
               await  this.shouldRestart(dberror);
                return;
            }
            

            CApiResponce result = await this.GetDBRInfo(this.auth_url, new_appkey);
            if(result.statuscode != 200){

                dBError dberror;
                if (result.statuscode == 999)
                {
                    dberror = new dBError("E008", result.statuscode.ToString(), result.reasonphrase);
                }
                else
                {
                    dberror = new dBError("E006", result.statuscode.ToString(), result.reasonphrase);
                }

                if (this.connectionstate.get_newLifeCycle()) throw dberror;
                await this.shouldRestart(dberror);
                return;
            }

            string uri = (result.secured) ? "https://" : "http://";
            uri = uri + result.wsip + ":" + result.wsport;

            this.options = new SocketIOOptions
            {
                EIO = 3,
                Reconnection = false,
                Transport = TransportProtocol.WebSocket,
                ConnectionTimeout = (this.connectionTimeout <= 0) ? TimeSpan.FromMilliseconds(20000) : TimeSpan.FromMilliseconds (this.connectionTimeout),
                Query = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("sessionkey", result.sessionkey),
                            new KeyValuePair<string, string>("version", "1.1"),
                            new KeyValuePair<string, string>("libtype", "csharp"),
                            new KeyValuePair<string, string>("cf", Convert.ToInt32( this.cf.enable).ToString())
                        }

            };

            
            this.ClientSocket = new SocketIO(uri, this.options);


            this.ClientSocket.OnConnected += (sender, e) =>{
            
            };
        

            this.ClientSocket.On("db", async (response) =>
            {

                try
                {
                   
                    dbMessage dbm = this.Deserialize(response);

                   
                  _ = Task.Run( () =>  this.IOMessage(dbm));
                   

                }
                catch (Exception ){
                }
                

            });



            this.ClientSocket.OnDisconnected += async (sender, e) =>
            {
                await this.IODisconnect(e as string);
            };




            this.ClientSocket.OnError += (sender, e) =>
            {
                int k = 1;
            };


            if (this.lifeCycle == 0) {
               await this.connectionstate.handledispatcher(states.CONNECTING, "");
            }

            try
            {
                await this.ClientSocket.ConnectAsync();//.ConfigureAwait(false);
            }catch(Exception ){
              
            }
            
        }



        private dbMessage Deserialize(SocketIOResponse response)
        {
            dbMessage dbmessage = new dbMessage();
           

            for (int i = 0; i < response.Count; i++)
            {
                switch (i)
                {
                    case 0:

                        dbmessage.dbmsgtype = response.GetValue<int>(i);
                        dbmessage.dbType = (MessageType)dbmessage.dbmsgtype;
                        break;
                    case 1:
                        dbmessage.subject = response.GetValue<string>(i);
                        break;
                    case 2:
                        dbmessage.rsub = response.GetValue<string>(i);

                        break;
                    case 3:
                        try
                        {
                            dbmessage.sid = response.GetValue<string>(i);
                        }
                        catch (Exception)
                        {
                            dbmessage.sid = "0";
                        }
                        break;
                    case 4:
                        try
                        {
                            dbmessage.payload = Encoding.UTF8.GetString(response.GetValue<byte[]>(i));
                        }
                        catch (Exception)
                        {

                            dbmessage.payload = "";
                        }
                            
                        
                        break;
                    case 5:
                        dbmessage.fenceid = response.GetValue<string>(i);

                        break;
                    case 6:
                        try
                        {
                            switch(response.GetValue(i).ValueKind)
                            {
                                case JsonValueKind.Null:
                                    dbmessage.rspend = false;
                                    break;
                                case JsonValueKind.String:
                                    dbmessage.rspend = string.IsNullOrEmpty(response.GetValue<string>(i)) ? false : Convert.ToBoolean(response.GetValue<string>(i));
                                    break;
                                case JsonValueKind.False:
                                    dbmessage.rspend = false;
                                    break;
                                case JsonValueKind.True:
                                    dbmessage.rspend = true;
                                    break;


                            }
                        }
                        catch (Exception )
                        {

                            dbmessage.rspend = false;
                        }


                        break;
                    case 7:
                        try
                        {
                            switch (response.GetValue(i).ValueKind)
                            {
                                case JsonValueKind.Null:
                                    dbmessage.rtrack = false;
                                    break;
                                case JsonValueKind.String:
                                    dbmessage.rtrack = string.IsNullOrEmpty(response.GetValue<string>(i)) ? false : Convert.ToBoolean(response.GetValue<string>(i));
                                    break;
                                case JsonValueKind.False:
                                    dbmessage.rtrack = false;
                                    break;
                                case JsonValueKind.True:
                                    dbmessage.rtrack = true;
                                    break;


                            }
                        }
                        catch (Exception)
                        {

                            dbmessage.rtrack = false;
                        }


                        break;
                    case 8:
                        dbmessage.rtrackstat = response.GetValue<string>(i);

                        break;
                    case 9:


                        try
                        {
                            dbmessage.t1 = response.GetValue<Int64>(i);
                        }
                        catch (Exception)
                        {
                            dbmessage.t1 = 0;
                        }



                        break;
                    case 10:


                        try
                        {
                            dbmessage.latency = response.GetValue<Int64>(i);
                        }
                        catch (Exception)
                        {

                            dbmessage.latency = 0;
                        }
                        
                        break;
                    case 11:
                        try
                        {
                            dbmessage.globmatch = response.GetValue<int>(i);
                        }
                        catch (Exception)
                        {

                            dbmessage.globmatch = 0;
                        }
                        

                        break;
                    case 12:
                        dbmessage.sourceid = response.GetValue<string>(i);

                        break;
                    case 13:
                        dbmessage.sourceip = response.GetValue<string>(i);

                        break;
                    case 14:
                        try
                        {
                            dbmessage.replylatency = string.IsNullOrEmpty(response.GetValue<string>(i)) ? false : Convert.ToBoolean(response.GetValue<string>(i));
                        }
                        catch (Exception)
                        {

                            dbmessage.replylatency = false;
                        }


                        break;
                    case 15:

                        try
                        {
                            if(response.GetValue(i).ValueKind == JsonValueKind.Null)
                            {
                                dbmessage.oqueumonitorid = "";
                            }

                            if (response.GetValue(i).ValueKind == JsonValueKind.Number)
                            {
                                dbmessage.oqueumonitorid = response.GetValue<Int64>(i).ToString();
                            }

                            if (response.GetValue(i).ValueKind == JsonValueKind.String)
                            {
                                dbmessage.oqueumonitorid = response.GetValue<string>(i).ToString();
                            }

                        }
                        catch (Exception)
                        {

                            dbmessage.oqueumonitorid ="";
                        }
                       

                        break;
                    default:
                        break;

                }
            }
            return dbmessage;
        }



        public async Task IODisconnect(string reason)
        {
            await this.channel.send_OfflineEvents();
            await this.rpc.send_OfflineEvents();
            switch (reason)
            {
                case "io server disconnect":
                    await this.connectionstate.handledispatcher(states.ERROR, new dBError("E061"));
                    if (this.ClientSocket != null) this.ClientSocket.Dispose();
                    if (!this.autoReconnect){
                        
                        await this.channel.cleanUp_All();
                        await this.rpc.clean_all();
                        //this.connectionstate.state = "";
                        this.lifeCycle = 0;
                        this.retryCount = 0;
                        this.connectionstate.set_newLifeCycle(true);
                        await this.connectionstate.handledispatcher(states.DISCONNECTED, "");
                    }
                    else{
                        await this.reconnect();
                    }
                    break;
                case "io client disconnect":
                    if (this.isServerReconnect) {
                        await this.connectionstate.handledispatcher(states.CONNECTION_BREAK, new dBError("E062"));
                        if (this.ClientSocket != null) this.ClientSocket.Dispose();
                        if (!this.autoReconnect){
                            
                            await this.channel.cleanUp_All();
                            await this.rpc.clean_all();
                            //this.connectionstate.state = "";
                            this.lifeCycle = 0;
                            this.retryCount = 0;
                            this.connectionstate.set_newLifeCycle(true);
                            await this.connectionstate.handledispatcher(states.DISCONNECTED, "");
                        }
                        else{
                            await this.reconnect();
                        }

                    } else{
                      
                       
                        if (this.ClientSocket != null) this.ClientSocket.Dispose();
                        await this.channel.cleanUp_All();
                        await this.rpc.clean_all();
                        //this.connectionstate.state = "";
                        this.lifeCycle = 0;
                        this.retryCount = 0;
                        this.connectionstate.set_newLifeCycle(true);
                        await this.connectionstate.handledispatcher(states.DISCONNECTED, "");

                    }

            break;

            default:
                    
                    await this.connectionstate.handledispatcher(states.CONNECTION_BREAK, new dBError("E063"));

                    if (this.ClientSocket != null) this.ClientSocket.Dispose();
                    if (!this.autoReconnect){
                        
                        await this.channel.cleanUp_All();
                        await this.rpc.clean_all();
                        //this.connectionstate.state = "";
                        this.lifeCycle = 0;
                        this.retryCount = 0;
                        this.connectionstate.set_newLifeCycle(true);
                        await this.connectionstate.handledispatcher(states.DISCONNECTED, "");
                    }
                    else{
                        await this.reconnect();
                    }
                    break;

            }
        }


        private async Task IOMessage(dbMessage dbmessage)
        {

            switch(dbmessage.dbType)
            {
                case MessageType.SYSTEM_MSG:
                    await Handle_SYSTEM_MSG(dbmessage.subject, dbmessage.payload, dbmessage.t1, dbmessage);
                    break;
                case MessageType.SERVER_SUBSCRIBE_TO_CHANNEL:
                   await this.Handle_SUBSCRIBE_MSG(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;
                case MessageType.UNSUBSCRIBE_DISCONNECT_FROM_CHANNEL:
                    await this.Handle_UNSUBSCRIBE_MSG(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;
                case MessageType.PUBLISH_TO_CHANNEL:
                    await this.Handle_PUBLISH_MSG(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;
                case MessageType.PARTICIPANT_JOIN:
                    await this.Handle_PARTICIPANT_JOIN_MSG(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;
                case MessageType.PARTICIPANT_LEFT:
                    await this.Handle_PARTICIPANT_LEFT_MSG(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;
                case MessageType.CF_CALL_RECEIVED:
                    await this.Handle_CF_CALL_RECEIVED_MSG(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;
                case MessageType.CF_CALL_RESPONSE:
                    await this.cf.handle_callResponse(dbmessage.sid, dbmessage.payload, dbmessage.rspend, dbmessage.rsub);
                    break;
                case MessageType.CF_RESPONSE_TRACKER:
                    await this.cf.handle_tracker_dispatcher(dbmessage.subject, dbmessage.rsub);
                    break;
                case MessageType.CF_CALLEE_QUEUE_EXCEEDED:
                    await this.cf.handle_exceed_dispatcher();
                    break;

                case MessageType.REGISTER_RPC_SERVER:
                    await this.Handle_REGISTER_RPC_SERVER(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;
                case MessageType.UNREGISTER_RPC_SERVER:
                    
                    await this.Handle_UNRegister_MSG(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;

                case MessageType.CONNECT_TO_RPC_SERVER:
                    await this.Handle_CONNECT_TO_RPC_SERVER(dbmessage.subject, dbmessage.payload, dbmessage);
                    break;
                case MessageType.RPC_CALL_RESPONSE:
                    CrpCaller rpccaller = this.rpc.get_object(dbmessage.sid);
                    if (rpccaller != null)
                    {
                        await rpccaller.handle_callResponse(dbmessage.sid, dbmessage.payload, dbmessage.rspend, dbmessage.rsub);
                    }
                    break;
                case MessageType.RPC_CALL_RECEIVED:
                    
                    if (!string.IsNullOrEmpty(dbmessage.sid)){
                        long lsid = 0;
                        long.TryParse(dbmessage.sid, out lsid);
                        if (lsid != 0)
                        {
                            Crpcserver rpcserver = this.rpc.get_rpcServerObject(dbmessage.sid) as Crpcserver;
                            await rpcserver.handle_dispatcher_WithObject(dbmessage.subject, dbmessage.rsub, dbmessage.sid, dbmessage.payload, dbmessage.sourceip, dbmessage.sourceid);
                        }
                    }
                    break;
                case MessageType.RPC_RESPONSE_TRACKER:
                    object rpccs = this.rpc.get_rpcServerObject(dbmessage.sid);
                    if (rpccs.GetType().Equals(typeof(Crpcserver)))
                    {
                        await (rpccs as Crpcserver).handle_tracker_dispatcher(dbmessage.subject, dbmessage.rsub);
                    }
                    else
                    {
                        await (rpccs as CrpCaller).handle_tracker_dispatcher(dbmessage.subject, dbmessage.rsub);
                    }

                    break;

                case MessageType.RPC_CALLEE_QUEUE_EXCEEDED:
                    object rpccsn = this.rpc.get_rpcServerObject(dbmessage.sid);
                    if (rpccsn.GetType().Equals(typeof(Crpcserver)))
                    {
                        await (rpccsn as Crpcserver).handle_exceed_dispatcher();
                    }
                    else
                    {
                        await (rpccsn as CrpCaller).handle_exceed_dispatcher();
                    }


                    break;



            }
        }


        private async Task reconnect()
        {
            if (this.retryCount >= this.maxReconnectionRetries)
            {
                try
                {
                    await this.connectionstate.handledispatcher(states.RECONNECT_FAILED, new dBError("E060"));
                    //if (this.ClientSocket != null) this.ClientSocket.Dispose();


                    await this.channel.cleanUp_All();
                    await this.rpc.clean_all();
                    //this.connectionstate.state = "";
                    this.lifeCycle = 0;
                    this.retryCount = 0;
                    this.connectionstate.set_newLifeCycle(true);
                    await this.connectionstate.handledispatcher(states.DISCONNECTED, "");
                }
                catch (Exception _)
                {

                }
            }
            else
            {
                this.retryCount++;
                this.dBridge_wait()
                    .Then(async (result) =>
                    {
                        this.connectionstate.reconnect_attempt = (int)this.retryCount;
                        await this.connectionstate.handledispatcher(states.RECONNECTING, this.retryCount);
                        await this.connect();
                    });
            }
        }

        private async Task shouldRestart(object ekey)
        {
            if (this.autoReconnect)
            {
                if (!this.connectionstate.get_newLifeCycle())
                {

                    if (ekey is string)
                       await this.connectionstate.handledispatcher(states.RECONNECT_ERROR, new dBError(ekey as string));
                    else
                       await this.connectionstate.handledispatcher(states.RECONNECT_ERROR, ekey);
                     await this.reconnect();
                }else{
                    if (ekey is string)
                       await this.connectionstate.handledispatcher(states.ERROR, new dBError(ekey as string));
                    else
                       await this.connectionstate.handledispatcher(states.ERROR, ekey);
                }
            }
        }



        private int getNextDelay() {
        int delay = 0;
        if (this.retryCount > 0) {
            delay = (int) ((double)this.minReconnectionDelay * Math.Pow(this.reconnectionDelayGrowFactor, this.retryCount - 1));
            delay = (delay > this.maxReconnectionDelay) ? (int) this.maxReconnectionDelay : delay;
            delay = (delay < this.minReconnectionDelay) ? (int) this.minReconnectionDelay : delay;
        }
        
        return delay;
    }


    private IPromise<object> dBridge_wait()
    {
            var promise = new Promise<object>();

            Task.Delay(this.getNextDelay()).ContinueWith(_ => {
                promise.Resolve(true);
            });
            return promise;
    }


    private async Task acceptOpen() {
            this.retryCount = 0;
            this.connectionstate.reconnect_attempt = (int)this.retryCount;
            if (this.ClientSocket.Connected) {
                    if (this.lifeCycle == 0) {
                       await this.connectionstate.handledispatcher(states.CONNECTED, "");
                        this.lifeCycle++;
                    } else {
                        await this.connectionstate.handledispatcher(states.RECONNECTED, "");
                    }
           }
        }



        private async Task Handle_SYSTEM_MSG(string subject , string payload, long t1 , dbMessage dbmessage)
        {
            long lib_latency = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - t1;

            switch (subject)
            {
                case "connection:success":
                    this.sessionid = payload;
                    
                    if (this.connectionstate.get_newLifeCycle())
                    {
                        if (this.cf.enable){
                            Task.Factory.StartNew(() => this.cf.functions(this.cf)).ConfigureAwait(false); 
                        }
                    }


                    if (!this.connectionstate.get_newLifeCycle())
                    {
                        await this.rpc.ReSubscribeAll();
                        await this.channel.ReSubscribeAll();
                    }


                    this.connectionstate.set_newLifeCycle(false);

                    if (t1 > 0)
                    {

                        await this.Rttpong(dbmessage.dbmsgtype, "rttpong", dbmessage.rsub, dbmessage.sid, dbmessage.payload, dbmessage.fenceid,
                                     dbmessage.rspend, dbmessage.rtrack, dbmessage.rtrackstat, dbmessage.t1, lib_latency, dbmessage.globmatch,
                                     dbmessage.sourceid, dbmessage.sourceip, dbmessage.replylatency, dbmessage.oqueumonitorid);
                    }


                    int milliseconds = (this.minUptime <= 0) ? 1000 : (int) this.minUptime;
                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;
                     await Task.Delay(milliseconds).ContinueWith(async (t) =>{
                         await this.acceptOpen();
                      }, cancellationToken);
                    break;
                case "rttping":
                    if (t1 > 0) {

                        await this.Rttpong(dbmessage.dbmsgtype, "rttpong", dbmessage.rsub, dbmessage.sid, dbmessage.payload, dbmessage.fenceid,
                                     dbmessage.rspend, dbmessage.rtrack, dbmessage.rtrackstat, dbmessage.t1, lib_latency, dbmessage.globmatch,
                                     dbmessage.sourceid, dbmessage.sourceip, dbmessage.replylatency, dbmessage.oqueumonitorid);
                     }
                    break;
                case "rttpong":
                    this.connectionstate.rttms =(ulong) lib_latency;
                    await this.connectionstate.handledispatcher(states.RTTPONG, lib_latency);
                    break;
                case "reconnect":
                        this.isServerReconnect = true;
                        await this.ClientSocket.DisconnectAsync();
                    break;
                default:
                    dBError err = new dBError("E082");
                    err.updateCode(subject, payload);

                    await this.connectionstate.handledispatcher(states.ERROR, err);
                    break;

            }
        }

    private async Task Rttpong(int dbmsgtype, string subject, string rsub, string sid, string payload, string fenceid,
                bool rspend, bool rtrack, string rtrackstat, long t1, long latency, int globmatch,
                string sourceid, string sourceip, bool replylatency, string oqueumonitorid)
        {
            await this.ClientSocket.EmitAsync("db", new object[] { dbmsgtype , subject, rsub,
                                                                    sid,  Encoding.UTF8.GetBytes(payload), fenceid,
                                                                    rspend, rtrack, rtrackstat, t1,
                                                                    latency, globmatch, sourceid, sourceip, replylatency,
                                                                    oqueumonitorid});
        }

        public async Task<bool> send(object[] data)
        {
            bool mflag = false;
            try
            {
                if (this.ClientSocket.Connected)
                {
                    mflag = true;
                    _ = Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            if (this.ClientSocket.Connected)
                            {
                                await _sendLock.WaitAsync().ConfigureAwait(false);
                                await this.ClientSocket.EmitAsync("db", data);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.ToLower().Contains("aborted"))
                            {
                                await this.disconnect();
                            }
                        }
                        finally
                        {
                            _sendLock.Release();
                        }
                    });
                }
            }
            catch (Exception ex)
            {

                if (ex.Message.ToLower().Contains("aborted"))
                {
                    await this.disconnect();
                }
            }
            finally
            {

            }
            return mflag;
        }


        public async Task<bool> send_x(object[] data)
        {
            bool mflag = false;
            try
            {
                if (this.ClientSocket.Connected)
                {
                    await _sendLock.WaitAsync().ConfigureAwait(false);
                    await this.ClientSocket.EmitAsync("db", data);
                    mflag = true;
                }
            }
            catch (Exception ex)
            {

                if (ex.Message.ToLower().Contains("aborted"))
                {

                    await this.disconnect();
                }


               
            }
            finally
            {
                _sendLock.Release();

            }
            return mflag;
        }


        public bool isSocketConnected()
    {
            return this.ClientSocket.Connected;
    }






        private async Task Handle_SUBSCRIBE_MSG(string subject, string payload, dbMessage dbmessage)
        {
            
            string subscribe_status = this.channel.get_subscribeStatus(dbmessage.sid);
            switch (subject)
            {
                case "success":
                        switch(subscribe_status)
                        {
                        case channelState.SUBSCRIPTION_INITIATED:
                           await this.channel.updateChannelsStatusAddChange(0, dbmessage.sid, channelState.SUBSCRIPTION_ACCEPTED, "");
                            break;

                        case channelState.SUBSCRIPTION_ACCEPTED:
                        case channelState.SUBSCRIPTION_PENDING:
                            await this.channel.updateChannelsStatusAddChange(1, dbmessage.sid, channelState.SUBSCRIPTION_ACCEPTED, "");
                            break;
                        default:
               
                            break;

                    }
                    break;
                default:
                    
                    dBError dberr = new dBError("E064");
                    dberr.updateCode(subject.ToUpper(), payload);

                    switch (subscribe_status)
                    {
                        case channelState.CONNECTION_INITIATED:
                            await this.channel.updateChannelsStatusAddChange(0, dbmessage.sid, channelState.SUBSCRIPTION_ERROR, dberr);
                            break;

                        case channelState.SUBSCRIPTION_ACCEPTED:
                        case channelState.SUBSCRIPTION_PENDING:
                            await this.channel.updateChannelsStatusAddChange(1, dbmessage.sid, channelState.SUBSCRIPTION_PENDING, dberr);
                            break;
                    }
                    break;
            }
        }



        private async Task Handle_UNSUBSCRIBE_MSG(string subject, string payload, dbMessage dbmessage)
        {
            string channeltype = this.channel.get_channelType(dbmessage.sid);
            switch (subject)
            {
                case "success":
                        switch (channeltype)
                        {
                            case "s":
                            await this.channel.updateChannelsStatusRemove(dbmessage.sid, channelState.UNSUBSCRIBE_ACCEPTED, "");
                                break;
                            case "c":
                            await this.channel.updateChannelsStatusRemove(dbmessage.sid, channelState.DISCONNECT_ACCEPTED, "");
                                break;
                    }
                    break;
                default:
                    dBError dberr = new dBError("E065");
                    dberr.updateCode(subject.ToUpper(), payload);

                    switch (channeltype)
                    {
                        case "s":
                            await this.channel.updateChannelsStatusRemove(dbmessage.sid, channelState.UNSUBSCRIBE_ERROR, dberr);
                            break;
                        case "c":
                            await this.channel.updateChannelsStatusRemove(dbmessage.sid, channelState.DISCONNECT_ERROR, dberr);
                            break;
                    }

                    break;
            }
        }

        private async Task Handle_PUBLISH_MSG(string subject, string payload, dbMessage dbmessage)
        {
            string mchannelName = this.channel.get_channelName(dbmessage.sid);
            metaData metadata = new metaData(mchannelName, subject, dbmessage.sourceid, dbmessage.oqueumonitorid, dbmessage.sourceip, dbmessage.t1);

            metadata.channelName = (mchannelName.ToLower().StartsWith("sys:*")) ? dbmessage.fenceid : mchannelName;

            await this.channel.handledispatcherEvents(subject, payload, mchannelName, metadata);
        }



        private dBExtraData ConvertExtraData(string sourceip, string sourceid, string channelname ="")
        {
            dBExtraData extraData = new dBExtraData();

            if (!string.IsNullOrEmpty(sourceid))
            {
                string[] strData = sourceid.Split("#".ToCharArray());
                if (strData.Length > 0) extraData.sessionid = strData[0];
                if (strData.Length > 1) extraData.libtype = strData[1];
                if (strData.Length > 2) extraData.sourceipv4 = strData[2];
                if (strData.Length >= 3) extraData.sourceipv6 = strData[3];
                if (strData.Length >= 4)
                {
                    extraData.sourcesysid = strData[4];        
               
                }
            }

            extraData.info = sourceip;

            if (!string.IsNullOrEmpty(channelname))
            {
                extraData.channelName = channelname;
            }
            return extraData;
        }


        private async Task Handle_PARTICIPANT_JOIN_MSG(string subject, string payload, dbMessage dbmessage)
        {


            string mchannelName = this.channel.get_channelName(dbmessage.sid);

            metaData metadata = new metaData(mchannelName, "dbridges:participant.joined", dbmessage.sourceid, dbmessage.oqueumonitorid, dbmessage.sid, dbmessage.t1);

            if (mchannelName.ToLower().StartsWith("sys") || mchannelName.ToLower().StartsWith("prs"))
            {
                if(mchannelName.ToLower().StartsWith("sys:*"))
                {
                    dBExtraData extdata = this.ConvertExtraData(dbmessage.sourceip, dbmessage.sourceid, dbmessage.fenceid);
                    metadata.sessionid = extdata.sessionid;
                    metadata.sourcesysid = extdata.sourcesysid;
                    await this.channel.handledispatcherEvents("dbridges:participant.joined", extdata, mchannelName, metadata);
                }
                else
                {
                    dBExtraData extdata = this.ConvertExtraData(dbmessage.sourceip, dbmessage.sourceid, "");
                    metadata.sessionid = extdata.sessionid;
                    metadata.sourcesysid = extdata.sourcesysid;
                    await this.channel.handledispatcherEvents("dbridges:participant.joined", extdata, mchannelName, metadata);
                }
            }
            else
            {
                await this.channel.handledispatcherEvents("dbridges:participant.joined", dbmessage.sourceid , mchannelName, metadata);
            }
        }


        private async Task Handle_PARTICIPANT_LEFT_MSG(string subject, string payload, dbMessage dbmessage)
        {


            string mchannelName = this.channel.get_channelName(dbmessage.sid);

            metaData metadata = new metaData(mchannelName, "dbridges:participant.left", dbmessage.sourceid, dbmessage.oqueumonitorid, dbmessage.sid, dbmessage.t1);

            if (mchannelName.ToLower().StartsWith("sys") || mchannelName.ToLower().StartsWith("prs"))
            {
                if (mchannelName.ToLower().StartsWith("sys:*"))
                {
                    dBExtraData extdata = this.ConvertExtraData(dbmessage.sourceip, dbmessage.sourceid, dbmessage.fenceid);
                    metadata.sessionid = extdata.sessionid;
                    metadata.sourcesysid = extdata.sourcesysid;
                    await this.channel.handledispatcherEvents("dbridges:participant.left", extdata, mchannelName, metadata);
                }
                else
                {
                    dBExtraData extdata = this.ConvertExtraData(dbmessage.sourceip, dbmessage.sourceid, "");
                    metadata.sessionid = extdata.sessionid;
                    metadata.sourcesysid = extdata.sourcesysid;
                    await this.channel.handledispatcherEvents("dbridges:participant.left", extdata, mchannelName, metadata);
                }
            }
            else
            {
                await this.channel.handledispatcherEvents("dbridges:participant.left", dbmessage.sourceid, mchannelName, metadata);
            }
        }

        private async Task Handle_CF_CALL_RECEIVED_MSG(string subject, string payload, dbMessage dbmessage)
        {
            if(string.IsNullOrEmpty(dbmessage.sid) || dbmessage.sid=="0")
            {
               await this.cf.handle_dispatcher(subject, dbmessage.rsub, dbmessage.sid, payload);
            }
        }



        private async Task Handle_REGISTER_RPC_SERVER(string subject, string payload, dbMessage dbmessage)
        {
         
            string rpc_status = this.rpc.get_rpcStatus(dbmessage.sid);
            

                switch (subject)
                {
                    case "success":
                        switch (rpc_status)
                        {
                            case rpcState.REGISTRATION_INITIATED:
                                await this.rpc.updateRegistrationStatusAddChange(0, dbmessage.sid, rpcState.REGISTRATION_ACCEPTED, "");
                                break;

                            case rpcState.REGISTRATION_ACCEPTED:
                            case rpcState.REGISTRATION_PENDING:
                                await this.rpc.updateRegistrationStatusAddChange(1, dbmessage.sid, rpcState.REGISTRATION_ACCEPTED, "");
                                break;
                            default:
               
                                break;

                        }
                        break;
                    default:

                        dBError dberr = new dBError("E081");
                        dberr.updateCode(subject.ToUpper(), payload);

                        switch (rpc_status)
                        {
                            case rpcState.REGISTRATION_INITIATED:
                                await this.rpc.updateRegistrationStatusAddChange(0, dbmessage.sid, rpcState.REGISTRATION_ERROR, dberr);
                                break;

                            case rpcState.REGISTRATION_ACCEPTED:
                            case rpcState.REGISTRATION_PENDING:
                                await this.rpc.updateRegistrationStatusAddChange(1, dbmessage.sid, rpcState.REGISTRATION_PENDING, dberr);
                                break;
                        }
                        break;
                }
            }


        private async Task Handle_CONNECT_TO_RPC_SERVER(string subject, string payload, dbMessage dbmessage)
        {
            string rpc_status = this.rpc.get_rpcStatus(dbmessage.sid);
            switch (subject)
            {
                case "success":
                    switch (rpc_status)
                    {
                        case rpcState.RPC_CONNECTION_INITIATED:
                            await this.rpc.updateRegistrationStatusAddChange(0, dbmessage.sid, rpcState.RPC_CONNECTION_ACCEPTED, "");
                            break;

                        case rpcState.RPC_CONNECTION_ACCEPTED:
                        case rpcState.RPC_CONNECTION_PENDING:
                            await this.rpc.updateRegistrationStatusAddChange(1, dbmessage.sid, rpcState.RPC_CONNECTION_ACCEPTED, "");
                            break;
                        default:
               
                            break;

                    }
                    break;
                default:

                    dBError dberr = new dBError("E082");
                    dberr.updateCode(subject.ToUpper(), payload);

                    switch (rpc_status)
                    {
                        case rpcState.RPC_CONNECTION_INITIATED:
                            await this.rpc.updateRegistrationStatusAddChange(0, dbmessage.sid, rpcState.RPC_CONNECTION_ERROR, dberr);
                            break;

                        case rpcState.RPC_CONNECTION_ACCEPTED:
                        case rpcState.RPC_CONNECTION_PENDING:
                            await this.rpc.updateRegistrationStatusAddChange(1, dbmessage.sid, rpcState.RPC_CONNECTION_PENDING, dberr);
                            break;
                    }
                    break;
            }
        }


        private async Task Handle_UNRegister_MSG(string subject, string payload, dbMessage dbmessage)
        {
            string rpc_status = this.rpc.get_rpcStatus(dbmessage.sid);
            switch (subject)
            {
                case "success":

                    await this.rpc.removeRegistration(dbmessage.sid, rpcState.UNREGISTRATION_ACCEPTED, "");
                    break;
                default:
                    dBError dberr = new dBError("E081");
                    dberr.updateCode(subject.ToUpper(), payload);
                    await this.rpc.removeRegistration(dbmessage.sid, rpcState.UNREGISTRATION_ERROR, dberr);
                    break;

            }
        }



    }
}
