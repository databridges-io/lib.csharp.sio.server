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

namespace dBridges.Utils
{
   public class metaData
    {

        public string channelName { get; set; }
        public string  eventName { get; }
        public string sourcesysid { get; set; }
        public string  sqnum { get; }
        public string sessionid { get; set; }
        public long intime { get; }
        public bool  isError { get; }



        public metaData()
        {
            this.channelName = "";
            this.eventName = "";
            this.sourcesysid = null;
            this.sqnum = null;
            this.sessionid = null;
            this.intime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this.isError = false;
        }


        public metaData(string c, string e)
        {
            this.channelName = c;
            this.eventName = e;
            this.sourcesysid = null;
            this.sqnum = null;
            this.sessionid = null;
            this.intime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this.isError = false;
        }

        public metaData(string c, string e, string s, string sq, string sid, long it)
        {
            this.channelName = c;
            this.eventName = e;
            this.sourcesysid = s;
            this.sqnum = sq;
            this.sessionid = sid;
            this.intime = it;
        }


        
    public override String ToString()
        {
            return new StringBuilder()
                    .Append("{channelName:").Append(this.channelName)
                    .Append(", eventName:").Append(this.eventName)
                    .Append(", sourceid:").Append(this.sourcesysid)
                    .Append(", sqnum:").Append(this.sqnum)
                    .Append(", sessionid:").Append(this.sessionid)
                    .Append(", intime:").Append(this.intime)
                    .Append("}").ToString();
        }
    }
}
