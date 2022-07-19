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

namespace dBridges.remoteprocedure
{
    class rpcState
    {
        public const string REGISTRATION_INITIATED = "registration_initiated";
        public const string REGISTRATION_PENDING = "registration_pending";
        public const string REGISTRATION_ACCEPTED = "registration_accepted";
        public const string REGISTRATION_ERROR = "registration_error";

        public const string RPC_CONNECTION_INITIATED = "rpc_connection_initiated";
        public const string RPC_CONNECTION_PENDING = "rpc_connection_pending";
        public const string RPC_CONNECTION_ACCEPTED = "rpc_connection_accepted";
        public const string RPC_CONNECTION_ERROR = "rpc_connection_error";

        public const string UNREGISTRATION_INITIATED = "unregister_initiated";
        public const string UNREGISTRATION_ACCEPTED = "unregister_accepted";
        public const string UNREGISTRATION_ERROR = "unregister_error";

        public const string RPC_DISCONNECT_INITIATED = "rpc_disconnect_initiated";
        public const string RPC_DISCONNECT_ACCEPTED = "rpc_disconnect_accepted";
        public const string RPC_DISCONNECT_ERROR = "rpc_disconnect_error";

    }
}
