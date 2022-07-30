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
    public static class errorMessage
    {
        //version:  20220419
        public static readonly Dictionary<string, int[]> Lookup = new Dictionary<string, int[]>(){
            {"E001", new int [] {1,1}},
            {"E002", new int [] {1,2}},
            {"E003", new int [] {1,2}},
            {"E005", new int [] {1,12}},
            {"E006", new int [] {1,5}},
            {"E008", new int [] {1,5}},
            {"E009", new int [] {1,7}},
            {"E010", new int [] {1,7}},
            {"E011", new int [] {4,8}},
            {"E012", new int [] {5,9}},
            {"E013", new int [] {5,10}},
            {"E014", new int [] {6,8}},
            {"E015", new int [] {6,11}},
            {"E016", new int [] {6,11}},
            {"E017", new int [] {6,14}},
            {"E019", new int [] {19,8}},
            {"E020", new int [] {19,11}},
            {"E021", new int [] {19,11}},
            {"E022", new int [] {19,14}},
            {"E023", new int [] {19,11}},
            {"E024", new int [] {11,8}},
            {"E025", new int [] {11,11}},
            {"E026", new int [] {11,11}},
            {"E027", new int [] {11,14}},
            {"E028", new int [] {11,11}},
            {"E029", new int [] {11,15}},
            {"E030", new int [] {12,16}},
            {"E031", new int [] {12,17}},
            {"E032", new int [] {12,8}},
            {"E033", new int [] {20,8}},
            {"E035", new int [] {20,11}},
            {"E036", new int [] {20,14}},
            {"E037", new int [] {20,11}},
            {"E038", new int [] {20,24}},
            {"E039", new int [] {20,11}},
            {"E040", new int [] {3,3}},
            {"E041", new int [] {10,13}},
            {"E042", new int [] {20,19}},
            {"E043", new int [] {25,18}},
            {"E044", new int [] {25,18}},
            {"E045", new int [] {25,18}},
            {"E046", new int [] {25,18}},
            {"E047", new int [] {26,8}},
            {"E048", new int [] {21,18}},
            {"E049", new int [] {21,18}},
            {"E051", new int [] {21,18}},
            {"E052", new int [] {21,18}},
            {"E053", new int [] {21,8}},
            {"E054", new int [] {15,3}},
            {"E055", new int [] {22,3}},
            {"E058", new int [] {6,20}},
            {"E059", new int [] {6,20}},
            {"E060", new int [] {1,21}},
            {"E061", new int [] {24,22}},
            {"E062", new int [] {24,23}},
            {"E063", new int [] {1,8}},
            {"E064", new int [] {7,3}},
            {"E065", new int [] {14,3}},
            {"E066", new int [] {16,9}},
            {"E067", new int [] {16,10}},
            {"E068", new int [] {27,8}},
            {"E069", new int [] {27,25}},
            {"E070", new int [] {13,3}},
            {"E071", new int [] {2,3}},
            {"E072", new int [] {26,26}},
            {"E073", new int [] {26,26}},
            {"E074", new int [] {17,9}},
            {"E075", new int [] {17,10}},
            {"E076", new int [] {18,9}},
            {"E077", new int [] {18,10}},
            {"E079", new int [] {15,8}},
            {"E080", new int [] {15,25}},
            {"E081", new int [] {8,3}},
            {"E082", new int [] {9,3}},
            {"E105", new int [] {27,37}},
            {"E106", new int [] {23,37}},
            {"E107", new int [] {27,38}},
            {"E108", new int [] {23,38}},
            {"E109", new int [] {20,38}},
            {"E110", new int [] {32,39}},
            {"E111", new int [] {32,10}},
            {"E112", new int [] {33,39}},
            {"E113", new int [] {33,10}}        
        };
    }
}
