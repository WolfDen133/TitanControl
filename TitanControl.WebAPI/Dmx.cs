

using System.Net.Http;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    /// <summary>
    /// Implements WebAPI methods related to the DMX output.
    /// </summary>
    public class Dmx
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Playbacks"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Dmx(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;

        }

        /// <summary>
        /// Sets the streaming ACN merge priority.
        /// </summary>
        /// <param name="mergePriority">The merge priority where a higher priority will take presidence over a lower priority.</param>
        public void SetMergePriority(int mergePriority = 100)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Dmx/SetMergePriority?mergePriority={mergePriority}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Allow the user to enable or disable DMX output.
        /// </summary>
        /// <param name="freeze">If set to true freeze the output.</param>
        public void FreezeDmx(bool freeze)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Dmx/FreezeDmx?freeze={freeze}");
            }, priority: TaskPriority.Normal);
        }
    }
}
