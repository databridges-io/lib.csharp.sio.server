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
    public static class states
    {
        public static readonly string CONNECTED = "connected";
        public static readonly string ERROR = "connect_error";
        public static readonly string DISCONNECTED = "disconnected";
        public static readonly string RECONNECTING = "reconnecting";
        public static readonly string CONNECTING = "connecting";
        public static readonly string STATE_CHANGE = "state_change";
        public static readonly string RECONNECT_ERROR = "reconnect_error";
        public static readonly string RECONNECT_FAILED = "reconnect_failed";
        public static readonly string RECONNECTED = "reconnected";
        public static readonly string CONNECTION_BREAK = "connection_break";

        public static readonly string RTTPONG = "rttpong";
        public static readonly string RTTPING = "rttping";

        public static readonly string[] supportedEvents = {"connect_error", "connected", "disconnected",
                                                                "reconnecting", "connecting", "state_change",
                                                                "reconnect_error", "reconnect_failed", "reconnected",
                                                                "connection_break", "rttpong"};

    }
}
