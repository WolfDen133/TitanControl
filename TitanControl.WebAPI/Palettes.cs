using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class Palettes
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Palettes"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Palettes(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Applies a palette to the current fixture selection or if not fixtures are selected all fixtures.
        /// </summary>
        /// <param name="handle">The handle for the palette you want to apply.</param>
        /// <param name="withTimes">Whether the palette should snap or run with the current palette fade times.</param>
        /// <returns></returns>
        public void ApplyPalette(HandleReference handle, bool withTimes = true)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Palette/ApplyPalette?{handle.ToQueryArgument("handle")}&usePaletteTimes={withTimes}");
            }, priority: TaskPriority.Normal);
        }
    }
}
