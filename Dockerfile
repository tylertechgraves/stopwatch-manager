FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine3.16 as netCoreBuild
ARG CONFIGURATION=Release
WORKDIR /build
COPY stopwatch-manager stopwatch-manager
WORKDIR /build/stopwatch-manager
RUN dotnet publish -c ${CONFIGURATION} -o out -v normal --framework net7.0
RUN dotnet publish -c ${CONFIGURATION} -o out -v normal --framework net6.0