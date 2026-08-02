using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Queue;
using static System.Net.WebRequestMethods;

namespace TitanControl.WebAPI
{
    public class Groups
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        internal Groups(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        public void Level(HandleReference handle, float value)
        {
            _queue.Enqueue(async token =>
            {
                var request = await http.GetAsync($"titan/script/2/Group/SetGroupHandleFaderLevel?{handle.ToQueryArgument("handle")}&value={value}&accuracy=1.00");
                return request;
            }, priority: TaskPriority.Normal);
        }
    }
}
