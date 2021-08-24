# Serial Data Monitor

![serialDataMonitor](small.gif)

## Description

Serial Data Monitor is a multiplatform (Windows, Linux, Mac, ...) tool to interactively receive/edit/monitor data and send commands to an ECU (Electronic Control Unit, can be pic, AVR, ARM, Arduino, ...) via the serial port. It is fast, real time (the embedded system is the master) and proudly made with Godot Engine. The language used is C#. This tool is already used in production but has (for sure) some bugs to discover. Also the MAC version has not been tested since I don't have a MAC, so developpers are welcome.

## Table of Contents

The **host** folder contains all the development files

The **ecu** folder contains an examples of a bare metal application (included the .ini file) and the naked monitor.h and monitor.c files

The **binaries** folder contains the application for various platforms. See the related README files

## Installation

Download the zip file for your platform, unzip on a folder of your choice and execute the application. You have to write the right .ini file that match the data sent by your ECU. Data sent by the ECU is configured in monitor.c

## Usage/Documentation

See [wiki](https://github.com/papyDoctor/serialDataMonitor/wiki).

In summary, the application configure itself (GUI, behavior,...) based on informations given in  **ONE** .ini file. This .ini file is writen by the user.

Also two files (monitor.h and monitor.c) must be added and configured to the ECU project

## Contributing

This project can be enhanced in several ways, ideas and contributors welcomed. A good feature would be to have an Arduino (monitor.hpp and monitor.cpp)

## Credits

Thanks to Godot Engine developpers who have done this excellent multiplatform development tool

## License

MIT license
