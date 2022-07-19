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

namespace dBridges.channel
{
   public static  class channelState
    {
         public const string  SUBSCRIPTION_INITIATED = "subscription_initiated";
         public const string  SUBSCRIPTION_PENDING = "subscription_pending";
         public const string SUBSCRIPTION_ACCEPTED = "subscription_accepted";
         public const string SUBSCRIPTION_ERROR = "subscription_error";

         public const string CONNECTION_INITIATED = "connection_initiated";
         public const string CONNECTION_PENDING = "connection_pending";
         public const string CONNECTION_ACCEPTED = "connection_accepted";
         public const string CONNECTION_ERROR = "connection_error";

         public const string UNSUBSCRIBE_INITIATED = "unsubscribe_initiated";
         public const string UNSUBSCRIBE_ACCEPTED = "unsubscribe_accepted";
         public const string UNSUBSCRIBE_ERROR = "unsubscribe_error";

         public const string DISCONNECT_INITIATED = "disconnect_initiated";
         public const string DISCONNECT_ACCEPTED = "disconnect_accepted";
         public const string DISCONNECT_ERROR = "disconnect_error";

    }
}
