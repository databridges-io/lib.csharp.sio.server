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
    public static class util
    {
        public static Random generator = new Random();
        private static readonly object _GenerateUniqueId_Lock = new object();

        public static string GenerateUniqueId()
     {
            string r = "";
            lock (_GenerateUniqueId_Lock)
            {
                 r = generator.Next().ToString();
            }

            return r;
     }


    public  static async Task<bool> updatedBNewtworkSC(object dbcore, Messages.MessageType dbmsgtype, string channelName, string sid, string channelToken, string subject= null, string source_id= null, UInt64 t1= 0, string seqnum= null)
    {
            
            byte[] mpayload;

            if (!string.IsNullOrEmpty(channelToken)){
                mpayload = Encoding.UTF8.GetBytes(channelToken);
            }else {
                mpayload = Encoding.UTF8.GetBytes("");
            }

            object [] msgDB = new object[]
                    { dbmsgtype, subject, null ,  sid , mpayload ,  channelName,  null , null, null, t1 , null , 0 , source_id, null, null,seqnum  };

          
            bool asyncStates = await  (dbcore as dBridges).send(msgDB);
            return asyncStates;

        }

                                                                                            
    public static async Task<bool> updatedBNewtworkCF(object dbcore, Messages.MessageType dbmsgtype, string sessionid ,string functionName, string returnSubject, string sid, string payload, bool rspend, bool rtrack)
        {
            byte[] mpayload;

            if (!string.IsNullOrEmpty(payload)){
                mpayload = Encoding.UTF8.GetBytes(payload);
            }
            else
            {
                mpayload = Encoding.UTF8.GetBytes("");
            }

            object nsid = string.IsNullOrEmpty(sid) ? null : sid;
            if(sid == "0")
            {
                nsid = 0;
            }


            object[] msgDB;

                msgDB = new object[]
                    { dbmsgtype, string.IsNullOrEmpty(functionName)?null:functionName, string.IsNullOrEmpty(returnSubject)?null:returnSubject ,nsid , mpayload ,  sessionid,  rspend , rtrack, null, null , null , 0 , null, null, null,null  };


            bool asyncStates =  await (dbcore as dBridges).send(msgDB);

            
            return asyncStates;

        }


        public static string DictionaryToString(Dictionary<string, string> dictionary)
        {
            string dictionaryString = "{";
            foreach (KeyValuePair<string, string> keyValues in dictionary)
            {
                string mkey = "\"" + keyValues.Key + "\"";
                string mvalue = "\"" + keyValues.Value + "\"";
                dictionaryString += mkey + " : " + mvalue + ", ";
            }
            return dictionaryString.TrimEnd(',', ' ') + "}";
        }
    }
}
