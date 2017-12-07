#!/bin/bash
docker run --rm \
  --interactive \
  --tty \
  --volume $(pwd)/TileServer:/var/www/default \
  --volume $(pwd)/default:/usr/jexus/siteconf/default \
  --volume /Volumes/25A2/arcgiscache:/arcgiscache \
  --publish 8088:80 \
  --name tile-server \
  beginor/jexus:5.8.2.21
