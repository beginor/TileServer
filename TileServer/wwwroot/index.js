require([
    "esri/config",
    "esri/Map",
    "esri/views/MapView",
    "esri/layers/TileLayer",
    "dojo/domReady!"
], function(config, Map, MapView, TileLayer) {
    config.request.corsEnabledServers.push({
        host: 'localhost:8088',
        withCredentials: true
    });
    var layer = new TileLayer({
        url: "http://localhost:8088/arcgis/rest/services/BaseMap/AreaMap/MapServer"
    });
    // Add layer to map
    var map = new Map({
        // basemap: "streets",
        ground: "world-elevation"
    });
    var view = new MapView({
        container: "map",  // Reference to the DOM node that will contain the view
        map: map  // References the map object created in step 3
    });
    map.layers.add(layer);
});