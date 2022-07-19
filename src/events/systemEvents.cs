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
    public static class systemEvents
    {
        public const String SUBSCRIBE_SUCCESS = "dbridges:subscribe.success";
        public const String SUBSCRIBE_FAIL = "dbridges:subscribe.fail";
        public const String ONLINE = "dbridges:channel.online";
        public const String OFFLINE = "dbridges:channel.offline";
        public const String REMOVE = "dbridges:channel.removed";
        public const String UNSUBSCRIBE_SUCCESS = "dbridges:unsubscribe.success";
        public const String UNSUBSCRIBE_FAIL = "dbridges:unsubscribe.fail";
        public const String CONNECT_SUCCESS = "dbridges:connect.success";
        public const String CONNECT_FAIL = "dbridges:connect.fail";
        public const String DISCONNECT_SUCCESS = "dbridges:disconnect.success";
        public const String DISCONNECT_FAIL = "dbridges:disconnect.fail";
        public const String RESUBSCRIBE_SUCCESS = "dbridges:resubscribe.success";
        public const String RESUBSCRIBE_FAIL = "dbridges:resubscribe.fail";
        public const String RECONNECT_SUCCESS = "dbridges:reconnect.success";
        public const String RECONNECT_FAIL = "dbridges:reconnect.fail";
        public const String PARTICIPANT_JOINED = "dbridges:participant.joined";
        public const String PARTICIPANT_LEFT = "dbridges:participant.left";

        public const String REGISTRATION_SUCCESS = "dbridges:rpc.server.registration.success";
        public const String REGISTRATION_FAIL = "dbridges:rpc.server.registration.fail";
        public const String SERVER_ONLINE = "dbridges:rpc.server.online";
        public const String SERVER_OFFLINE = "dbridges:rpc.server.offline";
        public const String UNREGISTRATION_SUCCESS = "dbridges:rpc.server.unregistration.success";
        public const String UNREGISTRATION_FAIL = "dbridges:rpc.server.unregistration.fail";
        public const String RPC_CONNECT_SUCCESS = "dbridges:rpc.server.connect.success";
        public const String RPC_CONNECT_FAIL = "dbridges:rpc.server.connect.fail";

        //public const String RPC_RECONNECT_SUCCESS = "dbridges:server.reconnect.success";
        //public const String RPC_RECONNECT_FAIL = "dbridges:server.reconnect.fail";

    }
}
