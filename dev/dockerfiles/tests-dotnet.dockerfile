FROM alpine as proj-env

WORKDIR /app

COPY ./src ./src
COPY ./tests ./tests

# On supprime tous les fichiers excepté les csproj
RUN find . ! -name '*.csproj' -type f -exec rm -f {} +

# On supprime les dossiers qui servent plus à rien
RUN find . -type d -empty -delete

# ----------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:7.0

WORKDIR /app

COPY --from=proj-env /app /app

RUN PROJECTS=$(find . -name "*.csproj"); \
    for proj in $PROJECTS; \
    do dotnet restore $proj; \
    done

COPY ./src ./src
COPY ./tests ./tests

WORKDIR /app/tests

ENTRYPOINT ["bash", "./tests.sh"]
