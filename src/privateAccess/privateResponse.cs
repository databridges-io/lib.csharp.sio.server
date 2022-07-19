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
using dBridges.Utils;

namespace dBridges.privateAccess
{
    class privateResponse
    {
        private string name;
        private object  rcCore;
        private string m_type;
        private string sid;

        public privateResponse(string m_type, string name, string sid, object rcCore)
        {
            this.name = name;
            this.rcCore = rcCore;
            this.m_type = m_type;
            this.sid = sid;
        }

        public privateResponse(string name, string sid, string rcCore)
        {
            this.name = name;
            this.rcCore = rcCore;
            this.m_type = "";
            this.sid = sid;
        }
    
    }
}
