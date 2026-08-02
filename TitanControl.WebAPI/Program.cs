using System.Diagnostics;
using TitanControl.WebAPI.Data.Model;

namespace TitanControl.WebAPI;

class Program
{
    static void Main()
    {
        Titan titan = new("127.0.0.1");

        Debug.WriteLine("Connecting to Titan...");

        titan.Start();
        Debug.WriteLine(titan.IsConnected().Result);
    }
}
