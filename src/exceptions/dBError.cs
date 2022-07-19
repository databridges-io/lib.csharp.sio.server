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

namespace dBridges.exceptions
{
    public class dBError : Exception
    {
        public string code;
        public string source;
        private string _ekey;
        public string message;
       
        private void updateClassProperty(string ekey, string s, string c, string m)
        {
            this._ekey = ekey;
            this.source = s;
            this.code = c;
            this.message = m;

        }

        public dBError(string ekey, string codeext = "", string message = "") : base(message)
        {
            this.updateClassProperty("", "", "", "");

            if (!errorMessage.Lookup.ContainsKey(ekey))
            {
                this.updateClassProperty(ekey, "DBLIB_EXCEPTION", ekey+ "_undefined", "");
                return;
            }
            
             int[] value = errorMessage.Lookup[ekey];
            if(value.Length != 2)
            {
                this.updateClassProperty(ekey, "DBLIB_EXCEPTION", ekey + "_undefined", "");
                return;
            }


            if (!sourceMessage.sourceLookup.ContainsKey(value[0])){
                this.updateClassProperty(ekey, "DBLIB_EXCEPTION", ekey + "_undefined", "");
                return;
            }

            if (!codeMessage.codeLookup.ContainsKey(value[1]))
            {
                this.updateClassProperty(ekey, "DBLIB_EXCEPTION", ekey + "_undefined", "");
                return;
            }

            this.updateClassProperty(ekey, sourceMessage.sourceLookup[value[0]], codeMessage.codeLookup[value[1]] , "");


            if (!(string.IsNullOrEmpty(codeext) || string.IsNullOrWhiteSpace(codeext)))
            {
                if (this.code.EndsWith("_"))
                {
                    this.code = this.code + codeext;
                }
                else
                {
                    this.code = this.code + "_" + codeext;
                }
            }


            if(!(string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)))
            {
                this.message = message;
            }

        }

        public void updateCode(string code, string message = "") {
         
            if (!(string.IsNullOrEmpty(code) || string.IsNullOrWhiteSpace(code)))
            {
                if (string.IsNullOrEmpty(this.code))
                {
                    this.code = code;
                }
                else
                {
                    if (!this.code.EndsWith("_"))
                    {
                        this.code = this.code + "_" + code;
                    }
                    else
                    {
                        this.code = code;
                    }
                }
            }
         

            if (!(string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)))
            {
                this.message = message;
            }
        }

        public string GetEKEY()
        {
            return this._ekey;
        }


        public override String ToString()
        {
            return new StringBuilder()
                    .Append("{source:").Append(this.source)
                    .Append(", code:").Append(this.code)
                    .Append(", message:").Append(this.message)
                    .Append("}").ToString();
        }

    }
}
