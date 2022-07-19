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


    public class dBExtraData
    {
        public string channelName { get; set; }

        public string sessionid { get; set; }
        public string libtype { get; set; }
        public string sourceipv4 { get; set; }
        public string sourceipv6 { get; set; }
        public string info { get; set; }
        public string sourcesysid { get; set; }
        

        public dBExtraData()
        {
            this.sessionid = "";
            this.libtype = "";
            this.sourceipv4 = "";
            this.sourceipv6 = "";
            this.info = "";
            this.sourcesysid = "";
            this.channelName = "";
           // this.sysinfo = new CSysInfo("");
        }

        public dBExtraData(string sessionid, string libtype, string sourceipv4, string sourceipv6, string msourceid, string sourcesysid)
        {
            this.sessionid = sessionid;
            this.libtype = libtype;
            this.sourceipv4 = sourceipv4;
            this.sourceipv6 = sourceipv6;
            this.info = msourceid;
            this.sourcesysid = sourcesysid;
            this.channelName = "";
        
        }


        public void dBUpdateExtraData(string sessionid, string libtype, string sourceipv4, string sourceipv6, string msourceid, string sourcesysid)
        {
            this.sessionid = sessionid;
            this.libtype = libtype;
            this.sourceipv4 = sourceipv4;
            this.sourceipv6 = sourceipv6;
            this.info = msourceid;
            this.sourcesysid = sourcesysid;
            this.channelName = "";
        
        }


    public override String ToString()
        {
            return new StringBuilder()
                    
                    .Append("{ sessionid: ").Append(this.sessionid)
                    .Append(", libtype: ").Append(this.libtype)
                    .Append(", sourceipv4: ").Append(this.sourceipv4)
                    .Append(", sourceipv6: ").Append(this.sourceipv6)
                    .Append(", info: ").Append(this.info)
                    .Append(" }").ToString();

        }



    }
}
