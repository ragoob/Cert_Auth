
FROM golang:alpine as cert

RUN apk update \
    && apk add git openssl nss-tools \
    && rm -rf /var/cache/apk/*

RUN cd /go && \
    go get -u github.com/FiloSottile/mkcert && \
    cd src/github.com/FiloSottile/mkcert && \
    go build -o /bin/mkcert

WORKDIR /root/.local/share/mkcert
# copied from %localappdata%\mkcert
COPY gd_bundle-g2-g1.crt /root/.local/share/mkcert/
COPY serveraptardotcomapi.key /root/.local/share/mkcert/
COPY fa86e37d418a393e.pem /root/.local/share/mkcert/
COPY fa86e37d418a393e.crt /root/.local/share/mkcert/

# make a pfx with known password by making a pem/key then converting it
# includes minikube and docker ips
RUN  openssl pkcs12 -export -out aptardotcomapi.pfx -inkey serveraptardotcomapi.key -in fa86e37d418a393e.pem -certfile gd_bundle-g2-g1.crt -passout pass:https-api



FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build

WORKDIR /src
COPY *.csproj .
RUN dotnet restore

# also copies rootCA-key.pem, but we'll throw this image away in a minute
COPY . .
RUN dotnet publish -c Debug -o /dist


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

COPY --from=cert /bin/mkcert /bin/mkcert
# root cert but not root private key:
COPY --from=cert /root/.local/share/mkcert/rootCA.pem /root/.local/share/mkcert/gd_bundle-g2-g1.crt
COPY --from=cert /root/.local/share/mkcert/serveraptardotcomapi.pfx /app/
# TODO: consider `chmod 600 *.pem and *.pfx`

# tell ASP.NET Core to use the cert
ENV Kestrel__Certificates__Default__Path=/app/serveraptardotcomapi.pfx
ENV Kestrel__Certificates__Default__Password=https-api

# trust root cert
RUN apk update \
    && apk add nss-tools \
    && rm -rf /var/cache/apk/* \
    && mkcert -install \
    && apk del nss-tools \
    && rm -rf /bin/mkcert

WORKDIR /app
COPY --from=build /dist /app

# so no HSTS:
ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_URLS https://+:4001;http://+:4000
EXPOSE 4000 4001

ENTRYPOINT ["dotnet", "Secured_api.dll"]
