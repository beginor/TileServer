require([
    'dojo/request',
    'esri/config',
    'esri/Map',
    'esri/views/SceneView',
    'esri/widgets/BasemapGallery',
    'esri/Basemap',
    'esri/layers/TileLayer',
    'esri/widgets/Expand',
    'dojo/domReady!'
], function (
    request, config, Map, SceneView, BasemapGallery, Basemap, TileLayer,
    Expand
) {
    config.request.corsEnabledServers.push('localhost:8088');

    var map = new Map({
        basemap: 'streets',
        ground: "world-elevation"
    });

    var view = new SceneView({
        container: 'viewDiv',
        map: map,
        center: [113.2, 23.4],
        zoom: 7
    });

    var baseMap = '/arcgis/rest/services/BaseMap';

    request.get(baseMap, { handleAs: 'json' }).then(function (data) {
        var basemaps = data.filter(function (item) { return item != 'BaseMap_GDMask'; }).map(function (item) {
            return new Basemap({
                id: item,
                title: item,
                thumbnailUrl: baseMap + '/' + item + '/MapServer/tile/7/55/104',
                baseLayers: [
                    new TileLayer({
                        url: baseMap + '/' + item + '/MapServer'
                    }),
                    new TileLayer({
                        url: baseMap + '/' + 'BaseMap_GDMask' + '/MapServer'
                    })
                ]
            })
        });
        var basemapGallery = new BasemapGallery({
            container: document.createElement('div'),
            view: view,
            source: basemaps
        });

        var expand = new Expand({
            view: view,
            expandIconClass: 'esri-icon-basemap',
            expandTooltip: '底图图库',
            content: basemapGallery.domNode
        });

        // Add the widget to the top-right corner of the view
        view.ui.add(expand, {
            position: 'top-right'
        });
        // set basemap
        map.basemap = basemaps[0];
    });

});
