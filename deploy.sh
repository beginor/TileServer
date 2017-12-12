#!/bin/bash
cd TileServer
msbuild /t:Build /p:Configuration=Release
scp -r bin wwwroot/ map.json docker703:~/
ssh docker703 -t '
cd /mnt/vol7/docker/tile-server
docker-compose down
rm -rf jexus/web/bin jexus/web/wwwroot jexus/web/map.json
sudo mv -v ~/wwwroot ~/bin ~/map.json jexus/web
docker-compose up -d
'
