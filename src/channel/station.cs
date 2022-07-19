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
using dBridges.Utils;
using dBridges.Messages;
using dBridges.exceptions;
using dBridges.events;
using dBridges.privateAccess;
using dBridges.Tokens;
using System.Text.RegularExpressions;
using RSG;
using dBridges.remoteprocedure;
using System.Collections.Concurrent;


namespace dBridges.channel
{
    public class station
    {
        private static readonly List<string> channel_type = new List<string> 
        { "pvt" ,   "prs", "sys"};

        private static readonly List<string> list_of_supported_functionname = new List<string>
        {"channelMemberList", "channelMemberInfo", "timeout" ,  "err" };

        private Dictionary<string, object> channelsid_registry;
        private Dictionary<string, string> channelname_sid;

      //  private ConcurrentDictionary<string, object> c_channelsid_registry;
      //  private ConcurrentDictionary<string, string> c_channelname_sid;


        private object dbcore;
        private Action_dispatcher dispatch;

        public station(object dBCoreObject)
        {
            
            this.channelsid_registry = new Dictionary<string, object>();
            this.channelname_sid = new Dictionary<string, string>();

       //     this.c_channelsid_registry = new ConcurrentDictionary<string, object>();
       //     this.c_channelname_sid = new ConcurrentDictionary<string, string>();

            this.dbcore = dBCoreObject;
            this.dispatch = new Action_dispatcher();

        }


        public void bind(string eventName, Delegate callback)
        {
            this.dispatch.bind(eventName, callback);
        }

        public void unbind(string eventName, Delegate callback)
        {
            this.dispatch.unbind(eventName, callback);
        }

        public void bind_all(Delegate callback)
        {
            this.dispatch.bind_all(callback);
        }

        public void unbind_all(Delegate callback)
        {
            this.dispatch.unbind_all(callback);
        }


        public async Task handledispatcherEvents(string eventName , object eventInfo= null, string channelName= null, object metadata= null)
        {
            await this.dispatch.emit_channel(eventName, eventInfo,metadata);
            string sid = this.channelname_sid[channelName];
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            if (m_object != null)
            {
                string type = m_object["type"] as string;
                if (type == "s")
                {
                    channel cobject = m_object["ino"] as channel;
                    await cobject.emit_channel(eventName, eventInfo, metadata);
                }
               
            }
        }

        public bool isPrivateChannel(string channelName)
        {
            bool flag = false;
            if (channelName.Contains(":"))
            {
                string[] sdata = channelName.ToLower().Split(':');
                if (station.channel_type.Contains(sdata[0])) flag = true;
            }
            return flag;
        }

        public async Task communicateR(int mtype, string channelName, string sid, string access_token)
        {
            bool cStatus = false;
            if (mtype == 0)
            {
                cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.SERVER_SUBSCRIBE_TO_CHANNEL, channelName, sid, access_token, null, null);
            }
            if (!cStatus) throw (new dBError("E024"));
        }


        public async Task ReSubscribe(string sid)
        {
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string access_token = null;
            bool mprivate = this.isPrivateChannel(m_object["name"] as string);

            switch (m_object["status"] as string)
            {
                case channelState.SUBSCRIPTION_INITIATED:
                case channelState.SUBSCRIPTION_ACCEPTED:
                    try
                    {
                            await this.communicateR(0, m_object["name"] as string, sid, access_token);
                    }
                    catch (dBError e)
                    {
                        List<string> eventse = new List<string> { systemEvents.OFFLINE };
                       await this.handleSubscribeEvents(eventse, e, m_object);
                    }
                    break;
                case channelState.CONNECTION_INITIATED:
                case channelState.CONNECTION_ACCEPTED:
                    try
                    {
                      await this .communicateR(0, m_object["name"] as string, sid, access_token);
                     
                    }
                    catch (Exception e)
                    {
                        List<string> eventse = new List<string> { systemEvents.OFFLINE };
                       await this.handleSubscribeEvents(eventse, e.Message, m_object);
                    }
                    break;
                case channelState.UNSUBSCRIBE_INITIATED:
                    (m_object["ino"] as channel).set_isOnline(false);
                    List<string> events = new List<string> { systemEvents.UNSUBSCRIBE_SUCCESS, systemEvents.REMOVE };
                   await this.handleSubscribeEvents(events, "", m_object);
                    this.channelname_sid.Remove(m_object["name"] as string);
                    this.channelsid_registry.Remove(sid);
                    break;
                default:
                    break;
            }
        }




