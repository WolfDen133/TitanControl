using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class Fixtures
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fixtures"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Fixtures(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }
    }
}
