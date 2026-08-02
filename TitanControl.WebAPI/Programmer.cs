using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Queue;

namespace TitanControl.WebAPI
{
    public class Programmer
    {
        private HttpClient http = null!;
        private PriorityTaskQueue _queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Playbacks"/> class.
        /// </summary>
        /// <param name="http">The HTTP.</param>
        internal Programmer(HttpClient http, PriorityTaskQueue queue)
        {
            this.http = http;
            _queue = queue;
        }

        /// <summary>
        /// Sets all attributes in the selected fixtures to their locate values. For a moving head this is normally straight down and open white.
        /// </summary>
        public void LocateSelectedFixtures()
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Programmer/Editor/Fixtures/LocateSelectedFixtures?allAttributes=true");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Sets the dimmer level of the selected fixtures to the given level.
        /// </summary>
        /// <param name="level">The level as a percentage.</param>
        public void SetDimmerLevel(double level)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Programmer/Editor/Fixtures/SetDimmerLevel?level={level}");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Sets the attribute current value of all currently selected fixtures.
        /// </summary>
        /// <param name="controlName">Name of the control.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="value">The value to set the control to where 1 is full and 0 is off..</param>
        public void SetControlValue(string controlName, string functionName, double value)
        {
            _queue.Enqueue(async token => 
            { 
                return await http.GetAsync($"titan/script/2/Programmer/Editor/Fixtures/SetControlValueByName?controlName={controlName}&functionName={functionName}&value={value}&programmer=true&createRestorePoint=false"); 
            }, priority: TaskPriority.Normal);
        }

        public async Task SetControlValueById(int controlId, int functionId, float value)
        {
            await http.GetAsync($"titan/script/2/Programmer/Editor/Fixtures/SetControlValueById?controlId={controlId}&functionId={functionId}&value={value}&programmer=true&createRestorePoint=true");
        }

        /// <summary>
        /// Sets the selected fixture colour mix channels to levels to recreate the specified HSI value.
        /// </summary>
        /// <param name="hue">The hue.</param>
        /// <param name="saturation">The saturation.</param>
        /// <param name="intensity">The intensity.</param>
        public void SetColourControlHSI(double hue, double saturation, double intensity)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Programmer/Editor/Fixtures/SetColourControlValues?hsi={hue},{saturation},{intensity}&programmer=true&createRestorePoint=false");
            }, priority: TaskPriority.Normal);
        }

        /// <summary>
        /// Sets the selected dimmer x fade.
        /// </summary>
        /// <param name="xFadeOn">if set to true [x fade on].</param>
        public void SetSelectedDimmerxFade(bool xFadeOn)
        {
            _queue.Enqueue(async token =>
            {
                return await http.GetAsync($"titan/script/2/Programmer/Editor/Fixtures/SetSelectedDimmerxFade?xFadeOn={xFadeOn}");
            }, priority: TaskPriority.Normal);
        }

    }
}