        public async Task  ReSubscribeAll()
        {
            foreach (KeyValuePair<string, string> entry in this.channelname_sid)
            {
                await this.ReSubscribe(entry.Value);

            }
        }


        public bool isEmptyOrSpaces(string str)
        {
            str = str.Trim();
            return string.IsNullOrEmpty(str);
        }


        private bool isNetworkConnected(string name, int valid_type = 0)
        {
            if(!((this.dbcore as dBridges).connectionstate.isconnected))
            {
                switch(valid_type)
                {
                    case 0:
                        throw (new dBError("E024"));
                        
                    case 1:
                        throw (new dBError("E030"));
                        
                    case 2:
                        throw (new dBError("E014"));
                        
                    case 3:
                        throw (new dBError("E019"));
                        
                    case 4:
                        throw (new dBError("E033"));                        
                }
            }
            return true;
        }


        private bool isEmptyORBlank(string name, int valid_type = 0)
        {
            if (isEmptyOrSpaces(name))
            {
                switch (valid_type)
                {
                    case 0:
                        throw (new dBError("E025"));
                        
                    case 1:
                        throw (new dBError("E030"));
                        
                    case 2:
                        throw (new dBError("E016"));
                        
                    case 3:
                        throw (new dBError("E021"));
                        
                    case 4:
                        throw (new dBError("E037"));
                            
                }
            }
            return true;
        }


        private bool validataNameLength(string name, int valid_type = 0)
        {
            if (name.Length > 64)
            {
                switch (valid_type)
                {
                    case 0:
                        throw (new dBError("E027"));
                        
                    case 1:
                        throw (new dBError("E030"));
                        
                    case 2:
                        throw (new dBError("E017"));
                        
                    case 3:
                        throw (new dBError("E022"));
                        
                    case 4:
                        throw (new dBError("E036"));
                        
                }
            }
            return true;
        }


        private bool isvalidSyntex(string name)
        {
            Regex rgx = new Regex("^[a-zA-Z0-9.:_*]+$");
            return rgx.IsMatch(name);
        }
        private bool validateSyntax(string name , int valid_type = 0)
        {
            if (!isvalidSyntex(name))
            {
                switch (valid_type)
                {
                    case 0:
                        throw (new dBError("E028"));

                    case 1:
                        throw (new dBError("E030"));

                    case 2:
                        throw (new dBError("E015"));

                    case 3:
                        throw (new dBError("E023"));

                    case 4:
                        throw (new dBError("E039"));

                }
            }
                return true;
        }

        private bool validatePreDefinedName(string name, int valid_type = 0)
        {

            if (name.Contains(":"))
            {
                string[] sdata = name.ToLower().Split(':');
                if (!(station.channel_type.Contains(sdata[0])))
                {
                    switch (valid_type)
                    {
                        case 0:
                            throw (new dBError("E028"));
                            
                        case 1:
                            throw (new dBError("E030"));
                            
                        case 2:
                            throw (new dBError("E015"));
                            
                        case 3:
                            throw (new dBError("E023"));
                            
                        case 4:
                            throw (new dBError("E039"));
                    }

                } 
            }
            return true;
        }


        private bool isChannelNameExists(string name, int valid_type = 0)
        {
            if (this.channelname_sid.ContainsKey(name))
            {
                switch (valid_type)
                {
                    case 0:
                        throw (new dBError("E025"));

                    case 1:
                        throw (new dBError("E030"));

                    case 2:
                        throw (new dBError("E016"));

                    case 3:
                        throw (new dBError("E021"));

                    case 4:
                        throw (new dBError("E037"));

                }
            }
            return true;
        }


        private void validateName(string name, int valid_type=0)
        {
            try
            {
                this.isNetworkConnected(name, valid_type);
                this.isEmptyORBlank(name, valid_type);
                this.validataNameLength(name, valid_type);
                this.validateSyntax(name, valid_type);
                this.validatePreDefinedName(name, valid_type);
            }
            catch(dBError e)
            {
                throw e;
            }
        }

