FROM mcr.microsoft.com/dotnet/sdk:9.0

COPY dotnet.sh /usr/local/bin/dotnet.sh
RUN chmod +x /usr/local/bin/dotnet.sh

ENTRYPOINT ["/usr/local/bin/dotnet.sh"]
