using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using KMLServer.Models;
using System.Text.Json;
using SharpKml.Dom;
using SharpKml.Base;
using SharpKml.Engine;
using Microsoft.AspNetCore.Hosting;

namespace KMLServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KMLController : ControllerBase
    {


        private readonly ILogger<KMLController> _logger;
        private readonly IWebHostEnvironment _env;

        HydrantModel _model;

        public KMLController(ILogger<KMLController> logger, IWebHostEnvironment env)
        {
            _logger = logger;

            var filePath = Path.Join(env.ContentRootPath, "Data", "Hydrants.geojson");
            try
            {
                string json = System.IO.File.ReadAllText(filePath);

                _model = JsonSerializer.Deserialize<HydrantModel>(json);
            }
            catch { }


        }

        [HttpGet]
        public IActionResult Get(string bbox)
        {
            #region Setup
            var doc = new Document();
            
            Folder folder = new();

            var iconStyle = new IconStyle()
            {
                Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/pal4/icon19.png"))
            };

            var balloonStyle = new BalloonStyle
            {
                Text = @"
$[description]<br>
<table border=1>
<tr>
<th>Name</th><th>Value</th>
</tr>
<tr><td>Flow</td><td>$[Flow]</tr>
<tr><td>Main Size</td><td>$[MainSize]</tr>
</table>
"
            };

            var style = new Style()
            {
                Id = "HydrantStyle",
                Icon = iconStyle,
                Balloon = balloonStyle
            };

            doc.AddStyle(style);
            #endregion

            #region FilterData
            if (bbox is not null)
            {
                var s = bbox.Split(",");
                // 0 = west, 1 = s, 2 = E, 3 = North

                var westBounds = float.Parse(s[0]);
                var southBounds = float.Parse(s[1]);
                var eastBounds = float.Parse(s[2]);
                var northBounds = float.Parse(s[3]);

                foreach (var h in _model.features)
                {
                    var longitude = h.geometry.coordinates[0];
                    var latitude = h.geometry.coordinates[1];

                    if (latitude < southBounds || latitude > northBounds)
                        continue;

                    if (longitude < westBounds || longitude > eastBounds)
                        continue;

                    var point = new Point();
                    point.Coordinate = new Vector(latitude, longitude); 

                    var toolTip = new Description { Text = h.properties.LOCATION };

                    var placemark = new Placemark
                    {
                        Geometry = point,
                        Description = toolTip,
                        StyleUrl = new Uri("#HydrantStyle", UriKind.Relative)
                    };
                    placemark.ExtendedData = new();
                    placemark.ExtendedData.AddData(new Data { Name = "Flow", Value = h.properties.FLOW_AT_20.ToString() });
                    placemark.ExtendedData.AddData(new Data { Name = "MainSize", Value = h.properties.MAIN_SIZE });

                    folder.AddFeature(placemark);

                }
            }
            #endregion

            var howMany = folder.Features.Count();
            System.Diagnostics.Debug.WriteLine($"Qualified {howMany}");

            // IMPORTANT PART HERE
            // too many features, just return zip
            if (howMany > 100)
                folder = new();

            doc.AddFeature(folder);

            var kml = KmlFile.Create(doc, false);

            var stream = new MemoryStream();
            kml.Save(stream);

            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/vnd.google-earth.kml+xml");

        }
    }
}