        public async Task<object> communicate(int mtype, string channelName, bool mprivate)
        {
            bool cStatus = false;
            Dictionary<string, object> m_value;
            string access_token = null;
            string sid = util.GenerateUniqueId();

           cStatus = await util.updatedBNewtworkSC(this.dbcore, MessageType.SERVER_SUBSCRIBE_TO_CHANNEL, channelName, sid, access_token, null,  null , 0 ,  null);
            if (!cStatus)
            {
                if (mtype == 0)
                {
                    throw (new dBError("E024"));
                }
                else
                {
                    throw (new dBError("E024"));
                }
            }

            channel m_channel = new channel(channelName, sid, this.dbcore);
            m_value = new Dictionary<string, object>  {
            { "name", channelName },
            {"type",  "s" },
            {"status",channelState.SUBSCRIPTION_INITIATED },
            {"ino", m_channel } };
            
           
            this.channelsid_registry.Add(sid, m_value);
            this.channelname_sid.Add(channelName, sid);
            return m_channel;
        }


      
        public async Task<channel> subscribe(string channelName)
        {
   
            try
            {
                this.validateName(channelName);
            }
            catch (Exception error)
            {
                throw (error);
            }

            if (this.channelname_sid.ContainsKey(channelName)) throw (new dBError("E029"));


            bool mprivate = this.isPrivateChannel(channelName);


            object m_channel;

            try
            {
                m_channel = await this.communicate(0, channelName, mprivate);
            }
            catch (Exception error)
            {
                throw (error);
            }
            return m_channel as channel;
        }


        

        public async Task unsubscribe(string channelName)
        {

            if (string.IsNullOrEmpty(channelName) || string.IsNullOrWhiteSpace(channelName)) { throw (new dBError("E030")); }

            if (!this.channelname_sid.ContainsKey(channelName)) { throw (new dBError("E030")); }


            string sid = this.channelname_sid[channelName];

            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            bool m_status = false;
            string mtype = m_object["type"] as string;

            if (mtype != "s") { throw (new dBError("E030")); }
            string mstatus = m_object["status"] as string;
            if (mstatus == channelState.UNSUBSCRIBE_INITIATED) { throw (new dBError("E031")); }


            if (mstatus ==channelState.SUBSCRIPTION_ACCEPTED ||
                mstatus ==channelState.SUBSCRIPTION_INITIATED ||
                mstatus ==channelState.SUBSCRIPTION_PENDING ||
                mstatus ==channelState.SUBSCRIPTION_ERROR ||
                mstatus ==channelState.UNSUBSCRIBE_ERROR)
            {
                m_status = await util.updatedBNewtworkSC(this.dbcore, MessageType.UNSUBSCRIBE_DISCONNECT_FROM_CHANNEL, channelName, sid, null);
            }
            if (!m_status) { throw (new dBError("E032")); }

            m_object["status"] =channelState.UNSUBSCRIBE_INITIATED;
            this.channelsid_registry[sid] = m_object;
        }


        


        public async Task handleSubscribeEvents(List<string> eventName, object eventData, Dictionary<string, object> m_object)
        {

            for (int i = 0; i < eventName.Count; i++)
            {
                string mtype = m_object["type"] as string;
                if (mtype == "s")
                {
                    string channelName = (m_object["ino"] as channel).getChannelName();
                    metaData md = new metaData(channelName, eventName[i]);

                    await this.dispatch.emit_channel(eventName[i], eventData, md);
                    await (m_object["ino"] as channel).emit_channel(eventName[i], eventData, md);

                }

            }
        }



