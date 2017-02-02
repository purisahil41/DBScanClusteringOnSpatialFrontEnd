<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="ClusteringTool.WebForm1" %>

<%--<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>--%>


<!DOCTYPE html>
<html>
<head runat="server">
    <meta name="viewport" content="initial-scale=1.0, user-scalable=no">
    <meta charset="utf-8">
    <title>Simple markers</title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" />
    <style>
        /* Always set the map height explicitly to define the size of the div
       * element that contains the map. */
        #map {
            height: 100%;
            display: none;
        }

        #opening {
            opacity: 0.95;
            height: 100vh;
            width: 100vw;
            background: url('Chicago_Image.jpg') no-repeat;
            background-size: cover;
            z-index: 5;
        }

        #modal {
            height: 100vh;
            width: 100vw;
            display: none;
            background: rgba(128, 128, 128, 0.5);
            position: absolute;
            top: 0;
            left: 0;
        }
        /* Optional: Makes the sample page fill the window. */
        html, body {
            height: 100%;
            margin: 0;
            padding: 0;
        }

        #getInput {
            position: absolute;
            height: 30px;
            width: 200px;
            top: 2%;
            cursor: pointer;
            left: 45%;
            color: black;
            box-sizing: border-box;
            margin: 0 auto;
            padding: 8px;
            max-width: 200px;
            background: #fff; /* fallback color for old browsers */
            background: rgba(255, 255, 255, 1);
            border-radius: 8px;
            text-align: center;
            text-decoration: none;
            letter-spacing: 1px;
            transition: all 0.3s ease-out;
            display: none;
        }

        #UserInput {
            position: absolute;
            width: 40%;
            height: 40%;
            top: 30%;
            left: 30%;
        }

        .LabelDef {
            float: left;
            width: 20%;
            margin-right: 5%;
        }

        .input-group {
            width: 75%;
        }

        button {
            margin-left: 10%;
        }

        .displayCrime {
            display: block;
        }

        .hideCrime {
            display: none;
        }

        #textOpening {
            position: absolute;
            top: 10%;
            left: 30%;
            color: white;
            height: 100px;
            width: 560px;
            font-size: 25px;
            font-weight: bold;
            padding-left: 20px;
            border-left: 2px solid;
            font-family: Century Gothic, sans-serif;
            font-style: normal;
        }

        #policeIcon {
            background: url('crime-and-community-safety.png');
            background-size: cover;
            height: 100px;
            width: 100px;
            top: 10%;
            left: 21%;
            position: absolute;
            -webkit-filter: invert(100%);
            filter: invert(100%);
        }

        a {
            display: inline-block;
            border-radius: 50%;
        }

            a:hover .left, a:hover .top, a:hover .bottom, a:hover .right {
                border: 0.5em solid black;
                transition: 1s;
            }

                a:hover .left:after, a:hover .top:after, a:hover .bottom:after, a:hover .right:after {
                    border-top: 0.5em solid white;
                    border-right: 0.5em solid white;
                    transition: 1s;
                }

        .bottom {
            padding-left: 20px;
            padding-top: 8px;
            display: inline-block;
            width: 6em;
            height: 6em;
            border: 0.5em solid white;
            border-radius: 50%;
            margin-left: 0.75em;
        }

            .bottom:after {
                content: '';
                display: inline-block;
                margin-top: 0.6em;
                width: 2em;
                height: 2em;
                border-top: 0.5em solid black;
                border-right: 0.5em solid black;
                -moz-transform: rotate(135deg);
                -webkit-transform: rotate(135deg);
                transform: rotate(135deg);
            }

        .hideMe {
            transition: 3s;
            height: 0;
        }
    </style>
