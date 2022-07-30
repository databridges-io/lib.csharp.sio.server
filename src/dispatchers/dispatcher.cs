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
using dBridges.exceptions;
using dBridges.Utils;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace dBridges.dispatchers
{
    public class dispatcher
    {
       

        private ConcurrentDictionary<string , List<Delegate>> c_local_register;
        private List<Delegate> global_register;

        public dispatcher()
        {
       

            this.c_local_register = new ConcurrentDictionary<string, List<Delegate>>();
            this.global_register = new List<Delegate>();
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

                }catch(Exception)
                {
                   
                }
            }
            else
            {
                
                try
                {
                    List<Delegate> tout;
                    if (this.c_local_register.TryGetValue(eventName, out tout))
                        tout.Add(callback);
                }catch(Exception)
                {
                
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
                    catch(Exception)
                    {
            
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
                    catch(Exception)
                    {
            
                    }
                }
            }
        }


        public void unbind_all(Delegate callback=null)
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
                    foreach (Delegate callback in tout)
                    {
                        try
                        {
                            if (info == null)
                            {
                                foreach (var d in callback.GetInvocationList())
                                {
                                    object[] args = new object[] { eventname };
                                    Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                                }
                            }
                            else
                            {
                                if (info == null) { info = eventname; }

                                foreach (var d in callback.GetInvocationList())
                                {
                                    object[] args = new object[] { info };
                                    Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                                }
                            }
                        }catch (Exception){}
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

                    foreach (var d in callback.GetInvocationList())
                    {
                        object[] args = new object[] { payload, metadata };
                        Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                    }

                }
                catch (Exception)
                {
            
                }
            }



            if (this.c_local_register.ContainsKey(eventname))
            {
                List<Delegate> tout;
                if (this.c_local_register.TryGetValue(eventname, out tout))
                {
                    foreach (Delegate callback in tout)
                    {
                        try
                        {
                            foreach (var d in callback.GetInvocationList())
                            {
                                object[] args = new object[] { payload, metadata };
                                Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                            }
                        }
                        catch (Exception)
                        {
            
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
                                foreach (var d in callback.GetInvocationList())
                                {
                                    object[] args = new object[] { inparameter, response, rsub };
                                    Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                                }


                            }
                            catch (Exception)
                            {
          
                            }
                        }
                    }
                }
            }
        }


        public async Task emit_clientfunction(string functionName, object inparameter, object response)
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
                            foreach (var d in callback.GetInvocationList())
                            {
                                object[] args = new object[] { inparameter, response };
                                Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                            }
                        }
                        catch (Exception)
                        {
          
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

                    foreach (var d in callback.GetInvocationList())
                    {
                        object[] args = new object[] { channelName, EventInfo, metadata };
                        Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                    }
                }
                catch (Exception)
                {
                    
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
                            foreach (var d in callback.GetInvocationList())
                            {
                                object[] args = new object[] { channelName, EventInfo, metadata };
                                Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                            }
                        }
                        catch (Exception)
                        {
                            
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

                            foreach (var d in callback.GetInvocationList())
                            {
                                object[] args = new object[] { inparameter, response };
                                Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                            }

                
                        }
                        catch (Exception)
                        {
                            
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
                            foreach (var d in callback.GetInvocationList())
                            {
                                object[] args = new object[] { channelname, sessionid, action, response };
                                Task.Factory.StartNew(() => d.Method.Invoke(d.Target, args));
                            }

                        }
                        catch (Exception )
                        {
                            
                        }
                    }
                }
            }

        }
    }
};
