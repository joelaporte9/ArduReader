using System.Management;
using System.IO.Ports;

namespace ArduReader.Models
{
    public class GetDeviceData
    {
        List<string> devicenameList {get; set;} = new List<string>();

        public List<string> GetDeviceNames()
        {
            using (var devices = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports1 = devices.Get().Cast<ManagementBaseObject>().ToList();
                devicenameList = (from n in portnames
                                join p in ports1 on n equals p["DeviceID"].ToString()
                                select n + " - " + p["Caption"]).ToList();
            }

            return devicenameList;
        } 
    }
}