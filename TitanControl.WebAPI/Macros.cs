
using System.Net.Http;
using System.Net.Http.Json;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class Macros
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Playbacks"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Macros(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Gets the macro ids for macros in the current show.
        /// </summary>
        /// <param name="includeUnassignedHandles">if set to <c>true</c> [include unassigned handles].</param>
        /// <returns></returns>
        public Task<IEnumerable<string>?> GetMacroIds(bool includeUnassignedHandles = false)
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<IEnumerable<string>>($"titan/script/2/UserMacros/GetAllMacroIds?includeUnassignedHandles={includeUnassignedHandles}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Exports the macro the specified macro from the show and return the macro as XML.
        /// </summary>
        /// <param name="macroId">The ID of the macro to export.</param>
        /// <returns>THe macro in XML format.</returns>
        public Task<string?> ExportMacro(string macroId)
        {
            return _queue.Enqueue(async token =>
            {
                return await http.GetFromJsonAsync<string>($"titan/script/2/UserMacros/ExportXml?macroId={macroId}");
            }, priority: TaskPriority.Normal);
        }

        private class ImportArguments
        {
            public string Script { get; set; } = null!;
        }

        /// <summary>
        /// Imports the XML macro script into the current show so it can be used in that show.
        /// </summary>
        /// <param name="macroScript">The XML macro script containing the macro to be imported.</param>
        public void ImportMacro(string macroScript)
        {
            _queue.Enqueue(async token =>
            {
                return await http.PostAsJsonAsync($"titan/script/2/UserMacros/ImportXml", new ImportArguments() { Script = macroScript });
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Recalls the macro adn runs it within the current show.
        /// </summary>
        /// <param name="macroId">The ID of the macro to recall and run.</param>
        public void RecallMacro(string macroId)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/UserMacros/RecallMacroById?macroId={macroId}");
            }, priority: TaskPriority.Normal);
        }
    }
}
