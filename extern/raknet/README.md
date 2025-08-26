
# RakNet 3.611

Face of Mankind utilizes RakNet for all server communication. This is a slightly modified version of the library
that removes the Autopatcher and some other unused features.

## Building

Due to the age of the library, there are a few hurdles to building it. The main hurdle is that it was written for
C++03, and modern compilers have deprecated some of the features it uses. The [CMakeLists.txt](CMakeLists.txt)
file includes some workarounds for these issues on Windows, however, Linux requires using GCC 4.8.
