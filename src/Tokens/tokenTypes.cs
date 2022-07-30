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

namespace dBridges.Tokens
{
    public static class tokenTypes
    {
        public static readonly String CHANNELSUBSCRIBE = "channel.subscribe";
        public static readonly String CHANNELCONNECT = "channel.connect";
        public static readonly String RPCCONNECT = "rpc.connect";
        public static readonly String RPCREGISTER = "rpc.register";
        public static readonly String SYSTEM_CHANNELSUBSCRIBE = "system_channel.subscribe";

    }
}
