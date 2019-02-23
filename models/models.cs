using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace bmsPiFunction
{
    public class Reading
    {
        public string timestamp { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
        public string sensorId { get; set; }
        public string id { get; set; }

    }
}