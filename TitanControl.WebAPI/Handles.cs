using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Data.Model;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class Handles
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleWorlds"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Handles(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        #region Handles
        
        public Task<Handle[]?> GetHandles(string handleGroupId = "", int pageIndex = -1, bool verbose = false)
        {
            return _queue.Enqueue(async token =>
            {
                if (!string.IsNullOrEmpty(handleGroupId))
                {
                    if (pageIndex > 0)
                    {
                        return await http.GetFromJsonAsync<Handle[]>($"titan/handles/{handleGroupId}{(verbose ? "?verbose=true" : "")}");
                    }
                    else
                    {
                        return await http.GetFromJsonAsync<Handle[]>($"titan/handles/{handleGroupId}/{pageIndex}{(verbose ? "?verbose=true" : "")}");
                    }
                }
                else
                {
                    return await http.GetFromJsonAsync<Handle[]>($"titan/handles{(verbose ? "?verbose=true" : "")}");
                }
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Gets the handle information for a handle using the user number of the playback handle.
        /// </summary>
        /// <param name="userNumber">The playback user number to search for.</param>
        /// <returns>The handle information for the requested playback handle.</returns>
        public Task<Handle?> GetHandleFromUserNumber(int userNumber, string handleType = "playbackHandle")
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<Handle>($"titan/script/2/Handles/GetHandleFromUserNumber?handleXmlNodeName={handleType}&userNumber={userNumber}");
            }, priority: TaskPriority.Normal);
        }

        public Task<Handle?> GetHandleFromTitanId(int titanId, string handleType = "playbackHandle")
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<Handle>($"titan/script/2/Handles/GetHandleFromId?handleXmlNodeName={handleType}&id={titanId}");
            }, priority: TaskPriority.Normal);
        }

        #endregion

        #region Handle Worlds

        public void SetHandleWorld(Guid worldId)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Handles/SetHandleWorld?worldId={worldId}");
            }, priority: TaskPriority.Normal);
        }

        public void RenameHandleWorld(Guid worldId, string worldLegend)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Handles/RenameHandleWorld?worldId={worldId}&worldLegend={worldLegend}");
            }, priority: TaskPriority.Normal);
        }

        public Task<string?> CurrentWorldId()
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<string>($"titan/get/2/Handles/CurrentWorldId");
            }, priority: TaskPriority.Normal);
        }

        public Task<string?> CurrentWorldName()
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<string>($"titan/get/2/Handles/CurrentWorldName");
            }, priority: TaskPriority.Normal);
        }

        #endregion
    }
}
