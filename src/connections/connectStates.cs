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
using dBridges.events;
using dBridges.exceptions;


namespace dBridges.connections
{
    public class connectStates
    {
        public string state;
        public bool isconnected;
        
        private Action_dispatcher registry;
        private bool newLifeCycle;
        public int reconnect_attempt;
        private object dbcore;
        public UInt64 rttms;
        private static string[] no_changelist = { "reconnect_attempt", "rttpong", "rttping" };


    public connectStates(object dBCoreObject)
        {
            this.state = "";
            this.isconnected = false;
            this.registry = new Action_dispatcher();
            this.newLifeCycle = true;
            this.reconnect_attempt = 0;
            this.dbcore = dBCoreObject;
            this.rttms = 0;
        }


        public async Task  rttping(string payload="")
        {
            long t1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            bool m_status = await util.updatedBNewtworkSC(this.dbcore, MessageType.SYSTEM_MSG, null, null, payload,
                                           "rttping", null, (ulong)t1);
            if (!m_status)
            {
                throw new dBError("E011");
            }
        }


        public void set_newLifeCycle(bool value)
        {
            this.newLifeCycle = value;
	    }

        public bool get_newLifeCycle()
        {
            return this.newLifeCycle;
	    }

        public void bind(string eventName , Delegate callback)
        {
            
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrWhiteSpace(eventName)  ) {
                throw (new dBError("E012"));
            }

            if(callback == null)
            {
                throw (new dBError("E013"));
            }

            if(!states.supportedEvents.Contains(eventName))
            {
                 throw (new dBError("E013"));
            }
            
            this.registry.bind(eventName , callback);
	}


    public void unbind()
    {
        this.registry.unbind(); 
	}

    public void unbind(string eventName, Delegate callback = null)
    {
        this.registry.unbind(eventName, callback);
    }


        public void updatestates(string eventName)
        {

            if ( (eventName == states.CONNECTED) ||
                 (eventName == states.RECONNECTED) ||
                 (eventName == states.RTTPONG) ||
                 (eventName == states.RTTPING) )
            {
                this.isconnected = true;
            }
            else
            {
                this.isconnected = false;
            }

        }


    public async Task handledispatcher(string eventName , object eventInfo)
    {
            string previous = this.state;

            if (!connectStates.no_changelist.Contains(eventName))
            {
                this.state = eventName;
            }

            this.updatestates(eventName);
            if (eventName != previous)
            {
                if (!connectStates.no_changelist.Contains(eventName))
                {
                    if (!connectStates.no_changelist.Contains(previous))
                    {
                        stateChange sc = new stateChange(previous, eventName);
                        this.state = eventName;
                        if (eventName == states.DISCONNECTED)
                        {
                            this.state = "";
                        }
                        await this.registry.emit_connectionState (states.STATE_CHANGE, sc);
                    }
                }
            }

            if (eventInfo != null) {
                await this.registry.emit_connectionState(eventName, eventInfo);
            }
            else
            {
                await this.registry.emit_connectionState(eventName);
            }
            if (eventName == "reconnected") this.state = "connected";
    }



    }
};
