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

namespace dBridges.Utils
{
   public class CrpcMetaData
    {
        public string serverName { get; set; }
        public string eventName { get; }
       


        public CrpcMetaData()
        {
            this.serverName = "";
            this.eventName = "";
            
        }


        public CrpcMetaData(string c, string e)
        {
            this.serverName = c;
            this.eventName = e;
            
        }

        public CrpcMetaData(string c, string e, string s, string sq, string sid, long it)
        {
            this.serverName = c;
            this.eventName = e;
            
        }



        public override String ToString()
        {
            return new StringBuilder()
                    .Append("{serverName:").Append(this.serverName)
                    .Append(", eventName:").Append(this.eventName)
                    .Append("}").ToString();
        }
    }
}
