# MCargaModBus
Integration software between two systems which reads temperatures from a database (from a particular system), compiles them and from time to time sends to an industrial PLC board using ModBus protocol. Developed in C#.

This application uses log4net for Logging purposes and debugging in case there are problems or for monitoring progress.

It uses 3 Different Threads:
_Connection Thread_ - Simple Connection thread that attempts from time to time to establish connection with ModBus. Once the connection is established, the thread will be destroyed and it will start both the Main Thread and the Heartbeat Thread.
_Main Thread_ - Manages the start of the other threads, and only exists to not block the interface access and updates the temperature data from time to time. I.E. quitting the application or showing the "About" screen.
_Heartbeat thread_ - Once the connection with the ModBus Server has been established the HeartBeat is responsible for transmitting from time to time a heartbeat counter (according to demanding spec) from the database to the ModBus server.


It will read configuration information stored on the registry which will be used as parameters for the application to work:
- Database data (user, password, address, port)
- ModBus data (address, initial address for storage)
- Time (in seconds, how long it will take before transmitting the data)
- Heartbeat (how long it will take to transmit a heartbeat counter data).
