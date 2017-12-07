require([
    'dojo/request',
    'esri/config',
    'esri/Map',
    'esri/views/MapView',
    'esri/layers/TileLayer',
    'dojo/domReady!'
], function(request, config, Map, MapView, TileLayer) {
    // config.request.corsEnabledServers.push({
    //     host: 'localhost:8088',
    //     withCredentials: true
    // });
    var baseUrl = '/arcgis/rest/services/BaseMap/';
    var layer = new TileLayer({
        url: '/arcgis/rest/services/BaseMap/AreaMap/MapServer'
    });
    // Add layer to map
    var map = new Map({
        // basemap: 'streets',
        ground: 'world-elevation'
    });
    var view = new MapView({
        container: 'map',  // Reference to the DOM node that will contain the view
        map: map,  // References the map object created in step 3
        center: [113.2, 23.4],
        zoom: 7
    });
    map.layers.add(layer);
});
