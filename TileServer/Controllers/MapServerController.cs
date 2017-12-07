using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TileServer.Controllers {

    [RoutePrefix("arcgis/rest/services/BaseMap")]
    public class MapServerController : ApiController {

        private readonly string cacheFolder;
        private readonly string mapInfoFile;

        public MapServerController() {
            cacheFolder = ConfigurationManager.AppSettings["cacheFolder"];
            if (string.IsNullOrEmpty(cacheFolder)) {
                throw new InvalidOperationException(
                    "cacheFolder not found in appSettings"
                );
            }
            if (!Directory.Exists(cacheFolder)) {
                throw new DirectoryNotFoundException(
                    $"cache Folder {cacheFolder} does not exist!"
                );
            }
            mapInfoFile = ConfigurationManager.AppSettings["mapInfoFile"];
            if (string.IsNullOrEmpty(mapInfoFile)) {
                throw new InvalidOperationException(
                    "mapInfo not found in appSettings"
                );
            }
            if (!File.Exists(mapInfoFile)) {
                throw new FileNotFoundException(
                    $"map info file {mapInfoFile} does not exists!"
                );
            }
        }

        // GET: arcgis/rest/services/{tileName}/MapServer?f=pjson
        [HttpGet, Route("{mapName}/MapServer")]
        public IHttpActionResult GetTileMapLayerInfo(string mapName) {
            try {
                var text = File.ReadAllText(mapInfoFile);
                var json = JsonConvert.DeserializeObject<JObject>(text);
                json["mapName"] = mapName;
                text = json.ToString();
                var callback = Request.GetQueryNameValuePairs().FirstOrDefault(
                        q => q.Key.Equals("callback", StringComparison.OrdinalIgnoreCase)
                    );
                var hasCallback = !string.IsNullOrEmpty(callback.Value);
                if (hasCallback) {
                    text = $"{callback.Value}({text})";
                }
                var response = Request.CreateResponse(HttpStatusCode.OK);
                var content = new StringContent(text);
                content.Headers.ContentType = hasCallback
                    ? new MediaTypeHeaderValue("text/javascript")
                    : new MediaTypeHeaderValue("application/json");
                response.Content = content;
                return ResponseMessage(response);
            }
            catch (Exception ex) {
                return InternalServerError(ex);
            }
        }

        // GET: arcgis/rest/services/BaseMap/ADMap/MapServer/tile/1/0/1
        [HttpGet, Route("{mapName}/MapServer/tile/{level:int}/{row:int}/{col:int}")]
        public async Task<IHttpActionResult> GetTile(
            string mapName,
            int level,
            int row,
            int col
        ) {
            try {
                var tilePath = Path.Combine(
                    cacheFolder,
                    mapName,
                    "Layers",
                    "_alllayers"
                );
                if (!Directory.Exists(tilePath)) {
                    tilePath = Path.Combine(
                        cacheFolder,
                        "BaseMap_" + mapName,
                        "Layers",
                        "_alllayers"
                    );
                }
                if (!Directory.Exists(tilePath)) {
                    return NotFound();
                }
                var buff = await GetTileContent(tilePath, level, row, col);
                if (buff.Length == 0) {
                    return NotFound();
                }
                var content = new ByteArrayContent(buff);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = content;
                return ResponseMessage(response);
            }
            catch (Exception ex) {
                return InternalServerError(ex);
            }
        }

        private static async Task<byte[]> GetTileContent(
            string tilePath,
            int lev,
            int r,
            int c
        ) {
            int rowGroup = 128 * (r / 128);
            int colGroup = 128 * (c / 128);
            // try get from bundle
            // string.Format("{0}\\L{1:D2}\\R{2:X4}C{3:X4}.{4}", tilePath, lev, rowGroup, colGroup, "bundle");
            var bundleFileName = Path.Combine(
                tilePath,
                $"L{lev:D2}",
                $"R{rowGroup:X4}C{colGroup:X4}.bundle"
            );
            int index = 128 * (r - rowGroup) + (c - colGroup);
            if (File.Exists(bundleFileName)) {
                using (var fs = new FileStream(bundleFileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    fs.Seek(64 + 8 * index, SeekOrigin.Begin);
                    // 获取位置索引并计算切片位置偏移量
                    byte[] indexBytes = new byte[4];
                    await fs.ReadAsync(indexBytes, 0, 4);
                    long offset = (indexBytes[0] & 0xff)
                        + (long)(indexBytes[1] & 0xff) * 256
                        + (long)(indexBytes[2] & 0xff) * 65536
                        + (long)(indexBytes[3] & 0xff) * 16777216;
                    // 获取切片长度索引并计算切片长度
                    long startOffset = offset - 4;
                    fs.Seek(startOffset, SeekOrigin.Begin);
                    byte[] lengthBytes = new byte[4];
                    await fs.ReadAsync(lengthBytes, 0, 4);
                    int length = (lengthBytes[0] & 0xff)
                        + (lengthBytes[1] & 0xff) * 256
                        + (lengthBytes[2] & 0xff) * 65536
                        + (lengthBytes[3] & 0xff) * 16777216;
                    //根据切片位置和切片长度获取切片
                    var tileBytes = new byte[length];
                    await fs.ReadAsync(tileBytes, 0, tileBytes.Length);
                    fs.Close();
                    return tileBytes;
                }
            }
            // try pngfile
            // var tileFileName = string.Format("{0}\\L{1:D2}\\R{2:X8}\\C{3:X8}", tilePath, lev, r, c);
            var tile = Path.Combine(
                tilePath,
                lev.ToString("D2"),
                r.ToString("X8"),
                c.ToString("X8")
            );
            if (File.Exists(tile + ".png")) {
                tile = tile + ".png";
            }
            else if (File.Exists(tile + ".jpg")) {
                tile = tile + ".jpg";
            }
            else {
                return new byte[0];
            }
            using (var fs = new FileStream(tile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                //获取位置索引并计算切片位置偏移量
                int length = (int)fs.Length;
                byte[] fileBytes = new byte[length];
                await fs.ReadAsync(fileBytes, 0, length);
                fs.Close();
                return fileBytes;
            }
        }

    }

}
