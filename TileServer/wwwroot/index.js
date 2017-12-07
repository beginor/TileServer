require([
    'dojo/request',
    'esri/Map',
    'esri/views/SceneView',
    'esri/widgets/BasemapGallery',
    'esri/Basemap',
    'esri/layers/TileLayer',
    'dojo/domReady!'
], function (
    request, Map, SceneView, BasemapGallery, Basemap, TileLayer
) {
        var map = new Map({
            basemap: 'gray'
        });

        var view = new SceneView({
            container: 'viewDiv',
            map: map,
            center: [113.2, 23.4],
            zoom: 6
        });

        var baseMap = '/arcgis/rest/services/BaseMap';

        request.get(baseMap, { handleAs: 'json' })
        .then(function (data) {
            var basemaps = data.map(function (item) {
                return new Basemap({
                    id: item,
                    title: item,
                    thumbnailUrl: baseMap + '/' + item + '/MapServer/tile/7/55/104',
                    baseLayers: [
                        new TileLayer({
                            url: baseMap + '/' + item + '/MapServer'
                        })
                    ]
                })
            });
            var basemapGallery = new BasemapGallery({
                view: view,
                source: basemaps
            });

            // Add the widget to the top-right corner of the view
            view.ui.add(basemapGallery, {
                position: 'top-right'
            });
        });

    });
