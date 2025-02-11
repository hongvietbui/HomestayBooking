# Sử dụng image cơ bản của .NET SDK 9 để build project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Sao chép file csproj và restore các dependencies (nếu có)
COPY *.csproj ./
RUN dotnet restore

# Sao chép toàn bộ mã nguồn và build ứng dụng
COPY . ./
RUN dotnet publish -c Release -o out

# Sử dụng image .NET 9 runtime nhẹ để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Khai báo cổng chạy ứng dụng
EXPOSE 8080 8081

# Khởi chạy ứng dụng
ENTRYPOINT ["dotnet", "EXE202.dll"]

