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

namespace dBridges.events
{
    public static class channelConnectEvent
    {
        public static readonly List<String> channelEvents =  new List<string>() {systemEvents.CONNECT_SUCCESS,
                          systemEvents.CONNECT_FAIL,
                          systemEvents.RECONNECT_SUCCESS,
                          systemEvents.RECONNECT_FAIL,
                          systemEvents.DISCONNECT_SUCCESS,
                          systemEvents.DISCONNECT_FAIL,
                          systemEvents.ONLINE,
                          systemEvents.OFFLINE,
                          systemEvents.REMOVE,
                          systemEvents.PARTICIPANT_JOINED,
                          systemEvents.PARTICIPANT_LEFT };
    }
}
