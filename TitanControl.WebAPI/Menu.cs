using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class Menu
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fixtures"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Menu(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Inject an input to the menu system. Inputs are button hardware actions such as button presses and fader movements and pseudo actions such as fixture handle press or palette key press
        /// </summary>
        /// <param name="type">Type of the input. (OnButtonDown, OnButtonUp, etc)</param>
        /// <param name="id">The id of the input.</param>
        /// <param name="group">The panel group or region of the input.</param>
        /// <param name="index">The index of the input in that group.</param>
        public void InjectInput(string type, string id, string group, int index)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Menu/InjectInput?type={type}&id={id}&group={group}&index={index}");
            }, priority: TaskPriority.High);
        }
    }
}
