/*
 * Copyright (c) InWorldz Halcyon Developers
 * Copyright (c) Contributors, http://opensimulator.org/
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Timers;
using System.Threading.Tasks;

using log4net;
using OpenMetaverse;

using OpenSim.Framework;
using OpenSim.Framework.Console;
using OpenSim.Framework.Servers;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Grid.Framework;

namespace OpenSim.Grid.GridServer
{
    /// <summary>
    /// </summary>
    public class GridServerBase : BaseOpenSimServer, IGridServiceCore
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected GridConfig m_config;
        private InWorldz.RemoteAdmin.RemoteAdmin m_radmin;

        public GridConfig Config
        {
            get { return m_config; }
        }

        protected List<IGridPlugin> m_plugins = new List<IGridPlugin>();

        public void Work()
        {
            m_console.Notice("Enter help for a list of commands\n");

            while (true)
            {
                m_console.Prompt();
            }
        }

        public GridServerBase()
        {
            m_console = new LocalConsole("Grid");
            MainConsole.Instance = m_console;
        }

        protected override void StartupSpecific()
        {
            m_config = new GridConfig("GRID SERVER", (Path.Combine(Util.configDir(), "GridServer_Config.xml")));

            m_log.Info("[GRID]: Starting HTTP process");
            m_httpServer = new BaseHttpServer(m_config.HttpPort, null);

            LoadPlugins();

            m_httpServer.Start();

            m_radmin = new InWorldz.RemoteAdmin.RemoteAdmin();
            m_radmin.AddCommand("GridService", "Shutdown", GridServerShutdownHandler);
            m_radmin.AddHandler(m_httpServer);

            base.StartupSpecific();
        }

        public object GridServerShutdownHandler(IList args, IPEndPoint remoteClient)
        {
            m_radmin.CheckSessionValid(new UUID((string)args[0]));

            try
            {
                int delay = (int)args[1];
                string message;

                if (delay > 0)
                    message = "Server is going down in " + delay.ToString() + " second(s).";
                else
                    message = "Server is going down now.";

                m_log.DebugFormat("[RADMIN] Shutdown: {0}", message);

                // Perform shutdown
                if (delay > 0)
                    System.Threading.Thread.Sleep(delay * 1000);

                // Do this on a new thread so the actual shutdown call returns successfully.
                Task.Factory.StartNew(Shutdown);
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[RADMIN] Shutdown: failed: {0}", e.Message);
                m_log.DebugFormat("[RADMIN] Shutdown: failed: {0}", e.ToString());
                throw;
            }

            m_log.Info("[RADMIN]: Shutdown Administrator Request complete");
            return true;
        }

        protected virtual void LoadPlugins()
        {
            PluginLoader<IGridPlugin> loader =
                new PluginLoader<IGridPlugin>(new GridPluginInitializer(this));

            loader.Load("/OpenSim/GridServer");
            m_plugins = loader.Plugins;
        }

        public override void ShutdownSpecific()
        {
            foreach (IGridPlugin plugin in m_plugins) plugin.Dispose();
        }

        #region IServiceCore
        protected Dictionary<Type, object> m_moduleInterfaces = new Dictionary<Type, object>();

        /// <summary>
        /// Register an Module interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iface"></param>
        public void RegisterInterface<T>(T iface)
        {
            lock (m_moduleInterfaces)
            {
                if (!m_moduleInterfaces.ContainsKey(typeof(T)))
                {
                    m_moduleInterfaces.Add(typeof(T), iface);
                }
            }
        }

        public bool TryGet<T>(out T iface)
        {
            if (m_moduleInterfaces.ContainsKey(typeof(T)))
            {
                iface = (T)m_moduleInterfaces[typeof(T)];
                return true;
            }
            iface = default(T);
            return false;
        }

        public T Get<T>()
        {
            return (T)m_moduleInterfaces[typeof(T)];
        }

        public BaseHttpServer GetHttpServer()
        {
            return m_httpServer;
        }
        #endregion
    }
}
