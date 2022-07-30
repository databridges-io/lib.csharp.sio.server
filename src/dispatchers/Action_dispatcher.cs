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
using System.Collections.Concurrent;
using dBridges.exceptions;
using dBridges.Utils;

namespace dBridges.dispatchers
{
   public class Action_dispatcher
    {
        private ConcurrentDictionary<string, List<Delegate>> c_local_register;
        private List<Delegate> global_register;


        public Action_dispatcher()
        {
            this.c_local_register = new ConcurrentDictionary<string, List<Delegate>>();
            this.global_register = new List<Delegate>();
        }



        public bool isExists(string eventName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrWhiteSpace(eventName)) return false;
            return this.c_local_register.ContainsKey(eventName);
        }

        public void bind(string eventName, Delegate callback)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrWhiteSpace(eventName)) { throw new dBError("E012"); }
            if (callback == null) { throw new dBError("E013"); }
            if (!this.c_local_register.ContainsKey(eventName))
            {
                try
                {
                    List<Delegate> t = new List<Delegate>();
                    t.Add(callback);
                    this.c_local_register.TryAdd(eventName, t);

                }
                catch (Exception exp)
                {
                    Console.WriteLine("dispatcher:: Exception :" + exp.Message + "event name :" + eventName);
                }
            }
            else
            {

                try
                {
                    List<Delegate> tout;
                    if (this.c_local_register.TryGetValue(eventName, out tout))
                        tout.Add(callback);
                }
                catch (Exception exp)
                {
                    Console.WriteLine("dispatcher:: Exception :" + exp.Message + "event name :" + eventName);
                }
            }

        }


        public void bind_all(Delegate callback)
        {
            if (callback == null) { throw new dBError("E013"); }
            this.global_register.Add(callback);
        }

        public void unbind()
        {

            this.c_local_register.Clear();
        }

        public void unbind(string eventname, Delegate callback = null)
        {
            List<Delegate> v_value;
            bool is_removed = false;
            if (this.c_local_register.ContainsKey(eventname))
            {
                if (callback == null)
                {
                    try
                    {
                        is_removed = this.c_local_register.TryRemove(eventname, out v_value);
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine("dispatcher:: Exception :" + exp.Message + "event name :" + eventname);
                    }
                }
                else
                {
                    try
                    {
                        List<Delegate> tout;
                        if (this.c_local_register.TryGetValue(eventname, out tout))
                            tout.Remove(callback);

                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine("dispatcher:: Exception :" + exp.Message + "event name :" + eventname);
                    }
                }
            }
        }


        public void unbind_all(Delegate callback = null)
        {
            if (callback == null)
            {
                this.global_register.Clear();
            }
            else
            {
                this.global_register.Remove(callback);
            }
        }


        public async Task emit_connectionState(string eventname, object info = null)
        {
            if (this.c_local_register.ContainsKey(eventname))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(eventname, out tout))
                {
                    foreach (var callback in tout)
                    {
                        try
                        {
                            if (info == null)
                            {
                                Action<object> cb = callback as Action<object>;
                                await Task.Factory.StartNew(() => cb(eventname)).ConfigureAwait(false);
                            }
                            else
                            {
                                if (info == null) { info = eventname; }

                                Action<object> cb = callback as Action<object>;
                                await Task.Factory.StartNew(() => cb(info)).ConfigureAwait(false);
                            }
                        }
                        catch (Exception e) { Console.WriteLine(e); }
                    }
                }
            }
        }




        public async Task emit_channel(string eventname, object payload = null, object metadata = null)
        {
            foreach (var callback in this.global_register)
            {
                try
                {

                    Action<object, object> cb = callback as Action<object, object>;
                    await Task.Factory.StartNew(() => cb(payload, metadata)).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (this.c_local_register.ContainsKey(eventname))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(eventname, out tout))
                {
                    foreach (var callback in tout)
                    {
                        try
                        {
                            Action<object, object> cb = callback as Action<object, object>;
                            await Task.Factory.StartNew(() => cb(payload, metadata)).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }
        }




        public async Task emit_publish(string eventname, object payload = null, object metadata = null)
        {
            foreach (var callback in this.global_register)
            {
                try
                {

                    Action<object, object> cb = callback as Action<object, object>;
                    await Task.Factory.StartNew(() => cb(payload, metadata)).ConfigureAwait(false);
                }catch (Exception){

                }
            }

            if (this.c_local_register.ContainsKey(eventname))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(eventname, out tout))
                {
                    foreach (var callback in tout)
                    {
                        try
                        {
                            Action<object, object> cb = callback as Action<object, object>;
                            await Task.Factory.StartNew(() => cb(payload, metadata)).ConfigureAwait(false);
                        }catch (Exception){
                      
                        }
                    }
                }
            }
        }




        public async Task emit_clientfunction(string functionName, object inparameter, object response = null, object rsub = null)
        {
            if (this.c_local_register.ContainsKey(functionName))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(functionName, out tout))
                {
                    foreach (var callback in tout)
                    {
                        if (callback != null)
                        {
                            try
                            {
                                Action<object, object, object> cb = callback as Action<object, object, object>;
                                await Task.Factory.StartNew(() => cb(inparameter, response, rsub)).ConfigureAwait(false);
                            }
                            catch (Exception e){
                                Console.WriteLine(e);
                            }
                        }
                    }
                }
            }
        }


        public async Task emit_clientfunction(string functionName, object inparameter, object response)
        {
            //await Task.Delay(1);
            if (this.c_local_register.ContainsKey(functionName))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(functionName, out tout))
                {
                    foreach (var callback in tout)
                    {
                        try
                        {
                            Action<object, object> cb = callback as Action<object, object>;
                            await Task.Factory.StartNew(() => cb(inparameter, response)).ConfigureAwait(false);
                        }catch (Exception){
                        
                        }
                    }
                }
            }

        }





        public async Task emit(string eventname, object EventInfo = null, object channelName = null, object metadata = null)
        {
            

            foreach (var callback in this.global_register)
            {
                try
                {
                    Action<object, object, object> cb = callback as Action<object, object, object>;
                    await Task.Factory.StartNew(() => cb(channelName, EventInfo, metadata)).ConfigureAwait(false);
                }catch (Exception){
                }
            }

            if (this.c_local_register.ContainsKey(eventname))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(eventname, out tout))
                {
                    foreach (var callback in tout)
                    {
                        try
                        {
                            Action<object, object, object> cb = callback as Action<object, object, object>;
                            await Task.Factory.StartNew(() => cb(channelName, EventInfo, metadata)).ConfigureAwait(false);
                        } catch (Exception){
                            
                        }
                    }
                }
            }
        }

        public async Task emit_cf(string functionName, object inparameter = null, object response = null)
        {
            if (this.c_local_register.ContainsKey(functionName))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(functionName, out tout))
                {

                    foreach (var callback in tout)
                    {
                        try
                        {
                            Action<object, object> cb = callback as Action<object, object>;
                            await Task.Factory.StartNew(() => cb(inparameter, response)).ConfigureAwait(false);
                        }
                        catch (Exception){
                        }
                    }
                }
            }

        }

        public async Task emit2(string eventname, object channelname, object sessionid, object action, object response)
        {
            if (this.c_local_register.ContainsKey(eventname))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(eventname, out tout))
                {

                    foreach (var callback in tout)
                    {
                        try
                        {
                            Action<object, object, object, object> cb = callback as Action<object, object, object, object>;
                            await Task.Factory.StartNew(() => cb(channelname, sessionid, action, response)).ConfigureAwait(false);
                        }
                        catch (Exception){
                        }
                    }
                }
            }

        }
    }
}

