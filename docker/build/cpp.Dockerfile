FROM ubuntu:14.04

# GCC 4.8 is REQUIRED for RakNet compatibility.
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y \
	build-essential gcc-4.8 g++-4.8 git wget tar \
	&& rm -rf /var/lib/apt/lists/*

# Make sure we use the right compiler version.
RUN update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-4.8 100 \
 && update-alternatives --install /usr/bin/g++ g++ /usr/bin/g++-4.8 100

# Modern CMake is required for the build script.
RUN wget -q https://github.com/Kitware/CMake/releases/download/v3.28.3/cmake-3.28.3-linux-x86_64.tar.gz \
	&& tar --strip-components=1 -xz -f cmake-3.28.3-linux-x86_64.tar.gz -C /usr/local \
	&& rm cmake-3.28.3-linux-x86_64.tar.gz

COPY cpp.sh /usr/local/bin/cpp.sh
RUN chmod +x /usr/local/bin/cpp.sh

ENTRYPOINT ["/usr/local/bin/cpp.sh"]
