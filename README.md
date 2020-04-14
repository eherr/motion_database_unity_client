
# Motion Database Unity Client


Unity app used by the [web client] (https://github.com/eherr/motion_database_webclient) of the [motion database](https://github.com/eherr/motion_database_server) for the visualization of motions.  
An instance of the database is running on [https://motion.dfki.de](https://motion.dfki.de/index.php/motion-capture-data/).

To integrate the app with the web client it needs to be build for WebGL:
https://docs.unity3d.com/Manual/webgl-building.html

The output folder needs be called "webviewer" and the resulting content of the folder needs to be copied into the folder /assets/Unity/Build of the web client. This is way the hard coded path to the app does not need to be updated.

## Controls

Camera control:  
Rotation: Left Mouse Button  
Translation: Right Mouse Button  
Zoom: Mouse Wheel  



## License
Copyright (c) 2019 DFKI GmbH. 

The application uses [Newtonsoft.Json for Unity](https://github.com/jilleJr/Newtonsoft.Json-for-Unity) to support processing of binary encoded JSON data.
