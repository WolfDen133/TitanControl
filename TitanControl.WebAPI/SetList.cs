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
    public class SetList
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetList"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal SetList(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Gets the information for the specified set list.
        /// </summary>
        /// <param name="setListId">The ID of the set list to return information for.</param>
        /// <returns>The set list information.</returns>
        public Task<SetList?> GetSetList(int setListId)
        {
            return _queue.Enqueue(async token =>
            {
                return (await http.GetFromJsonAsync<JsonInformation<SetList>>($"titan/setlist/{setListId}"))?.Information;
            }, priority: TaskPriority.Low);
        }

        /// <summary>
        /// Gets the set list track information for a track within a set list.
        /// </summary>
        /// <param name="setListId">The ID of the set list containing the track.</param>
        /// <param name="trackId">The ID of the track within the set list.</param>
        /// <returns>The set list track information.</returns>
        public Task<SetListTrack?> GetSetListTrack(int setListId, int trackId)
        {
            return _queue.Enqueue(async token =>
            {
                return (await http.GetFromJsonAsync<JsonInformation<SetListTrack>>($"titan/setListId/{setListId}/track/{trackId}"))?.Information;
            }, priority: TaskPriority.Low);
        }

        /// <summary>
        /// Select the active set list.
        /// </summary>
        /// <param name="handle">The handle for the set list.</param>
        public void SelectList(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/SetList/SelectListHandle?{handle.ToQueryArgument("setListHandle")}");
            }, priority: TaskPriority.Low);
        }


        /// <summary>
        /// Gets the handle for the active set list track.
        /// </summary>
        /// <returns>The track handle for the active track.</returns>
        public Task<Handle?> GetActiveTrack()
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<Handle>($"titan/get/2/SetList/ActiveTrack");
            }, priority: TaskPriority.Low);
        }

        /// <summary>
        /// Fires the next track in the active list.
        /// </summary>
        public void NextTrack()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/SetList/NextTrack");
            }, priority: TaskPriority.Low);
        }

        /// <summary>
        /// Fires the previous track in the active list.
        /// </summary>
        public void PreviousTrack()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/SetList/PreviousTrack");
            }, priority: TaskPriority.Low);
        }

        /// <summary>
        /// Fires the track.
        /// </summary>
        /// <param name="handle">The handle of the track to fire.</param>
        public void FireTrack(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/SetList/FireTrack?{handle.ToQueryArgument("trackHandle")}");
            }, TaskPriority.Low);
        }

    }
}
