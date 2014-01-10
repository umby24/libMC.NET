#  libMC.NET

.NET Library for Minecraft

libMC.NET is currently a Client-only library for interacting with Minecraft servers using the .NET Framework.

The currently supported Minecraft version is Minecraft **1.7.4**.

For an example of very basic usage, see the TestClient.

This library is designed with ease of use in mind, and handles all networking and encryption for you already. This gives you a great base on which to build a client without having to yourself deal with all the overhead that is the minecraft network protocol.

The library also handles name verification with Minecraft.net, and supports Mojang's newest authentication scheme, **Yggdrasil**.

If you wish to contribute to the project, feel free to fork and create a pull request.

If you find a bug, create an issue so it can be addressed.

## Functionality
For basic clients, you may simply hook the events provided by the library and handle the data provided to you. This should provide you with enough to make a well working client. Do note that the library is not complete and more items will continue to be added.

If you wish to make a slightly more advanced client, you can hook the PacketReceived event, and handle all the packets coming in that way. This will enable you to access the low level data structures of each packet and parse this information for yourself.

For sending data, you will have to build a packet up yourself. After building it up however, the library will take care of the rest and will send it to the server for you.
