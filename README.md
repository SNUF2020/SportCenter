# SportCenter
After shut down of SportTracks desktop application I creat my own "SportTracks"

STRUCTURE of CODE:
- Fields (DataBase, MapControl) -> rest in Helper_FieldsParameters
- General/Form1 (Methods, Events)
- DataBase (Methods, Events)
- MapControl (Methods, Events)
- ControlPanel (Methods, Events)
- Elevation Chart
- SC IO

Used sources:
Based on code (from https://sourceforge.net/projects/opensportsman/) data from USB device (Garmin Forerunner 305) will be fetched (track and lap data)
Original code from  mauricemarinus under GNU General Public License version 2.0 (GPLv2)

Embedding of elevation data: https://github.com/itinero/srtm

GpxReader: Source code based on dlg.krakow.pl code (copyright (c) 2011-2016, dlg.krakow.pl) 
//  Doku GPX-Writer/Reader: https://github.com/macias/Gpx/blob/master/Gpx/Implementation/GpxWriter.cs

data compression: see https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp 
-> first solution from https://stackoverflow.com/users/613130/xanatos