</head>
<body>
    <div id="opening">
        <div id="policeIcon"></div>
        <div id="textOpening"><span style="font-size: 35px; text-align: center;">Heads Up! </span>
            <br />
            <span>Find Out whats wrong in the neighbourhood!</span></div>
        <div id="hideCurtain" style="display: inline-block; vertical-align: middle; position: absolute; top: 80%; left: 47%;">
            <a href="#">
                <span class="bottom"></span>
            </a>
        </div>

    </div>
    <div id="map"></div>
    <div id="modal">
        <div id="UserInput">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h3 class="panel-title">Give Your Coordinates!</h3>
                </div>
                <div class="panel-body">
                    <label class="LabelDef" for="basic-url">Latitude</label>
                    <div class="input-group">
                        <input type="text" class="form-control" id="user-latitude" aria-describedby="basic-addon3">
                    </div>
                    <br />
                    <label class="LabelDef" for="basic-url">Longitude</label>
                    <div class="input-group">

                        <input type="text" class="form-control" id="user-longitude" aria-describedby="basic-addon3">
                    </div>
                    <br />
                    <button type="button" id="GetLocation" class="btn btn-default">Get Location!</button>
                    <button type="button" id="GetCrime" class="btn btn-default">Get Crime!</button>
                    <button type="button" id="Cancel" class="btn btn-default">Cancel!</button>
                </div>
            </div>
        </div>

    </div>
    <div id="getInput">Enter your location!</div>

    <script src="https://code.jquery.com/jquery-3.1.1.min.js"></script>
    <script>

       
            $('#hideCurtain').click(function()
            {
                $('#hideCurtain').hide();
                $('#map').show();
                $('#opening').css({'transition': '3s', 'background-position': '0px -200px'});
                $('#opening').css({'transition': '3s', 'height': '0px'});
                $('#policeIcon').css({'transition': '4s', 'top': '400px'});
                $('#textOpening').css({'transition': '4s', 'top': '400px'});
                initMap();
                setTimeout(function()
                {
                    $('#getInput').show();
                }, 2000);
            
            });
       
            function initMap() {
                var myLatLng = { lat: 41.87911338523551, lng: -87.6499218013019 };
                var map = new google.maps.Map(document.getElementById('map'), {
                    zoom: 12,
                    center: myLatLng
                });
                var marker ;
                <% 
        for (int i = 0; i < clusterList.Count; i++)
        { %>
                var markers =[];
                var infoWindows = [];
                <%
            for (int j = 0; j < clusterList[i].crimeEntries.Count; j++)
            {              
            
            %>
           
                marker = new google.maps.Marker({
                    position: <%=getPointValues(clusterList[i].crimeEntries[j].point)%>,
                    title: '<%=getPointData(clusterList[i].crimeEntries[j])%>',
                    map: map
                });
           
                markers.push(marker);
           
           
                <%  } %>
                // Add a marker clusterer to manage the markers.
                var markerCluster = new MarkerClusterer(map, markers,
                      {imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m'});        
                <%       
        }
        %>
                $('#GetLocation').click(function()
                {
                    $('#modal').hide();
                    $('#getInput').hide();
                    var listener1 = map.addListener('click', function(event) {
                        var latitude = event.latLng.lat();
                        var longitude = event.latLng.lng();   
                        google.maps.event.removeListener(listener1);

                        $('#modal').show();
                        $('#user-latitude').val(latitude);
                        $('#user-longitude').val(longitude);
                        $('#getInput').hide();
                    });

                });

            
          


                $('#GetCrime').click(function()
                {
                    var dataValue = {longitudeLatitude : '"'+ $('#user-latitude').val() + "#@#" + $('#user-longitude').val() + '"'};
                    $.ajax({
                        type: "GET",
                        url: "WebForm1.aspx/GetCrime",
                        data: dataValue,
                        contentType: 'application/json; charset=utf-8',
                        dataType: 'json',
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            alert("Request: " + XMLHttpRequest.toString() + "\n\nStatus: " + textStatus + "\n\nError: " + errorThrown);
                        },
                        success: function(data){
                            var markerCrime = new google.maps.Marker({
                                position: {lat :  JSON.parse($('#user-latitude').val()), lng :  JSON.parse($('#user-longitude').val())},
                                icon: "http://icons.iconarchive.com/icons/paomedia/small-n-flat/96/map-marker-icon.png",
                                animation: google.maps.Animation.DROP,
                                map: map
                            });                        

                            var infowindow = new google.maps.InfoWindow({
                                content: data.d
                            });

                            markerCrime.addListener('click', function() {
                                infowindow.open(map, markerCrime);
                            });


                            // Adding data for New Points-------------
                            $.ajax({
                                type: "GET",
                                url: "WebForm1.aspx/GetRelatedPoints",
                                contentType: 'application/json; charset=utf-8',
                                dataType: 'json',
                                error: function (XMLHttpRequest, textStatus, errorThrown) {
                                    alert("Request: " + XMLHttpRequest.toString() + "\n\nStatus: " + textStatus + "\n\nError: " + errorThrown);
                                },
                                success :  function(dataReturn){
                                    var stringReturn = dataReturn.d;
                                    var arrReturn = stringReturn.split("|||");
                                  
                                        for(var k = 0; k < arrReturn.length; k++)
                                        {
                                            if(arrReturn[k] != "")
                                            {

                                            var markerDetails = arrReturn[k];
                                            var markerSplit = markerDetails.split("@@");
                                            var markerLatitude = markerSplit[0];
                                            var markerLongitude = markerSplit[1];
                                            var markerTitle = markerSplit[2];
                                            var markerCrimeNew = new google.maps.Marker({
                                                position:{lat :JSON.parse(markerLongitude), lng : JSON.parse(markerLatitude)},
                                                title: markerTitle,
                                                animation: google.maps.Animation.DROP,
                                                icon: "robbery.png",
                                                map: map
                                            });
                                        }
                                    }

                                }
                           
                            });
  
                            //----------------------------------------


                            google.maps.event.addListener(infowindow, 'domready', function() {
                                $("#nextCrime").on('click', function()
                                {   
                                    var currentInfo = $(this).siblings('.displayCrime');
                                    var nextInfo = $(currentInfo).next();
                                    $(currentInfo).removeClass('displayCrime').addClass('hideCrime');
                                    $(nextInfo).removeClass('hideCrime').addClass('displayCrime');
                                });
                           
                            });

                            map.setZoom(13);      // This will trigger a zoom_changed on the map
                            map.setCenter(new google.maps.LatLng(JSON.parse($('#user-latitude').val()),  JSON.parse($('#user-longitude').val())));
                            var cityCircle = new google.maps.Circle({
                                strokeColor: '#FF0000',
                                strokeOpacity: 0.8,
                                strokeWeight: 2,
                                fillColor: '#FF0000',
                                fillOpacity: 0.35,
                                map: map,
                                center: {lat :  JSON.parse($('#user-latitude').val()), lng :  JSON.parse($('#user-longitude').val())},
                                radius: <%= pointDistance * 1010%>
                                });

                            <%--     <%
        foreach(KeyValuePair<CrimeEntryOutput, double> crimeEntry in listCrimeEntryOutput)
        { 
            
            %>
             var markerCrime = new google.maps.Marker({
                 position: {lat : <%=crimeEntry.Key.crimeEntry.point.latitude%>, lng :  <%=crimeEntry.Key.crimeEntry.point.longitude%>},
                            icon: "http://icons.iconarchive.com/icons/icons-land/vista-map-markers/96/Map-Marker-Flag-1-Left-Azure-icon.png",
                            map: map
                        });
<% }

        %>--%>
                      
                        },
                        complete: function (jqXHR, status) {
                            $('#modal').hide();
                            $('#getInput').show(); 
                        
                       
                        }
                    });
                });
            }

        

            $('#Cancel').click(function (){
                $('#modal').hide();
                $('#getInput').show();
            });
       

            $('#getInput').click(function (){
                $('#modal').show();
                $('#getInput').hide();
            });

    </script>
    <script src="https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/markerclusterer.js">
    </script>



    <script async defer
        src="https://maps.googleapis.com/maps/api/js?key=AIzaSyDfPVyfFdM0YU_JBY-R0HVfLwOlh1rtgLw&callback=initMap">
    </script>
</body>
</html>
