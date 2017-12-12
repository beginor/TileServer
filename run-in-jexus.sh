#!/bin/bash
docker run --rm \
  --interactive \
  --tty \
  --volume $(pwd)/TileServer:/var/www/default \
  --volume $(pwd)/default:/usr/jexus/siteconf/default \
  --volume /Volumes/share/directories/arcgiscache:/arcgiscache \
  --publish 2019:80 \
  --name tile-server \
  beginor/jexus:5.8.2.21
