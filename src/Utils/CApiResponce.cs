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
    class CApiResponce
    {
        public bool secured { get; set; }
        public string wsip { get; set; }
        public string wsport { get; set; }
        public string sessionkey { get; set; }
        public int statuscode { get; set; }
        public string reasonphrase { get; set; }


        public CApiResponce()
        {
            this.secured = false;
            this.wsip = "";
            this.wsport = "";
            this.sessionkey = "";
            this.statuscode = 0;
            this.reasonphrase = "";
        }

        public void update(int statuscode, string reasonphrase)
        {
            this.statuscode = statuscode;
            this.reasonphrase = reasonphrase;
        }
    }
}
