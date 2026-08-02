using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class Selection
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Playbacks"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Selection(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Allows you to select a range fixture in the editor, when you select a fixture you can modify values for the selected fixtures.
        /// </summary>
        /// <param name="handle">The handle.</param>
        public void SelectFixturesFromHandles(IEnumerable<HandleReference> handle)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Selection/Context/Programmer/SelectFixturesFromHandles?{handle.ToQueryArgument("handleList")}");
            }, priority: TaskPriority.Normal);
        }

        public Task<int[]?> GetSelectedFixtureIds()
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<int[]>($"titan/script/2/Selection/Context/Programmer/GetSelectedFixtureIds");
            }, priority: TaskPriority.Normal);
        }

        public void NextFixture()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Programmer/Editor/Selection/PatternNext");
            }, priority: TaskPriority.Normal);
        }

        public void PreviousFixture()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Programmer/Editor/Selection/PatternPrevious");
            }, priority: TaskPriority.Normal);
        }

        public void ClearPattern()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Programmer/Editor/Selection/ClearPatternSelect");
            }, priority: TaskPriority.Normal);
        }


        /// <summary>
        /// Clears the current selection and creates a restore point.
        /// </summary>
        public void Clear()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Selection/Context/Programmer/Clear");
            }, priority: TaskPriority.Normal);
        }

    }
}
