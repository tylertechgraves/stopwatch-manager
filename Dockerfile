FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine
ARG CONFIGURATION=Release
WORKDIR /build
COPY . .
WORKDIR /build/stopwatch-manager
RUN dotnet publish -c ${CONFIGURATION} -o out -v normal