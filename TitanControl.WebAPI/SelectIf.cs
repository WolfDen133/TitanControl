using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class SelectIf
    {
        private HttpClient http;
        private PriorityTaskQueue _queue;

        public SelectIf(HttpClient httpClient, PriorityTaskQueue queue)
        {
            http = httpClient;
            _queue = queue;
        }

        public void SelectPlayback(HandleReference handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/SelectIf/PlaybackSelected?{handle.ToQueryArgument("handle")}");
            }, priority: TaskPriority.Normal);
        }
    }
}
