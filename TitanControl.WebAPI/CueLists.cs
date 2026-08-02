using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data.Conversion;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Data.Model;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class CueLists
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CueLists"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal CueLists(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Gets the cue information given for a cue in a cue list using the titan ID.
        /// </summary>
        /// <param name="playbackId">The playback ID of the cue list containing the cue.</param>
        /// <param name="cueId">The ID of the cue to get information about.</param>
        /// <returns>The cue information for the specified cue.</returns>
        public Task<Cue?> GetCue(int playbackId, int cueId)
        {
            return _queue.Enqueue(async token =>
            {
                return (await http.GetFromJsonAsync<JsonInformation<Cue>>($"titan/playback/{playbackId}/cue/{cueId}"))?.Information;
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Gets or sets the live cue number of the connected cueList.
        /// </summary>
        /// <returns>The live cue number of the connected cueList.</returns>
        public Task<float> LiveCueNumber()
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<float>($"titan/get/2/CueLists/LiveCueNumber");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Sets the next cue for the specified cue list.
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        /// <param name="cueNumber">The cue number.</param>
        public void SetNextCue(HandleReference handle, float cueNumber)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/SetNextCue?{handle.ToQueryArgument("handle")}&stepNumber={cueNumber}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Plays the given playback. If it's paused it continues, if it's running it starts the next step
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        public void Play(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/Play?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Plays the given playback but overides the fade time with the supplied value.
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        /// <param name="fadeInTime">The fade in time.</param>
        public void PlayWithTime(HandleReference handle, TimeSpan fadeInTime)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/PlayWithTime?{handle.ToQueryArgument("handle")}&time={fadeInTime.TotalSeconds}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Pauses the given playback, or optionally, if the playback is already paused, goes back.
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        /// <param name="goBackIfPaused">Whether to perform the go back / cancel link functionality instead if the playback is already paused.</param>
        public void Pause(HandleReference handle, bool goBackIfPaused = false)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/Pause?{handle.ToQueryArgument("handle")}&goBackIfPaused={goBackIfPaused}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Un-pauses a paused cue list if paused.
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        /// <param name="time">An optional override time.</param>
        public void Resume(HandleReference handle, TimeSpan? time = null)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/Resume?{handle.ToQueryArgument("handle")}&time={time?.TotalSeconds ?? 0}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Reviews the live cue of the supplied playback. This snaps to the previous cue then runs the live cue again.
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        public void Review(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/Review?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Snaps back on the supplied playback. This snaps (fires without fades) to the cue previous to the live one.
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        public void SnapBack(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/SnapBack?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Plays back on the supplied playback. Fires the cue previous to the live one.
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        public void GoBack(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/GoBack?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Cuts the next step to live, that is runs it without fade times. Operates on the supplied playback.
        /// </summary>
        /// <param name="handle">The handle reference for the CueList.</param>
        public void CutNextCueToLive(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/CutNextCueToLive?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Plays the next cue in the specified chase or cue list.
        /// <param name="handle">The handle reference for the CueList.</param>
        public void NextStep(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/CueLists/NextStep?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }
    }
}
