FROM microsoft/dotnet:2.1-sdk as build-image

WORKDIR /home/app

COPY ./*.sln ./
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ./${file%.*}/ && mv $file ./${file%.*}/; done

RUN dotnet restore

COPY . .

RUN dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="./TestResults/coverage.xml" --verbosity=normal --results-directory /TestResults/ --logger "trx;LogFileName=test_results.trx" ./Tests/Tests.csproj

RUN dotnet publish ./AccountOwnerServer/AccountOwnerServer.csproj -o /publish/

FROM microsoft/dotnet:2.1-aspnetcore-runtime

WORKDIR /publish

COPY --from=build-image /publish .

COPY --from=build-image /TestResults /TestResults

ENV TEAMCITY_PROJECT_NAME = ${TEAMCITY_PROJECT_NAME}

ENTRYPOINT ["dotnet", "AccountOwnerServer.dll"]