        public async Task updateSubscribeStatus(string sid, string status, object reason)
        {
            if (!this.channelsid_registry.ContainsKey(sid)) return;
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;

            string mtype = m_object["type"] as string;

            switch (mtype)
            {
                case "s":
                    switch (status)
                    {
                        case channelState.SUBSCRIPTION_ACCEPTED:
                            m_object["status"] = status;
                            this.channelsid_registry[sid] = m_object;
                            (m_object["ino"] as channel).set_isOnline(true);
                            List<string> events = new List<string> { systemEvents.SUBSCRIBE_SUCCESS, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(events, "", m_object);
                            break;
                        default:
                            m_object["status"] = status;
                            this.channelsid_registry[sid] = m_object;
                            (m_object["ino"] as channel).set_isOnline(false);
                            List<string> eventssd = new List<string> { systemEvents.SUBSCRIBE_FAIL};
                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            this.channelname_sid.Remove(m_object["name"] as string);
                            this.channelsid_registry.Remove(sid);
                            break;
                    }
                    break;
               
                default:
                    break;
            }

        }

        public async Task updateSubscribeStatusRepeat(string sid, string status, object reason)
        {
            if (!this.channelsid_registry.ContainsKey(sid)) return;
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;

            string mtype = m_object["type"] as string;

            switch (mtype)
            {
                case "s":
                    switch (status)
                    {
                        case channelState.SUBSCRIPTION_ACCEPTED:
                            m_object["status"] = status;
                            this.channelsid_registry[sid] = m_object;
                            (m_object["ino"] as channel).set_isOnline(true);
                            List<string> events = new List<string> {systemEvents.RESUBSCRIBE_SUCCESS, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(events, "", m_object);
                            break;
                        default:
                            m_object["status"] = status;
                            this.channelsid_registry[sid] = m_object;
                            (m_object["ino"] as channel).set_isOnline(false);
                            List<string> eventssd = new List<string> { systemEvents.OFFLINE };
                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            break;

                    }
                    break;
                default:
                    break;
            }

        }

        public async Task updateChannelsStatusAddChange(int life_cycle, string sid, string status, object reason)
        {
            if (life_cycle == 0)  
            {
                await this.updateSubscribeStatus(sid, status, reason);
            }
            else
            { 
                await this.updateSubscribeStatusRepeat(sid, status, reason);
            }
        }


        public async Task updateChannelsStatusRemove(string sid, string status, object reason)
        {
            if (!this.channelsid_registry.ContainsKey(sid)) return;
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;

            string mtype = m_object["type"] as string;

            switch (mtype)
            {
                case "s":
                    switch (status)
                    {
                        case channelState.UNSUBSCRIBE_ACCEPTED:
                            m_object["status"] = status;
                            this.channelsid_registry[sid] = m_object;
                            (m_object["ino"] as channel).set_isOnline(false);
                            List<string> events = new List<string> { systemEvents.UNSUBSCRIBE_SUCCESS, systemEvents.REMOVE };
                            await this.handleSubscribeEvents(events, "", m_object);
                            this.channelname_sid.Remove(m_object["name"] as string);
                            this.channelsid_registry.Remove(sid);
                            break;
                        default:
                            m_object["status"] =channelState.SUBSCRIPTION_ACCEPTED;
                            this.channelsid_registry[sid] = m_object;
                            (m_object["ino"] as channel).set_isOnline(true);
                            List<string> eventssd = new List<string> { systemEvents.UNSUBSCRIBE_FAIL, systemEvents.ONLINE };
                            await this.handleSubscribeEvents(eventssd, reason, m_object);
                            break;

                    }
                    break;
                default:
                    break;
            }

        }

        public bool _isonline(string sid)
        {
            if (!this.channelsid_registry.ContainsKey(sid)) return false;
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string mstatus = m_object["status"] as string;
            if (mstatus ==channelState.CONNECTION_ACCEPTED ||
                mstatus ==channelState.SUBSCRIPTION_ACCEPTED) return true;

            return false;
        }

        public bool isOnline(string channelName)
        {
            if (!this.channelname_sid.ContainsKey(channelName)) return false;
            if (!(this.dbcore as dBridges).isSocketConnected()) return false;
            string sid = this.channelname_sid[channelName];
            return this._isonline(sid);
        }

        public List<Dictionary<string, object>> list()
        {
            List<Dictionary<string, object>> m_data = new List<Dictionary<string, object>>();

            foreach (KeyValuePair<string, object> entry in this.channelsid_registry)
            {
                Dictionary<string, object> m_object = entry.Value as Dictionary<string, object>;
                Dictionary<string, object> mtemp = new Dictionary<string, object> { {"name",  m_object["name"] as string } ,
                                                                                    {"type", (m_object["type"] as string == "s")? "subscribe": "connect"  },
                                                                                    {"isonlne", this._isonline(entry.Key) }};
                m_data.Add(mtemp);
            }
            return m_data;
        }

        public async Task  send_OfflineEvents()
        {
            
            foreach (KeyValuePair<string, object> entry in this.channelsid_registry)
            {
                Dictionary<string, object> m_object = entry.Value as Dictionary<string, object>;
                metaData md = new metaData(m_object["name"] as string, systemEvents.OFFLINE);
                await this.handledispatcherEvents(systemEvents.OFFLINE, "", m_object["name"] as string, md);

              
            }

        }

        public string get_subscribeStatus(string sid)
        {
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string mstatus = m_object["status"] as string;
            return mstatus;
        }


        public string get_channelType(string sid)
        {
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string ntype = m_object["type"] as string;
            return ntype;
        }


        public string get_channelName(string sid)
        {
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string name = m_object["name"] as string;
            return name;
        }


        public string getConnectStatus(string sid)
        {
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string mstatus = m_object["status"] as string;
            return mstatus;
        }


        public object getChannel(string sid)
        {
            if (!this.channelsid_registry.ContainsKey(sid)) return null;
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            object mobject = m_object["ino"];
            return mobject;
        }

        public string getChannelName(string sid)
        {
            if (!this.channelsid_registry.ContainsKey(sid)) return null;
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string name = m_object["name"] as string;
            return name;
        }

        public bool isSubscribedChannel(string sid)
        {
            bool mflag = false;
            if (!this.channelsid_registry.ContainsKey(sid)) return mflag;
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string ntype = m_object["type"] as string;
            if (ntype == "s")
            {
                mflag = true;
            }
            return mflag;
        }


        public void clean_channel(string sid)
        {
            Dictionary<string, object> m_object = this.channelsid_registry[sid] as Dictionary<string, object>;
            string ntype = m_object["type"] as string;
            if (ntype == "s"){
                channel cn = m_object["ino"] as channel;
                cn.unbind();
                cn.unbind_all();

            }
        }


        public async Task cleanUp_All()
        {
            foreach (KeyValuePair<string, string> entry in this.channelname_sid)
            {
                metaData md = new metaData(entry.Key as string, systemEvents.REMOVE);

                await this.handledispatcherEvents(systemEvents.REMOVE, "", entry.Key,  md);
                clean_channel(entry.Value);
            }
            this.channelname_sid.Clear();
            this.channelsid_registry.Clear();
            //this.dispatch.unbind();
            //this.dispatch.unbind_all();
        }





        public async Task publish(string channelName, string eventName, string eventData, string exclude_session_id = null, string source_id = null, string seqnum = null)
        {


            if (channelName.ToLower() == "sys:*") throw (new dBError("E015"));


            try
            {
                this.validateName(channelName,2);
            }
            catch (dBError error)
            {
                throw (error);
            }

            if (string.IsNullOrEmpty(eventName) || string.IsNullOrWhiteSpace(eventName)) throw new dBError("E059");


            bool m_status = await util.updatedBNewtworkSC(this.dbcore, MessageType.SERVER_PUBLISH_TO_CHANNEL, channelName, (exclude_session_id ==  null)?null:exclude_session_id ,  eventData, eventName , source_id,  0, seqnum);


            if (!m_status) throw (new dBError("E014"));

            return;

        }




        public async Task sendmsg(string channelName, string eventName, string eventData, string to_session_id, string source_id= null, string seqnum= null)
        {


            if (channelName.ToLower() == "sys:*") throw (new dBError("E020"));

            try
            {
                this.validateName(channelName, 3);
    
            }
            catch (dBError error)
            {
                throw (error);
            }

            if (channelName.ToLower().StartsWith("prs:"))
            {
                if (string.IsNullOrEmpty(source_id) || string.IsNullOrWhiteSpace(source_id)) throw (new dBError("E020"));
            }

            bool m_status = await util.updatedBNewtworkSC(this.dbcore, MessageType.SERVER_CHANNEL_SENDMSG, channelName, (to_session_id ==null)?null:to_session_id ,  eventData, eventName , source_id, 0, seqnum);
            if (!m_status) throw (new dBError("E019"));
            return;
        }



        public async Task<IPromise<object>> call(string channelName, string functionName, string inparameter, UInt64 ttlms, Action<object> progress_callback)
        {
            var promise = new Promise<object>();
            try
            {
                this.validateName(channelName, 4);

            }catch (dBError error)
            {
                promise.Reject(error);
            }

            if (!station.list_of_supported_functionname.Contains(functionName))
            {
                promise.Reject(new dBError("E038"));
            }


            if(channelName.ToLower().StartsWith("prs:") || channelName.ToLower().StartsWith("sys:"))
            {
                CrpCaller caller = (this.dbcore as dBridges).rpc.ChannelCall(channelName);

                IPromise<object> p = await caller.call(functionName, inparameter, ttlms, progress_callback);
                     p.Then((result) =>
                     {
                         //(this.dbcore as dBridges).rpc.ClearChannel(channelName);
                         promise.Resolve(result);
                     })
                    .Catch((exec) => 
                    {
                        //(this.dbcore as dBridges).rpc.ClearChannel(channelName);
                        promise.Reject(exec); 
                    });

            }
            else
            {
                promise.Reject(new dBError("E039"));
            }
            return promise;
        }


        }

    }
