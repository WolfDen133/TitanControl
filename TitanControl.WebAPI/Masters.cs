using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class Masters
    {
        private HttpClient http;
        private PriorityTaskQueue _queue;

        public Masters(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Fires the specified playback at the specified level..
        /// </summary>
        /// <param name="userNumber">The user number of the playback to fire.</param>
        /// <param name="level">The level to fire the playback at where 1 is full and 0 is off.</param>
        public void Level(HandleReference handle, float level)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Masters/SetMaster?{handle.ToQueryArgument("handle")}&value={level}");
            }, priority: TaskPriority.Normal);
        }

        /* Broken
        public async Task TapTempo (HandleReference handle, DateTime dateTime)
        {
            var responce = http.GetAsync($"titan/script/2/Masters/TapTempo?{handle.ToQueryArgument("handle")}&panelTimeStamp={dateTime}").Result;
        }*/ 

        public Task<float> GetBPM(HandleReference? handle = null, MasterTypes type = MasterTypes.None)
        {
            return _queue.Enqueue(async token =>
            {
                if (handle == null)
                {
                    await http.GetAsync("titan/script/2/Handles/ClearHandleOptionsFilter");

                    return await http.GetFromJsonAsync<float>($"titan/get/2/HandleOptions/Masters/{ToQArgument(type)}/BPM");
                }

                if (type != MasterTypes.BPM) return -1f;

                await http.GetAsync($"titan/script/2/Handles/SetSourceHandleFromHandle?{handle.ToQueryArgument("handle")}");
                await http.GetAsync("titan/script/2/Handles/FilterHandleOptions");

                float value = await http.GetFromJsonAsync<float>($"titan/get/2/HandleOptions/Masters/{ToQArgument(type)}/BPM");

                await http.GetAsync("titan/script/2/Handles/ClearHandleOptionsFilter");

                return value;
                
            }, priority: TaskPriority.Low);
        }

        public Task<int> GetRange (HandleReference? handle = null, MasterTypes type = MasterTypes.None)
        {
            return _queue.Enqueue(async token =>
            {
                if (handle == null || type == MasterTypes.None)
                {
                    return -1;
                }

                await http.GetAsync($"titan/script/2/Handles/SetSourceHandleFromHandle?{handle.ToQueryArgument("handle")}");
                await http.GetAsync("titan/script/2/Handles/FilterHandleOptions");

                int value = await http.GetFromJsonAsync<int>($"titan/get/2/HandleOptions/Masters/{ToQArgument(type)}/Range");

                await http.GetAsync("titan/script/2/Handles/ClearHandleOptionsFilter");
                return value;

            }, priority: TaskPriority.Low);
        }

        public void SetSourceFromHandle (HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Handles/SetSourceHandleFromHandle?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Low);
        }

        public void Flash(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Masters/Flash?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }

        public void ClearFlash(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Masters/ClearFlash?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }

        public void FilterHandleOptions()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync("titan/script/2/Handles/FilterHandleOptions");
            }, priority: TaskPriority.Low);
        }

        public void ClearHandleOptionsFilter ()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync("titan/script/2/Handles/ClearHandleOptionsFilter");
            }, priority: TaskPriority.Low);
        }

        private static string ToQArgument(MasterTypes type)
        {
            return type switch
            {
                MasterTypes.Size => "Scaleable",
                MasterTypes.Rate => "Scaleable",
                MasterTypes.BPM => "Bpm",
                _ => RateMasterTypes.None
            };
        }
    }
}
