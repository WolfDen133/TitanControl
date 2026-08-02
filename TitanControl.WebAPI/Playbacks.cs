using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Data.Conversion;
using TitanControl.WebAPI.Data.Model;
using TitanControl.WebAPI.Queue;
using static System.Net.WebRequestMethods;

namespace TitanControl.WebAPI
{
    /// <summary>
    /// Implements WebAPI methods related to controlling playbacks.
    /// </summary>
    public class Playbacks
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Playbacks"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Playbacks(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Gets the playback information given the titan ID for the playback.
        /// </summary>
        /// <param name="playbackId">The playback ID of the cue list containing the cue.</param>
        /// <returns>The playback information for the specified playback.</returns>
        public Task<Playback?> GetPlayback(int playbackId)
        {
            return _queue.Enqueue(async token =>
            {
                return (await http.GetFromJsonAsync<JsonInformation<Playback>>($"titan/playback/{playbackId}"))?.Information;
            }, priority: TaskPriority.High);
        }

        /// <summary>
        /// Fires the specified playback at full.
        /// </summary>
        /// <param name="userNumber">The user number of the playback to fire.</param>
        public void Fire(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/FirePlaybackAtLevel?{handle.ToQueryArgument("handle")}&level=1&alwaysRefire=false");
            }, priority: TaskPriority.High);
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
                return await http.GetAsync($"titan/script/2/Playbacks/FirePlaybackAtLevel?{handle.ToQueryArgument("handle")}&level={level}&alwaysRefire=false");
            }, priority: TaskPriority.High);
        }

        public void FlashDown(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/FlashPlayback?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.High);
        }

        public void FlashUp(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/ClearFlashPlayback?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.High);
        }

        public void TimedFlashDown(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/FlashTimedPlayback?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.High);
        }

        public void TimedFlashUp(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/ClearFlashTimedPlayback?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.High);
        }

        public void ToggleLatch (HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/ToggleLatchPlayback?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.High);
        }

        /// <summary>
        /// Kills the specified playback aithout releasing.
        /// </summary>
        /// <param name="userNumber">The user number of the playback to kill.</param>
        public void Kill(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/KillPlayback?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.High);
        }

        public void Release(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/ReleasePlayback?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.High);
        }

        public void TapTempo(HandleReference handle, DateTime dateTime)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/TapTempo?{handle.ToQueryArgument("handle")}&panelTimeStamp={dateTime}");
            }, priority: TaskPriority.High);
        }

        /// <summary>
        /// Kills all running playbacks without releasing.
        /// </summary>
        public void KillAll()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/KillAllPlaybacks");
            }, priority: TaskPriority.High);
        }

        /// <summary>
        /// Records a single cue on a playback. The new cue is created using the information in the programmer and the current record mode.
        /// </summary>
        /// <param name="group">The handle group the new cue is to be recorded on.</param>
        /// <param name="index">The handle ID in the group the cue is to be recorded on.</param>
        /// <param name="updateOnly">if set to true [update only].</param>
        public void StoreCue(string group, int index, bool updateOnly = false)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Playbacks/StoreCue?group={group}&index={index}&updateOnly={updateOnly}");
            }, priority: TaskPriority.High);
        }
    }
}
