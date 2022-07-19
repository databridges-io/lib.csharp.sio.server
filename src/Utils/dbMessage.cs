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
using dBridges.Messages;

namespace dBridges.Utils
{
    class dbMessage
    {
        public int dbmsgtype { get; set; }
        public MessageType dbType { get; set; }
        public string subject { get; set; }
        public string rsub { get; set; }
        public string sid { get; set; }
        public string payload { get; set; }
        public string fenceid { get; set; }
        public bool rspend { get; set; }
        public bool rtrack { get; set; }
        public string rtrackstat { get; set; }
        public long t1 { get; set; }
        public long latency { get; set; }
        public int globmatch { get; set; }
        public string sourceid { get; set; }
        public string sourceip { get; set; }
        public bool replylatency { get; set; }
        public string oqueumonitorid { get; set; }
    }
}
